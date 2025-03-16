using System;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Class to store application settings
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Path to the TensorFlow/ONNX model file
        /// </summary>
        public string ModelPath { get; set; }

        /// <summary>
        /// Name for Pose 1
        /// </summary>
        public string Pose1Name { get; set; }

        /// <summary>
        /// Name for Pose 2
        /// </summary>
        public string Pose2Name { get; set; }

        /// <summary>
        /// Name for Pose 3
        /// </summary>
        public string Pose3Name { get; set; }

        /// <summary>
        /// Path to audio file for Pose 1
        /// </summary>
        public string Pose1AudioPath { get; set; }

        /// <summary>
        /// Path to audio file for Pose 2
        /// </summary>
        public string Pose2AudioPath { get; set; }

        /// <summary>
        /// Path to audio file for Pose 3
        /// </summary>
        public string Pose3AudioPath { get; set; }
    }
}
