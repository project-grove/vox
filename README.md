![Stability: alpha](https://img.shields.io/badge/stability-alpha-orange.svg)

# About
This project is a cross-platform .NET Core/Standard 3D audio engine based on OpenAL.

----
### Features
----
* Based on the OpenAL 1.1 specification
* Multiple output and sound capture devices
* Full 3D sound support with different distance models, pitch shifting and other stuff
* Bundled with PCM WAV, OGG and MP3 importers (thanks to [NLayer](https://github.com/project-grove/NLayer) and [NVorbis](https://github.com/project-grove/NVorbis))

----
### Examples
----

For runnable examples check out the [Samples](https://github.com/project-grove/vox/tree/master/Samples) directory.

###### Simple audio playback
```csharp
var outputDevice = new OutputDevice();
...
var buffer = Wav.Load("never_gonna_give_you_up.wav");
...
var source = new SoundSource();
source.Enqueue(buffer);
source.Play();
```

###### Microphone capture
```csharp
var inputDevice = new CaptureDevice();
...
inputDevice.StartCapture();
...
var samples = new byte[0];
// If you're running it in a game loop, you don't need a while loop here
// Just allocate a big enough buffer and reuse it
while (something)
{
    inputDevice.ProcessSamples((data, bytesRead) =>
    {
        var oldSize = samples.Length;
        Array.Resize(ref samples, oldSize + bytesRead);
        Array.Copy(data, 0, samples, oldSize, bytesRead);
    });
    Thread.Sleep(100);
}
...
inputDevice.StopCapture();
...
// Do something with the samples array
// Put the data from it in a SoundBuffer, for example
```


###### 3D audio playback
Check out the [SoundSource](https://project-grove.github.io/vox/api/Vox.SoundSource.html) and [Listener](https://project-grove.github.io/vox/api/Vox.SoundListener.html) API documentation to take a look at what you can do.

----
### Platform support
----
See the [OpenAL.NETCore](https://github.com/project-grove/OpenAL.NETCore) project.