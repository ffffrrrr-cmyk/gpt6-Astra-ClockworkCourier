using System;
using System.IO;
using UnityEngine;

namespace ClockworkCourier
{
    public static class SaveStore
    {
        public static string PathName => Path.Combine(Application.persistentDataPath, Array.Exists(Environment.GetCommandLineArgs(), argument => argument == "-u2Smoke") ? "smoke-progress-v1.json" : "courier-progress-v1.json");
        public static string LastWarning { get; private set; }

        public static ProgressData Decode(string text)
        {
            var progress = JsonUtility.FromJson<ProgressData>(text);
            if (progress == null || !progress.Valid()) throw new InvalidDataException("Invalid progress data");
            return progress;
        }

        public static ProgressData Load()
        {
            LastWarning = null;
            if (!File.Exists(PathName)) return new ProgressData();
            try { return Decode(File.ReadAllText(PathName)); }
            catch (Exception exception)
            {
                LastWarning = "存档无法读取，已载入初始进度。原文件已保留。";
                Debug.LogWarning("Save load: " + exception.Message);
                try { File.Copy(PathName, PathName + ".invalid", true); } catch (IOException) { }
                return new ProgressData();
            }
        }

        public static bool Write(ProgressData progress)
        {
            try
            {
                if (!progress.Valid()) throw new InvalidDataException("Invalid progress state");
                Directory.CreateDirectory(Application.persistentDataPath);
                string temporary = PathName + ".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(progress, true));
                if (File.Exists(PathName)) File.Replace(temporary, PathName, PathName + ".bak");
                else File.Move(temporary, PathName);
                return true;
            }
            catch (Exception exception)
            {
                LastWarning = "存档写入失败，本局仍可继续。";
                Debug.LogWarning("Save write: " + exception.Message);
                return false;
            }
        }
    }
}
