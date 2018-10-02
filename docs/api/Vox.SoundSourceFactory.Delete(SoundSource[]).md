# Delete(SoundSource[])

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.SoundSourceFactory](Vox.SoundSourceFactory.md)

------



Deletes the specified sound sources. If they belong to the same output
device, deletes them efficiently with one native call, otherwise
falling back to Dispose. Already disposed sources are skipped.


## Syntax

```csharp
public static void Delete(
	SoundSource[] sources
)
```

------

[Back to index](index.md)