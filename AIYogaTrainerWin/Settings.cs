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
        /// URL to the model API
        /// </summary>
        public string ModelUrl { get; set; } = "";
        
        /// <summary>
        /// Whether audio feedback is enabled
        /// </summary>
        public bool AudioFeedback { get; set; } = true;
        
        /// <summary>
        /// Hold time in seconds
        /// </summary>
        public int HoldTime { get; set; } = 3;
        
        /// <summary>
        /// Selected Pose IDs for the training session
        /// </summary>
        public List<string> SelectedPoseIds { get; set; } = new List<string>();
        
        /// <summary>
        /// Maximum difficulty level to include in training
        /// </summary>
        public int MaxDifficultyLevel { get; set; } = 3;
        
        /// <summary>
        /// Number of poses per training session
        /// </summary>
        public int PosesPerSession { get; set; } = 5;
        
        // For backward compatibility - kept as settable properties
        public string Pose1Name { get; set; } = "Mountain Pose";
        public string Pose2Name { get; set; } = "Warrior Pose";
        public string Pose3Name { get; set; } = "Tree Pose";
        public string Pose4Name { get; set; } = "Downward Dog";
        
        /// <summary>
        /// Constructor to initialize default settings
        /// </summary>
        public YogaAppSettings()
        {
            // Set default selected poses
            SelectedPoseIds = new List<string> { "mountain", "warrior1", "tree", "downdog", "triangle" };
        }
    }
}
