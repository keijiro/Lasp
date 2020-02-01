#pragma once

// The LASP driver that handles PortAudio objects,
// internal filter bank and ring buffers.

#define _USE_MATH_DEFINES
#include "cmath"
#include "portaudio.h"
#include "Debug.h"
#include "RingBuffer.h"
#include "BiquadFilter.h"
#include "../KissFft/kiss_fft.h"

namespace Lasp
{
    class Driver
    {
    public:

        Driver() : stream_(nullptr)
        {
            auto err = Pa_Initialize();
            if (err == paNoError)
                LASP_LOG("Initialized.");
            else
                LASP_PAERROR("Initialization failed.", err);
        }

        ~Driver()
        {
            if (stream_ != nullptr) CloseStream();

            auto err = Pa_Terminate();
            if (err == paNoError)
                LASP_LOG("Finalized.");
            else
                LASP_PAERROR("Finalization failed.", err);
        }

        bool OpenStream()
        {
            if (stream_ != nullptr)
            {
                LASP_WARN("The input stream has been already opened.");
                return true;
            }

            // Try to open the input stream with sample rates in this order:
            //   48kHz -> 44.1kHz -> device default
            // Why don't we use the device default from the first? Because it
            // may give 96kHz. That's too high for our applications.
            LASP_LOG("Try to open the default input device with 48kHz sample rate.");
            auto err = TryOpenStream(48000);
            if (err != paNoError)
            {
                LASP_LOG("Failed. Retry with 44.1kHz.");
                err = TryOpenStream(44100);
                if (err != paNoError)
                {
                    LASP_LOG("Failed again. This time retry with the device default rate.");
                    auto info = Pa_GetDeviceInfo(Pa_GetDefaultInputDevice());
                    if (info == nullptr)
                    {
                        LASP_PAERROR("Failed to access the default input device.", err);
                        return false;
                    }

                    err = TryOpenStream(static_cast<float>(info->defaultSampleRate));
                    if (err != paNoError)
                    {
                        LASP_PAERROR("Failed to open the default input device.", err);
                        return false;
                    }
                }
            }

            // Initialize the filter bank.
            auto fc = 960.0f / sampleRate_;
            filters_[0].setLowpass(fc, 0.15f);
            filters_[1].setLowpass(fc, 0.15f);
            filters_[2].setBandpass(fc, 0.15f);
            filters_[3].setBandpass(fc, 0.15f);
            filters_[4].setHighpass(fc, 0.15f);
            filters_[5].setHighpass(fc, 0.15f);

            // Open the input stream.
            err = Pa_StartStream(stream_);
            if (err != paNoError)
            {
                LASP_PAERROR("Failed to start the input stream.", err);
                Pa_CloseStream(stream_);
                stream_ = nullptr;
                return false;
            }

            // Show the stream information.
            auto info = Pa_GetStreamInfo(stream_);
            if (info != nullptr)
                LASP_LOG("Stream started. Sample rate = %f Hz, Input latency = %f sec.", info->sampleRate, info->inputLatency);
            else
                LASP_LOG("Stream started.");

            // Initialize hann windowing array for FFT
            for (auto i = 0; i < nFft; i++)
                hann_[i] = (1 - cos(2 * M_PI * i / nFft)) * 0.5;
        	
            return true;
        }

        void CloseStream()
        {
            if (stream_ == nullptr)
            {
                LASP_WARN("The input stream hasn't been opened.");
                return;
            }

            auto err = Pa_StopStream(stream_);
            if (err != paNoError) LASP_PAERROR("Failed to stop the input stream.", err);

            err = Pa_CloseStream(stream_);
            if (err != paNoError) LASP_PAERROR("Failed to close the input stream.", err);

            stream_ = nullptr;

            LASP_LOG("Stream closed.");
        }

        float getSampleRate() const
        {
            return sampleRate_;
        }

        const Lasp::RingBuffer& getBuffer(int index) const
        {
            return buffers_[index];
        }

        void setAvgFftBuffer(int avgType, float* dest, int length) const
        {
	        switch (avgType)
	        {
	        case 0:
		        setLinAvgFft(dest, length);
		        break;

	        case 1:
		        setLinAvgFft(dest, length);
		        break;
            default:
                setLinAvgFft(dest, length);
                break;
	        }
        }
    	
    private:

        PaStream* stream_;
        float sampleRate_;

        // Three-band filter bank.
        // We use two filters per band to get 24db/oct slope.
        // The filters are assigned in this order: [LPF1, LPF2, BPF1, BPF2, HPF1, HPF2]
        std::array<BiquadFilter, 6> filters_;

