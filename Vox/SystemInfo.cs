using static OpenAL.AL10;
using static OpenAL.ALC10;
using static Vox.ErrorHandler;

namespace Vox
{
/// <summary>
/// Contains various informational properties about audio subsystem
/// </summary>
public static class SystemInfo
{
	/// <summary>
	/// Gets the vendor string for the currently used sound device
	/// </summary>
	/// <exception cref="AudioLibraryException">Thrown if no current audio device was selected</exception>
	public static string OpenALVendor =>
		AL(() => alGetString(AL_VENDOR), "alGetString(AL_VENDOR)");

	/// <summary>
	/// Gets the renderer string for the currently used sound device
	/// </summary>
	/// <exception cref="AudioLibraryException">Thrown if no current audio device was selected</exception>
	public static string OpenALRenderer =>
		AL(() => alGetString(AL_RENDERER), "alGetString(AL_RENDERER)");

	/// <summary>
	/// Gets the version string for the currently used sound device
	/// </summary>
	/// <exception cref="AudioLibraryException">Thrown if no current audio device was selected</exception>
	public static string OpenALVersion =>
		AL(() => alGetString(AL_VERSION), "alGetString(AL_VERSION)");

	/// <summary>
	/// Gets the extensions string for the currently used sound device
	/// </summary>
	/// <exception cref="AudioLibraryException">Thrown if no current audio device was selected</exception>
	public static string OpenALExtensions =>
		AL(() => alGetString(AL_EXTENSIONS), "alGetString(AL_EXTENSIONS)");

	private static readonly int[] s_major = new int[1];
	private static readonly int[] s_minor = new int[1];

	/// <summary>
	/// Gets the ALC version for the currently used sound device
	/// </summary>
	/// <exception cref="AudioLibraryException">Thrown if no current audio device was selected</exception>
	public static string ALCVersion
	{
		get
		{
			unsafe
			{
				var device = OutputDevice.Current._handle;
				ALC((p) => alcGetIntegerv(p.device, ALC_MAJOR_VERSION, 1, p.s_major),
				    "alcGetIntegerv(ALC_MAJOR_VERSION)", device, (device, s_major));
				ALC((p) => alcGetIntegerv(p.device, ALC_MINOR_VERSION, 1, p.s_minor),
				    "alcGetIntegerv(ALC_MINOR_VERSION)", device, (device, s_minor));
				return string.Format("{0}.{1}", s_major[0].ToString(), s_minor[0].ToString());
			}
		}
	}
}
}