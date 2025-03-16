using System;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AIYogaTrainerWin
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting AI Yoga Trainer...");
            var app = new YogaTrainerApp();
            
            // Check for quick setup argument
            if (args.Length > 0 && args[0] == "--quick-setup")
            {
                app.QuickSetup();
            }
            
            app.Run();
        }
    }

    class YogaTrainerApp
    {
        private YogaAppSettings settings;
        private TrainingStats stats;
        private PoseManager poseManager;
        private ImageManager imageManager;
        private AudioManager audioManager;
        private AdvancedPoseManager advancedPoseManager;
        private SkeletonVisualizer skeletonVisualizer;
        private bool isRunning = false;
        private CancellationTokenSource? cancellationTokenSource;

        public YogaTrainerApp()
        {
            // Load settings first
            LoadSettings();
            
            // Initialize components
            imageManager = new ImageManager(settings);
            audioManager = new AudioManager(settings);
            skeletonVisualizer = new SkeletonVisualizer();
            
            // Then initialize pose managers with settings
            poseManager = new PoseManager(settings);
            advancedPoseManager = new AdvancedPoseManager(settings, imageManager, audioManager);
            
            // Load stats
            LoadStats();
        }

        /// <summary>
        /// Quick setup for demo purposes - sets default values for immediate testing
        /// </summary>
        public void QuickSetup()
        {
            // Set up model URL if not already set
            if (string.IsNullOrEmpty(settings.ModelUrl))
            {
                settings.ModelUrl = "https://teachablemachine.withgoogle.com/models/yoga-model";
                Console.WriteLine("Setting model URL to default value");
            }
            
            // Set up pose names if they are default values
            if (settings.Pose1Name == "Pose 1") settings.Pose1Name = "Mountain Pose";
            if (settings.Pose2Name == "Pose 2") settings.Pose2Name = "Warrior Pose";
            if (settings.Pose3Name == "Pose 3") settings.Pose3Name = "Tree Pose";
            
            // Make sure poses 1-3 are active
            settings.Pose1Active = true;
            settings.Pose2Active = true;
            settings.Pose3Active = true;
            
            // Save settings
            SaveSettings();
            Console.WriteLine("Quick setup completed!");
        }

        public void Run()
        {
            bool exitApp = false;

            while (!exitApp)
            {
                Console.Clear();
                PrintHeader("AI YOGA TRAINER");
                
                // Show quick setup message if needed
                if (string.IsNullOrEmpty(settings.ModelUrl))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠️ No model URL set - please configure in Settings first");
                    Console.WriteLine("   or type 'Q' for quick setup");
                    Console.ResetColor();
                    Console.WriteLine();
                }
                
                Console.WriteLine("1. START TRAINING");
                Console.WriteLine("2. SETTINGS");
                Console.WriteLine("3. VIEW RESULTS");
                Console.WriteLine("4. EXIT");
                Console.WriteLine();
                Console.Write("Select an option (1-4): ");

                var key = Console.ReadKey(true);
                
                // Quick setup shortcut
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    QuickSetup();
                    Console.WriteLine("\nQuick setup completed. Press any key to continue...");
                    Console.ReadKey(true);
                    continue;
                }
                
                switch (key.KeyChar)
                {
                    case '1':
                        StartTraining();
                        break;
                    case '2':
                        ShowSettings();
                        break;
                    case '3':
                        ShowResults();
                        break;
                    case '4':
                        exitApp = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFile = "settings.json";
                if (File.Exists(settingsFile))
                {
                    string json = File.ReadAllText(settingsFile);
                    settings = JsonSerializer.Deserialize<YogaAppSettings>(json) ?? new YogaAppSettings();
                }
                else
                {
                    // Create default settings
                    settings = new YogaAppSettings();
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                settings = new YogaAppSettings();
            }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("settings.json", json);
                
                // Update pose manager with new settings
                if (poseManager != null)
                {
                    poseManager.UpdatePoseSettings(settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        private void LoadStats()
        {
            try
            {
                string statsFile = "training_stats.json";
                if (File.Exists(statsFile))
                {
                    string json = File.ReadAllText(statsFile);
                    stats = JsonSerializer.Deserialize<TrainingStats>(json) ?? new TrainingStats();
                }
                else
                {
                    // Create default stats
                    stats = new TrainingStats
                    {
                        TotalSessions = 0,
                        AverageAccuracy = 0,
                        BestPose = "",
                        LastTrainingDate = DateTime.MinValue,
                        PosePerformance = new Dictionary<string, PosePerformanceData>()
                    };
                    SaveStats();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading stats: {ex.Message}");
                stats = new TrainingStats
                {
                    TotalSessions = 0,
                    AverageAccuracy = 0,
                    BestPose = "",
                    LastTrainingDate = DateTime.MinValue,
                    PosePerformance = new Dictionary<string, PosePerformanceData>()
                };
            }
        }

        private void SaveStats()
        {
            try
            {
                string json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("training_stats.json", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving stats: {ex.Message}");
            }
        }

        private void StartTraining()
        {
            Console.Clear();
            PrintHeader("TRAINING SESSION - INTERACTIVE PROTOTYPE MODE");

            if (string.IsNullOrEmpty(settings.ModelUrl))
            {
                Console.WriteLine("You need to set up a model URL in settings first.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            
            Console.WriteLine($"🔗 Connected to model: {settings.ModelUrl}");
            Console.WriteLine($"⚙️ Detection threshold: {settings.DetectionThreshold}%");
            Console.WriteLine($"⏱️ Hold time: {settings.HoldTime} seconds");
            Console.WriteLine($"🔊 Audio feedback: {(settings.AudioFeedback ? "On" : "Off")}");
            
            // List active poses
            var activePoses = poseManager.GetActivePoses();
            Console.WriteLine("\n🧘 Active poses:");
            foreach (var pose in activePoses)
            {
                Console.WriteLine($"- {pose.Name}");
            }
            
            Console.WriteLine("\n📷 Preparing camera...");
            Thread.Sleep(1000);
            
            Console.WriteLine("✅ Camera ready! Starting pose detection...");
            
            // Use interactive testing mode instead of automated simulation
            InteractiveTrainingMode();
            
            // Update stats
            stats.TotalSessions++;
            stats.LastTrainingDate = DateTime.Now;
            SaveStats();
            
            isRunning = false;
            Console.WriteLine("\nSession ended. Press any key to continue...");
            Console.ReadKey(true);
        }
        
        private void InteractiveTrainingMode()
        {
            Dictionary<string, float> poseAccuracies = new Dictionary<string, float>();
            string bestPoseName = "";
            float bestPoseAccuracy = 0;
            int totalFrames = 0;
            float sessionAccuracy = 0;
            bool exitTraining = false;
            
            // Set initial pose
            advancedPoseManager.SetCurrentPose(1); // Start with pose 1
            
            // Display quick start options
            Console.Clear();
            PrintHeader("INTERACTIVE TRAINING MODE - SIMPLE START");
            Console.WriteLine("Do you want a simple or advanced training interface?");
            Console.WriteLine();
            Console.WriteLine("1. 🌟 SIMPLE TEST - Quick one-click pose testing");
            Console.WriteLine("2. 🔍 ADVANCED - Full testing options interface");
            Console.WriteLine();
            Console.Write("Select mode (1-2): ");
            
            var modeKey = Console.ReadKey(true);
            Console.WriteLine(modeKey.KeyChar);
            
            // Quick demo mode with minimal steps
            if (modeKey.KeyChar == '1')
            {
                // Quick test for all poses
                QuickPoseTestingMode();
                return;
            }
            
            // Regular advanced mode
            Console.WriteLine("\n📊 INTERACTIVE TRAINING MODE 📊");
            Console.WriteLine("This prototype allows you to test the pose detection system");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
            
            while (!exitTraining && !cancellationTokenSource.IsCancellationRequested)
            {
                Console.Clear();
                PrintHeader("INTERACTIVE TRAINING - PROTOTYPE");
                
                int currentPose = advancedPoseManager.CurrentPoseNumber;
                string currentPoseName = GetPoseName(currentPose);
                
                Console.WriteLine($"🎯 Current target pose: {currentPoseName}");
                Console.WriteLine($"🧘 Current active pose sequence: 1→2→3→1");
                Console.WriteLine($"📈 Session accuracy: {sessionAccuracy:F1}%");
                Console.WriteLine();
                
                Console.WriteLine("TESTING OPTIONS:");
                Console.WriteLine("1. 🔄 Simulate successful pose detection (Random accuracy)");
                Console.WriteLine("2. ⭐ Simulate perfect pose match (100% accuracy)");
                Console.WriteLine("3. ❌ Simulate failed pose detection (Below threshold)");
                Console.WriteLine("4. 🔄 Manually progress to next pose");
                Console.WriteLine("5. 📊 Show current session statistics");
                Console.WriteLine("6. 🔲 View skeleton comparison visualization");
                Console.WriteLine("7. 🔚 End training session");
                Console.WriteLine();
                Console.Write("Select option (1-7): ");
                
                var key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);
                
                Random random = new Random();
                float accuracy = 0;
                PoseModel detectedPose;
                
                switch (key.KeyChar)
                {
                    case '1': // Random successful pose
                        accuracy = random.Next((int)settings.DetectionThreshold, 100);
                        Console.WriteLine($"\n[Simulating pose detection with {accuracy:F1}% accuracy]");
                        
                        detectedPose = new PoseModel(currentPoseName);
                        detectedPose.UpdateConfidence(accuracy);
                        
                        ProcessDetectedPose(detectedPose, ref totalFrames, ref sessionAccuracy, ref poseAccuracies, ref bestPoseName, ref bestPoseAccuracy);
                        break;
                        
                    case '2': // Perfect match
                        accuracy = 100;
                        Console.WriteLine("\n[Simulating perfect pose match with 100% accuracy]");
                        
                        detectedPose = new PoseModel(currentPoseName);
                        detectedPose.UpdateConfidence(accuracy);
                        
                        ProcessDetectedPose(detectedPose, ref totalFrames, ref sessionAccuracy, ref poseAccuracies, ref bestPoseName, ref bestPoseAccuracy);
                        break;
                        
                    case '3': // Failed detection
                        accuracy = random.Next(20, (int)settings.DetectionThreshold - 1);
                        Console.WriteLine($"\n[Simulating failed pose detection with {accuracy:F1}% accuracy]");
                        Console.WriteLine($"No pose detected (below {settings.DetectionThreshold}% threshold)");
                        Console.WriteLine("Try adjusting your position to match one of the trained poses");
                        
                        totalFrames++;
                        sessionAccuracy = ((sessionAccuracy * (totalFrames - 1)) + accuracy) / totalFrames;
                        break;
                        
                    case '4': // Manual progression
                        int nextPose = (currentPose % 3) + 1;
                        string nextPoseName = GetPoseName(nextPose);
                        Console.WriteLine($"\nManually progressing from {currentPoseName} to {nextPoseName}");
                        advancedPoseManager.SetCurrentPose(nextPose);
                        break;
                        
                    case '5': // Show statistics
                        Console.WriteLine("\n📊 SESSION STATISTICS 📊");
                        Console.WriteLine($"Total frames processed: {totalFrames}");
                        Console.WriteLine($"Overall session accuracy: {sessionAccuracy:F1}%");
                        
                        if (!string.IsNullOrEmpty(bestPoseName))
                        {
                            Console.WriteLine($"Best pose: {bestPoseName} ({bestPoseAccuracy:F1}%)");
                        }
                        
                        if (poseAccuracies.Count > 0)
                        {
                            Console.WriteLine("\nIndividual Pose Performance:");
                            foreach (var pose in poseAccuracies)
                            {
                                Console.WriteLine($"- {pose.Key}: {pose.Value:F1}% accuracy");
                            }
                        }
                        break;
                        
                    case '6': // Skeleton visualization
                        SimulateSkeletonComparison(currentPose);
                        break;
                        
                    case '7': // End session
                        exitTraining = true;
                        Console.WriteLine("\nEnding training session...");
                        break;
                        
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
                
                if (!exitTraining && key.KeyChar != '7')
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
        
        /// <summary>
        /// Simple quick test mode for all poses with minimal steps
        /// </summary>
        private void QuickPoseTestingMode()
        {
            Console.Clear();
            PrintHeader("QUICK POSE TESTING");
            Console.WriteLine("This mode will test all active poses in sequence with a single keystroke");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);
            
            // Test each active pose
            for (int poseNumber = 1; poseNumber <= 3; poseNumber++)
            {
                if (GetPoseActive(poseNumber))
                {
                    // Set current pose
                    advancedPoseManager.SetCurrentPose(poseNumber);
                    string poseName = GetPoseName(poseNumber);
                    
                    Console.Clear();
                    PrintHeader("QUICK POSE TEST");
                    Console.WriteLine($"🧘 Testing: {poseName} (Pose {poseNumber}/3)");
                    Console.WriteLine();
                    
                    // Generate reference and detected skeletons
                    var referenceSkeleton = imageManager.GetReferenceSkeleton(poseNumber);
                    
                    // Create a reasonably accurate detection for demo purposes
                    Random random = new Random();
                    int accuracy = random.Next(60, 95);
                    var detectedSkeleton = imageManager.SimulateDetectedSkeleton(poseNumber, accuracy);
                    
                    // Quick skeleton visualization 
                    VisualizeSkeletons(referenceSkeleton, detectedSkeleton);
                    
                    // Compare the skeletons
                    float comparisonResult = imageManager.CompareSkeletons(referenceSkeleton, detectedSkeleton, settings.DetectionThreshold);
                    
                    // Get keypoint match data
                    var keypointMatchData = imageManager.GetKeypointMatchData();
                    
                    // Display results
                    Console.WriteLine($"\n📊 Match accuracy: {comparisonResult:F1}%");
                    DrawProgressBar(comparisonResult, 100, 40, ConsoleColor.Cyan);
                    
                    if (comparisonResult >= settings.DetectionThreshold)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ Pose match successful!");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("⚠️ Pose match below threshold");
                        Console.ResetColor();
                    }
                    
                    // Get personalized pose feedback
                    var poseModel = poseManager.GetPose($"Pose{poseNumber}");
                    var personalizedSuggestions = imageManager.GeneratePersonalizedSuggestions(poseModel);
                    
                    // Display personalized feedback
                    Console.WriteLine("\n🔍 PERSONALIZED GUIDANCE:");
                    foreach (var suggestion in personalizedSuggestions)
                    {
                        // Color code based on severity
                        if (suggestion.Contains("[Critical]"))
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else if (suggestion.Contains("[Major]"))
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                        }
                        else if (suggestion.Contains("[Minor]"))
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        
                        Console.WriteLine($"• {suggestion}");
                        Console.ResetColor();
                    }
                    
                    // Show quick progress so user understands hold time
                    Console.WriteLine("\nSimulating pose hold:");
                    for (int i = 0; i <= settings.HoldTime; i++)
                    {
                        int percent = (i * 100) / settings.HoldTime;
                        Console.Write($"\rHolding pose: {percent}% complete ({i}/{settings.HoldTime}s)");
                        Thread.Sleep(300); // Quick simulation for demo
                    }
                    
                    // Update and display session statistics
                    if (comparisonResult >= settings.DetectionThreshold)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n\n✨ SUCCESS! You've completed the {poseModel.Name} pose.");
                        Console.ResetColor();
                    }
                    
                    Console.WriteLine("\n\nPress any key to continue to next pose...");
                    Console.ReadKey(true);
                }
            }
            
            // Final pose comparison screen
            Console.Clear();
            PrintHeader("ENHANCED POSE COMPARISON");
            Console.WriteLine("Select which pose to analyze in detail:");
            Console.WriteLine("1. Pose 1: " + settings.Pose1Name);
            Console.WriteLine("2. Pose 2: " + settings.Pose2Name);
            Console.WriteLine("3. Pose 3: " + settings.Pose3Name);
            Console.WriteLine();
            Console.Write("Select pose (1-3): ");
            
            var key = Console.ReadKey(true);
            Console.WriteLine(key.KeyChar);
            
            int selectedPose;
            if (key.KeyChar >= '1' && key.KeyChar <= '3')
            {
                selectedPose = key.KeyChar - '0';
            }
            else
            {
                selectedPose = 1; // Default to pose 1
            }
            
            // Show the enhanced visualization
            SimulateSkeletonComparison(selectedPose);
            
            Console.WriteLine("\nQuick test completed!");
        }
        
        private void ProcessDetectedPose(PoseModel detectedPose, ref int totalFrames, ref float sessionAccuracy, 
                                  ref Dictionary<string, float> poseAccuracies, ref string bestPoseName, ref float bestPoseAccuracy)
        {
            totalFrames++;
            Console.WriteLine($"Detected pose: {detectedPose.Name} ({detectedPose.Confidence:F1}% confidence)");
            
            // Track pose accuracy
            if (!poseAccuracies.ContainsKey(detectedPose.Name))
            {
                poseAccuracies[detectedPose.Name] = detectedPose.Confidence;
            }
            else
            {
                poseAccuracies[detectedPose.Name] = (poseAccuracies[detectedPose.Name] + detectedPose.Confidence) / 2;
            }
            
            // Track best pose
            if (poseAccuracies[detectedPose.Name] > bestPoseAccuracy)
            {
                bestPoseName = detectedPose.Name;
                bestPoseAccuracy = poseAccuracies[detectedPose.Name];
            }
            
            int currentPose = advancedPoseManager.CurrentPoseNumber;
            string currentPoseName = GetPoseName(currentPose);
            
            // Check if pose matches current target pose for hold time calculations
            if (detectedPose.Name == currentPoseName)
            {
                // Update pose hold timing
                if (advancedPoseManager.UpdatePoseHold(detectedPose.Confidence))
                {
                    Console.WriteLine($"Successfully held the {detectedPose.Name} pose for {settings.HoldTime} seconds!");
                    Console.WriteLine("Moving to next pose...");
                    
                    // Progress to next pose
                    int nextPose = (currentPose % 3) + 1; // 1->2->3->1
                    string nextPoseName = GetPoseName(nextPose);
                    Console.WriteLine($"Transitioning from {currentPoseName} to {nextPoseName}");
                    advancedPoseManager.SetCurrentPose(nextPose);
                }
                else if (advancedPoseManager.IsCurrentPoseActive)
                {
                    int remainingSeconds = advancedPoseManager.GetRemainingHoldTime();
                    int holdPercentage = advancedPoseManager.GetHoldTimePercentage();
                    
                    if (holdPercentage > 0)
                    {
                        Console.WriteLine($"Holding pose... {holdPercentage}% complete ({remainingSeconds}s remaining)");
                    }
                }
            }
            
            // Provide audio feedback if enabled
            if (settings.AudioFeedback)
            {
                Console.WriteLine("[Audio feedback provided]");
                audioManager.ProvideFeedback(detectedPose.Confidence, detectedPose.Name);
            }
            
            // Update session accuracy
            sessionAccuracy = ((sessionAccuracy * (totalFrames - 1)) + detectedPose.Confidence) / totalFrames;
        }

        private async Task SimulateTrainingSession(CancellationToken cancellationToken)
        {
            try
            {
                // Track session stats
                Dictionary<string, float> poseAccuracies = new Dictionary<string, float>();
                string bestPoseName = "";
                float bestPoseAccuracy = 0;
                int totalFrames = 0;
                float sessionAccuracy = 0;
                
                Console.WriteLine("\nStarting pose detection...");
                
                for (int i = 0; i < 30 && !cancellationToken.IsCancellationRequested; i++)
                {
                    // Simulate camera frame processing
                    Console.WriteLine("\n[Processing frame]");
                    
                    // Simulate pose detection
                    PoseModel detectedPose = poseManager.SimulateDetection(settings.DetectionThreshold);
                    totalFrames++;
                    
                    if (detectedPose != null)
                    {
                        // Update pose accuracies tracking
                        if (!poseAccuracies.ContainsKey(detectedPose.Name))
                        {
                            poseAccuracies[detectedPose.Name] = 0;
                        }
                        
                        // Weighted average for this pose
                        float currentAccuracy = poseAccuracies[detectedPose.Name];
                        int poseCount = (int)(currentAccuracy * 10); // Simple way to track frequency
                        poseAccuracies[detectedPose.Name] = ((currentAccuracy * poseCount) + detectedPose.Confidence) / (poseCount + 1);
                        
                        // Check if this is the best pose
                        if (poseAccuracies[detectedPose.Name] > bestPoseAccuracy)
                        {
                            bestPoseAccuracy = poseAccuracies[detectedPose.Name];
                            bestPoseName = detectedPose.Name;
                        }
                        
                        // Display the detected pose and confidence
                        Console.WriteLine($"Detected: {detectedPose.Name} ({detectedPose.Confidence:F1}%)");
                        
                        // Show adjustment instruction based on skeleton comparison
                        string adjustment = detectedPose.GetRandomAdjustment();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Adjustment: {adjustment}");
                        Console.WriteLine($"Comparing skeleton from model with camera view skeleton...");
                        Console.ResetColor();
                        
                        // Check if pose is held with sufficient accuracy
                        if (detectedPose.Confidence > settings.AudioThreshold)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"Pose accuracy exceeds threshold! ({detectedPose.Confidence:F1}% > {settings.AudioThreshold}%)");
                            Console.ResetColor();
                            
                            // Audio feedback
                            if (settings.AudioFeedback)
                            {
                                Console.WriteLine("[Audio success cue played]");
                            }
                            
                            // Automatic pose progression
                            if (settings.AutoPoseProgression)
                            {
                                int nextPose = 1;
                                
                                // Progress from Pose 1 -> Pose 2 -> Pose 3 -> Pose 1
                                if (detectedPose.Name == settings.Pose1Name)
                                {
                                    Console.WriteLine($"Great! Now transitioning to {settings.Pose2Name}");
                                    nextPose = 2;
                                }
                                else if (detectedPose.Name == settings.Pose2Name)
                                {
                                    Console.WriteLine($"Excellent! Now transitioning to {settings.Pose3Name}");
                                    nextPose = 3;
                                }
                                else if (detectedPose.Name == settings.Pose3Name)
                                {
                                    Console.WriteLine($"Perfect! Now returning to {settings.Pose1Name}");
                                    nextPose = 1;
                                }
                                
                                // Set the new pose in the advanced manager
                                advancedPoseManager.SetCurrentPose(nextPose);
                                
                                // Compare skeleton data between model and camera view
                                Console.WriteLine("\n[SKELETON COMPARISON VISUALIZATION]");
                                
                                // Get reference skeleton from the selected pose
                                var referenceSkeleton = imageManager.GetReferenceSkeleton(nextPose);
                                
                                // Simulate detected skeleton with varying accuracy
                                Random random = new Random();
                                int accuracy = random.Next(30, 100); // 30-99% accuracy
                                var detectedSkeleton = imageManager.SimulateDetectedSkeleton(nextPose, accuracy);
                                
                                // Compare the skeletons
                                float comparisonResult = imageManager.CompareSkeletons(referenceSkeleton, detectedSkeleton, settings.DetectionThreshold);
                                
                                Console.WriteLine($"Reference skeleton keypoints: {referenceSkeleton.Keypoints.Count}");
                                Console.WriteLine($"Detected skeleton keypoints: {detectedSkeleton.Keypoints.Count}");
                                Console.WriteLine($"Comparison result: {comparisonResult:F1}% match");
                                
                                // Provide audio feedback based on result
                                audioManager.ProvideFeedback(comparisonResult, GetPoseName(nextPose));
                            }
                        }
                        else
                        {
                            // Regular audio feedback
                            if (settings.AudioFeedback)
                            {
                                Console.WriteLine("[Audio guidance cue played]");
                            }
                        }
                        
                        // Calculate session accuracy
                        sessionAccuracy = ((sessionAccuracy * (totalFrames - 1)) + detectedPose.Confidence) / totalFrames;
                    }
                    else
                    {
                        Console.WriteLine($"No pose detected (below {settings.DetectionThreshold}% threshold)");
                        Console.WriteLine("Try adjusting your position to match one of the trained poses");
                    }
                    
                    // Display overall session stats
                    Console.WriteLine($"Session accuracy: {sessionAccuracy:F1}%");
                    
                    // Wait for next frame
                    await Task.Delay(1000, cancellationToken);
                }
                
                // Update overall stats
                if (!string.IsNullOrEmpty(bestPoseName))
                {
                    stats.BestPose = bestPoseName;
                    stats.AverageAccuracy = ((stats.AverageAccuracy * (stats.TotalSessions)) + sessionAccuracy) / (stats.TotalSessions + 1);
                    
                    // Update individual pose stats
                    foreach (var pose in poseAccuracies)
                    {
                        if (!stats.PosePerformance.ContainsKey(pose.Key))
                        {
                            stats.PosePerformance[pose.Key] = new PosePerformanceData();
                        }
                        
                        var poseStats = stats.PosePerformance[pose.Key];
                        poseStats.TotalAttempts++;
                        poseStats.AverageAccuracy = ((poseStats.AverageAccuracy * (poseStats.TotalAttempts - 1)) + pose.Value) / poseStats.TotalAttempts;
                    }
                }
                
                Console.WriteLine("\nTraining session complete!");
                Console.WriteLine($"Overall session accuracy: {sessionAccuracy:F1}%");
                
                if (!string.IsNullOrEmpty(bestPoseName))
                {
                    Console.WriteLine($"Your best pose was: {bestPoseName} ({bestPoseAccuracy:F1}%)");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nTraining session cancelled.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError during training: {ex.Message}");
            }
        }

        private void ShowSettings()
        {
            bool exitSettings = false;
            
            while (!exitSettings)
            {
                Console.Clear();
                PrintHeader("SETTINGS");
                
                Console.WriteLine($"1. 🔗 Model URL: {settings.ModelUrl}");
                Console.WriteLine($"2. 🔊 Audio Feedback: {(settings.AudioFeedback ? "On" : "Off")}");
                Console.WriteLine($"3. 🎚️ Audio Threshold: {settings.AudioThreshold}%");
                Console.WriteLine($"4. ⏱️ Hold Time: {settings.HoldTime} seconds");
                Console.WriteLine($"5. 📊 Detection Threshold: {settings.DetectionThreshold}%");
                Console.WriteLine($"6. 🔄 Auto Pose Progression: {(settings.AutoPoseProgression ? "On" : "Off")}");
                Console.WriteLine("7. 🧘 Manage Poses");
                Console.WriteLine("8. 💾 Save and Return");
                Console.WriteLine();
                Console.Write("Select an option (1-8): ");
                
                var key = Console.ReadKey(true);
                
                switch (key.KeyChar)
                {
                    case '1':
                        EditModelUrl();
                        break;
                    case '2':
                        settings.AudioFeedback = !settings.AudioFeedback;
                        break;
                    case '3':
                        EditAudioThreshold();
                        break;
                    case '4':
                        EditHoldTime();
                        break;
                    case '5':
                        EditDetectionThreshold();
                        break;
                    case '6':
                        settings.AutoPoseProgression = !settings.AutoPoseProgression;
                        break;
                    case '7':
                        ManagePoses();
                        break;
                    case '8':
                        SaveSettings();
                        exitSettings = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press any key to continue...");
                        Console.ReadKey(true);
                        break;
                }
            }
        }
        
        private void EditAudioThreshold()
        {
            Console.Write("\nEnter Audio Threshold percentage (1-100): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int threshold) && threshold >= 1 && threshold <= 100)
            {
                settings.AudioThreshold = threshold;
            }
            else
            {
                Console.WriteLine("Invalid input. Threshold must be between 1 and 100.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        private void EditModelUrl()
        {
            Console.Write("\nEnter Model URL: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                settings.ModelUrl = input;
            }
        }

        private void EditHoldTime()
        {
            Console.Write("\nEnter Hold Time in seconds (1-3): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int holdTime) && holdTime >= 1 && holdTime <= 3)
            {
                settings.HoldTime = holdTime;
            }
            else
            {
                Console.WriteLine("Invalid input. Hold time must be between 1 and 3.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
        
        private void EditDetectionThreshold()
        {
            Console.Write("\nEnter Detection Threshold percentage (1-100): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int threshold) && threshold >= 1 && threshold <= 100)
            {
                settings.DetectionThreshold = threshold;
            }
            else
            {
                Console.WriteLine("Invalid input. Threshold must be between 1 and 100.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
        
        private void ManagePoses()
        {
            bool exitPoseMenu = false;
            
            while (!exitPoseMenu)
            {
                Console.Clear();
                PrintHeader("MANAGE POSES");
                
                // Show pose status and whether they have images
                string pose1ImageIcon = string.IsNullOrEmpty(settings.Pose1ImagePath) ? "📷" : "🖼️";
                string pose2ImageIcon = string.IsNullOrEmpty(settings.Pose2ImagePath) ? "📷" : "🖼️";
                string pose3ImageIcon = string.IsNullOrEmpty(settings.Pose3ImagePath) ? "📷" : "🖼️";
                
                Console.WriteLine($"1. {pose1ImageIcon} Pose 1: {settings.Pose1Name} [{(settings.Pose1Active ? "Active" : "Inactive")}]");
                Console.WriteLine($"2. {pose2ImageIcon} Pose 2: {settings.Pose2Name} [{(settings.Pose2Active ? "Active" : "Inactive")}]");
                Console.WriteLine($"3. {pose3ImageIcon} Pose 3: {settings.Pose3Name} [{(settings.Pose3Active ? "Active" : "Inactive")}]");
                Console.WriteLine($"4. Pose 4: {settings.Pose4Name} [{(settings.Pose4Active ? "Active" : "Inactive")}]");
                Console.WriteLine($"5. Pose 5: {settings.Pose5Name} [{(settings.Pose5Active ? "Active" : "Inactive")}]");
                Console.WriteLine($"6. Pose 6: {settings.Pose6Name} [{(settings.Pose6Active ? "Active" : "Inactive")}]");
                Console.WriteLine("7. 🔙 Return to Settings");
                Console.WriteLine();
                Console.Write("Select a pose to edit (1-7): ");
                
                var key = Console.ReadKey(true);
                
                if (key.KeyChar >= '1' && key.KeyChar <= '6')
                {
                    int poseNumber = key.KeyChar - '0';
                    EditPose(poseNumber);
                }
                else if (key.KeyChar == '7')
                {
                    exitPoseMenu = true;
                }
                else
                {
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
        
        private void EditPose(int poseNumber)
        {
            Console.Clear();
            PrintHeader($"EDIT POSE {poseNumber}");
            
            string poseName = GetPoseName(poseNumber);
            bool poseActive = GetPoseActive(poseNumber);
            string imagePath = settings.GetPoseImagePath(poseNumber);
            string imageStatus = string.IsNullOrEmpty(imagePath) ? "📷 Not set" : $"🖼️ {imagePath}";
            
            Console.WriteLine($"Current name: {poseName}");
            Console.WriteLine($"Status: {(poseActive ? "✅ Active" : "❌ Inactive")}");
            Console.WriteLine($"Image: {imageStatus}");
            Console.WriteLine();
            Console.WriteLine("1. ✏️ Edit Name");
            Console.WriteLine("2. 🔄 Toggle Active Status");
            Console.WriteLine("3. 📤 Upload Image (Set Image Path)");
            Console.WriteLine("4. 🔙 Return");
            Console.WriteLine();
            Console.Write("Select an option (1-4): ");
            
            var key = Console.ReadKey(true);
            
            switch (key.KeyChar)
            {
                case '1':
                    EditPoseName(poseNumber);
                    break;
                case '2':
                    TogglePoseActive(poseNumber);
                    break;
                case '3':
                    UploadPoseImage(poseNumber);
                    break;
                case '4':
                    // Return to pose menu
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey(true);
                    break;
            }
        }
        
        private void UploadPoseImage(int poseNumber)
        {
            Console.Clear();
            PrintHeader($"UPLOAD IMAGE FOR POSE {poseNumber}");
            
            Console.WriteLine("In a real application, this would open a file dialog.");
            Console.WriteLine("For this simulation, please enter a file path:");
            Console.Write("\nEnter image file path: ");
            
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                settings.SetPoseImagePath(poseNumber, input);
                Console.WriteLine($"\nImage path set to: {input}");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }
        
        /// <summary>
        /// Simulates comparison between skeletons from camera and reference
        /// </summary>
        private void SimulateSkeletonComparison(int poseNumber)
        {
            Console.Clear();
            PrintHeader("ENHANCED SKELETON COMPARISON");
            
            string poseName = GetPoseName(poseNumber);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"🧘 Analyzing pose: {poseName}");
            Console.ResetColor();
            Console.WriteLine();
            
            // Get reference skeleton from the selected pose
            var referenceSkeleton = imageManager.GetReferenceSkeleton(poseNumber);
            
            // Simulation options for testing
            Console.WriteLine("Select simulation accuracy level:");
            Console.WriteLine("1. Low accuracy (30-50%)");
            Console.WriteLine("2. Medium accuracy (50-75%)");
            Console.WriteLine("3. High accuracy (75-95%)");
            Console.WriteLine("4. Perfect match (95-100%)");
            Console.WriteLine("5. Custom accuracy");
            Console.Write("\nSelect option (1-5): ");
            
            var key = Console.ReadKey(true);
            Console.WriteLine(key.KeyChar);
            
            // Set accuracy based on selection
            Random random = new Random();
            int accuracy;
            switch (key.KeyChar)
            {
                case '1':
                    accuracy = random.Next(30, 51);
                    break;
                case '2':
                    accuracy = random.Next(50, 76);
                    break;
                case '3':
                    accuracy = random.Next(75, 96);
                    break;
                case '4':
                    accuracy = random.Next(95, 101);
                    break;
                case '5':
                    Console.Write("\nEnter custom accuracy (0-100): ");
                    string input = Console.ReadLine() ?? "75";
                    if (!int.TryParse(input, out accuracy))
                        accuracy = 75;
                    accuracy = Math.Clamp(accuracy, 0, 100);
                    break;
                default:
                    accuracy = random.Next(50, 90);
                    break;
            }
            
            Console.Clear();
            
            // Simulate detected skeleton with specified accuracy
            var detectedSkeleton = imageManager.SimulateDetectedSkeleton(poseNumber, accuracy);
            
            // Compare the skeletons with enhanced algorithm
            float comparisonResult = imageManager.CompareSkeletons(referenceSkeleton, detectedSkeleton, settings.DetectionThreshold);
            
            // Get detailed keypoint match data
            var keypointMatchData = imageManager.GetKeypointMatchData();
            
            // Convert keypointMatchData to the format needed by SkeletonVisualizer
            Dictionary<string, double> matchPercentages = new Dictionary<string, double>();
            foreach (var kvp in keypointMatchData)
            {
                matchPercentages[ConvertKeypointName(kvp.Key)] = kvp.Value;
            }
            
            // Create PoseKeypoints objects for reference and detected skeletons
            PoseKeypoints referencePoseKeypoints = ConvertSkeletonToPoseKeypoints(referenceSkeleton);
            PoseKeypoints detectedPoseKeypoints = ConvertSkeletonToPoseKeypoints(detectedSkeleton);
            
            // Use the enhanced visualization system
            skeletonVisualizer.DrawSkeletonComparison(referencePoseKeypoints, detectedPoseKeypoints, matchPercentages);
            
            // Overall comparison results
            Console.WriteLine("\n📊 COMPARISON SUMMARY:");
            
            // Progress bar for overall match
            DrawProgressBar(comparisonResult, 100, 40, ConsoleColor.Cyan);
            
            Console.WriteLine($"🔹 Overall match: {comparisonResult:F1}%");
            Console.WriteLine($"🔹 Detection threshold: {settings.DetectionThreshold}%");
            Console.WriteLine($"🔹 Reference keypoints: {referenceSkeleton.Keypoints.Count}");
            Console.WriteLine($"🔹 Detected keypoints: {detectedSkeleton.Keypoints.Count}");
            
            // Map keypoint names to body parts for better user understanding
            Dictionary<string, string> keypointToBodyPart = new Dictionary<string, string>
            {
                {"nose", "Head position"},
                {"leftShoulder", "Left shoulder"},
                {"rightShoulder", "Right shoulder"},
                {"leftElbow", "Left elbow"},
                {"rightElbow", "Right elbow"},
                {"leftWrist", "Left wrist"},
                {"rightWrist", "Right wrist"},
                {"leftHip", "Left hip"},
                {"rightHip", "Right hip"},
                {"leftKnee", "Left knee"},
                {"rightKnee", "Right knee"},
                {"leftAnkle", "Left ankle"},
                {"rightAnkle", "Right ankle"}
            };
            
            // Sort keypoints by match percentage (worst matches first)
            var sortedKeypoints = keypointMatchData
                .OrderBy(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            
            // Overall match status
            Console.WriteLine();
            if (comparisonResult >= settings.DetectionThreshold)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Pose match is ABOVE threshold! Good job!");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("⚠️ Pose match is BELOW threshold. Adjustments needed.");
                Console.ResetColor();
            }
            
            // Provide intelligent suggestions for improvement based on lowest scores
            ProvideIntelligentSuggestions(sortedKeypoints, keypointToBodyPart);
            
            // Provide audio feedback based on result
            audioManager.ProvideFeedback(comparisonResult, poseName);
            Console.WriteLine();
            
            // Update pose hold status
            if (advancedPoseManager.UpdatePoseHold(comparisonResult))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🎉 Successfully held the {poseName} pose for {settings.HoldTime} seconds!");
                Console.ResetColor();
            }
            else if (advancedPoseManager.IsCurrentPoseActive)
            {
                int remainingSeconds = advancedPoseManager.GetRemainingHoldTime();
                int holdPercentage = advancedPoseManager.GetHoldTimePercentage();
                
                if (holdPercentage > 0)
                {
                    DrawProgressBar(holdPercentage, 100, 30, ConsoleColor.Cyan);
                    Console.WriteLine($"⏱️ Holding pose... {holdPercentage}% complete ({remainingSeconds}s remaining)");
                }
            }
            
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }
        
        /// <summary>
        /// Converts a technical keypoint name to a format used by PoseKeypoints
        /// </summary>
        private string ConvertKeypointName(string keypointName)
        {
            switch (keypointName.ToLower())
            {
                case "nose": return "Head";
                case "leftshoulder": return "LeftShoulder";
                case "rightshoulder": return "RightShoulder";
                case "leftelbow": return "LeftElbow";
                case "rightelbow": return "RightElbow";
                case "leftwrist": return "LeftWrist";
                case "rightwrist": return "RightWrist";
                case "lefthip": return "LeftHip";
                case "righthip": return "RightHip";
                case "leftknee": return "LeftKnee";
                case "rightknee": return "RightKnee";
                case "leftankle": return "LeftAnkle";
                case "rightankle": return "RightAnkle";
                default: return keypointName;
            }
        }
        
        /// <summary>
        /// Converts an ImageManager.Skeleton to PoseKeypoints for visualization
        /// </summary>
        private PoseKeypoints ConvertSkeletonToPoseKeypoints(ImageManager.Skeleton skeleton)
        {
            PoseKeypoints poseKeypoints = new PoseKeypoints();
            
            foreach (var keypoint in skeleton.Keypoints)
            {
                string name = keypoint.Name.ToLower();
                double x = keypoint.X;
                double y = keypoint.Y;
                
                switch (name)
                {
                    case "nose":
                        poseKeypoints.Head = (x, y);
                        break;
                    case "leftshoulder":
                        poseKeypoints.LeftShoulder = (x, y);
                        break;
                    case "rightshoulder":
                        poseKeypoints.RightShoulder = (x, y);
                        break;
                    case "leftelbow":
                        poseKeypoints.LeftElbow = (x, y);
                        break;
                    case "rightelbow":
                        poseKeypoints.RightElbow = (x, y);
                        break;
                    case "leftwrist":
                        poseKeypoints.LeftWrist = (x, y);
                        break;
                    case "rightwrist":
                        poseKeypoints.RightWrist = (x, y);
                        break;
                    case "lefthip":
                        poseKeypoints.LeftHip = (x, y);
                        break;
                    case "righthip":
                        poseKeypoints.RightHip = (x, y);
                        break;
                    case "leftknee":
                        poseKeypoints.LeftKnee = (x, y);
                        break;
                    case "rightknee":
                        poseKeypoints.RightKnee = (x, y);
                        break;
                    case "leftankle":
                        poseKeypoints.LeftAnkle = (x, y);
                        break;
                    case "rightankle":
                        poseKeypoints.RightAnkle = (x, y);
                        break;
                }
            }
            
            // Set neck position as midpoint between shoulders if not specified
            if (poseKeypoints.Neck == (0, 0) && poseKeypoints.LeftShoulder != (0, 0) && poseKeypoints.RightShoulder != (0, 0))
            {
                double neckX = (poseKeypoints.LeftShoulder.x + poseKeypoints.RightShoulder.x) / 2;
                double neckY = (poseKeypoints.LeftShoulder.y + poseKeypoints.RightShoulder.y) / 2;
                poseKeypoints.Neck = (neckX, neckY);
            }
            
            return poseKeypoints;
        }
        
        /// <summary>
        /// Visualizes reference and detected skeletons side by side with ASCII art
        /// </summary>
        private void VisualizeSkeletons(ImageManager.Skeleton reference, ImageManager.Skeleton detected)
        {
            int width = 30;
            int height = 15;
            char[,] refCanvas = new char[height, width];
            char[,] detCanvas = new char[height, width];
            char[,] overlapCanvas = new char[height, width];
            
            // Initialize canvases
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    refCanvas[y, x] = ' ';
                    detCanvas[y, x] = ' ';
                    overlapCanvas[y, x] = ' ';
                }
            }
            
            // Draw skeletons on canvases
            DrawSkeletonOnCanvas(reference, refCanvas, width, height);
            DrawSkeletonOnCanvas(detected, detCanvas, width, height);
            
            // Create overlap representation (enhanced visualization)
            CreateOverlapCanvas(reference, detected, overlapCanvas, width, height);
            
            // Print header
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("ENHANCED SKELETON VISUALIZATION - POSE ANALYSIS");
            Console.ResetColor();
            Console.WriteLine(new string('-', 90));
            
            Console.WriteLine("REFERENCE MODEL               DETECTED POSE                 OVERLAP ANALYSIS");
            Console.WriteLine(new string('-', 90));
            
            // Print all three canvases side by side
            for (int y = 0; y < height; y++)
            {
                // Reference skeleton
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("| ");
                for (int x = 0; x < width; x++)
                {
                    Console.Write(refCanvas[y, x]);
                }
                
                // Separator
                Console.Write(" | ");
                Console.ResetColor();
                
                // Detected skeleton
                for (int x = 0; x < width; x++)
                {
                    Console.Write(detCanvas[y, x]);
                }
                
                // Separator
                Console.Write(" | ");
                
                // Overlap canvas with color-coded insights
                for (int x = 0; x < width; x++)
                {
                    char c = overlapCanvas[y, x];
                    
                    if (c == 'M') // Match
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("●");
                        Console.ResetColor();
                    }
                    else if (c == 'R') // Reference only
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("◯");
                        Console.ResetColor();
                    }
                    else if (c == 'D') // Detected only
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.Write("◊");
                        Console.ResetColor();
                    }
                    else if (c == '+') // Connection line - match
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+");
                        Console.ResetColor();
                    }
                    else if (c == '-') // Connection line - mismatch
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write("-");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(c);
                    }
                }
                Console.WriteLine(" |");
            }
            
            Console.WriteLine(new string('-', 90));
            Console.WriteLine("Legend: ● Perfect match  ◯ Reference point  ◊ Detected point");
            Console.WriteLine("        + Matched connection  - Misaligned connection");
        }
        
        /// <summary>
        /// Creates an overlap canvas showing alignment between reference and detected skeletons
        /// </summary>
        private void CreateOverlapCanvas(ImageManager.Skeleton reference, ImageManager.Skeleton detected, char[,] canvas, int width, int height)
        {
            int canvasHeight = canvas.GetLength(0);
            int canvasWidth = canvas.GetLength(1);
            
            // Compare keypoints
            foreach (var kvp in reference.Keypoints)
            {
                string keypointName = kvp.Key;
                var refPoint = kvp.Value;
                
                int refX = (int)(refPoint.X * (canvasWidth - 1));
                int refY = (int)(refPoint.Y * (canvasHeight - 1));
                
                if (detected.Keypoints.ContainsKey(keypointName))
                {
                    var detPoint = detected.Keypoints[keypointName];
                    int detX = (int)(detPoint.X * (canvasWidth - 1));
                    int detY = (int)(detPoint.Y * (canvasHeight - 1));
                    
                    // Calculate distance between points
                    double distance = Math.Sqrt(Math.Pow(refX - detX, 2) + Math.Pow(refY - detY, 2));
                    
                    if (distance < 2) // Points are close enough to be considered a match
                    {
                        // Mark as a match
                        if (refX >= 0 && refX < canvasWidth && refY >= 0 && refY < canvasHeight)
                        {
                            canvas[refY, refX] = 'M';
                        }
                    }
                    else
                    {
                        // Mark reference point
                        if (refX >= 0 && refX < canvasWidth && refY >= 0 && refY < canvasHeight)
                        {
                            canvas[refY, refX] = 'R';
                        }
                        
                        // Mark detected point
                        if (detX >= 0 && detX < canvasWidth && detY >= 0 && detY < canvasHeight)
                        {
                            canvas[detY, detX] = 'D';
                        }
                    }
                }
                else
                {
                    // Reference point only
                    if (refX >= 0 && refX < canvasWidth && refY >= 0 && refY < canvasHeight)
                    {
                        canvas[refY, refX] = 'R';
                    }
                }
            }
            
            // Compare connections with color coding
            CompareConnectionsOnCanvas(reference, detected, canvas, "nose", "leftShoulder");
            CompareConnectionsOnCanvas(reference, detected, canvas, "nose", "rightShoulder");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftShoulder", "rightShoulder");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftShoulder", "leftElbow");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftElbow", "leftWrist");
            CompareConnectionsOnCanvas(reference, detected, canvas, "rightShoulder", "rightElbow");
            CompareConnectionsOnCanvas(reference, detected, canvas, "rightElbow", "rightWrist");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftShoulder", "leftHip");
            CompareConnectionsOnCanvas(reference, detected, canvas, "rightShoulder", "rightHip");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftHip", "rightHip");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftHip", "leftKnee");
            CompareConnectionsOnCanvas(reference, detected, canvas, "leftKnee", "leftAnkle");
            CompareConnectionsOnCanvas(reference, detected, canvas, "rightHip", "rightKnee");
            CompareConnectionsOnCanvas(reference, detected, canvas, "rightKnee", "rightAnkle");
        }
        
        /// <summary>
        /// Compares connections between reference and detected skeletons
        /// </summary>
        private void CompareConnectionsOnCanvas(ImageManager.Skeleton reference, ImageManager.Skeleton detected, char[,] canvas, string point1Name, string point2Name)
        {
            bool refHasConnection = reference.Keypoints.ContainsKey(point1Name) && reference.Keypoints.ContainsKey(point2Name);
            bool detHasConnection = detected.Keypoints.ContainsKey(point1Name) && detected.Keypoints.ContainsKey(point2Name);
            
            int canvasHeight = canvas.GetLength(0);
            int canvasWidth = canvas.GetLength(1);
            
            if (refHasConnection && detHasConnection)
            {
                var refP1 = reference.Keypoints[point1Name];
                var refP2 = reference.Keypoints[point2Name];
                var detP1 = detected.Keypoints[point1Name];
                var detP2 = detected.Keypoints[point2Name];
                
                // Calculate angle of both connections
                double refAngle = Math.Atan2(refP2.Y - refP1.Y, refP2.X - refP1.X);
                double detAngle = Math.Atan2(detP2.Y - detP1.Y, detP2.X - detP1.X);
                
                // Calculate angle difference
                double angleDiff = Math.Abs(refAngle - detAngle);
                while (angleDiff > Math.PI) angleDiff = 2 * Math.PI - angleDiff;
                
                // Convert to degrees
                double angleDiffDegrees = angleDiff * 180 / Math.PI;
                
                // Check lengths 
                double refLength = Math.Sqrt(Math.Pow(refP2.X - refP1.X, 2) + Math.Pow(refP2.Y - refP1.Y, 2));
                double detLength = Math.Sqrt(Math.Pow(detP2.X - detP1.X, 2) + Math.Pow(detP2.Y - detP1.Y, 2));
                double lengthRatio = Math.Min(refLength, detLength) / Math.Max(refLength, detLength);
                
                char lineChar = (angleDiffDegrees < 15 && lengthRatio > 0.8) ? '+' : '-';
                
                int x1 = (int)(refP1.X * (canvasWidth - 1));
                int y1 = (int)(refP1.Y * (canvasHeight - 1));
                int x2 = (int)(refP2.X * (canvasWidth - 1));
                int y2 = (int)(refP2.Y * (canvasHeight - 1));
                
                DrawComparisonLine(canvas, x1, y1, x2, y2, lineChar);
            }
        }
        
        /// <summary>
        /// Draws line with specific character for comparison
        /// </summary>
        private void DrawComparisonLine(char[,] canvas, int x1, int y1, int x2, int y2, char lineChar)
        {
            int width = canvas.GetLength(1);
            int height = canvas.GetLength(0);
            
            // Bresenham's line algorithm
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                if (x1 >= 0 && x1 < width && y1 >= 0 && y1 < height)
                {
                    // Don't overwrite keypoints
                    if (canvas[y1, x1] != 'M' && canvas[y1, x1] != 'R' && canvas[y1, x1] != 'D')
                    {
                        canvas[y1, x1] = lineChar;
                    }
                }
                
                if (x1 == x2 && y1 == y2) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
        
        /// <summary>
        /// Draws a skeleton on an ASCII canvas
        /// </summary>
        private void DrawSkeletonOnCanvas(ImageManager.Skeleton skeleton, char[,] canvas, int width, int height)
        {
            if (skeleton == null || skeleton.Keypoints.Count == 0)
                return;
                
            int canvasHeight = canvas.GetLength(0);
            int canvasWidth = canvas.GetLength(1);
            
            // Draw joints as points
            foreach (var kvp in skeleton.Keypoints)
            {
                int x = (int)(kvp.Value.X * (canvasWidth - 1));
                int y = (int)(kvp.Value.Y * (canvasHeight - 1));
                
                if (x >= 0 && x < canvasWidth && y >= 0 && y < canvasHeight)
                {
                    canvas[y, x] = 'O';
                }
            }
            
            // Draw lines between joints if both points exist
            DrawLineIfExists(skeleton, canvas, "nose", "leftShoulder");
            DrawLineIfExists(skeleton, canvas, "nose", "rightShoulder");
            DrawLineIfExists(skeleton, canvas, "leftShoulder", "rightShoulder");
            DrawLineIfExists(skeleton, canvas, "leftShoulder", "leftElbow");
            DrawLineIfExists(skeleton, canvas, "leftElbow", "leftWrist");
            DrawLineIfExists(skeleton, canvas, "rightShoulder", "rightElbow");
            DrawLineIfExists(skeleton, canvas, "rightElbow", "rightWrist");
            DrawLineIfExists(skeleton, canvas, "leftShoulder", "leftHip");
            DrawLineIfExists(skeleton, canvas, "rightShoulder", "rightHip");
            DrawLineIfExists(skeleton, canvas, "leftHip", "rightHip");
            DrawLineIfExists(skeleton, canvas, "leftHip", "leftKnee");
            DrawLineIfExists(skeleton, canvas, "leftKnee", "leftAnkle");
            DrawLineIfExists(skeleton, canvas, "rightHip", "rightKnee");
            DrawLineIfExists(skeleton, canvas, "rightKnee", "rightAnkle");
        }
        
        /// <summary>
        /// Draws a line between two keypoints if they both exist
        /// </summary>
        private void DrawLineIfExists(ImageManager.Skeleton skeleton, char[,] canvas, string point1Name, string point2Name)
        {
            if (skeleton.Keypoints.ContainsKey(point1Name) && skeleton.Keypoints.ContainsKey(point2Name))
            {
                var p1 = skeleton.Keypoints[point1Name];
                var p2 = skeleton.Keypoints[point2Name];
                
                int canvasHeight = canvas.GetLength(0);
                int canvasWidth = canvas.GetLength(1);
                
                int x1 = (int)(p1.X * (canvasWidth - 1));
                int y1 = (int)(p1.Y * (canvasHeight - 1));
                int x2 = (int)(p2.X * (canvasWidth - 1));
                int y2 = (int)(p2.Y * (canvasHeight - 1));
                
                DrawLine(canvas, x1, y1, x2, y2);
            }
        }
        
        /// <summary>
        /// Draws a line on the canvas using Bresenham's line algorithm
        /// </summary>
        private void DrawLine(char[,] canvas, int x1, int y1, int x2, int y2)
        {
            int canvasHeight = canvas.GetLength(0);
            int canvasWidth = canvas.GetLength(1);
            
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                if (x1 >= 0 && x1 < canvasWidth && y1 >= 0 && y1 < canvasHeight)
                {
                    // Don't overwrite joints (only draw line characters where there isn't a joint)
                    if (canvas[y1, x1] != 'O')
                    {
                        canvas[y1, x1] = '|';
                    }
                }
                
                if (x1 == x2 && y1 == y2) break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
        
        /// <summary>
        /// Draws a progress bar in the console
        /// </summary>
        private void DrawProgressBar(float current, float max, int barLength, ConsoleColor color)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            
            float percentage = Math.Min(current / max, 1.0f);
            int filledLength = (int)(barLength * percentage);
            
            Console.Write("[");
            for (int i = 0; i < barLength; i++)
            {
                if (i < filledLength)
                    Console.Write("█");
                else
                    Console.Write(" ");
            }
            
            Console.Write($"] {percentage * 100:F1}%");
            Console.WriteLine();
            
            Console.ForegroundColor = originalColor;
        }
        
        /// <summary>
        /// Provides intelligent suggestions based on keypoint match data
        /// </summary>
        private void ProvideIntelligentSuggestions(Dictionary<string, float> sortedKeypoints, Dictionary<string, string> keypointToBodyPart)
        {
            // Only provide suggestions if there are keypoints below the threshold
            var lowScoreKeypoints = sortedKeypoints
                .Where(kv => kv.Value < settings.DetectionThreshold)
                .Take(3)
                .ToList();
                
            if (lowScoreKeypoints.Count > 0)
            {
                Console.WriteLine("\n📝 SUGGESTED ADJUSTMENTS:");
                
                foreach (var kvp in lowScoreKeypoints)
                {
                    string keypointName = kvp.Key;
                    string bodyPart = keypointToBodyPart.ContainsKey(keypointName) 
                        ? keypointToBodyPart[keypointName] 
                        : keypointName;
                    
                    // Generate specific suggestion based on keypoint
                    string suggestion = GenerateSuggestionForKeypoint(keypointName);
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"🔸 {suggestion}");
                    Console.ResetColor();
                }
            }
        }
        
        /// <summary>
        /// Generates a specific suggestion for a keypoint
        /// </summary>
        private string GenerateSuggestionForKeypoint(string keypointName)
        {
            switch (keypointName)
            {
                case "nose":
                    return "Keep your head aligned properly with your spine.";
                case "leftShoulder":
                case "rightShoulder":
                    return "Adjust your shoulder position to match the reference pose.";
                case "leftElbow":
                    return "Position your left arm at the correct angle.";
                case "rightElbow":
                    return "Position your right arm at the correct angle.";
                case "leftWrist":
                    return "Check the position of your left hand.";
                case "rightWrist":
                    return "Check the position of your right hand.";
                case "leftHip":
                case "rightHip":
                    return "Align your hips properly for better balance.";
                case "leftKnee":
                    return "Adjust the angle of your left knee.";
                case "rightKnee":
                    return "Adjust the angle of your right knee.";
                case "leftAnkle":
                    return "Check the position of your left foot.";
                case "rightAnkle":
                    return "Check the position of your right foot.";
                default:
                    return "Adjust your position to better match the reference pose.";
            }
        }

        private string GetPoseName(int poseNumber)
        {
            return poseNumber switch
            {
                1 => settings.Pose1Name,
                2 => settings.Pose2Name,
                3 => settings.Pose3Name,
                4 => settings.Pose4Name,
                5 => settings.Pose5Name,
                6 => settings.Pose6Name,
                _ => $"Pose {poseNumber}"
            };
        }
        
        private bool GetPoseActive(int poseNumber)
        {
            return poseNumber switch
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
        
        private void TogglePoseActive(int poseNumber)
        {
            switch (poseNumber)
            {
                case 1:
                    settings.Pose1Active = !settings.Pose1Active;
                    break;
                case 2:
                    settings.Pose2Active = !settings.Pose2Active;
                    break;
                case 3:
                    settings.Pose3Active = !settings.Pose3Active;
                    break;
                case 4:
                    settings.Pose4Active = !settings.Pose4Active;
                    break;
                case 5:
                    settings.Pose5Active = !settings.Pose5Active;
                    break;
                case 6:
                    settings.Pose6Active = !settings.Pose6Active;
                    break;
            }
        }

        private void EditPoseName(int poseNumber)
        {
            Console.Write($"\nEnter name for Pose {poseNumber}: ");
            string? input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                switch (poseNumber)
                {
                    case 1:
                        settings.Pose1Name = input;
                        break;
                    case 2:
                        settings.Pose2Name = input;
                        break;
                    case 3:
                        settings.Pose3Name = input;
                        break;
                    case 4:
                        settings.Pose4Name = input;
                        break;
                    case 5:
                        settings.Pose5Name = input;
                        break;
                    case 6:
                        settings.Pose6Name = input;
                        break;
                }
            }
        }

        private void ShowResults()
        {
            Console.Clear();
            PrintHeader("TRAINING RESULTS");
            
            Console.WriteLine($"Total Sessions: {stats.TotalSessions}");
            Console.WriteLine($"Average Accuracy: {stats.AverageAccuracy:F1}%");
            
            if (!string.IsNullOrEmpty(stats.BestPose))
            {
                Console.WriteLine($"Best Pose: {stats.BestPose}");
            }
            
            if (stats.LastTrainingDate != DateTime.MinValue)
            {
                Console.WriteLine($"Last Training: {stats.LastTrainingDate:dd/MM/yyyy}");
            }
            
            if (stats.PosePerformance != null && stats.PosePerformance.Count > 0)
            {
                Console.WriteLine("\nIndividual Pose Performance:");
                foreach (var pose in stats.PosePerformance)
                {
                    Console.WriteLine($"- {pose.Key}: {pose.Value.AverageAccuracy:F1}% ({pose.Value.TotalAttempts} attempts)");
                }
            }
            
            Console.WriteLine("\nPress any key to return to main menu...");
            Console.ReadKey(true);
        }

        private void PrintHeader(string title)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 50));
            Console.WriteLine(title.PadLeft((50 + title.Length) / 2).PadRight(50));
            Console.WriteLine(new string('=', 50));
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    // Tracking performance data for individual poses
    class PosePerformanceData
    {
        public int TotalAttempts { get; set; } = 0;
        public double AverageAccuracy { get; set; } = 0;
    }

    class TrainingStats
    {
        public int TotalSessions { get; set; }
        public double AverageAccuracy { get; set; }
        public string BestPose { get; set; } = "";
        public DateTime LastTrainingDate { get; set; }
        public Dictionary<string, PosePerformanceData> PosePerformance { get; set; } = new Dictionary<string, PosePerformanceData>();
    }
}