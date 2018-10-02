# SetData(PCM, byte[], int)

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.SoundBuffer](Vox.SoundBuffer.md)

------



Sets the audio buffer PCM data.


## Syntax

```csharp
public void SetData(
	PCM format,
	byte[] data,
	int frequency
)
```

### Parameters

`format`

Audio format of the input data

`data`

Byte array with sound data

`frequency`

Sound frequency

## Remarks

8-bit PCM data is expressed as an unsigned value over the range 0 to 255, 128 being an
audio output level of zero. 16-bit PCM data is expressed as a signed value over the
range -32768 to 32767, 0 being an audio output level of zero. Stereo data is expressed
in interleaved format, left channel first. Buffers containing more than one channel of data
will be played without 3D spatialization.

------

[Back to index](index.md)