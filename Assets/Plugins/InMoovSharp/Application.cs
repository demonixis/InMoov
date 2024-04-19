namespace Demonixis.InMoovSharp
{
#if !INMOOV_UNITY
    public enum RuntimePlatform
    {
        Android,
        LinuxPlayer,
        LinuxEditor,
        WindowsPlayer,
        WindowsEditor,
        OSXPlayer,
        OSXEditor,
        IPhonePlayer,
        tvOS
    }

    public static class Application
    {
        public static RuntimePlatform platform
        {
            get
            {
                if (OperatingSystem.IsAndroid())
                    return RuntimePlatform.Android;
                else if (OperatingSystem.IsLinux())
                    return RuntimePlatform.LinuxPlayer;
                else if (OperatingSystem.IsMacOS())
                    return RuntimePlatform.OSXPlayer;
                else if (OperatingSystem.IsIOS())
                    return RuntimePlatform.IPhonePlayer;
                else if (OperatingSystem.IsTvOS())
                    return RuntimePlatform.tvOS;

                return RuntimePlatform.WindowsPlayer;
            }
        }

        public static bool isMobile => platform is
            RuntimePlatform.Android or
            RuntimePlatform.IPhonePlayer or
            RuntimePlatform.tvOS;

        public static string companyName = "Demonixis Games";
        public static string appVersion = "1.0.0";
        public static string productName = "InMoovUnity";
        public static string identifier = "net.demonixis.inmoovunity";
        public static string persistentDataPath => Path.Combine(Directory.GetCurrentDirectory());
        public static string streamingAssetsPath => Path.Combine(Directory.GetCurrentDirectory(), "StreamingAssets");
    }
#endif
}