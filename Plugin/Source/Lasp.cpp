#include "pa_debugprint.h"
#include "IUnityInterface.h"
#include "Driver.h"
#include <memory>
#include <string>

namespace
{
    void PaPrintCallback(const char* log)
    {
        std::string temp(log);
        // Remove tailing newline character.
        if (!temp.empty() && temp.back() == '\n') temp.pop_back();
        Lasp::Debug::log("PortAudio> %s", temp.c_str());
    }
}

extern "C"
{
    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
    {
        PaUtil_SetDebugPrintFunction(PaPrintCallback);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
    {
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspReplaceLogger(Lasp::Debug::LogFunction p)
    {
        Lasp::Debug::setLogFunction(p);
    }

    void UNITY_INTERFACE_EXPORT * UNITY_INTERFACE_API LaspCreateDriver()
    {
        return new Lasp::Driver();
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspDeleteDriver(void* driver)
    {
        delete reinterpret_cast<Lasp::Driver*>(driver);
    }

    bool UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspOpenStream(void* driver)
    {
        return reinterpret_cast<Lasp::Driver*>(driver)->OpenStream();
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspCloseStream(void* driver)
    {
        reinterpret_cast<Lasp::Driver*>(driver)->CloseStream();
    }

    float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspGetSampleRate(void* driver)
    {
        return reinterpret_cast<Lasp::Driver*>(driver)->getSampleRate();
    }

    float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspGetPeakLevel(void* driver, int bufferIndex, float duration)
    {
        auto pd = reinterpret_cast<Lasp::Driver*>(driver);
        auto range = static_cast<size_t>(pd->getSampleRate() * duration);
        return pd->getBuffer(bufferIndex).getPeakLevel(range);
    }

    float UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspCalculateRMS(void* driver, int bufferIndex, float duration)
    {
        auto pd = reinterpret_cast<Lasp::Driver*>(driver);
        auto range = static_cast<size_t>(pd->getSampleRate() * duration);
        return pd->getBuffer(bufferIndex).calculateRMS(range);
    }

    int32_t UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API LaspRetrieveWaveform(void* driver, int bufferIndex, float* dest, int32_t length)
    {
        auto pd = reinterpret_cast<Lasp::Driver*>(driver);
        auto& buffer = pd->getBuffer(bufferIndex);
        buffer.copyRecentFrames(dest, length);
        return std::min(length, static_cast<int32_t>(buffer.getSize()));
    }
}
