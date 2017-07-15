#include "stdafx.h"
#include <cstdio>
#include <cmath>
#include "portaudio.h"
#include "IUnityInterface.h"
#include "RingBuffer.h"

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
    Lasp::RingBuffer ringBuffer;

    static int AudioCallback(
        const void* inputBuffer,
		void* /* outputBuffer */,
        unsigned long framesPerBuffer,
        const PaStreamCallbackTimeInfo* timeInfo,
        PaStreamCallbackFlags statusFlags,
        void* userData
    )
    {
		auto in = reinterpret_cast<const float*>(inputBuffer);
		for (auto i = 0u; i < framesPerBuffer; i++) ringBuffer.pushFrame(*in++);
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

	float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspGetPeakLevel(float duration)
	{
        return ringBuffer.getPeak(static_cast<size_t>(duration * 48000));
	}

    float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspGetRmsLevel(float duration)
    {
        return ringBuffer.getRMS(static_cast<size_t>(duration * 48000));
    }

    int32_t UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspCopyWaveform(float* dest, int32_t length)
    {
        ringBuffer.copyRecentFrames(dest, length);
        return min(length, static_cast<int32_t>(ringBuffer.size()));
    }
}