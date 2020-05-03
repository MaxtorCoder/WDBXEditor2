using System;
using System.IO;

namespace WDBXEditor2.Misc
{
    class SettingStorage
    {
        protected static JsonSettings settings = null;

        public static void Initialize()
        {
            settings = new JsonSettings(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                AppDomain.CurrentDomain.FriendlyName + ".json"
            ));
        }

        public static void Store(string key, string value)
        {
            settings[key] = value;
            settings.Save();
        }

        public static string Get(string key)
        {
            return settings[key];
        }

        public static void Remove(string key)
        {
            settings.RemoveSetting(key);
            settings.Save();
        }

        public static void Save()
        {
            settings.Save();
        }
    }
}
