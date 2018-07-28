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
                    var major = new int[1];
                    var minor = new int[1];
                    var device = OutputDevice.Current._handle;
                    ALC(() => 
                        alcGetIntegerv(device, ALC_MAJOR_VERSION, 1, major),
                        "alcGetIntegerv(ALC_MAJOR_VERSION)", device);
                    ALC(() => 
                        alcGetIntegerv(device, ALC_MINOR_VERSION, 1, major),
                        "alcGetIntegerv(ALC_MINOR_VERSION)", device);
                    return $"{major[0]}.{minor[0]}";
                }
            }
        }
    }
}