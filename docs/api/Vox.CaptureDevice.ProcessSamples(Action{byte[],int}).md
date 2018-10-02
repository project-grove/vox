# ProcessSamples(Action<byte[], int>)

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.CaptureDevice](Vox.CaptureDevice.md)

------



Reads all available samples and passes them to the callback.


## Syntax

```csharp
public void ProcessSamples(
	Action<byte[], int> callback
)
```

## Remarks
If you intend to use sample data later, , because the buffer is pooled and will be reused.
------

[Back to index](index.md)