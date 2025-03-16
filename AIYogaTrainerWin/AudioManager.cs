using System;
using System.IO;
using NAudio.Wave;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Manages audio playback for pose transitions
    /// </summary>
    public class AudioManager
    {
        /// <summary>
        /// Plays an audio file
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        public void PlayAudio(string filePath)
        {
            try
            {
                // Check if file exists
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Audio file not found: {filePath}");
                    return;
                }

                // Play the audio file using NAudio
                Task.Run(() =>
                {
                    try
                    {
                        using (var audioFile = new AudioFileReader(filePath))
                        using (var outputDevice = new WaveOutEvent())
                        {
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            
                            // Wait until playback is finished
                            while (outputDevice.PlaybackState == PlaybackState.Playing)
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error playing audio: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing audio playback: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays the default audio for a pose
        /// </summary>
        /// <param name="poseNumber">The pose number (1-3)</param>
        public void PlayPoseAudio(int poseNumber)
        {
            // Load audio file path from settings or use default
            string audioFile = $"pose{poseNumber}.wav";
            
            // Try to get audio path from settings
            try
            {
                string settingsJson = File.ReadAllText("settings.json");
                var settings = System.Text.Json.JsonSerializer.Deserialize<Settings>(settingsJson);
                
                switch (poseNumber)
                {
                    case 1:
                        if (!string.IsNullOrEmpty(settings.Pose1AudioPath))
                            audioFile = settings.Pose1AudioPath;
                        break;
                    case 2:
                        if (!string.IsNullOrEmpty(settings.Pose2AudioPath))
                            audioFile = settings.Pose2AudioPath;
                        break;
                    case 3:
                        if (!string.IsNullOrEmpty(settings.Pose3AudioPath))
                            audioFile = settings.Pose3AudioPath;
                        break;
                }
            }
            catch
            {
                // Use default audio file if settings can't be loaded
            }
            
            PlayAudio(audioFile);
        }
    }
}
