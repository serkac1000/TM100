using System;
using System.Collections.Generic;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Class to store application settings
    /// </summary>
    public class YogaAppSettings
    {
        /// <summary>
        /// URL to the Teachable Machine model API
        /// </summary>
        public string ModelUrl { get; set; } = "";
        
        /// <summary>
        /// Whether audio feedback is enabled
        /// </summary>
        public bool AudioFeedback { get; set; } = true;
        
        /// <summary>
        /// Audio feedback threshold percentage
        /// </summary>
        public int AudioThreshold { get; set; } = 70;
        
        /// <summary>
        /// Hold time in seconds (1-3)
        /// </summary>
        public int HoldTime { get; set; } = 3;
        
        /// <summary>
        /// Detection threshold percentage (default 50% - this is the minimum accuracy needed for a pose to be considered matched)
        /// </summary>
        public int DetectionThreshold { get; set; } = 50;
        
        /// <summary>
        /// Automatic pose progression when threshold is exceeded
        /// </summary>
        public bool AutoPoseProgression { get; set; } = true;
        
        /// <summary>
        /// Names for poses (up to 6)
        /// </summary>
        public string Pose1Name { get; set; } = "Pose 1";
        public string Pose2Name { get; set; } = "Pose 2";
        public string Pose3Name { get; set; } = "Pose 3";
        public string Pose4Name { get; set; } = "Pose 4";
        public string Pose5Name { get; set; } = "Pose 5";
        public string Pose6Name { get; set; } = "Pose 6";
        
        /// <summary>
        /// Whether each pose is active in the training session
        /// </summary>
        public bool Pose1Active { get; set; } = true;
        public bool Pose2Active { get; set; } = true;
        public bool Pose3Active { get; set; } = true;
        public bool Pose4Active { get; set; } = false;
        public bool Pose5Active { get; set; } = false;
        public bool Pose6Active { get; set; } = false;
        
        /// <summary>
        /// Image file paths for each pose
        /// </summary>
        public string Pose1ImagePath { get; set; } = "";
        public string Pose2ImagePath { get; set; } = "";
        public string Pose3ImagePath { get; set; } = "";
        public string Pose4ImagePath { get; set; } = "";
        public string Pose5ImagePath { get; set; } = "";
        public string Pose6ImagePath { get; set; } = "";
        
        /// <summary>
        /// Gets the image path for a specific pose number
        /// </summary>
        public string GetPoseImagePath(int poseNumber)
        {
            return poseNumber switch
            {
                1 => Pose1ImagePath,
                2 => Pose2ImagePath,
                3 => Pose3ImagePath,
                4 => Pose4ImagePath,
                5 => Pose5ImagePath,
                6 => Pose6ImagePath,
                _ => ""
            };
        }
        
        /// <summary>
        /// Sets the image path for a specific pose number
        /// </summary>
        public void SetPoseImagePath(int poseNumber, string path)
        {
            switch (poseNumber)
            {
                case 1: Pose1ImagePath = path; break;
                case 2: Pose2ImagePath = path; break;
                case 3: Pose3ImagePath = path; break;
                case 4: Pose4ImagePath = path; break;
                case 5: Pose5ImagePath = path; break;
                case 6: Pose6ImagePath = path; break;
            }
        }
    }
}