        // Ring buffers used for storing filtered results.
        // The buffers are assigned in this order: [non-filtered, low, middle, high]
        std::array<RingBuffer, 4> buffers_;

    	// Buffer to store FFT results
        RingBuffer fftBuffer_;
    	// Number of FFT bands
        static int const nFft = 128;
    	// Kiss FFT variables
        kiss_fft_cfg config_;
        kiss_fft_cpx inbuf_[nFft] = { 0 };
        kiss_fft_cpx outbuf_[nFft] = { 0 };
    	// Array for hann windowing
        float hann_[nFft] = { 0.0 };
    	
        PaError TryOpenStream(float sampleRate)
        {
            auto deviceInfo = Pa_GetDeviceInfo(Pa_GetDefaultInputDevice());
			if (deviceInfo == nullptr) return paDeviceUnavailable;

            PaStreamParameters params;
            params.channelCount = 1;
            params.device = Pa_GetDefaultInputDevice();
            params.hostApiSpecificStreamInfo = nullptr;
            params.sampleFormat = paFloat32;
            params.suggestedLatency = deviceInfo->defaultLowInputLatency;

        	auto err = Pa_OpenStream(
                &stream_,
                &params,
                nullptr,
                sampleRate,
                paFramesPerBufferUnspecified,
                paNoFlag,
                AudioCallback,
                this
            );

            if (err == paNoError) sampleRate_ = sampleRate;

            return err;
        }

        static int AudioCallback(
            const void* inputBufferPointer,
            void* /* outputBufferPointer */,
            unsigned long framesPerBuffer,
            const PaStreamCallbackTimeInfo* timeInfo,
            PaStreamCallbackFlags statusFlags,
            void* userData
        )
        {
            auto inputBuffer = reinterpret_cast<const float*>(inputBufferPointer);
            auto driver = reinterpret_cast<Driver*>(userData);

            auto& buffer_raw = driver->buffers_[0];
            auto& buffer_lpf = driver->buffers_[1];
            auto& buffer_bpf = driver->buffers_[2];
            auto& buffer_hpf = driver->buffers_[3];

            auto& lpf1 = driver->filters_[0];
            auto& lpf2 = driver->filters_[1];
            auto& bpf1 = driver->filters_[2];
            auto& bpf2 = driver->filters_[3];
            auto& hpf1 = driver->filters_[4];
            auto& hpf2 = driver->filters_[5];

            for (auto i = 0u; i < framesPerBuffer; i++)
            {
                auto input = inputBuffer[i];
                buffer_raw.pushFrame(input);
                buffer_lpf.pushFrame(lpf2.feedSample(lpf1.feedSample(input)));
                buffer_bpf.pushFrame(bpf2.feedSample(bpf1.feedSample(input)));
                buffer_hpf.pushFrame(hpf2.feedSample(hpf1.feedSample(input)));
            }

        	// FFT
            auto nFft = driver->nFft;
            auto& fftBuffer = driver->fftBuffer_;
            auto& hannRes = driver->hann_;
            auto& config = driver->config_;
        	auto& in = driver->inbuf_;
            auto& out = driver->outbuf_;

        	// Setup complex number array for FFT input
            for (auto i = 0; i < nFft; i++) {
                in[i].r = inputBuffer[i] * hannRes[i];
                in[i].i = 0;
            }

        	// Perform FFT calculations
            config = kiss_fft_alloc(nFft, 0, NULL, NULL);
            kiss_fft(config, in, out);
            for (auto j = 0; j < nFft / 2; j++)
            {
            	// Skipping out[0], which is the DC bin (0Hz)
	            const auto offset = j + 1;
                fftBuffer.pushFrame(sqrt(out[offset].r * out[offset].r + out[offset].i * out[offset].i));
            }

            kiss_fft_free(config);
            return 0;
        }

        void setLinAvgFft(float* dest, int length) const
        {
            // Linear averaging to reduce the number of FFT bands
            const auto fft = new float[fftBuffer_.getSize()];
            const auto avgWidth = int(nFft / 2 / length);
            fftBuffer_.copyRecentFrames(fft, fftBuffer_.getSize());

            for (auto i = 0; i < length; i++)
            {
                float avg = 0;
                int j;
                for (j = 0; j < avgWidth; j++)
                {
                    const auto offset = j + i * avgWidth;
                    if (offset < nFft)
                        avg += fft[offset];
                    else
                        break;
                }
                avg /= float(j + 1);
                dest[i] = avg;
            }

            delete[] fft;
        }
    };
}
