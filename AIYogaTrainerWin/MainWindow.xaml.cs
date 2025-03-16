using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SkeletonDetector skeletonDetector;
        private PoseRecognizer poseRecognizer;
        private AudioManager audioManager;
        
        private bool isRunning = false;
        
        // Border brushes for active and inactive poses
        private readonly SolidColorBrush activePoseBrush = new SolidColorBrush(Colors.Cyan);
        private readonly SolidColorBrush inactivePoseBrush = new SolidColorBrush(Colors.Gray);
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize components
            InitializeComponents();
            
            // Load pose images if available
            LoadPoseImages();
            
            // Add log message
            AddLogMessage("Application initialized.");
            
            // Update status displays
            UpdateStatusDisplays();
        }

        /// <summary>
        /// Initialize application components
        /// </summary>
        private void InitializeComponents()
        {
            try
            {
                // Create skeleton detector
                skeletonDetector = new SkeletonDetector(AppSettings.Settings.CameraIndex);
                skeletonDetector.NewFrame += SkeletonDetector_NewFrame;
                skeletonDetector.KeypointsDetected += SkeletonDetector_KeypointsDetected;
                
                // Create pose recognizer
                poseRecognizer = new PoseRecognizer();
                poseRecognizer.ConfidenceThreshold = AppSettings.Settings.ConfidenceThreshold;
                poseRecognizer.PoseRecognized += PoseRecognizer_PoseRecognized;
                
                // Create audio manager
                audioManager = new AudioManager();
                
                // Try to load model if path is specified
                if (!string.IsNullOrEmpty(AppSettings.Settings.ModelUrl))
                {
                    LoadModel(AppSettings.Settings.ModelUrl);
                }
                
                // Set initial border colors
                Pose1Border.BorderBrush = inactivePoseBrush;
                Pose2Border.BorderBrush = inactivePoseBrush;
                Pose3Border.BorderBrush = inactivePoseBrush;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing components: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AddLogMessage($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Load pose reference images from settings
        /// </summary>
        private void LoadPoseImages()
        {
            try
            {
                // Load pose 1 image
                if (File.Exists(AppSettings.Settings.Pose1ImagePath))
                {
                    Pose1Image.Source = new BitmapImage(new Uri(AppSettings.Settings.Pose1ImagePath));
                }
                
                // Load pose 2 image
                if (File.Exists(AppSettings.Settings.Pose2ImagePath))
                {
                    Pose2Image.Source = new BitmapImage(new Uri(AppSettings.Settings.Pose2ImagePath));
                }
                
                // Load pose 3 image
                if (File.Exists(AppSettings.Settings.Pose3ImagePath))
                {
                    Pose3Image.Source = new BitmapImage(new Uri(AppSettings.Settings.Pose3ImagePath));
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error loading pose images: {ex.Message}");
            }
        }

        /// <summary>
        /// Load AI model from specified path
        /// </summary>
        /// <param name="modelPath">Path to the TensorFlow model</param>
        private void LoadModel(string modelPath)
        {
            try
            {
                if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
                {
                    AddLogMessage("Model file not found: " + modelPath);
                    ModelStatusText.Text = "Model: Not Found";
                    return;
                }
                
                bool success = poseRecognizer.LoadModel(modelPath);
                
                if (success)
                {
                    AddLogMessage("Model loaded successfully: " + Path.GetFileName(modelPath));
                    ModelStatusText.Text = "Model: Loaded";
                }
                else
                {
                    AddLogMessage("Failed to load model: " + Path.GetFileName(modelPath));
                    ModelStatusText.Text = "Model: Load Failed";
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error loading model: {ex.Message}");
                ModelStatusText.Text = "Model: Error";
            }
        }

        /// <summary>
        /// Start button click handler
        /// </summary>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartYogaTrainer();
        }

        /// <summary>
        /// Stop button click handler
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopYogaTrainer();
        }

        /// <summary>
        /// Settings button click handler
        /// </summary>
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create and show settings window
            SettingsWindow settingsWindow = new SettingsWindow();
            
            // If settings are changed and saved, reload them
            if (settingsWindow.ShowDialog() == true)
            {
                // Apply new settings
                poseRecognizer.ConfidenceThreshold = AppSettings.Settings.ConfidenceThreshold;
                
                // Reload model if changed
                if (!string.IsNullOrEmpty(AppSettings.Settings.ModelUrl) && 
                    File.Exists(AppSettings.Settings.ModelUrl))
                {
                    LoadModel(AppSettings.Settings.ModelUrl);
                }
                
                // Reload pose images
                LoadPoseImages();
                
                AddLogMessage("Settings updated.");
            }
        }

        /// <summary>
        /// Start the yoga trainer
        /// </summary>
        private void StartYogaTrainer()
        {
            if (isRunning)
                return;
            
            try
            {
                // Check if model is loaded
                if (!File.Exists(AppSettings.Settings.ModelUrl))
                {
                    MessageBox.Show("Please load a TensorFlow model in settings first.", 
                        "Model Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AddLogMessage("Cannot start: Model not found.");
                    return;
                }
                
                // Start the camera
                bool cameraStarted = skeletonDetector.Start();
                
                if (!cameraStarted)
                {
                    MessageBox.Show("Could not start camera. Please check your webcam connection.", 
                        "Camera Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    AddLogMessage("Failed to start camera.");
                    return;
                }
                
                isRunning = true;
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                CameraStatusText.Text = "Camera: Running";
                
                AddLogMessage("Yoga trainer started. Strike a pose!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting yoga trainer: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                AddLogMessage($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Stop the yoga trainer
        /// </summary>
        private void StopYogaTrainer()
        {
            if (!isRunning)
                return;
            
            try
            {
                // Stop the camera
                skeletonDetector.Stop();
                
                // Reset UI
                WebcamFeed.Source = null;
                SkeletonCanvas.Children.Clear();
                CurrentPoseText.Text = "Not Detected";
                ConfidenceText.Text = "Confidence: 0%";
                
                // Reset pose highlight
                Pose1Border.BorderBrush = inactivePoseBrush;
                Pose2Border.BorderBrush = inactivePoseBrush;
                Pose3Border.BorderBrush = inactivePoseBrush;
                
                isRunning = false;
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
                CameraStatusText.Text = "Camera: Stopped";
                
                AddLogMessage("Yoga trainer stopped.");
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error stopping: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for new frames from the skeleton detector
        /// </summary>
        private void SkeletonDetector_NewFrame(object sender, FrameEventArgs e)
        {
            if (e.Frame == null)
                return;
            
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Convert Bitmap to BitmapSource for display
                    WebcamFeed.Source = BitmapToImageSource(e.Frame);
                    
                    // Draw skeleton on canvas
                    skeletonDetector.DrawSkeleton(SkeletonCanvas, e.Frame.Width, e.Frame.Height);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating frame: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Event handler for keypoints detected by the skeleton detector
        /// </summary>
        private void SkeletonDetector_KeypointsDetected(object sender, KeypointsEventArgs e)
        {
            if (e.Keypoints == null || e.Keypoints.Length == 0)
                return;
            
            try
            {
                // Process keypoints with the pose recognizer
                var confidenceScores = poseRecognizer.RunModel(e.Keypoints);
                
                // Update UI on the UI thread
                Dispatcher.Invoke(() =>
                {
                    // Find max confidence for display
                    float maxConfidence = 0;
                    for (int i = 0; i < confidenceScores.Length; i++)
                    {
                        if (confidenceScores[i] > maxConfidence)
                        {
                            maxConfidence = confidenceScores[i];
                        }
                    }
                    
                    // Update confidence display
                    ConfidenceText.Text = $"Confidence: {maxConfidence:P0}";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing keypoints: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for pose recognition events
        /// </summary>
        private void PoseRecognizer_PoseRecognized(object sender, PoseRecognizedEventArgs e)
        {
            // Update UI on the UI thread
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Update current pose text
                    CurrentPoseText.Text = e.PoseName;
                    ConfidenceText.Text = $"Confidence: {e.Confidence:P0}";
                    
                    // Update pose highlight
                    Pose1Border.BorderBrush = (e.PoseName == "Pose1") ? activePoseBrush : inactivePoseBrush;
                    Pose2Border.BorderBrush = (e.PoseName == "Pose2") ? activePoseBrush : inactivePoseBrush;
                    Pose3Border.BorderBrush = (e.PoseName == "Pose3") ? activePoseBrush : inactivePoseBrush;
                    
                    // Add to log
                    AddLogMessage($"Detected {e.PoseName} with {e.Confidence:P0} confidence.");
                    
                    // Play corresponding audio
                    PlayPoseAudio(e.PoseName);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling pose recognition: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Play audio for recognized pose
        /// </summary>
        /// <param name="poseName">Name of the recognized pose</param>
        private void PlayPoseAudio(string poseName)
        {
            try
            {
                string audioPath = "";
                
                // Determine which audio file to play
                switch (poseName)
                {
                    case "Pose1":
                        audioPath = AppSettings.Settings.Pose1AudioPath;
                        break;
                    case "Pose2":
                        audioPath = AppSettings.Settings.Pose2AudioPath;
                        break;
                    case "Pose3":
                        audioPath = AppSettings.Settings.Pose3AudioPath;
                        break;
                }
                
                // Play audio if file exists
                if (!string.IsNullOrEmpty(audioPath) && File.Exists(audioPath))
                {
                    audioManager.PlayAudio(audioPath);
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error playing audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Add message to status log
        /// </summary>
        /// <param name="message">Message to add</param>
        private void AddLogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                // Format with timestamp
                string formattedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                
                // Add to log with newline
                StatusLog.Text += (StatusLog.Text.Length > 0 ? Environment.NewLine : "") + formattedMessage;
                
                // Scroll to bottom
                ScrollToBottom();
            });
        }

        /// <summary>
        /// Scroll status log to bottom
        /// </summary>
        private void ScrollToBottom()
        {
            var parent = StatusLog.Parent as ScrollViewer;
            if (parent != null)
            {
                parent.ScrollToBottom();
            }
        }

        /// <summary>
        /// Convert Bitmap to BitmapSource for display in WPF Image control
        /// </summary>
        /// <param name="bitmap">Bitmap to convert</param>
        /// <returns>BitmapSource for WPF</returns>
        private BitmapSource BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Important for cross-thread access
                
                return bitmapImage;
            }
        }

        /// <summary>
        /// Update all status displays
        /// </summary>
        private void UpdateStatusDisplays()
        {
            // Update camera status text
            CameraStatusText.Text = isRunning ? "Camera: Running" : "Camera: Not Started";
            
            // Update model status text
            if (!string.IsNullOrEmpty(AppSettings.Settings.ModelUrl) && 
                File.Exists(AppSettings.Settings.ModelUrl))
            {
                ModelStatusText.Text = "Model: Loaded";
            }
            else
            {
                ModelStatusText.Text = "Model: Not Loaded";
            }
        }

        /// <summary>
        /// Window closing event handler
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Clean up resources
            StopYogaTrainer();
            
            skeletonDetector?.Dispose();
            poseRecognizer?.Dispose();
            audioManager?.Dispose();
        }
    }
}
