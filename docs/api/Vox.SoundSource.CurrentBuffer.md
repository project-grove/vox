# CurrentBuffer

**Property**

**Namespace:** [Vox](Vox.md)

**Declared in:** [Vox.SoundSource](Vox.SoundSource.md)

------



Gets or sets the first buffer on the queue.


## Syntax

```csharp
public SoundBuffer CurrentBuffer { public get; public set; }
```

## Remarks
If set to null, releases the current buffer queue (only in Initial and Stopped states).
------

[Back to index](index.md)