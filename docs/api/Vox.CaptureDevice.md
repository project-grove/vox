# CaptureDevice

**Class**

**Namespace:** [Vox](Vox.md)

**Base types:**

* [IDisposable](#.md)


**Declared in:** [Vox](Vox.md)

------



Represents a sound input device.


## Members

### Property
* [Format](Vox.CaptureDevice.Format.md)
* [Frequency](Vox.CaptureDevice.Frequency.md)
* [BufferSize](Vox.CaptureDevice.BufferSize.md)
* [IsCapturing](Vox.CaptureDevice.IsCapturing.md)
* [AvailableSamples](Vox.CaptureDevice.AvailableSamples.md)
* [BytesPerSample](Vox.CaptureDevice.BytesPerSample.md)
* [IsDisposed](Vox.SoundSource.IsDisposed.md)

### Constructor
* [CaptureDevice()](Vox.CaptureDevice.CaptureDevice().md)
* [CaptureDevice(string, int, PCM, int, ArrayPool<byte>)](Vox.CaptureDevice.CaptureDevice(string,int,PCM,int,ArrayPool{byte}).md)

### Method
* [Close()](Vox.OutputDevice.Close().md)
* [Dispose()](Vox.SoundSource.Dispose().md)
* [Equals(object)](Vox.SoundSource.Equals(object).md)
* [GetCaptureDevices()](Vox.CaptureDevice.GetCaptureDevices().md)
* [GetDefaultCaptureDevice()](Vox.CaptureDevice.GetDefaultCaptureDevice().md)
* [GetHashCode()](Vox.SoundSource.GetHashCode().md)
* [ProcessSamples(Action<byte[], int>)](Vox.CaptureDevice.ProcessSamples(Action{byte[],int}).md)
* [ProcessSamples(int, Action<byte[], int>)](Vox.CaptureDevice.ProcessSamples(int,Action{byte[],int}).md)
* [StartCapture()](Vox.CaptureDevice.StartCapture().md)
* [StopCapture()](Vox.CaptureDevice.StopCapture().md)
* [ToggleCapture()](Vox.CaptureDevice.ToggleCapture().md)

------

[Back to index](index.md)