#pragma once

// Debug helpers

#include <cstdio>
#include <cstdarg>

namespace Lasp
{
    class Debug
    {
    public:

        typedef void(*LogFunction)(const char*);

        static void setLogFunction(LogFunction function)
        {
            logFunction(function);
        }

        static void log(const char* format, ...)
        {
            if (logFunction() == nullptr) return;
            char buffer[1024];
            va_list args;
            va_start(args, format);
            std::vsnprintf(buffer, sizeof(buffer), format, args);
            logFunction()(buffer);
            va_end(args);
        }

    private:

        static LogFunction logFunction(LogFunction replacement = nullptr)
        {
            static LogFunction pointer;
            if (replacement != nullptr) pointer = replacement;
            return pointer;
        }
    };
}

#if defined(_DEBUG)
#define LASP_LOG(format, ...) Lasp::Debug::log("LASP - " format, ##__VA_ARGS__)
#define LASP_WARN(format, ...) Lasp::Debug::log("LASP warning - " format, ##__VA_ARGS__)
#define LASP_PAERROR(message, error) Lasp::Debug::log("LASP error - %s (PortAudio: %s)", message, Pa_GetErrorText(error))
#else
#define LASP_LOG(format, ...)
#define LASP_WARN(format, ...)
#define LASP_PAERROR(message, error) ((void)error)
#endif 
