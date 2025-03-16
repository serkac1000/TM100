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
            app.Run();
        }
    }

    class YogaTrainerApp
    {
        private YogaAppSettings settings;
        private TrainingStats stats;
        private PoseLibrary poseLibrary;
        private bool isRunning = false;
        private CancellationTokenSource? cancellationTokenSource;

        public YogaTrainerApp()
        {
            // Initialize the pose library first
            poseLibrary = new PoseLibrary();
            
            // Then load settings and stats
            LoadSettings();
            LoadStats();
        }

        public void Run()
        {
            bool exitApp = false;

            while (!exitApp)
            {
                Console.Clear();
                PrintHeader("AI YOGA TRAINER");
                
                Console.WriteLine("1. START TRAINING");
                Console.WriteLine("2. SETTINGS");
                Console.WriteLine("3. VIEW RESULTS");
                Console.WriteLine("4. EXIT");
                Console.WriteLine();
                Console.Write("Select an option (1-4): ");

                var key = Console.ReadKey(true);
                
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
                    settings = new YogaAppSettings
                    {
                        ModelUrl = "",
                        AudioFeedback = true,
                        HoldTime = 3,
                        Pose1Name = "Mountain Pose",
                        Pose2Name = "Warrior Pose",
                        Pose3Name = "Tree Pose",
                        Pose4Name = "Downward Dog"
                    };
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                settings = new YogaAppSettings
                {
                    ModelUrl = "",
                    AudioFeedback = true,
                    HoldTime = 3,
                    Pose1Name = "Mountain Pose",
                    Pose2Name = "Warrior Pose",
                    Pose3Name = "Tree Pose",
                    Pose4Name = "Downward Dog"
                };
            }
        }

        private void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("settings.json", json);
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
                        LastTrainingDate = DateTime.MinValue
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
                    LastTrainingDate = DateTime.MinValue
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
            PrintHeader("TRAINING SESSION");

            if (string.IsNullOrEmpty(settings.ModelUrl))
            {
                Console.WriteLine("You need to set up a model URL in settings first.");
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
                return;
            }

            isRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            
            Console.WriteLine($"Connected to model: {settings.ModelUrl}");
            Console.WriteLine($"Hold time: {settings.HoldTime} seconds");
            Console.WriteLine($"Audio feedback: {(settings.AudioFeedback ? "On" : "Off")}");
            Console.WriteLine("\nPreparing camera...");
            Thread.Sleep(1000);
            
            Console.WriteLine("Camera ready! Starting pose detection...");
            
            // Simulate a training session
            SimulateTrainingSession(cancellationTokenSource.Token).Wait();
            
            // Update stats
            stats.TotalSessions++;
            stats.AverageAccuracy = ((stats.AverageAccuracy * (stats.TotalSessions - 1)) + 31.2) / stats.TotalSessions;
            stats.BestPose = settings.Pose3Name;
            stats.LastTrainingDate = DateTime.Now;
            SaveStats();
            
            isRunning = false;
            Console.WriteLine("\nSession ended. Press any key to continue...");
            Console.ReadKey(true);
        }

        private async Task SimulateTrainingSession(CancellationToken cancellationToken)
        {
            string[] poses = new string[] 
            { 
                settings.Pose1Name, 
                settings.Pose2Name, 
                settings.Pose3Name, 
                settings.Pose4Name 
            };
            
            int currentPoseIndex = 0;
            
            Console.WriteLine($"\nStarting with {poses[currentPoseIndex]}");
            
            try
            {
                Random random = new Random();
                
                for (int i = 0; i < 10 && !cancellationToken.IsCancellationRequested; i++)
                {
                    // Display pose guidance
                    Console.WriteLine($"\nPerforming {poses[currentPoseIndex]}...");
                    
                    // Simulate processing time and pose detection
                    for (int j = 0; j < settings.HoldTime; j++)
                    {
                        float accuracy = (float)random.NextDouble() * 100;
                        Console.WriteLine($"Accuracy: {accuracy:F1}%");
                        await Task.Delay(1000, cancellationToken);
                    }
                    
                    // Move to next pose
                    currentPoseIndex = (currentPoseIndex + 1) % poses.Length;
                    Console.WriteLine($"\nGreat job! Next pose: {poses[currentPoseIndex]}");
                    
                    if (settings.AudioFeedback)
                    {
                        Console.WriteLine("[Audio cue played]");
                    }
                    
                    await Task.Delay(1500, cancellationToken);
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
                
                Console.WriteLine($"1. Model URL: {settings.ModelUrl}");
                Console.WriteLine($"2. Audio Feedback: {(settings.AudioFeedback ? "On" : "Off")}");
                Console.WriteLine($"3. Hold Time: {settings.HoldTime} seconds");
                Console.WriteLine($"4. Pose 1 Name: {settings.Pose1Name}");
                Console.WriteLine($"5. Pose 2 Name: {settings.Pose2Name}");
                Console.WriteLine($"6. Pose 3 Name: {settings.Pose3Name}");
                Console.WriteLine($"7. Pose 4 Name: {settings.Pose4Name}");
                Console.WriteLine("8. Save and Return");
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
                        EditHoldTime();
                        break;
                    case '4':
                        EditPoseName(1);
                        break;
                    case '5':
                        EditPoseName(2);
                        break;
                    case '6':
                        EditPoseName(3);
                        break;
                    case '7':
                        EditPoseName(4);
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
            Console.Write("\nEnter Hold Time in seconds (1-10): ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int holdTime) && holdTime >= 1 && holdTime <= 10)
            {
                settings.HoldTime = holdTime;
            }
            else
            {
                Console.WriteLine("Invalid input. Hold time must be between 1 and 10.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
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

    // Removed redundant Settings class since we're now using AppSettings

    class TrainingStats
    {
        public int TotalSessions { get; set; }
        public double AverageAccuracy { get; set; }
        public string BestPose { get; set; } = "";
        public DateTime LastTrainingDate { get; set; }
    }
}