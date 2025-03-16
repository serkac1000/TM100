using System;
using System.Threading.Tasks;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Manages audio feedback during yoga pose training
    /// </summary>
    public class AudioManager
    {
        private bool isEnabled;

        public AudioManager(bool enabled = true)
        {
            isEnabled = enabled;
        }

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        public bool IsEnabled()
        {
            return isEnabled;
        }

        /// <summary>
        /// Plays an audio cue when a pose is detected successfully
        /// </summary>
        public void PlayPoseDetectedSound()
        {
            if (!isEnabled) return;
            
            // In a real application, this would play an actual sound
            Console.WriteLine("[Audio: Pose detected successfully]");
        }

        /// <summary>
        /// Plays an audio cue when the user needs to adjust their pose
        /// </summary>
        public void PlayAdjustPoseSound()
        {
            if (!isEnabled) return;
            
            // In a real application, this would play an actual sound
            Console.WriteLine("[Audio: Please adjust your pose]");
        }

        /// <summary>
        /// Plays an audio cue to hold the current pose
        /// </summary>
        public void PlayHoldPoseSound()
        {
            if (!isEnabled) return;
            
            // In a real application, this would play an actual sound
            Console.WriteLine("[Audio: Hold this pose]");
        }

        /// <summary>
        /// Plays an audio instruction for the specified pose
        /// </summary>
        public void PlayPoseInstructions(string poseName)
        {
            if (!isEnabled) return;

            // In a real application, this would play pose-specific instructions
            Console.WriteLine($"[Audio: Instructions for {poseName}]");
        }

        /// <summary>
        /// Plays a countdown timer
        /// </summary>
        public async Task PlayCountdown(int seconds)
        {
            if (!isEnabled) return;

            for (int i = seconds; i > 0; i--)
            {
                Console.WriteLine($"[Audio: {i}]");
                await Task.Delay(1000);
            }
            
            Console.WriteLine("[Audio: Time's up!]");
        }

        /// <summary>
        /// Provides audio feedback on pose accuracy
        /// </summary>
        public void PlayAccuracyFeedback(double accuracy)
        {
            if (!isEnabled) return;

            if (accuracy > 90)
            {
                Console.WriteLine("[Audio: Excellent form!]");
            }
            else if (accuracy > 70)
            {
                Console.WriteLine("[Audio: Good form, minor adjustments needed]");
            }
            else if (accuracy > 50)
            {
                Console.WriteLine("[Audio: Try adjusting your position]");
            }
            else
            {
                Console.WriteLine("[Audio: Let's try a different pose]");
            }
        }

        /// <summary>
        /// Plays end of session summary
        /// </summary>
        public void PlaySessionSummary(int posesCompleted, double averageAccuracy)
        {
            if (!isEnabled) return;
            
            Console.WriteLine($"[Audio: Session complete! You completed {posesCompleted} poses with an average accuracy of {averageAccuracy:F1}%]");
        }
    }
}