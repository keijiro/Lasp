#include "pa_debugprint.h"
#include "IUnityInterface.h"
#include "Driver.h"
#include <memory>

#if defined(_MSC_VER)
#define WIN32_LEAN_AND_MEAN
#define NOMINMAX
#include <windows.h>
#endif

extern "C"
{
    static void DebugPrintCallback(const char* log)
    {
        std::printf("%s", log);
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
    {
        PaUtil_SetDebugPrintFunction(DebugPrintCallback);

    #if defined(_DEBUG) && defined(_MSC_VER)
        // Create a new console and bind stdout/stderr to it.
        FILE * pConsole;
        AllocConsole();
        freopen_s(&pConsole, "CONOUT$", "wb", stdout);
        freopen_s(&pConsole, "CONOUT$", "wb", stderr);
    #endif
    }

    void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
    {
    #if defined(_DEBUG) && defined(_MSC_VER)
        // Close the stdout/stderr console.
        std::fclose(stdout);
        std::fclose(stderr);
        FreeConsole();
    #endif
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
