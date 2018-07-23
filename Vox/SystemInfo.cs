using static OpenAL.AL10;
using static Vox.ErrorHandler;

namespace Vox
{
    public static class SystemInfo
    {
        public static string OpenALVendor => 
            AL(() => alGetString(AL_VENDOR), "alGetString(AL_VENDOR)");
        public static string OpenALRenderer => 
            AL(() => alGetString(AL_RENDERER), "alGetString(AL_RENDERER)");
        public static string OpenALVersion => 
            AL(() => alGetString(AL_VERSION), "alGetString(AL_VERSION)");
        public static string OpenALExtensions => 
            AL(() => alGetString(AL_EXTENSIONS), "alGetString(AL_EXTENSIONS)");
    }
}