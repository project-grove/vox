# Delete(SoundBuffer[])

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.SoundBufferFactory](Vox.SoundBufferFactory.md)

------



Deletes the specified sound buffers. If they belong to the same output
device, deletes them efficiently with one native call, otherwise
falling back to Dispose. Already disposed buffers are skipped.


## Syntax

```csharp
public static void Delete(
	SoundBuffer[] buffers
)
```

------

[Back to index](index.md)