using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Handles image-related operations for the application
    /// </summary>
    public class ImageManager
    {
        private YogaAppSettings settings;
        private string imageDirectory;
        
        /// <summary>
        /// Creates a new instance of the ImageManager
        /// </summary>
        public ImageManager(YogaAppSettings settings)
        {
            this.settings = settings;
            
            // Create directory for storing pose images if it doesn't exist
            imageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PoseImages");
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory);
            }
        }
        
        /// <summary>
        /// Uploads an image for a specific pose
        /// </summary>
        public string UploadImage(int poseNumber, string sourcePath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                {
                    throw new FileNotFoundException($"Image file not found: {sourcePath}");
                }
                
                string fileName = $"pose{poseNumber}_{Path.GetFileName(sourcePath)}";
                string destPath = Path.Combine(imageDirectory, fileName);
                
                // Copy the file
                File.Copy(sourcePath, destPath, true);
                
                // Update settings
                settings.SetPoseImagePath(poseNumber, destPath);
                
                return destPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading image: {ex.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// Gets the image path for a specific pose
        /// </summary>
        public string GetImagePath(int poseNumber)
        {
            string path = settings.GetPoseImagePath(poseNumber);
            
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return ""; // Return empty if no image or file doesn't exist
            }
            
            return path;
        }
        
        /// <summary>
        /// Checks if a pose has an associated image
        /// </summary>
        public bool HasImage(int poseNumber)
        {
            string path = GetImagePath(poseNumber);
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
        
        /// <summary>
        /// Represents a skeletal keypoint
        /// </summary>
        public class Keypoint
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Confidence { get; set; }
            
            public Keypoint(float x, float y, float confidence)
            {
                X = x;
                Y = y;
                Confidence = confidence;
            }
        }
        
        /// <summary>
        /// Represents a skeleton with keypoints
        /// </summary>
        public class Skeleton
        {
            public Dictionary<string, Keypoint> Keypoints { get; private set; }
            
            public Skeleton()
            {
                Keypoints = new Dictionary<string, Keypoint>();
            }
            
            public void AddKeypoint(string name, Keypoint keypoint)
            {
                Keypoints[name] = keypoint;
            }
        }
        
        /// <summary>
        /// Performs advanced comparison between reference and detected skeletons
        /// </summary>
        public float CompareSkeletons(Skeleton reference, Skeleton detected, int threshold)
        {
            if (reference == null || detected == null)
            {
                return 0;
            }
            
            // This implementation uses a weighted comparison approach that considers:
            // 1. Position accuracy of keypoints (Euclidean distance)
            // 2. Relative angles between connected keypoints
            // 3. Confidence values of the detected keypoints
            // 4. Different importance weights for different body parts
            
            float totalScore = 0;
            float maxPossibleScore = 0;
            
            // Define keypoint importance weights (more important joints have higher weights)
            Dictionary<string, float> keypointWeights = new Dictionary<string, float>
            {
                {"nose", 0.5f},
                {"leftShoulder", 1.0f},
                {"rightShoulder", 1.0f},
                {"leftElbow", 1.0f},
                {"rightElbow", 1.0f},
                {"leftWrist", 0.8f},
                {"rightWrist", 0.8f},
                {"leftHip", 1.2f},
                {"rightHip", 1.2f},
                {"leftKnee", 1.0f},
                {"rightKnee", 1.0f},
                {"leftAnkle", 0.8f},
                {"rightAnkle", 0.8f}
            };
            
            // Step 1: Compare individual keypoint positions with weighted importance
            foreach (var kvp in reference.Keypoints)
            {
                string keypointName = kvp.Key;
                Keypoint refPoint = kvp.Value;
                float weight = keypointWeights.ContainsKey(keypointName) ? keypointWeights[keypointName] : 1.0f;
                
                if (detected.Keypoints.TryGetValue(keypointName, out Keypoint detectedPoint))
                {
                    // Calculate Euclidean distance normalized to 0-1 range
                    float dx = refPoint.X - detectedPoint.X;
                    float dy = refPoint.Y - detectedPoint.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    // Convert distance to similarity score (closer = higher score)
                    // Distance of 0 = 100% match, distance of 0.3 or more = 0% match
                    float positionScore = Math.Max(0, 1.0f - (distance / 0.3f));
                    
                    // Apply confidence factor
                    float confidenceFactor = detectedPoint.Confidence;
                    float weightedScore = positionScore * confidenceFactor * weight;
                    
                    // Add to total score
                    totalScore += weightedScore;
                    maxPossibleScore += weight;
                }
                else
                {
                    // Missing keypoint in detected skeleton
                    maxPossibleScore += weight;
                }
            }
            
            // Step 2: Calculate joint angle comparisons for key body parts
            // This helps ensure the pose has the correct form beyond just keypoint positions
            Dictionary<string, float> angleScores = CalculateJointAngleScores(reference, detected);
            float angleWeight = 1.5f; // Make angles an important factor
            
            foreach (var score in angleScores)
            {
                totalScore += score.Value * angleWeight;
                maxPossibleScore += angleWeight;
            }
            
            // Calculate final percentage score
            float matchPercentage = maxPossibleScore > 0 ? (totalScore / maxPossibleScore) * 100 : 0;
            
            // Store detailed keypoint match data for visualization
            StoreKeypointMatchData(reference, detected);
            
            return matchPercentage;
        }
        
        // Stores match data for each keypoint to be used in visualization
        private Dictionary<string, float> keypointMatchScores = new Dictionary<string, float>();
        
        /// <summary>
        /// Get keypoint-specific match scores for visualization
        /// </summary>
        public Dictionary<string, float> GetKeypointMatchData()
        {
            return keypointMatchScores;
        }
        
        /// <summary>
        /// Generates personalized suggestions based on keypoint match data
        /// </summary>
        public List<string> GeneratePersonalizedSuggestions(PoseModel pose, int maxSuggestions = 3)
        {
            if (pose == null || keypointMatchScores.Count == 0)
            {
                return new List<string> { "Stand in the correct starting position to receive personalized feedback." };
            }
            
            // Find the keypoints with the lowest match scores
            var orderedKeypoints = keypointMatchScores
                .OrderBy(kvp => kvp.Value)
                .Where(kvp => kvp.Value < 70) // Only suggest improvements for keypoints below 70% match
                .Take(maxSuggestions)
                .ToList();
            
            List<string> suggestions = new List<string>();
            
            // Generate specific suggestions for the keypoints that need the most improvement
            foreach (var keypoint in orderedKeypoints)
            {
                string keypointName = keypoint.Key;
                float matchScore = keypoint.Value;
                
                // Get a suggestion specific to this keypoint and its current match score
                string suggestion = pose.GetKeypointAdjustment(keypointName, keypointMatchScores);
                
                // Convert technical keypoint names to user-friendly terms
                string friendlyName = GetFriendlyKeypointName(keypointName);
                
                // Add color-coded severity indicators based on match percentage
                string severityPrefix = "";
                if (matchScore < 30)
                {
                    severityPrefix = "[Critical] ";
                }
                else if (matchScore < 50)
                {
                    severityPrefix = "[Major] ";
                }
                else if (matchScore < 70)
                {
                    severityPrefix = "[Minor] ";
                }
                
                suggestions.Add($"{severityPrefix}{friendlyName}: {suggestion}");
            }
            
            // If no specific improvements needed, give positive reinforcement
            if (suggestions.Count == 0)
            {
                suggestions.Add("Great form! Try to hold the pose for a few breaths.");
                suggestions.Add("Your alignment looks excellent! Focus on your breathing now.");
            }
            
            return suggestions;
        }
        
        /// <summary>
        /// Converts technical keypoint names to user-friendly terms
        /// </summary>
        private string GetFriendlyKeypointName(string keypointName)
        {
            switch (keypointName)
            {
                case "leftShoulder": return "Left Shoulder";
                case "rightShoulder": return "Right Shoulder";
                case "leftElbow": return "Left Elbow";
                case "rightElbow": return "Right Elbow";
                case "leftWrist": return "Left Wrist";
                case "rightWrist": return "Right Wrist";
                case "leftHip": return "Left Hip";
                case "rightHip": return "Right Hip";
                case "leftKnee": return "Left Knee";
                case "rightKnee": return "Right Knee";
                case "leftAnkle": return "Left Ankle";
                case "rightAnkle": return "Right Ankle";
                case "nose": return "Head Position";
                default: return keypointName;
            }
        }
        
        /// <summary>
        /// Stores individual keypoint match data for detailed feedback
        /// </summary>
        private void StoreKeypointMatchData(Skeleton reference, Skeleton detected)
        {
            keypointMatchScores.Clear();
            
            foreach (var kvp in reference.Keypoints)
            {
                string keypointName = kvp.Key;
                Keypoint refPoint = kvp.Value;
                
                if (detected.Keypoints.TryGetValue(keypointName, out Keypoint detectedPoint))
                {
                    // Calculate position similarity
                    float dx = refPoint.X - detectedPoint.X;
                    float dy = refPoint.Y - detectedPoint.Y;
                    float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                    
                    // Convert to percentage match (0.2 or greater distance = 0% match)
                    float matchPercentage = Math.Max(0, 1.0f - (distance / 0.2f)) * 100f;
                    
                    // Apply confidence factor
                    matchPercentage *= detectedPoint.Confidence;
                    
                    // Store result
                    keypointMatchScores[keypointName] = matchPercentage;
                }
                else
                {
                    keypointMatchScores[keypointName] = 0;
                }
            }
        }
        
        /// <summary>
        /// Calculate angle similarities between reference and detected skeletons
        /// </summary>
        private Dictionary<string, float> CalculateJointAngleScores(Skeleton reference, Skeleton detected)
        {
            Dictionary<string, float> scores = new Dictionary<string, float>();
            
            // Calculate and compare important angles
            
            // Right arm angle
            if (HasRequiredKeypoints(reference, detected, "rightShoulder", "rightElbow", "rightWrist"))
            {
                float refAngle = CalculateAngle(
                    reference.Keypoints["rightShoulder"], 
                    reference.Keypoints["rightElbow"], 
                    reference.Keypoints["rightWrist"]);
                
                float detectedAngle = CalculateAngle(
                    detected.Keypoints["rightShoulder"], 
                    detected.Keypoints["rightElbow"], 
                    detected.Keypoints["rightWrist"]);
                
                scores["rightArmAngle"] = CalculateAngleSimilarity(refAngle, detectedAngle);
            }
            
            // Left arm angle
            if (HasRequiredKeypoints(reference, detected, "leftShoulder", "leftElbow", "leftWrist"))
            {
                float refAngle = CalculateAngle(
                    reference.Keypoints["leftShoulder"], 
                    reference.Keypoints["leftElbow"], 
                    reference.Keypoints["leftWrist"]);
                
                float detectedAngle = CalculateAngle(
                    detected.Keypoints["leftShoulder"], 
                    detected.Keypoints["leftElbow"], 
                    detected.Keypoints["leftWrist"]);
                
                scores["leftArmAngle"] = CalculateAngleSimilarity(refAngle, detectedAngle);
            }
            
            // Right leg angle
            if (HasRequiredKeypoints(reference, detected, "rightHip", "rightKnee", "rightAnkle"))
            {
                float refAngle = CalculateAngle(
                    reference.Keypoints["rightHip"], 
                    reference.Keypoints["rightKnee"], 
                    reference.Keypoints["rightAnkle"]);
                
                float detectedAngle = CalculateAngle(
                    detected.Keypoints["rightHip"], 
                    detected.Keypoints["rightKnee"], 
                    detected.Keypoints["rightAnkle"]);
                
                scores["rightLegAngle"] = CalculateAngleSimilarity(refAngle, detectedAngle);
            }
            
            // Left leg angle
            if (HasRequiredKeypoints(reference, detected, "leftHip", "leftKnee", "leftAnkle"))
            {
                float refAngle = CalculateAngle(
                    reference.Keypoints["leftHip"], 
                    reference.Keypoints["leftKnee"], 
                    reference.Keypoints["leftAnkle"]);
                
                float detectedAngle = CalculateAngle(
                    detected.Keypoints["leftHip"], 
                    detected.Keypoints["leftKnee"], 
                    detected.Keypoints["leftAnkle"]);
                
                scores["leftLegAngle"] = CalculateAngleSimilarity(refAngle, detectedAngle);
            }
            
            return scores;
        }
        
        /// <summary>
        /// Check if all required keypoints exist in both skeletons
        /// </summary>
        private bool HasRequiredKeypoints(Skeleton reference, Skeleton detected, params string[] keypointNames)
        {
            foreach (var name in keypointNames)
            {
                if (!reference.Keypoints.ContainsKey(name) || !detected.Keypoints.ContainsKey(name))
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// Calculate angle between three keypoints (in degrees)
        /// </summary>
        private float CalculateAngle(Keypoint p1, Keypoint p2, Keypoint p3)
        {
            // Calculate vectors
            float v1x = p1.X - p2.X;
            float v1y = p1.Y - p2.Y;
            float v2x = p3.X - p2.X;
            float v2y = p3.Y - p2.Y;
            
            // Calculate dot product
            float dotProduct = v1x * v2x + v1y * v2y;
            
            // Calculate magnitudes
            float v1Mag = (float)Math.Sqrt(v1x * v1x + v1y * v1y);
            float v2Mag = (float)Math.Sqrt(v2x * v2x + v2y * v2y);
            
            // Calculate angle in radians and convert to degrees
            if (v1Mag == 0 || v2Mag == 0) return 0;
            float cosAngle = dotProduct / (v1Mag * v2Mag);
            cosAngle = Math.Clamp(cosAngle, -1.0f, 1.0f);
            float angleRadians = (float)Math.Acos(cosAngle);
            return angleRadians * (180f / (float)Math.PI);
        }
        
        /// <summary>
        /// Calculate similarity between two angles (0-1 scale)
        /// </summary>
        private float CalculateAngleSimilarity(float angle1, float angle2)
        {
            float difference = Math.Abs(angle1 - angle2);
            
            // If difference is greater than 180, use the smaller angle
            if (difference > 180)
            {
                difference = 360 - difference;
            }
            
            // Convert to similarity score (0-1)
            // 0 degrees difference = 1.0 (perfect match)
            // 45 or more degrees difference = 0.0 (no match)
            return Math.Max(0, 1.0f - (difference / 45.0f));
        }
        
        /// <summary>
        /// Simulates getting a reference skeleton for a pose
        /// </summary>
        public Skeleton GetReferenceSkeleton(int poseNumber)
        {
            // In a real implementation, this would extract skeleton data from the image
            // For simulation purposes, we'll create dummy skeletons
            
            Skeleton skeleton = new Skeleton();
            
            // Create different keypoints depending on the pose
            switch (poseNumber)
            {
                case 1: // Pose 1 (e.g., Mountain Pose)
                    skeleton.AddKeypoint("nose", new Keypoint(0.5f, 0.2f, 0.9f));
                    skeleton.AddKeypoint("leftShoulder", new Keypoint(0.4f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("rightShoulder", new Keypoint(0.6f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("leftElbow", new Keypoint(0.35f, 0.4f, 0.8f));
                    skeleton.AddKeypoint("rightElbow", new Keypoint(0.65f, 0.4f, 0.8f));
                    skeleton.AddKeypoint("leftHip", new Keypoint(0.45f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("rightHip", new Keypoint(0.55f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("leftKnee", new Keypoint(0.45f, 0.75f, 0.8f));
                    skeleton.AddKeypoint("rightKnee", new Keypoint(0.55f, 0.75f, 0.8f));
                    skeleton.AddKeypoint("leftAnkle", new Keypoint(0.45f, 0.9f, 0.75f));
                    skeleton.AddKeypoint("rightAnkle", new Keypoint(0.55f, 0.9f, 0.75f));
                    break;
                    
                case 2: // Pose 2 (e.g., Warrior Pose)
                    skeleton.AddKeypoint("nose", new Keypoint(0.5f, 0.2f, 0.9f));
                    skeleton.AddKeypoint("leftShoulder", new Keypoint(0.4f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("rightShoulder", new Keypoint(0.6f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("leftElbow", new Keypoint(0.3f, 0.3f, 0.8f));
                    skeleton.AddKeypoint("rightElbow", new Keypoint(0.7f, 0.3f, 0.8f));
                    skeleton.AddKeypoint("leftWrist", new Keypoint(0.2f, 0.3f, 0.7f));
                    skeleton.AddKeypoint("rightWrist", new Keypoint(0.8f, 0.3f, 0.7f));
                    skeleton.AddKeypoint("leftHip", new Keypoint(0.45f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("rightHip", new Keypoint(0.55f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("leftKnee", new Keypoint(0.35f, 0.75f, 0.8f));
                    skeleton.AddKeypoint("rightKnee", new Keypoint(0.65f, 0.65f, 0.8f));
                    skeleton.AddKeypoint("leftAnkle", new Keypoint(0.25f, 0.9f, 0.75f));
                    skeleton.AddKeypoint("rightAnkle", new Keypoint(0.75f, 0.9f, 0.75f));
                    break;
                    
                case 3: // Pose 3 (e.g., Tree Pose)
                    skeleton.AddKeypoint("nose", new Keypoint(0.5f, 0.2f, 0.9f));
                    skeleton.AddKeypoint("leftShoulder", new Keypoint(0.4f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("rightShoulder", new Keypoint(0.6f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("leftElbow", new Keypoint(0.3f, 0.25f, 0.8f));
                    skeleton.AddKeypoint("rightElbow", new Keypoint(0.7f, 0.25f, 0.8f));
                    skeleton.AddKeypoint("leftWrist", new Keypoint(0.4f, 0.15f, 0.7f));
                    skeleton.AddKeypoint("rightWrist", new Keypoint(0.6f, 0.15f, 0.7f));
                    skeleton.AddKeypoint("leftHip", new Keypoint(0.45f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("rightHip", new Keypoint(0.55f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("leftKnee", new Keypoint(0.55f, 0.5f, 0.8f)); // Bent leg
                    skeleton.AddKeypoint("rightKnee", new Keypoint(0.55f, 0.75f, 0.8f));
                    skeleton.AddKeypoint("leftAnkle", new Keypoint(0.55f, 0.6f, 0.7f)); // Foot against leg
                    skeleton.AddKeypoint("rightAnkle", new Keypoint(0.55f, 0.9f, 0.75f));
                    break;
                
                default:
                    // Generic skeleton for other poses
                    skeleton.AddKeypoint("nose", new Keypoint(0.5f, 0.2f, 0.9f));
                    skeleton.AddKeypoint("leftShoulder", new Keypoint(0.4f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("rightShoulder", new Keypoint(0.6f, 0.3f, 0.9f));
                    skeleton.AddKeypoint("leftHip", new Keypoint(0.45f, 0.6f, 0.85f));
                    skeleton.AddKeypoint("rightHip", new Keypoint(0.55f, 0.6f, 0.85f));
                    break;
            }
            
            return skeleton;
        }
        
        /// <summary>
        /// Simulates detecting a skeleton from camera input
        /// </summary>
        public Skeleton SimulateDetectedSkeleton(int targetPoseNumber, int accuracy)
        {
            // Get the reference skeleton for the target pose
            Skeleton reference = GetReferenceSkeleton(targetPoseNumber);
            Skeleton detected = new Skeleton();
            
            Random random = new Random();
            
            // Add keypoints with some variance based on accuracy
            // Higher accuracy = closer to reference
            foreach (var kvp in reference.Keypoints)
            {
                string keypointName = kvp.Key;
                Keypoint refPoint = kvp.Value;
                
                // Calculate deviation based on accuracy (0-100)
                // Higher accuracy = less deviation
                float maxDeviation = 1.0f - (accuracy / 100.0f);
                float xDeviation = (float)((random.NextDouble() * 2 - 1) * maxDeviation * 0.2);
                float yDeviation = (float)((random.NextDouble() * 2 - 1) * maxDeviation * 0.2);
                
                // Apply deviation
                float x = Math.Clamp(refPoint.X + xDeviation, 0, 1);
                float y = Math.Clamp(refPoint.Y + yDeviation, 0, 1);
                
                // Confidence is based on accuracy
                float confidence = refPoint.Confidence * (accuracy / 100.0f);
                
                detected.AddKeypoint(keypointName, new Keypoint(x, y, confidence));
            }
            
            return detected;
        }
    }
}