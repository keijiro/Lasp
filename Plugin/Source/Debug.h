#pragma once

// Macros for logging/warning/errors

#if defined(_DEBUG)
#define LASP_LOG(format, ...) std::printf("LASP - "##format##"\n", __VA_ARGS__)
#define LASP_WARN(format, ...) std::fprintf(stderr, "LASP warning - "##format##"\n", __VA_ARGS__)
#define LASP_PAERROR(message, error) std::fprintf(stderr, "LASP error - %s (PortAudio: %s)\n", message, Pa_GetErrorText(error))
#else
#define LASP_LOG(format, ...)
#define LASP_WARN(format, ...)
#define LASP_PAERROR(message, error) ((void)error)
#endif