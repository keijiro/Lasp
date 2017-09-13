LASP Loopback
=============

**LASP Loopback** is an experimental build of **LASP** that allows Unity apps
analyzing its audio output via a custom mixer effect.

For details of the LASP plugin, please check [the original branch].

[the original branch]: https://github.com/keijiro/Lasp

System Requirements
-------------------

- Unity 2017.1 or later

At the moment, LASP only supports desktop platforms (Windows, macOS and Linux).

Installation
------------

Download [the unitypackage file] and import it to the project.

Before installing the package, the original version of LASP has to be removed
from the project to avoid conflicts. Remove `Assets/Lasp` diretory if it
exists.

[the unitypackage file]: LaspLoopback.unitypackage

How To Use
----------

LASP Loopback uses a custom audio effect to route audio signals into the
plugin. This audio effect has to be added to one of the audio tracks in the
audio mixer used in the scene. Typically, it's added to the master track.

![screenshot](https://i.imgur.com/7U11DwK.png)

The basic functionalities of LASP Lookback are almost the same to the original
LASP plugin. Please refer to the documentation in the original branch for
further usage.

TIPS
----

LASP Loopback can be used with [KlakLASP]. It's convenient to create audio
reactive behaviors.

[KlakLASP]: https://github.com/keijiro/KlakLasp

Current Limitations
-------------------

- LASP loopback can't be used with the original LASP plugin simultaneously.
- In most cases, the latency of LASP Loopback is less than the latency of
  Unity's audio output. The DSP buffer size in the audio settings should be
  tweaked to reduce the latency between them.

License
-------

[MIT](LICENSE.txt)
