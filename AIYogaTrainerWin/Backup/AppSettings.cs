using System;
using System.IO;
using System.Text.Json;

namespace AIYogaTrainerWin
{
    public static class AppSettings
    {
        private static string ConfigFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AIYogaTrainerWin",
            "settings.json");

        // Application settings data structure
        public static AppSettingsData Settings { get; private set; } = new AppSettingsData();

        // Initialize settings with default values
        public static void InitializeDefaultSettings()
        {
            Settings = new AppSettingsData
            {
                ModelUrl = "",
                Pose1ImagePath = "",
                Pose2ImagePath = "",
                Pose3ImagePath = "",
                CameraIndex = 0,
                ConfidenceThreshold = 0.5f
            };
        }

        // Load settings from file
        public static void LoadSettings()
        {
            if (File.Exists(ConfigFilePath))
            {
                string jsonContent = File.ReadAllText(ConfigFilePath);
                Settings = JsonSerializer.Deserialize<AppSettingsData>(jsonContent) 
                    ?? new AppSettingsData();
            }
            else
            {
                InitializeDefaultSettings();
                SaveSettings(); // Create default settings file
            }
        }

        // Save settings to file
        public static void SaveSettings()
        {
            string directoryPath = Path.GetDirectoryName(ConfigFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string jsonContent = JsonSerializer.Serialize(Settings, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(ConfigFilePath, jsonContent);
        }
    }

    // Settings data container class
    public class AppSettingsData
    {
        public string ModelUrl { get; set; } = "";
        public string Pose1ImagePath { get; set; } = "";
        public string Pose2ImagePath { get; set; } = "";
        public string Pose3ImagePath { get; set; } = "";
        public int CameraIndex { get; set; } = 0;
        public float ConfidenceThreshold { get; set; } = 0.5f;
        public string Pose1AudioPath { get; set; } = "";
        public string Pose2AudioPath { get; set; } = "";
        public string Pose3AudioPath { get; set; } = "";
    }
}
