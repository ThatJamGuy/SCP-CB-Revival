#if UNITY_EDITOR && SORTIFY
using System.Collections.Generic;

namespace Sortify
{
    public static class SortifyUserDataManager
    {
        private const string USERDATA_FILE_NAME = "Sortify_User.json";
        private static string _cachedUsername;
        private static Dictionary<string, bool> _cachedUserSettings;

        private static bool _dataLoaded = false;

        public static void SaveUsername(string username)
        {
            _cachedUsername = username;
            SaveUserDataToFile();
        }

        public static string GetUsername()
        {
            LoadUserDataIfNeeded();
            return _cachedUsername;
        }

        public static void SaveUserSetting(string settingName, bool value)
        {
            LoadUserDataIfNeeded();
            _cachedUserSettings[settingName] = value;
            SaveUserDataToFile();
        }

        public static bool GetUserSetting(string settingName, bool defaultValue = false)
        {
            LoadUserDataIfNeeded();
            if (_cachedUserSettings.TryGetValue(settingName, out bool value))
                return value;

            return defaultValue;
        }

        private static void SaveUserDataToFile()
        {
            List<UserSetting> userSettingsList = new List<UserSetting>();
            foreach (var kvp in _cachedUserSettings)
            {
                userSettingsList.Add(new UserSetting(kvp.Key, kvp.Value));
            }

            UserDataSerializer dataSerializer = new UserDataSerializer(_cachedUsername, userSettingsList);
            SortifyFileManager.SaveToFile(USERDATA_FILE_NAME, dataSerializer);
        }

        public static void LoadUserDataIfNeeded(bool forceLoad = false)
        {
            if (forceLoad)
                _dataLoaded = false;

            if (!_dataLoaded)
            {
                LoadUserDataFromFile();
                _dataLoaded = true;
            }
        }

        private static void LoadUserDataFromFile()
        {
            var loadedData = SortifyFileManager.LoadFromFile<UserDataSerializer>(USERDATA_FILE_NAME);
            if (loadedData != null)
            {
                _cachedUsername = loadedData.username;

                _cachedUserSettings = new Dictionary<string, bool>();
                if (loadedData.userSettings != null)
                {
                    foreach (var setting in loadedData.userSettings)
                    {
                        _cachedUserSettings[setting.settingName] = setting.value;
                    }
                }
            }
            else
            {
                _cachedUsername = string.Empty;
                _cachedUserSettings = new Dictionary<string, bool>();
            }
        }
    }

    [System.Serializable]
    public class UserSetting
    {
        public string settingName;
        public bool value;

        public UserSetting() { }

        public UserSetting(string settingName, bool value)
        {
            this.settingName = settingName;
            this.value = value;
        }
    }

    [System.Serializable]
    public class UserDataSerializer
    {
        public string username;
        public List<UserSetting> userSettings;

        public UserDataSerializer() { }

        public UserDataSerializer(string username, List<UserSetting> userSettings)
        {
            this.username = username;
            this.userSettings = userSettings;
        }
    }
}
#endif
