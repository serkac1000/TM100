using System;
using System.Collections.Generic;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Represents a pose model with detection logic
    /// </summary>
    public class PoseModel
    {
        /// <summary>
        /// The name of the pose
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Whether this pose is currently active for detection
        /// </summary>
        public bool Active { get; set; }
        
        /// <summary>
        /// The current detection confidence (0-100)
        /// </summary>
        public float Confidence { get; private set; }
        
        /// <summary>
        /// Adjustment instructions for this pose
        /// </summary>
        public List<string> AdjustmentInstructions { get; set; }
        
        /// <summary>
        /// Keypoint-specific adjustment instructions
        /// </summary>
        public Dictionary<string, List<string>> KeypointAdjustments { get; set; }
        
        /// <summary>
        /// Creates a new pose model
        /// </summary>
        public PoseModel(string name, bool active = true)
        {
            Name = name;
            Active = active;
            Confidence = 0;
            AdjustmentInstructions = new List<string>();
            KeypointAdjustments = new Dictionary<string, List<string>>();
        }
        
        /// <summary>
        /// Updates the detection confidence for this pose
        /// </summary>
        public void UpdateConfidence(float newConfidence)
        {
            Confidence = Math.Clamp(newConfidence, 0, 100);
        }
        
        /// <summary>
        /// Checks if the pose is detected based on threshold
        /// </summary>
        public bool IsDetected(int threshold)
        {
            return Active && Confidence >= threshold;
        }
        
        /// <summary>
        /// Gets a random adjustment instruction for this pose
        /// </summary>
        public string GetRandomAdjustment()
        {
            if (AdjustmentInstructions.Count == 0)
            {
                return "Focus on your form";
            }
            
            Random random = new Random();
            int index = random.Next(AdjustmentInstructions.Count);
            return AdjustmentInstructions[index];
        }
        
        /// <summary>
        /// Gets a specific adjustment instruction for a keypoint
        /// </summary>
        public string GetKeypointAdjustment(string keypointName, Dictionary<string, float> matchScores)
        {
            // If we don't have specific adjustments for this keypoint, return a general instruction
            if (!KeypointAdjustments.ContainsKey(keypointName) || KeypointAdjustments[keypointName].Count == 0)
            {
                return GetRandomAdjustment();
            }
            
            // Get the match score for this keypoint if available
            float matchScore = 0;
            if (matchScores.ContainsKey(keypointName))
            {
                matchScore = matchScores[keypointName];
            }
            
            // Select a random adjustment for this keypoint
            Random random = new Random();
            int index = random.Next(KeypointAdjustments[keypointName].Count);
            string adjustment = KeypointAdjustments[keypointName][index];
            
            // Add score-based context to the adjustment message
            if (matchScore < 30)
            {
                return $"Critical adjustment needed: {adjustment}";
            }
            else if (matchScore < 60)
            {
                return $"Try to: {adjustment}";
            }
            else if (matchScore < 85)
            {
                return $"Minor adjustment: {adjustment}";
            }
            else
            {
                return "Great alignment here!";
            }
        }
    }
    
    /// <summary>
    /// Manages the collection of poses for the application
    /// </summary>
    public class PoseManager
    {
        private Dictionary<string, PoseModel> poses;
        
        /// <summary>
        /// Create a new pose manager with default poses
        /// </summary>
        public PoseManager(YogaAppSettings settings)
        {
            poses = new Dictionary<string, PoseModel>();
            
            // Initialize poses from settings
            InitializePose("Pose1", settings.Pose1Name, settings.Pose1Active);
            InitializePose("Pose2", settings.Pose2Name, settings.Pose2Active);
            InitializePose("Pose3", settings.Pose3Name, settings.Pose3Active);
            InitializePose("Pose4", settings.Pose4Name, settings.Pose4Active);
            InitializePose("Pose5", settings.Pose5Name, settings.Pose5Active);
            InitializePose("Pose6", settings.Pose6Name, settings.Pose6Active);
            
            // Add adjustment instructions for common poses
            if (poses.TryGetValue("Pose1", out PoseModel pose1))
            {
                pose1.AdjustmentInstructions = new List<string>
                {
                    "Stand taller, elongate your spine",
                    "Distribute weight evenly through both feet",
                    "Engage your core muscles",
                    "Relax your shoulders away from your ears",
                    "Breathe deeply and steadily"
                };
                
                // Add keypoint-specific adjustments for Pose 1
                pose1.KeypointAdjustments = new Dictionary<string, List<string>>
                {
                    { "leftShoulder", new List<string> { 
                        "Lower your shoulders away from your ears", 
                        "Keep shoulders back and down"
                    }},
                    { "rightShoulder", new List<string> { 
                        "Lower your shoulders away from your ears", 
                        "Keep shoulders back and down" 
                    }},
                    { "leftHip", new List<string> { 
                        "Square your hips forward", 
                        "Keep your weight evenly distributed" 
                    }},
                    { "rightHip", new List<string> { 
                        "Square your hips forward", 
                        "Keep your weight evenly distributed" 
                    }},
                    { "leftKnee", new List<string> { 
                        "Straighten your leg without locking the knee", 
                        "Engage your quad muscles" 
                    }},
                    { "rightKnee", new List<string> { 
                        "Straighten your leg without locking the knee", 
                        "Engage your quad muscles" 
                    }},
                    { "leftAnkle", new List<string> { 
                        "Balance your weight evenly through all four corners of your foot", 
                        "Lift your arches slightly" 
                    }},
                    { "rightAnkle", new List<string> { 
                        "Balance your weight evenly through all four corners of your foot", 
                        "Lift your arches slightly" 
                    }}
                };
            }
            
            if (poses.TryGetValue("Pose2", out PoseModel pose2))
            {
                pose2.AdjustmentInstructions = new List<string>
                {
                    "Bend your front knee more",
                    "Keep your back leg straight",
                    "Extend your arms more firmly",
                    "Square your hips forward",
                    "Keep your stance wide and stable"
                };
                
                // Add keypoint-specific adjustments for Pose 2
                pose2.KeypointAdjustments = new Dictionary<string, List<string>>
                {
                    { "leftShoulder", new List<string> { 
                        "Keep your shoulders down and away from your ears", 
                        "Engage your shoulder blades" 
                    }},
                    { "rightShoulder", new List<string> { 
                        "Keep your shoulders down and away from your ears", 
                        "Engage your shoulder blades" 
                    }},
                    { "leftElbow", new List<string> { 
                        "Extend your arm fully without locking your elbow", 
                        "Keep your elbow in line with your shoulder" 
                    }},
                    { "rightElbow", new List<string> { 
                        "Extend your arm fully without locking your elbow", 
                        "Keep your elbow in line with your shoulder" 
                    }},
                    { "leftHip", new List<string> { 
                        "Square your hips forward", 
                        "Keep your hips level" 
                    }},
                    { "rightHip", new List<string> { 
                        "Square your hips forward", 
                        "Keep your hips level" 
                    }},
                    { "leftKnee", new List<string> { 
                        "Bend your front knee to 90 degrees", 
                        "Keep your knee aligned over your ankle" 
                    }},
                    { "rightKnee", new List<string> { 
                        "Keep your back leg straight", 
                        "Engage your quad muscles" 
                    }},
                    { "leftAnkle", new List<string> { 
                        "Press the outer edge of your back foot firmly into the floor", 
                        "Keep your ankle in line with your knee" 
                    }},
                    { "rightAnkle", new List<string> { 
                        "Root down through your heel", 
                        "Keep your ankle in line with your knee" 
                    }}
                };
            }
            
            if (poses.TryGetValue("Pose3", out PoseModel pose3))
            {
                pose3.AdjustmentInstructions = new List<string>
                {
                    "Focus on a fixed point to improve balance",
                    "Press your foot firmly into your inner thigh",
                    "Keep your hips level",
                    "Engage your standing leg",
                    "Bring your hands to heart center if needed for balance"
                };
                
                // Add keypoint-specific adjustments for Pose 3
                pose3.KeypointAdjustments = new Dictionary<string, List<string>>
                {
                    { "leftShoulder", new List<string> { 
                        "Keep your shoulders relaxed and away from your ears", 
                        "Open your chest" 
                    }},
                    { "rightShoulder", new List<string> { 
                        "Keep your shoulders relaxed and away from your ears", 
                        "Open your chest" 
                    }},
                    { "leftElbow", new List<string> { 
                        "Keep your elbows soft", 
                        "Bring your palms together at your heart center" 
                    }},
                    { "rightElbow", new List<string> { 
                        "Keep your elbows soft", 
                        "Bring your palms together at your heart center" 
                    }},
                    { "leftHip", new List<string> { 
                        "Keep your hips level", 
                        "Engage your core to stabilize" 
                    }},
                    { "rightHip", new List<string> { 
                        "Keep your hips level", 
                        "Engage your core to stabilize" 
                    }},
                    { "leftKnee", new List<string> { 
                        "Place your foot higher on your inner thigh", 
                        "Rotate your knee outward" 
                    }},
                    { "rightKnee", new List<string> { 
                        "Engage your standing leg", 
                        "Micro-bend your knee to avoid locking" 
                    }},
                    { "leftAnkle", new List<string> { 
                        "Press your foot firmly against your inner thigh", 
                        "Flex your foot" 
                    }},
                    { "rightAnkle", new List<string> { 
                        "Distribute your weight evenly across your standing foot", 
                        "Root down through all four corners of your foot" 
                    }}
                };
            }
            
            if (poses.TryGetValue("Pose4", out PoseModel pose4))
            {
                pose4.AdjustmentInstructions = new List<string>
                {
                    "Push the floor away, straighten your arms",
                    "Press your heels toward the floor",
                    "Keep your head between your arms",
                    "Create a straight line from hands to hips",
                    "Engage your core to support your spine"
                };
                
                // Add keypoint-specific adjustments for Pose 4
                pose4.KeypointAdjustments = new Dictionary<string, List<string>>
                {
                    { "leftShoulder", new List<string> { 
                        "Externally rotate your shoulders", 
                        "Push the floor away to create space between shoulders and ears" 
                    }},
                    { "rightShoulder", new List<string> { 
                        "Externally rotate your shoulders", 
                        "Push the floor away to create space between shoulders and ears" 
                    }},
                    { "leftElbow", new List<string> { 
                        "Straighten your arms without locking your elbows", 
                        "Rotate your elbow creases toward each other" 
                    }},
                    { "rightElbow", new List<string> { 
                        "Straighten your arms without locking your elbows", 
                        "Rotate your elbow creases toward each other" 
                    }},
                    { "leftHip", new List<string> { 
                        "Lift your hips high", 
                        "Draw your sit bones toward the ceiling" 
                    }},
                    { "rightHip", new List<string> { 
                        "Lift your hips high", 
                        "Draw your sit bones toward the ceiling" 
                    }},
                    { "leftKnee", new List<string> { 
                        "Straighten your legs", 
                        "Engage your quadriceps to lift your kneecaps" 
                    }},
                    { "rightKnee", new List<string> { 
                        "Straighten your legs", 
                        "Engage your quadriceps to lift your kneecaps" 
                    }},
                    { "leftAnkle", new List<string> { 
                        "Press your heels toward the floor", 
                        "Spread your toes wide" 
                    }},
                    { "rightAnkle", new List<string> { 
                        "Press your heels toward the floor", 
                        "Spread your toes wide" 
                    }}
                };
            }
        }
        
        /// <summary>
        /// Initialize a pose with settings
        /// </summary>
        private void InitializePose(string key, string name, bool active)
        {
            var pose = new PoseModel(name, active);
            poses[key] = pose;
        }
        
        /// <summary>
        /// Gets all active poses
        /// </summary>
        public List<PoseModel> GetActivePoses()
        {
            List<PoseModel> activePoses = new List<PoseModel>();
            
            foreach (var pose in poses.Values)
            {
                if (pose.Active)
                {
                    activePoses.Add(pose);
                }
            }
            
            return activePoses;
        }
        
        /// <summary>
        /// Gets a specific pose by its key
        /// </summary>
        public PoseModel GetPose(string key)
        {
            if (poses.TryGetValue(key, out PoseModel pose))
            {
                return pose;
            }
            
            throw new ArgumentException($"Pose with key '{key}' not found");
        }
        
        /// <summary>
        /// Updates a pose with new settings
        /// </summary>
        public void UpdatePoseSettings(YogaAppSettings settings)
        {
            if (poses.TryGetValue("Pose1", out PoseModel pose1))
            {
                pose1.Name = settings.Pose1Name;
                pose1.Active = settings.Pose1Active;
            }
            
            if (poses.TryGetValue("Pose2", out PoseModel pose2))
            {
                pose2.Name = settings.Pose2Name;
                pose2.Active = settings.Pose2Active;
            }
            
            if (poses.TryGetValue("Pose3", out PoseModel pose3))
            {
                pose3.Name = settings.Pose3Name;
                pose3.Active = settings.Pose3Active;
            }
            
            if (poses.TryGetValue("Pose4", out PoseModel pose4))
            {
                pose4.Name = settings.Pose4Name;
                pose4.Active = settings.Pose4Active;
            }
            
            if (poses.TryGetValue("Pose5", out PoseModel pose5))
            {
                pose5.Name = settings.Pose5Name;
                pose5.Active = settings.Pose5Active;
            }
            
            if (poses.TryGetValue("Pose6", out PoseModel pose6))
            {
                pose6.Name = settings.Pose6Name;
                pose6.Active = settings.Pose6Active;
            }
        }
        
        /// <summary>
        /// Simulates detection of poses with random confidence values
        /// </summary>
        public PoseModel SimulateDetection(int threshold)
        {
            Random random = new Random();
            List<PoseModel> activePoses = GetActivePoses();
            
            // Reset all confidences
            foreach (var pose in activePoses)
            {
                pose.UpdateConfidence(random.Next(101)); // 0-100 confidence
            }
            
            // Find the highest confidence pose above threshold
            PoseModel highestConfidencePose = null;
            float highestConfidence = threshold - 1; // Start below threshold
            
            foreach (var pose in activePoses)
            {
                if (pose.Confidence > highestConfidence)
                {
                    highestConfidence = pose.Confidence;
                    highestConfidencePose = pose;
                }
            }
            
            return highestConfidencePose; // Could be null if no pose above threshold
        }
    }
}