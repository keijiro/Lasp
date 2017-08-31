LASP
====

**LASP** is a Unity plugin providing low-latency, high-performance and
easy-to-use audio input functionality that is useful for creating audio
reactive visuals.

[Demo 1](http://radiumsoftware.tumblr.com/post/163009893309),
[Demo 2](http://radiumsoftware.tumblr.com/post/163095570430),
[Demo 3](http://radiumsoftware.tumblr.com/post/163137586857)

Features
--------

- Low latency (less than 16 ms) audio input.
- High performance audio analysis (peak level detection, RMS calculation) with
  C++ native code.
- Three band (low, middle, high) filter bank useful for detecting rhythmic
  accents.
- Supports Windows (WASAPI), macOS (Core Audio) and Linux (ALSA).

System Requirements
-------------------

- Unity 2017.1 or later

At the moment, LASP only supports desktop platforms (Windows, macOS and Linux).

Installation
------------

Download one of the unitypackage files from the [Releases] page and import it
to a project.

[Releases]: https://github.com/keijiro/Lasp/releases

How To Use
----------

All the public methods of LASP are implemented in [`AudioInput`] as static
class methods that can be called without any setup. Each of these methods has a
[`FilterType`] argument and returns filtered results based on the argument (or
just returns non-filtered results with `FilterType.Bypass`).

#### GetPeakLevel/GetPeakLevelDecibel

`GetPeakLevel` returns the peak level of the audio signal during the last
frame. `GetPeakLevelDecibel` returns the peak level in dBFS.

`AudioInput` automatically caches the results, so that these methods can be
called multiple times without wasting CPU time.

#### CalculateRMS/CalculateRMSDecibel

`CalculateRMS` calculates and returns the RMS (root mean square) of the audio
signal level during the last frame. `CalculateRMSDecibel` returns the RMS in
dBFS.

`AudioInput` automatically caches the results, so that these methods can be
called multiple times without wasting CPU time.

#### RetrieveWaveform

`RetrieveWaveform` copies the most recent waveform data from the internal
buffer to a given float array. The length of the array should be shorter than
the internal buffer. Less than 1024 would be good.

[`AudioInput`]: Assets/Lasp/AudioInput.cs
[`FilterType`]: Assets/Lasp/Internal/PluginEntry.cs#L9

Current Limitations
-------------------

- LASP always tries to use the system default device for recording. There is no
  way to use a device that is not assigned as default.
- LASP only supports monophonic input. Only the first channel (the left channel
  in case of stereo input) will be enabled when using a multi-channel audio
  device.

License
-------

[MIT](LICENSE.txt)
