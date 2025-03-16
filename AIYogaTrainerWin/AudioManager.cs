using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AIYogaTrainerWin
{
    public class AudioManager : IDisposable
    {
        private WaveOutEvent outputDevice;
        private AudioFileReader audioFile;
        private bool isPlaying = false;

        public AudioManager()
        {
            outputDevice = new WaveOutEvent();
            outputDevice.PlaybackStopped += (sender, args) => 
            {
                isPlaying = false;
                audioFile?.Dispose();
                audioFile = null;
            };
        }

        /// <summary>
        /// Plays an audio file asynchronously
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task PlayAudioAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Audio file not found", filePath);
            }

            await Task.Run(() =>
            {
                try
                {
                    // Stop any currently playing audio
                    StopAudio();

                    // Initialize and play the new audio
                    audioFile = new AudioFileReader(filePath);
                    outputDevice.Init(audioFile);
                    outputDevice.Play();
                    isPlaying = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Plays audio file synchronously
        /// </summary>
        /// <param name="filePath">Path to the audio file</param>
        public void PlayAudio(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Audio file not found", filePath);
            }

            try
            {
                // Stop any currently playing audio
                StopAudio();

                // Initialize and play the new audio
                audioFile = new AudioFileReader(filePath);
                outputDevice.Init(audioFile);
                outputDevice.Play();
                isPlaying = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stops the currently playing audio
        /// </summary>
        public void StopAudio()
        {
            if (isPlaying)
            {
                outputDevice.Stop();
                isPlaying = false;
                audioFile?.Dispose();
                audioFile = null;
            }
        }

        /// <summary>
        /// Disposes resources used by the audio manager
        /// </summary>
        public void Dispose()
        {
            StopAudio();
            outputDevice?.Dispose();
            outputDevice = null;
        }

        /// <summary>
        /// Checks if the specified audio file exists
        /// </summary>
        /// <param name="filePath">Path to check</param>
        /// <returns>True if file exists, otherwise false</returns>
        public static bool AudioFileExists(string filePath)
        {
            return !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
        }
    }
}
