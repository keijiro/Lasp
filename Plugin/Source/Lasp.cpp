#include "stdafx.h"
#include <cstdio>
#include <cmath>
#include "portaudio.h"
#include "IUnityInterface.h"
#include "RingBuffer.h"
#include "BiquadFilter.h"

#if defined(_DEBUG)
#define LASP_LOG(format, ...) std::printf("LASP: "##format##"\n", __VA_ARGS__)
#define LASP_ERROR(err) std::printf("LASP error (%d) %s\n", err, Pa_GetErrorText(err))
#else
#define LASP_LOG(x)
#define LASP_ERROR(err)
#endif

namespace
{
    PaStream* stream;

    Lasp::BiquadFilter filters[6];
    Lasp::RingBuffer ringBuffers[4];

    bool checkBufferIndex(int index)
    {
        return index >= 0 && index < 4;
    }

    int AudioCallback(
        const void* inputBufferPointer,
		void* /* outputBufferPointer */,
        unsigned long framesPerBuffer,
        const PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags,
        void* userData
    )
    {
		auto inputBuffer = reinterpret_cast<const float*>(inputBufferPointer);
        for (auto i = 0u; i < framesPerBuffer; i++)
        {
            auto input = inputBuffer[i];
            ringBuffers[0].pushFrame(input);
            ringBuffers[1].pushFrame(filters[1].feedSample(filters[0].feedSample(input))); // Low
            ringBuffers[2].pushFrame(filters[3].feedSample(filters[2].feedSample(input))); // Middle
            ringBuffers[3].pushFrame(filters[5].feedSample(filters[4].feedSample(input))); // High
        }
		return 0;
    }
}

extern "C"
{
	void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
#if defined(_DEBUG)
		// Open a console and bind stdout to it.
		FILE * pConsole;
		AllocConsole();
		freopen_s(&pConsole, "CONOUT$", "wb", stdout);
#endif
	}

	void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
	{
#if defined(_DEBUG)
		// Close the stdout console.
		std::fclose(stdout);
		FreeConsole();
#endif
	}

	bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspInitialize()
	{
		auto err = Pa_Initialize();
		if (err != paNoError)
		{
			LASP_ERROR(err);
			return false;
		}

		err = Pa_OpenDefaultStream(&stream, 1, 0, paFloat32, 48000, paFramesPerBufferUnspecified, AudioCallback, nullptr);
		if (err != paNoError)
		{
			LASP_ERROR(err);
			return false;
		}

		err = Pa_StartStream(stream);
		if (err != paNoError)
		{
			LASP_ERROR(err);
			return false;
		}

        filters[0].setLowpass(0.02f, 0.15f);
        filters[1].setLowpass(0.02f, 0.15f);
        filters[2].setBandpass(0.02f, 0.15f);
        filters[3].setBandpass(0.02f, 0.15f);
        filters[4].setHighpass(0.02f, 0.15f);
        filters[5].setHighpass(0.02f, 0.15f);

		LASP_LOG("Initialized");
		return true;
	}

	void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspFinalize()
	{
		if (stream)
		{
			Pa_StopStream(stream);
			Pa_CloseStream(stream);
		}

		Pa_Terminate();

		LASP_LOG("Finalized.");
	}

	float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspGetPeakLevel(int bufferIndex, float duration)
	{
        if (!checkBufferIndex(bufferIndex)) return 0;
        return ringBuffers[bufferIndex].getPeakLevel(static_cast<size_t>(duration * 48000));
	}

    float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspCalculateRMS(int bufferIndex, float duration)
    {
        if (!checkBufferIndex(bufferIndex)) return 0;
        return ringBuffers[bufferIndex].calculateRMS(static_cast<size_t>(duration * 48000));
    }

    int32_t UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspCopyWaveform(int bufferIndex, float* dest, int32_t length)
    {
        if (!checkBufferIndex(bufferIndex)) return 0;
        auto& buffer = ringBuffers[bufferIndex];
        buffer.copyRecentFrames(dest, length);
        return min(length, static_cast<int32_t>(buffer.size()));
    }
}