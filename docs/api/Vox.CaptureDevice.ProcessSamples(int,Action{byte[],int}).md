# ProcessSamples(int, Action<byte[], int>)

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.CaptureDevice](Vox.CaptureDevice.md)

------



Reads the specified amount of samples from the device and calls
the specified callback.


## Syntax

```csharp
public void ProcessSamples(
	int sampleCount,
	Action<byte[], int> callback
)
```

### Parameters

`sampleCount`

Sample count. Must be less or equal to [AvailableSamples](Vox.CaptureDevice.AvailableSamples.md).

`callback`

Callback

## Remarks
If you intend to use sample data later, , because the buffer is pooled and will be reused.
------

[Back to index](index.md)