gcc -Wall \
    -O2 -fPIC -Wl,--gc-sections \
    -D PA_USE_ALSA \
    -D _DEBUG \
    -D PA_ENABLE_DEBUG_OUTPUT \
    -I ../PortAudio/include \
    -I ../PortAudio/common \
    -I ../PortAudio/os/unix \
    ../PortAudio/common/pa_allocation.c \
    ../PortAudio/common/pa_converters.c \
    ../PortAudio/common/pa_cpuload.c \
    ../PortAudio/common/pa_debugprint.c \
    ../PortAudio/common/pa_dither.c \
    ../PortAudio/common/pa_front.c \
    ../PortAudio/common/pa_process.c \
    ../PortAudio/common/pa_ringbuffer.c \
    ../PortAudio/common/pa_stream.c \
    ../PortAudio/common/pa_trace.c \
    ../PortAudio/hostapi/alsa/pa_linux_alsa.c \
    ../PortAudio/os/unix/pa_unix_hostapis.c \
    ../PortAudio/os/unix/pa_unix_util.c \
    ../Source/Lasp.cpp \
    -l asound -lm -lpthread \
    -shared -o libLasp.so
