LASP
====

![gif](https://i.imgur.com/avAppLA.gif)
![gif](https://i.imgur.com/8KTOr2u.gif)

**LASP** is a Unity plugin providing low-latency, high-performance and
easy-to-use audio input functionality that is useful for creating audio
reactive visuals.

Features
--------

- Low latency (less than 16 ms) audio input.
- High performance audio analysis (peak level detection, RMS calculation) with
  C++ native implementation.
- Three band (low, middle, high) filter bank useful for detecting rhythmic
  accents.
- Dynamic normalization based on the most recent peak level.
- Multi platform support via [PortAudio].

[PortAudio]: http://www.portaudio.com

System Requirements
-------------------

- Unity 2018.3 or later

At the moment, LASP only supports desktop platforms (Windows, macOS and Linux).

Installation
------------

Download one of the unitypackage files from the [Releases] page and import it
to a project.

[Releases]: https://github.com/keijiro/Lasp/releases

You can also use [Git support on Package Manager] to import the package. Add
the following line to the `dependencies` section in the package manifest file
(`Packages/manifest.json`).

```
"jp.keijiro.lasp": "https://github.com/keijiro/Lasp.git#upm"
```

[Git support on Package Manager]:
    https://forum.unity.com/threads/git-support-on-package-manager.573673/

Audio Input Tracker Component
-----------------------------

The **Audio Input Tracker** component is used to receive audio input and
control other components by normalized audio level.

![GIF](https://i.imgur.com/ddCZSs5.gif)

It tracks the most recent peak level and calculates the normalized level value
based on the difference between the current level and the peak level. It only
outputs an effective value when the current level is in its dynamic range,
which is indicated by the gray band in the VU meter.

### Filter Type

Four types of filters are available: **Bypass**, **Low-Pass**, **Band-Pass**
and **High-Pass**. These filters are useful to detect a specific type of
rhythmic accents. For instance, the low-pass filter can be used to make a
behavior that reacts to kick drums and basslines.

### Dynamic Range (dB)

This specifies the range of audio level normalization; The output value becomes
zero when the input level is equal or lower than (*peak* - *dynamic range*).

### Peak Tracking

When enabled, it automatically tracks the peak level as explained above. When
disabled, the peak level is fixed at 0 dB. In this case, the effective range
can be manually controlled by the **Gain** property, which is only shown when
peak tracking is disabled.

### Hold And Fall Down

This adds a "peak-hold and fall down" behavior to the output value that is
commonly used in VU meters. This is useful to make choppy animation smooth.

![GIF](https://i.imgur.com/MEojdmD.gif)

Scripting Interface
-------------------

LASP also provides audio input functionality via the [`MasterInput`] class. All
the following methods are implemented as static methods and can be used without
any setup.

#### GetPeakLevel/GetPeakLevelDecibel

`GetPeakLevel`/`GetPeakLevelDecibel` are used to get the peak level of the
audio input during the last frame. `GetPeakLevel` returns the value in linear
scale; `GetPeakLevelDecibel` uses dBFS instead.

`MasterInput` automatically caches the results, so that these methods can be
called multiple times without wasting CPU time.

#### CalculateRMS/CalculateRMSDecibel

`CalculateRMS`/`CalculateRMSDecibel` are almost the same to
`GetPeakLevel`/`GetPeakLevelDecibel` but use RMS (root mean square) instead
of peak level.

#### RetrieveWaveform

`RetrieveWaveform` copies the most recent waveform data from the internal
buffer to a given float array. The length of the array must be shorter than the
internal buffer. It's safe to use 1024 or less.

[`MasterInput`]: Assets/Lasp/Runtime/MasterInput.cs#L16
[`FilterType`]: Assets/Lasp/Runtime/MasterInput.cs#L9

Tips
----

### Keep dynamic range as narrow as possible

Although it's important to have a wide dynamic range for expressiveness, it
tends to make animation slower and unclear. In general, it's recommended to
keep the dynamic range as narrow as possible to make animation fast and
accent-sensitive.

![GIF](https://i.imgur.com/kEvGqEG.gif)

Current Limitations
-------------------

- LASP always tries to use the system default device for recording. There is no
  way to use a device that is not assigned as default.
- LASP only supports monophonic input. Only the first channel (the left channel
  in case of stereo input) will be enabled when using a multi-channel audio
  device.
