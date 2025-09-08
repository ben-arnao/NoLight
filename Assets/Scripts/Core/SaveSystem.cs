using UnityEngine;

namespace RogueLike2D.Core
{
    // Placeholder save system using PlayerPrefs. Replace with persistent solution later.
    public static class SaveSystem
    {
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
