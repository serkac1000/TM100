using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Manages advanced pose features including transitions, skeleton comparison, and sequencing
    /// </summary>
    public class AdvancedPoseManager
    {
        private YogaAppSettings settings;
        private ImageManager imageManager;
        private AudioManager audioManager;
        
        private int currentPoseNumber = 1;
        private bool isHoldingPose = false;
        private DateTime holdStartTime;
        private int holdDurationSeconds = 0;
        
        /// <summary>
        /// Creates a new instance of the AdvancedPoseManager
        /// </summary>
        public AdvancedPoseManager(YogaAppSettings settings, ImageManager imageManager, AudioManager audioManager)
        {
            this.settings = settings;
            this.imageManager = imageManager;
            this.audioManager = audioManager;
            this.holdDurationSeconds = settings.HoldTime;
        }
        
        /// <summary>
        /// Gets the current pose number (1-6)
        /// </summary>
        public int CurrentPoseNumber => currentPoseNumber;
        
        /// <summary>
        /// Gets the current pose name
        /// </summary>
        public string CurrentPoseName
        {
            get
            {
                return currentPoseNumber switch
                {
                    1 => settings.Pose1Name,
                    2 => settings.Pose2Name,
                    3 => settings.Pose3Name,
                    4 => settings.Pose4Name,
                    5 => settings.Pose5Name,
                    6 => settings.Pose6Name,
                    _ => "Unknown Pose"
                };
            }
        }
        
        /// <summary>
        /// Gets whether the current pose is active
        /// </summary>
        public bool IsCurrentPoseActive
        {
            get
            {
                return currentPoseNumber switch
                {
                    1 => settings.Pose1Active,
                    2 => settings.Pose2Active,
                    3 => settings.Pose3Active,
                    4 => settings.Pose4Active,
                    5 => settings.Pose5Active,
                    6 => settings.Pose6Active,
                    _ => false
                };
            }
        }
        
        /// <summary>
        /// Sets the current pose by number
        /// </summary>
        public void SetCurrentPose(int poseNumber)
        {
            if (poseNumber < 1 || poseNumber > 6)
            {
                throw new ArgumentOutOfRangeException(nameof(poseNumber), "Pose number must be between 1 and 6");
            }
            
            // Only change if the new pose is active
            bool isActive = poseNumber switch
            {
                1 => settings.Pose1Active,
                2 => settings.Pose2Active,
                3 => settings.Pose3Active,
                4 => settings.Pose4Active,
                5 => settings.Pose5Active,
                6 => settings.Pose6Active,
                _ => false
            };
            
            if (!isActive)
            {
                Console.WriteLine($"Pose {poseNumber} is not active. Cannot switch to it.");
                return;
            }
            
            string oldPoseName = CurrentPoseName;
            currentPoseNumber = poseNumber;
            
            // Reset hold state
            isHoldingPose = false;
            
            // Announce transition if not the initial setup
            if (oldPoseName != CurrentPoseName)
            {
                audioManager.AnnouncePoseTransition(oldPoseName, CurrentPoseName);
            }
            
            Console.WriteLine($"Current pose set to {CurrentPoseName} (#{currentPoseNumber})");
        }
        
        /// <summary>
        /// Moves to the next pose in sequence
        /// </summary>
        public void MoveToNextPose()
        {
            int nextPose = currentPoseNumber;
            bool foundActive = false;
            
            // Try to find the next active pose in sequence
            for (int i = 1; i <= 6; i++)
            {
                // Calculate next pose number (wrapping from 6 back to 1)
                nextPose = currentPoseNumber + i > 6 ? (currentPoseNumber + i) % 6 : currentPoseNumber + i;
                if (nextPose == 0) nextPose = 6; // Handle the case where result is 0
                
                // Check if the pose is active
                bool isActive = nextPose switch
                {
                    1 => settings.Pose1Active,
                    2 => settings.Pose2Active,
                    3 => settings.Pose3Active,
                    4 => settings.Pose4Active,
                    5 => settings.Pose5Active,
                    6 => settings.Pose6Active,
                    _ => false
                };
                
                if (isActive)
                {
                    foundActive = true;
                    break;
                }
            }
            
            if (foundActive)
            {
                SetCurrentPose(nextPose);
            }
            else
            {
                Console.WriteLine("No active poses available to move to next.");
            }
        }
        
        /// <summary>
        /// Updates the pose hold status based on the detected accuracy
        /// </summary>
        public bool UpdatePoseHold(float accuracy)
        {
            // If currently holding the pose
            if (isHoldingPose)
            {
                // Check if accuracy has fallen below threshold
                if (accuracy < settings.DetectionThreshold)
                {
                    isHoldingPose = false;
                    Console.WriteLine("Lost pose hold.");
                    return false;
                }
                
                // Calculate hold duration
                TimeSpan holdDuration = DateTime.Now - holdStartTime;
                
                // Check if hold time has been reached
                if (holdDuration.TotalSeconds >= holdDurationSeconds)
                {
                    Console.WriteLine($"Successfully held {CurrentPoseName} for {holdDurationSeconds} seconds!");
                    
                    // Handle auto-progression if enabled
                    if (settings.AutoPoseProgression)
                    {
                        MoveToNextPose();
                    }
                    
                    isHoldingPose = false;
                    return true; // Successfully completed a pose hold
                }
                
                // Still holding, but not complete yet
                return false;
            }
            else
            {
                // Not currently holding - check if should start
                if (accuracy >= settings.DetectionThreshold)
                {
                    isHoldingPose = true;
                    holdStartTime = DateTime.Now;
                    Console.WriteLine($"Starting to hold {CurrentPoseName}...");
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// Gets the remaining hold time in seconds
        /// </summary>
        public int GetRemainingHoldTime()
        {
            if (!isHoldingPose)
            {
                return holdDurationSeconds;
            }
            
            TimeSpan holdDuration = DateTime.Now - holdStartTime;
            int remainingSeconds = holdDurationSeconds - (int)holdDuration.TotalSeconds;
            
            return Math.Max(0, remainingSeconds);
        }
        
        /// <summary>
        /// Gets the percentage of hold time completed
        /// </summary>
        public int GetHoldTimePercentage()
        {
            if (!isHoldingPose)
            {
                return 0;
            }
            
            TimeSpan holdDuration = DateTime.Now - holdStartTime;
            float percentage = (float)(holdDuration.TotalSeconds / holdDurationSeconds) * 100;
            
            return Math.Min(100, (int)percentage);
        }
        
        /// <summary>
        /// Gets a reference skeleton for the current pose
        /// </summary>
        public ImageManager.Skeleton GetCurrentPoseSkeleton()
        {
            return imageManager.GetReferenceSkeleton(currentPoseNumber);
        }
        
        /// <summary>
        /// Compares a detected skeleton to the current pose
        /// </summary>
        public float CompareToCurrent(ImageManager.Skeleton detected)
        {
            ImageManager.Skeleton reference = GetCurrentPoseSkeleton();
            return imageManager.CompareSkeletons(reference, detected, settings.DetectionThreshold);
        }
    }
}