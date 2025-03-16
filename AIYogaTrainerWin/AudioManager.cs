using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Manages audio feedback for the application
    /// </summary>
    public class AudioManager
    {
        private YogaAppSettings settings;
        private bool lastFeedbackState = false;
        
        /// <summary>
        /// Creates a new instance of the AudioManager
        /// </summary>
        public AudioManager(YogaAppSettings settings)
        {
            this.settings = settings;
        }
        
        /// <summary>
        /// Provides audio feedback based on pose accuracy
        /// </summary>
        public void ProvideFeedback(float accuracy, string poseName)
        {
            if (!settings.AudioFeedback)
            {
                return; // Audio feedback is disabled
            }
            
            bool shouldProvide = accuracy >= settings.AudioThreshold;
            
            // Only provide new feedback if state has changed
            if (shouldProvide != lastFeedbackState)
            {
                lastFeedbackState = shouldProvide;
                
                if (shouldProvide)
                {
                    // Success feedback
                    Console.Beep(1000, 200); // 1000Hz for 200ms
                    Task.Delay(100).Wait();
                    Console.Beep(1200, 300); // 1200Hz for 300ms
                    
                    // Simulate voice feedback
                    Console.WriteLine($"[AUDIO] Great job! {poseName} pose detected.");
                }
                else if (accuracy > 0)
                {
                    // Improvement needed feedback
                    Console.Beep(800, 200); // 800Hz for 200ms
                    
                    // Simulate voice feedback
                    Console.WriteLine($"[AUDIO] Getting closer to {poseName}. Keep adjusting your position.");
                }
            }
        }
        
        /// <summary>
        /// Announces a pose transition
        /// </summary>
        public void AnnouncePoseTransition(string fromPose, string toPose)
        {
            if (!settings.AudioFeedback)
            {
                return; // Audio feedback is disabled
            }
            
            // Transition sound
            Console.Beep(600, 150);
            Console.Beep(800, 150);
            Console.Beep(1000, 150);
            
            // Simulate voice feedback
            Console.WriteLine($"[AUDIO] Great job with {fromPose}! Moving to {toPose}.");
        }
        
        /// <summary>
        /// Announces the start of a training session
        /// </summary>
        public void AnnounceTrainingStart()
        {
            if (!settings.AudioFeedback)
            {
                return; // Audio feedback is disabled
            }
            
            // Startup sound
            Console.Beep(500, 200);
            Console.Beep(700, 200);
            Console.Beep(900, 200);
            
            // Simulate voice feedback
            Console.WriteLine("[AUDIO] Training session started. Get ready for your first pose!");
        }
        
        /// <summary>
        /// Announces the end of a training session
        /// </summary>
        public void AnnounceTrainingComplete()
        {
            if (!settings.AudioFeedback)
            {
                return; // Audio feedback is disabled
            }
            
            // Completion sound
            Console.Beep(900, 200);
            Console.Beep(700, 200);
            Console.Beep(500, 200);
            Task.Delay(300).Wait();
            Console.Beep(1200, 500);
            
            // Simulate voice feedback
            Console.WriteLine("[AUDIO] Training session complete! Great job!");
        }
    }
}