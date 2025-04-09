namespace PixelPunk.Core
{
    /// <summary>
    /// Contains all PlayerPrefs key constants used throughout the application
    /// </summary>
    public static class PlayerPrefsKeys
    {
        public static class Auth
        {
            public const string AccessToken = "auth_token";
            public const string RefreshToken = "refresh_token";
        }

        public static class Settings
        {
            public const string MusicVolume = "settings_music_volume";
            public const string SfxVolume = "settings_sfx_volume";
            public const string Language = "settings_language";
        }

        // Add more categories
    }
}
