# CaptureDevice(string, int, PCM, int, ArrayPool<byte>)

**Constructor**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.CaptureDevice](Vox.CaptureDevice.md)

------



Creates and opens a sound input device with the specified parameters.


## Syntax

```csharp
public CaptureDevice(
	string name,
	int frequency,
	PCM format,
	int bufSize,
	ArrayPool<byte> bufferPool
)
```

### Parameters

`name`

Name of the device to open

`frequency`

Sample frequency

`format`

Sound format

`bufSize`

Buffer size

`bufferPool`

Array pool for buffers.false. If null, defaults to shared.

## See also
* [GetCaptureDevices()](Vox.CaptureDevice.GetCaptureDevices().md)
* [GetDefaultCaptureDevice()](Vox.CaptureDevice.GetDefaultCaptureDevice().md)

------

[Back to index](index.md)