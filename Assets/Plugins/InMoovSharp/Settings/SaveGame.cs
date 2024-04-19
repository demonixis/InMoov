using Newtonsoft.Json;
using System;
using System.IO;

#if INMOOV_UNITY
using UnityEngine;
#endif

namespace Demonixis.InMoovSharp.Settings
{
    [Serializable]
    public static class SaveGame
    {
        public static bool UseLocalFolder { get; set; } = true;

        public static string GetAndroidPath(string? additionalPath = null)
        {
            var path = $"/storage/emulated/0/Android/data/{Application.identifier}/files";

            if (!string.IsNullOrEmpty(additionalPath))
            {
                path = $"{path}/{additionalPath}";
            }

            return path;
        }

        public static string GetStandalonePath(string? additionalPath = null)
        {
            string root = string.Empty;
            string path = string.Empty;

            if (UseLocalFolder)
            {
                root = Directory.GetCurrentDirectory();
                path = Path.Combine(root, "Data");
            }
            else
            {
                root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                path = Path.Combine(root, "My Games", Application.productName);
            }

            if (!string.IsNullOrEmpty(additionalPath))
                path = Path.Combine(path, additionalPath);

            return path;
        }

        public static string GetSavePath(string? additionalPath = null)
        {
            var path = GetStandalonePath(additionalPath);
            if (Application.platform == RuntimePlatform.Android)
                path = GetAndroidPath(additionalPath);

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception ex)
            {
                Robot.Log(ex.Message);
            }

            return path;
        }

        public static string SaveData(object data, string filename, string? additionalPath = null)
        {
            var json = data is string ? (string)data : JsonConvert.SerializeObject(data, Formatting.Indented);

            var savePath = Path.Combine(GetSavePath(additionalPath), filename);
            File.WriteAllText(savePath, json);

            return json;
        }

        public static T LoadData<T>(string filename, string? additionalPath = null)
        {
            var json = string.Empty;

            var filepath = Path.Combine(GetSavePath(additionalPath), filename);

            if (File.Exists(filepath))
                json = File.ReadAllText(filepath);

            if (!string.IsNullOrEmpty(json) && json != "{}")
            {
                var result = JsonConvert.DeserializeObject<T>(json);
                return result;
            }

            return default(T);
        }

        public static void ClearData(string filename)
        {
            var filepath = Path.Combine(GetSavePath(), filename);

            if (File.Exists(filepath))
                File.Delete(filepath);
        }
    }
}