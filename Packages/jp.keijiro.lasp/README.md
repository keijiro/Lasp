LASP
====

**LASP** is a Unity plugin providing low-latency audio input features that are
useful to create audio-reactive visuals.

Demos
-----

![gif](https://i.imgur.com/L98u4AI.gif)

**Sphere** is the simplest example of LASP. The sphere is scaled by input audio
using the Audio Level Tracker component.

![gif](https://i.imgur.com/4OVS00N.gif)

**LevelMeter** shows how to create audio-reactive behaviors using the
Audio Level Tracker component. It also uses the raw waveform API to draw the
waveform graph.

![screenshot](https://i.imgur.com/D51PENw.png)

**DeviceSelector** shows how to instantiate Audio Level Tracker and property
binders dynamically.

![gif](https://i.imgur.com/gVwN4qE.gif)

**Lissajous** is an example of use of the Input Stream class. It draws a
Lissajous curve using the interleaved waveform API.

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

**Audio Level Tracker** is a component for receiving audio input and
controlling other objects by normalized audio level.

![gif](https://i.imgur.com/ddCZSs5.gif)

It tracks the most recent peak level and calculates the normalized level based
on the difference between the current level and the peak level. It only outputs
an effective value when the current level is in its dynamic range, which is
indicated by the gray band in the level meter.

### Filter Type

Four types of filters are available: **Bypass**, **Low Pass**, **Band Pass**,
and **High Pass**. These filters are useful to detect a specific type of
rhythmic accents. For instance, you can use the low pass filter to create a
behavior that reacts to kick drums or basslines.

### Dynamic Range (dB)

The **Dynamic Range** specifies the range of audio level normalization. The
output value becomes zero when the input level is equal to or lower than
*Peak - Dynamic Range*.

### Auto Gain

When enabled, it automatically tracks the peak level, as explained above. When
disabled, it fixes the peak level at 0 dB. In this case, the effective range
can be manually controlled by the Gain property, which is only visible when
auto gain is disabled.

### Smooth Fall

When enabled, the output value gradually falls to the current actual audio
level. It's useful to make choppy animation smoother.

![gif](https://i.imgur.com/MEojdmD.gif)

Known Issue
-----------

When adding a property binder to an Audio Level Tracker component, the
following error message may be shown in Console.

> Generating diff  of this object for undo because the type tree changed.

This error is caused by [Issue 1198546]. Please wait for a fix to arrive.

[Issue 1198546]: https://issuetracker.unity3d.com/issues/serializedproperty-undo-does-not-work-properly-when-the-parent-serializedobject-is-a-script-with-managed-references