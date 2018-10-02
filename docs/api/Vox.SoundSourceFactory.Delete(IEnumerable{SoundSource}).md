# Delete(IEnumerable<SoundSource>)

**Method**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.SoundSourceFactory](Vox.SoundSourceFactory.md)

------



Deletes the specified sound sources. If they belong to the same output
device, deletes them efficiently with one native call, otherwise
falling back to Dispose.


## Syntax

```csharp
public static void Delete(
	IEnumerable<SoundSource> sources
)
```

------

[Back to index](index.md)