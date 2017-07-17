LASP
====

**LASP** is a Unity plugin that provides low-latency audio input functionality.

[Demo 1](http://radiumsoftware.tumblr.com/post/163009893309),
[Demo 2](http://radiumsoftware.tumblr.com/post/163095570430)

System Requirements
-------------------

- Unity 2017.1

At the moment, LASP only supports Windows (64 bit) and macOS (64 bit).

Installation
------------

Download one of the unitypackage files from the [Releases] page and import it
to a project.

[Releases]: https://github.com/keijiro/Lasp/releases

How To Use
----------

All of the public methods of LASP are implemented in `Lasp.AudioInput`.

#### `Lasp.AudioInput.GetPeakLevel`/`GetPeakLevelDecibel`

`GetPeakLevel` returns the peak level of the audio signal during the last
frame. `GetPeakLevelDecibel` returns the level in dBFS.

`AudioInput` automatically caches the results, so these functions can be called
multiple times without extra cost.

#### `Lasp.AudioInput.CalculateRMS`/`CalculateRMSDecibel`

`CalculateRMS` calculates and returns the RMS (root mean square) of the audio
signal during the last frame. `CalculateRMSDecibel` returns the level in dBFS.

`AudioInput` automatically caches the results, so these functions can be called
multiple times without extra cost.

#### `Lasp.AudioInput.RetrieveWaveform`

`RetrieveWaveform` copies the recent history of the audio waveform from the
internal buffer to a given float array.

License
-------

[MIT](LICENSE.md)
