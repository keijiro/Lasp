LASP
====

**LASP** is a Unity plugin providing low-latency audio input features that are
useful to create audio-reactive visuals.

Demos
-----

![gif](https://i.imgur.com/L98u4AI.gif)

**Sphere** is the simplest example of LASP that shows an animated sphere scaled
by the audio level. It uses the **Audio Level Tracker** component to calculate
the audio level and a **Property Binder** to animate the scale property of the
sphere.

![gif](https://i.imgur.com/4OVS00N.gif)

**LevelMeter** is a slightly advanced example of the use of Audio Level Tracker
that shows low/mid/high frequency band levels. It also uses the raw waveform
function to draw the waveform graph.

![screenshot](https://i.imgur.com/D51PENw.png)

**DeviceSelector** shows how to instantiate Audio Level Tracker and set Property
Binders programmatically at run time.

![gif](https://i.imgur.com/gVwN4qE.gif)

**Lissajous** is an example that draws a Lissajous curve using the **Input
Stream** class and its interleaved waveform function.

System Requirements
-------------------

- Unity 2019.3 or later

At the moment, LASP only supports desktop platforms (Windows, macOS, and Linux).

How To Install
--------------

This package uses the [scoped registry] feature to resolve package dependencies.
Please add the following sections to the manifest file (Packages/manifest.json).

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

To the `scopedRegistries` section:

```
{
  "name": "Unity NuGet",
  "url": "https://unitynuget-registry.azurewebsites.net",
  "scopes": [ "org.nuget" ]
},
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.lasp": "2.0.0"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Unity NuGet",
      "url": "https://unitynuget-registry.azurewebsites.net",
      "scopes": [ "org.nuget" ]
    },
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.lasp": "2.0.0",
...
```

Audio Level Tracker Component
-----------------------------

**Audio Level Tracker** is a component that receives an audio stream and
calculates the current audio level. It supports **Property Binders** that
modifies properties of external objects based on the normalized audio level.

![gif](https://i.imgur.com/wBsYq64.gif)

It tracks the most recent peak level and calculates the normalized level based
on the difference between the current level and the peak level. It only outputs
an effective value when the current level is in its dynamic range that is
indicated by the gray band in the level meter.

### Default Device / Device ID

As an audio source, you can use the **Default Device** or one of the available
audio devices by specifying its Device ID.

Device IDs are system-generated random string like `{4786-742f-9e42}`. On
Editor, you can use the **Select** button to find a device and get its ID. Note
that those ID strings are system-dependent. You have to reconfigure it when
running the project on a different platform.

For runtime use, you can use `AudioSystem.InputDevices` to enumerate the
available devices and get those IDs. Please check the DeviceSelector example for
further details.

### Channel

Select a channel to use as an input, or stay 0 for monaural devices.

### Filter Type

Four types of filters are available: **Bypass**, **Low Pass**, **Band Pass**,
and **High Pass**. These filters are useful to detect a specific type of
rhythmic accents. For instance, you can use the low pass filter to create a
behavior reacting to kick drums or basslines.

### Dynamic Range (dB)

The **Dynamic Range** specifies the range of audio level normalization. The
output value becomes zero when the input level is equal to or lower than
*Peak Level - Dynamic Range*.

### Auto Gain

When enabled, it automatically tracks the peak level, as explained above. When
disabled, it fixes the peak level at 0 dB. In this case, you can manually
control the effective range via the **Gain** property.

### Smooth Fall

When enabled, the output value gradually falls to the current actual audio
level. It's useful to make choppy animation smoother.

![gif](https://i.imgur.com/VKiZx4M.gif)

Scripting Interface
-------------------

There are several public methods/properties in LASP classes, such as
`AudioLevelTracker`, `AudioSystem`, `InputStream`. Please check the example
scripts for detailed usages.

Known Issue
-----------

When adding a property binder to an Audio Level Tracker component, the
following error message may be shown in Console.

> Generating diff  of this object for undo because the type tree changed.

This error is caused by [Issue 1198546]. Please wait for a fix to arrive.

[Issue 1198546]: https://issuetracker.unity3d.com/issues/serializedproperty-undo-does-not-work-properly-when-the-parent-serializedobject-is-a-script-with-managed-references
