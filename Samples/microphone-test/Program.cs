using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Vox;
using static System.Console;

namespace microphone_test
{
internal class Program
{
	private static void Main(string[] args)
	{
		if (args.Contains("--trace"))
		{
			VoxEvents.EnableTracing = true;
			VoxEvents.OpenALTraceSource.Switch.Level = SourceLevels.All;
			VoxEvents.OpenALTraceSource.Listeners.Add(new ConsoleTraceListener());
		}

		var inputDevice = new CaptureDevice();
		var outputDevice = new OutputDevice();
		WriteLine("Welcome to the microphone test sample.");
		WriteLine("Press ENTER to begin recording your microphone input.");
		ReadLine();

		// Start the capture
		inputDevice.StartCapture();
		WriteLine("Press any key to stop recording.");

		// since the device has an internal buffer, we can't get our
		// captured data at once, so we should process it step by step
		var samples = new byte[0];
		while (!KeyAvailable)
		{
			inputDevice.ProcessSamples((data, bytesRead) =>
			{
				var oldSize = samples.Length;
				Array.Resize(ref samples, oldSize + bytesRead);
				Array.Copy(data, 0, samples, oldSize, bytesRead);
			});
			Thread.Sleep(100);
		}

		// User requested us to stop the capture, let's stop it
		inputDevice.StopCapture();

		WriteLine("Stopped capturing, starting playback");
		// Create a new sound buffer and put our recorded data in it
		var buffer = new SoundBuffer(outputDevice);
		buffer.SetData(PCM.Mono8, samples);

		// Create a new sound source, enqueue our buffer on it and play
		var source = new SoundSource();
		source.IsLooping = true;
		source.Enqueue(buffer);
		source.Play();
		// Since the audio plays in another thread, we must wait a little bit,
		// otherwise the program will exit without waiting for playback to
		// complete
		Thread.Sleep(samples.Length * 1000 / 44100);

		// Don't forget to clean up
		inputDevice.Close();
		outputDevice.Close();
	}
}
}