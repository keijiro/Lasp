CMAKE_MINIMUM_REQUIRED(VERSION 2.8)

SET(LASP_INCLUDES
  ../PortAudio/common/pa_allocation.h
  ../PortAudio/common/pa_converters.h
  ../PortAudio/common/pa_cpuload.h
  ../PortAudio/common/pa_debugprint.h
  ../PortAudio/common/pa_dither.h
  ../PortAudio/common/pa_endianness.h
  ../PortAudio/common/pa_hostapi.h
  ../PortAudio/common/pa_memorybarrier.h
  ../PortAudio/common/pa_process.h
  ../PortAudio/common/pa_ringbuffer.h
  ../PortAudio/common/pa_stream.h
  ../PortAudio/common/pa_trace.h
  ../PortAudio/common/pa_types.h
  ../PortAudio/common/pa_util.h
  ../PortAudio/include/pa_linux_alsa.h
  ../PortAudio/include/portaudio.h
  ../PortAudio/os/unix/pa_unix_util.h
)

SET(LASP_SOURCES
  ../PortAudio/common/pa_allocation.c
  ../PortAudio/common/pa_converters.c
  ../PortAudio/common/pa_cpuload.c
  ../PortAudio/common/pa_debugprint.c
  ../PortAudio/common/pa_dither.c
  ../PortAudio/common/pa_front.c
  ../PortAudio/common/pa_process.c
  ../PortAudio/common/pa_ringbuffer.c
  ../PortAudio/common/pa_stream.c
  ../PortAudio/common/pa_trace.c
  ../PortAudio/hostapi/alsa/pa_linux_alsa.c
  ../PortAudio/os/unix/pa_unix_hostapis.c
  ../PortAudio/os/unix/pa_unix_util.c
)

SET(LASP_INCLUDE_PATHS ../PortAudio/include ../PortAudio/common ../PortAudio/os/unix ${ALSA_INCLUDE_DIRS})

SET(LASP_COMPILE_DEFINITIONS PA_USE_ALSA)
SET(LASP_LIBRARY_DEPENDENCIES ${ALSA_LIBRARIES} m pthread)
SET(LASP_PKGCONFIG_LDFLAGS "-lasound -lm -lpthread")

SET(LASP_COMPILE_DEFINITIONS ${LASP_COMPILE_DEFINITIONS} PA_ENABLE_DEBUG_OUTPUT)

ADD_LIBRARY(lasp SHARED ${LASP_INCLUDES} ${LASP_SOURCES})
SET_PROPERTY(TARGET lasp APPEND_STRING PROPERTY COMPILE_DEFINITIONS ${LASP_COMPILE_DEFINITIONS})
TARGET_INCLUDE_DIRECTORIES(lasp PRIVATE ${LASP_INCLUDE_PATHS})
TARGET_LINK_LIBRARIES(lasp ${LASPLIBRARY_DEPENDENCIES})
