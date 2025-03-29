# LASP

**LASP** is a Unity plugin that provides low-latency audio input features,
making it useful for creating audio-reactive visuals.

## Demos

![gif](https://i.imgur.com/L98u4AI.gif)

**Sphere** is the simplest example demonstrating LASP. It features an animated
sphere whose scale responds to the audio level. This example utilizes the
**Audio Level Tracker** component to measure the audio level and a **Property
Binder** to animate the sphere's scale property.

![gif](https://i.imgur.com/4OVS00N.gif)

**LevelMeter** is a slightly more advanced example of the **Audio Level
Tracker** in action. It visualizes the audio levels of low, mid, and high
frequency bands. Additionally, it makes use of the raw waveform function to
render a waveform graph.

![screenshot](https://i.imgur.com/D51PENw.png)

**DeviceSelector** demonstrates how to instantiate the **Audio Level Tracker**
and configure **Property Binders** programmatically at runtime.

![gif](https://i.imgur.com/gVwN4qE.gif)

**Lissajous** showcases how to draw a Lissajous curve using the **Input
Stream** class and its interleaved waveform function.

![gif](https://i.imgur.com/jT0Tj1o.gif)

**Spectrum** illustrates how to utilize the **Spectrum Analyzer** component to
extract and visualize the frequency spectrum of an audio input stream.

## System Requirements

- Unity 2022.3 LTS or later

Currently, LASP supports only desktop platforms (Windows, macOS, and Linux).

## Installation

You can install the LASP package (`jp.keijiro.lasp`) via the "Keijiro" scoped
registry using the Unity Package Manager. To add the registry to your project,
follow [these instructions].

[these instructions]:
  https://gist.github.com/keijiro/f8c7e8ff29bfe63d86b888901b82644c

## Audio Level Tracker Component

The **Audio Level Tracker** component receives an audio stream and calculates
the current audio level. It supports **Property Binders**, which modify the
properties of external objects based on the normalized audio level.

![gif](https://i.imgur.com/wBsYq64.gif)

This component tracks the most recent peak level and calculates the normalized
level based on the difference between the current level and the peak level. It
only outputs an effective value when the current level falls within its dynamic
range, indicated by the gray band in the level meter.

### Default Device / Device ID

As an audio source, you can use either the **Default Device** or a specific
audio device by specifying its Device ID.

Device IDs are system-generated random strings, such as `{4786-742f-9e42}`. In
the Editor, you can use the **Select** button to find a device and retrieve its
ID. Note that these ID strings are system-dependent, so you will need to
reconfigure them when running the project on a different platform.

At runtime, you can use `AudioSystem.InputDevices` to enumerate available
devices and obtain their IDs. Refer to the **DeviceSelector** example for more
details.

### Channel

Select a channel to use as input, or leave it at `0` for monaural devices.

### Filter Type

Four filter types are available: **Bypass**, **Low Pass**, **Band Pass**, and
**High Pass**. These filters are useful for detecting specific rhythmic accents.
For example, a **Low Pass** filter can be used to create a response to kick
drums or basslines.

### Dynamic Range (dB)

The **Dynamic Range** defines the range of audio level normalization. The output
value becomes zero when the input level is equal to or lower than  
*Peak Level - Dynamic Range*.

### Auto Gain

When enabled, this feature automatically tracks the peak level as explained
above. When disabled, the peak level is fixed at **0 dB**, allowing you to
manually control the effective range using the **Gain** property.

### Smooth Fall

When enabled, the output value gradually decreases toward the current actual
audio level, making choppy animations appear smoother.

![gif](https://i.imgur.com/VKiZx4M.gif)

## Spectrum Analyzer Component

![gif](https://i.imgur.com/sWRMSMG.gif)

The **Spectrum Analyzer** component processes an audio stream using **FFT
(Fast Fourier Transform)** to extract spectrum data.

It normalizes the spectrum values using the same algorithm as the **Audio Level
Tracker**. Refer to the section above for details.

You can access the spectrum data via the scripting API. For detailed usage,
see the **SpectrumAnalyzer** example.

## Spectrum To Texture Component

![screenshot](https://i.imgur.com/r9kxPF3.png)

The **Spectrum To Texture** component is a utility that converts spectrum data
into a texture, making it useful for creating visual effects with shaders.

There are two ways to utilize the spectrum texture:

### Via Render Texture

You can bake the spectrum data into a **Render Texture** by specifying it in the
**Render Texture** property. The render texture must meet the following
requirements:

- **Width**: Must match the spectrum resolution.
- **Height**: Must be `1`.
- **Format**: `R32_SFloat`

### Via Material Override

You can override a material's texture property using the **Material Override**
property. This method is convenient when applying the spectrum texture to a
**Mesh Renderer**.

## Scripting Interface

LASP provides several public methods and properties in its core classes, such as
`AudioLevelTracker`, `AudioSystem`, and `InputStream`. 

For detailed usage, refer to the example scripts.
