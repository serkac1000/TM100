using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO;
using OpenCvSharp;
using System.Text.Json;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PoseDetector poseDetector;
        private SkeletonVisualizer skeletonVisualizer;
        private AudioManager audioManager;
        private Settings settings;
        private CancellationTokenSource cancellationTokenSource;
        private bool isRunning = false;
        private string currentPose = "Pose1";
        private float[] currentConfidence = new float[3] { 0, 0, 0 };

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize components
            LoadSettings();
            skeletonVisualizer = new SkeletonVisualizer(SkeletonCanvas);
            audioManager = new AudioManager();
            
            // Initialize UI
            UpdateUI();
        }

        private void LoadSettings()
        {
            try
            {
                string settingsFile = "settings.json";
                if (File.Exists(settingsFile))
                {
                    string json = File.ReadAllText(settingsFile);
                    settings = JsonSerializer.Deserialize<Settings>(json);
                }
                else
                {
                    // Create default settings
                    settings = new Settings
                    {
                        ModelPath = "",
                        Pose1Name = "Pose 1",
                        Pose2Name = "Pose 2",
                        Pose3Name = "Pose 3"
                    };
                    SaveSettings();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                settings = new Settings
                {
                    ModelPath = "",
                    Pose1Name = "Pose 1",
                    Pose2Name = "Pose 2",
                    Pose3Name = "Pose 3"
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
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(settings.ModelPath))
            {
                MessageBox.Show("Please configure model path in settings first.", "Settings Required", MessageBoxButton.OK, MessageBoxImage.Information);
                SettingsButton_Click(sender, e);
                return;
            }

            try
            {
                StartSession();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopSession();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopSession();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Stop the session if running
            if (isRunning)
            {
                StopSession();
            }

            // Open settings window
            SettingsWindow settingsWindow = new SettingsWindow(settings);
            if (settingsWindow.ShowDialog() == true)
            {
                settings = settingsWindow.Settings;
                SaveSettings();
            }
        }

        private void StartSession()
        {
            if (isRunning)
                return;

            try
            {
                StatusText.Text = "Initializing...";
                
                // Initialize pose detector
                poseDetector = new PoseDetector(settings.ModelPath);
                
                // Start webcam capture
                cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => ProcessFrames(cancellationTokenSource.Token));
                
                // Update UI
                isRunning = true;
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                SettingsButton.IsEnabled = false;
                
                StatusText.Text = "Running";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start session: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StopSession();
            }
        }

        private void StopSession()
        {
            if (!isRunning)
                return;

            // Cancel processing
            cancellationTokenSource?.Cancel();
            
            // Clean up resources
            poseDetector?.Dispose();
            poseDetector = null;
            
            // Update UI
            isRunning = false;
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            SettingsButton.IsEnabled = true;
            
            StatusText.Text = "Ready";
            
            // Clear webcam feed and skeleton
            WebcamFeed.Source = null;
            SkeletonCanvas.Children.Clear();
        }

        private async Task ProcessFrames(CancellationToken cancellationToken)
        {
            try
            {
                using (VideoCapture capture = new VideoCapture(0))
                {
                    if (!capture.IsOpened())
                    {
                        Dispatcher.Invoke(() => 
                        {
                            MessageBox.Show("Failed to open webcam. Please check your connections.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            StopSession();
                        });
                        return;
                    }

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Capture frame
                        using (Mat frame = new Mat())
                        {
                            capture.Read(frame);
                            if (frame.Empty())
                                continue;

                            // Detect pose keypoints
                            var keypoints = poseDetector.DetectSkeleton(frame);
                            
                            // Run inference with detected keypoints
                            if (keypoints != null && keypoints.Length > 0)
                            {
                                currentConfidence = poseDetector.RunModel(keypoints);
                                CheckPoseTransition(currentConfidence);
                            }

                            // Update UI on UI thread
                            Dispatcher.Invoke(() => 
                            {
                                // Convert OpenCV Mat to WPF image
                                WebcamFeed.Source = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(frame);
                                
                                // Update skeleton visualization
                                if (keypoints != null && keypoints.Length > 0)
                                {
                                    skeletonVisualizer.DrawSkeleton(keypoints);
                                }
                                
                                // Update confidence values
                                UpdateConfidenceValues(currentConfidence);
                            });
                        }

                        // Add a small delay to reduce CPU usage
                        await Task.Delay(30, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, ignore
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => 
                {
                    MessageBox.Show($"Error processing frames: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    StopSession();
                });
            }
        }

        private void CheckPoseTransition(float[] confidence)
        {
            // Check for pose transitions based on confidence thresholds (50%)
            if (confidence[0] > 0.5 && currentPose != "Pose1")
            {
                currentPose = "Pose1";
                Dispatcher.Invoke(() => CurrentPoseText.Text = settings.Pose1Name);
                audioManager.PlayAudio("pose1.wav");
            }
            else if (confidence[1] > 0.5 && currentPose == "Pose1")
            {
                currentPose = "Pose2";
                Dispatcher.Invoke(() => CurrentPoseText.Text = settings.Pose2Name);
                audioManager.PlayAudio("pose2.wav");
            }
            else if (confidence[2] > 0.5 && currentPose == "Pose2")
            {
                currentPose = "Pose3";
                Dispatcher.Invoke(() => CurrentPoseText.Text = settings.Pose3Name);
                audioManager.PlayAudio("pose3.wav");
                
                // Loop back to Pose1
                currentPose = "Pose1";
                Dispatcher.Invoke(() => CurrentPoseText.Text = settings.Pose1Name);
            }
        }

        private void UpdateConfidenceValues(float[] confidence)
        {
            // Update the accuracy percentages on the UI
            Pose1Accuracy.Text = $"{confidence[0] * 100:F1}%";
            Pose2Accuracy.Text = $"{confidence[1] * 100:F1}%";
            Pose3Accuracy.Text = $"{confidence[2] * 100:F1}%";

            // Update progress bar based on current pose confidence
            if (currentPose == "Pose1")
                PoseProgressBar.Value = confidence[0] * 100;
            else if (currentPose == "Pose2")
                PoseProgressBar.Value = confidence[1] * 100;
            else if (currentPose == "Pose3")
                PoseProgressBar.Value = confidence[2] * 100;
        }

        private void UpdateUI()
        {
            // Update UI with current settings
            CurrentPoseText.Text = settings.Pose1Name;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources when window is closed
            StopSession();
            base.OnClosed(e);
        }
    }
}
