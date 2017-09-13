// Native audio plugin for loopback to LASP

#include "AudioPluginInterface.h"
#include "Driver.h"
#include <cstring>

namespace
{
    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK CreateCallback(
        UnityAudioEffectState* state
    )
    {
        LASP_LOG("Loopback started.");
        return UNITY_AUDIODSP_OK;
    }

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ReleaseCallback(
        UnityAudioEffectState* state
    )
    {
        LASP_LOG("Loopback disconnected.");
        return UNITY_AUDIODSP_OK;
    }

    UNITY_AUDIODSP_RESULT UNITY_AUDIODSP_CALLBACK ProcessCallback(
        UnityAudioEffectState* state, float* inBuffer, float* outBuffer,
        unsigned int length, int inChannels, int outChannels
    )
    {
        Lasp::Driver::updateSampleRate(static_cast<float>(state->samplerate));

        auto norm = 1.0f / outChannels;
        auto offs = 0u;

        for (unsigned int i = 0; i < length; i++)
        {
            auto sum = 0.0f;

            for (int ch = 0; ch < outChannels; ch++)
            {
                auto v = inBuffer[offs];
                outBuffer[offs++] = v;
                sum += v;
            }

            Lasp::Driver::feedSample(sum * norm);
        }

        return UNITY_AUDIODSP_OK;
    }

    UnityAudioEffectDefinition s_effectDefinition;
    UnityAudioEffectDefinition* s_effectList[] = { &s_effectDefinition };
}

extern "C" UNITY_AUDIODSP_EXPORT_API int UnityGetAudioEffectDefinitions(
    UnityAudioEffectDefinition*** definitionptr
)
{
    auto& def = s_effectDefinition;

    def.structsize = sizeof(UnityAudioEffectDefinition);
    def.paramstructsize = sizeof(UnityAudioParameterDefinition);
    def.apiversion = UNITY_AUDIO_PLUGIN_API_VERSION;

    strncpy(def.name, "LASP Loopback", sizeof(def.name));
    def.pluginversion = 0x010000;
    def.channels = 1;

    def.create = CreateCallback;
    def.release = ReleaseCallback;
    def.process = ProcessCallback;

    *definitionptr = s_effectList;
    return 1;
}
