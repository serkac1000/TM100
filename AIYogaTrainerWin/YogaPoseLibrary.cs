using System;
using System.Collections.Generic;
using System.Linq;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Represents a single yoga pose with all its properties
    /// </summary>
    public class YogaPose
    {
        /// <summary>
        /// Unique identifier for the pose
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Name of the yoga pose
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Sanskrit name of the pose (optional)
        /// </summary>
        public string SanskritName { get; set; }
        
        /// <summary>
        /// Difficulty level of the pose (1-5)
        /// </summary>
        public int DifficultyLevel { get; set; }
        
        /// <summary>
        /// Category or type of the pose
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Array of adjustment instructions for this pose
        /// </summary>
        public string[] AdjustmentInstructions { get; private set; }
        
        /// <summary>
        /// Brief description of the pose
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Creates a new yoga pose with required information
        /// </summary>
        public YogaPose(string id, string name, string sanskritName, int difficultyLevel, string category, string[] adjustmentInstructions, string description)
        {
            Id = id;
            Name = name;
            SanskritName = sanskritName;
            DifficultyLevel = Math.Clamp(difficultyLevel, 1, 5);
            Category = category;
            AdjustmentInstructions = adjustmentInstructions;
            Description = description;
        }
        
        /// <summary>
        /// Gets adjustment instructions for this pose
        /// </summary>
        public string[] GetAdjustmentInstructions()
        {
            return AdjustmentInstructions;
        }
        
        /// <summary>
        /// Returns a short display of the pose (for menus, etc.)
        /// </summary>
        public string GetDisplayName()
        {
            return $"{Name} ({SanskritName}) - Level {DifficultyLevel}";
        }
    }
    
    /// <summary>
    /// Manages a collection of yoga poses
    /// </summary>
    public class PoseLibrary
    {
        private List<YogaPose> poses;
        
        /// <summary>
        /// Creates a new pose library with default poses
        /// </summary>
        public PoseLibrary()
        {
            poses = new List<YogaPose>();
            LoadDefaultPoses();
        }
        
        /// <summary>
        /// Loads the default set of yoga poses
        /// </summary>
        private void LoadDefaultPoses()
        {
            // Basic standing poses
            poses.Add(new YogaPose(
                "mountain",
                "Mountain Pose",
                "Tadasana",
                1,
                "Standing",
                new string[] {
                    "Stand with feet together or hip-width apart",
                    "Distribute weight evenly through both feet",
                    "Engage thighs and lift knee caps",
                    "Lengthen the spine and lift the crown of the head",
                    "Relax shoulders away from ears"
                },
                "A foundational standing pose that improves posture and body awareness"
            ));
            
            poses.Add(new YogaPose(
                "tree",
                "Tree Pose",
                "Vrksasana",
                2,
                "Balance",
                new string[] {
                    "Focus on a fixed point to improve balance",
                    "Press your foot firmly into your inner thigh",
                    "Keep your hips level",
                    "Engage your core for stability",
                    "Lengthen through the crown of your head"
                },
                "A balancing pose that strengthens the legs and improves focus"
            ));
            
            poses.Add(new YogaPose(
                "warrior1",
                "Warrior I",
                "Virabhadrasana I",
                2,
                "Standing",
                new string[] {
                    "Align front knee over ankle",
                    "Turn back foot to 45-degree angle",
                    "Square hips toward the front",
                    "Reach arms overhead with shoulders relaxed",
                    "Engage core and lift through the chest"
                },
                "A strengthening pose that opens the chest and stretches the legs"
            ));
            
            poses.Add(new YogaPose(
                "warrior2",
                "Warrior II",
                "Virabhadrasana II",
                2,
                "Standing",
                new string[] {
                    "Bend front knee to 90 degrees over ankle",
                    "Keep back leg straight with foot parallel to back edge of mat",
                    "Extend arms with shoulders relaxed",
                    "Gaze over front middle finger",
                    "Keep hips open to the side"
                },
                "A pose that builds strength and stamina in the legs while opening the hips"
            ));
            
            poses.Add(new YogaPose(
                "downdog",
                "Downward-Facing Dog",
                "Adho Mukha Svanasana",
                1,
                "Inversion",
                new string[] {
                    "Push the floor away, straighten your arms",
                    "Press your heels toward the floor",
                    "Keep your head between your arms",
                    "Engage your core and lift your hips high",
                    "Spread your fingers wide for stability"
                },
                "An energizing pose that stretches the hamstrings and strengthens the arms"
            ));
            
            // More challenging poses
            poses.Add(new YogaPose(
                "triangle",
                "Triangle Pose",
                "Trikonasana",
                2,
                "Standing",
                new string[] {
                    "Keep both legs straight",
                    "Extend through both sides of the waist",
                    "Stack shoulders vertically",
                    "Gaze upward toward top hand",
                    "Keep chest open toward the side"
                },
                "A standing pose that stretches the legs and opens the chest"
            ));
            
            poses.Add(new YogaPose(
                "chair",
                "Chair Pose",
                "Utkatasana",
                2,
                "Standing",
                new string[] {
                    "Bend knees deeply as if sitting in a chair",
                    "Keep weight in the heels",
                    "Reach arms up by ears",
                    "Drop shoulders away from ears",
                    "Keep chest lifted and spine long"
                },
                "A strengthening pose for the legs that builds heat in the body"
            ));
            
            poses.Add(new YogaPose(
                "bridge",
                "Bridge Pose",
                "Setu Bandha Sarvangasana",
                2,
                "Backbend",
                new string[] {
                    "Press firmly into feet with knees hip-width apart",
                    "Lift hips toward ceiling",
                    "Keep thighs parallel",
                    "Interlace fingers beneath you",
                    "Roll shoulders under to open chest"
                },
                "A gentle backbend that opens the chest and strengthens the spine"
            ));
            
            poses.Add(new YogaPose(
                "pigeon",
                "Pigeon Pose",
                "Eka Pada Rajakapotasana",
                3,
                "Hip Opener",
                new string[] {
                    "Square hips toward the front",
                    "Flex front foot to protect knee",
                    "Keep back leg extended straight behind you",
                    "Walk hands forward for a deeper stretch",
                    "Breathe deeply into any tight areas"
                },
                "A deep hip opener that relieves sciatic pain and opens the glutes"
            ));
            
            poses.Add(new YogaPose(
                "crow",
                "Crow Pose",
                "Bakasana",
                4,
                "Arm Balance",
                new string[] {
                    "Place knees high on upper arms near armpits",
                    "Engage core and round upper back",
                    "Gaze slightly forward",
                    "Lift one foot at a time then both",
                    "Keep elbows narrow and hands firmly pressed down"
                },
                "An arm balancing pose that builds core and arm strength"
            ));
            
            poses.Add(new YogaPose(
                "headstand",
                "Headstand",
                "Sirsasana",
                5,
                "Inversion",
                new string[] {
                    "Create a firm base with forearms and interlaced fingers",
                    "Place crown of head on mat",
                    "Walk feet in toward head before lifting",
                    "Engage core and use abdominal strength to lift",
                    "Keep legs straight and engage thighs"
                },
                "An advanced inversion that improves circulation and builds core strength"
            ));
            
            poses.Add(new YogaPose(
                "lotus",
                "Lotus Pose",
                "Padmasana",
                4,
                "Seated",
                new string[] {
                    "Start in a comfortable seated position",
                    "Place right foot on left thigh",
                    "Then left foot on right thigh",
                    "Keep spine straight and shoulders relaxed",
                    "Only go as far as is comfortable for your knees"
                },
                "A seated meditation pose that opens the hips and creates a stable base"
            ));
        }
        
        /// <summary>
        /// Gets the total number of poses in the library
        /// </summary>
        public int Count => poses.Count;
        
        /// <summary>
        /// Gets a pose by its index
        /// </summary>
        public YogaPose GetPose(int index)
        {
            if (index >= 0 && index < poses.Count)
            {
                return poses[index];
            }
            
            throw new ArgumentOutOfRangeException(nameof(index), "Pose index is out of range");
        }
        
        /// <summary>
        /// Gets a pose by its ID
        /// </summary>
        public YogaPose GetPoseById(string id)
        {
            var pose = poses.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (pose == null)
            {
                throw new ArgumentException($"No pose found with ID '{id}'");
            }
            
            return pose;
        }
        
        /// <summary>
        /// Gets all poses in the library
        /// </summary>
        public List<YogaPose> GetAllPoses()
        {
            return new List<YogaPose>(poses);
        }
        
        /// <summary>
        /// Gets all poses matching a specific difficulty level
        /// </summary>
        public List<YogaPose> GetPosesByDifficulty(int difficultyLevel)
        {
            return poses.Where(p => p.DifficultyLevel == difficultyLevel).ToList();
        }
        
        /// <summary>
        /// Gets all poses in a specific category
        /// </summary>
        public List<YogaPose> GetPosesByCategory(string category)
        {
            return poses.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        
        /// <summary>
        /// Adds a custom pose to the library
        /// </summary>
        public void AddPose(YogaPose pose)
        {
            if (poses.Any(p => p.Id.Equals(pose.Id, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"A pose with ID '{pose.Id}' already exists");
            }
            
            poses.Add(pose);
        }
        
        /// <summary>
        /// Removes a pose from the library by its ID
        /// </summary>
        public bool RemovePose(string id)
        {
            var pose = poses.FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            if (pose != null)
            {
                return poses.Remove(pose);
            }
            
            return false;
        }
    }
}