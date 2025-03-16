using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OpenCvSharp;
using Point = System.Windows.Point;

namespace AIYogaTrainerWin
{
    public class SkeletonDetector : IDisposable
    {
        // OpenCV-related objects
        private VideoCapture capture;
        private bool isRunning = false;
        private readonly int cameraIndex;

        // Skeleton keypoints
        private readonly List<Point> keypoints = new List<Point>();
        
        // Constants for skeleton visualization
        private const int KeypointCount = 18; // Teachable Machine uses 18 keypoints
        
        // Keypoint connections for drawing skeleton
        private readonly (int, int)[] connections = new[]
        {
            (0, 1), // nose to neck
            (1, 2), // neck to right shoulder
            (2, 3), // right shoulder to right elbow
            (3, 4), // right elbow to right wrist
            (1, 5), // neck to left shoulder
            (5, 6), // left shoulder to left elbow
            (6, 7), // left elbow to left wrist
            (1, 8), // neck to right hip
            (8, 9), // right hip to right knee
            (9, 10), // right knee to right ankle
            (1, 11), // neck to left hip
            (11, 12), // left hip to left knee
            (12, 13), // left knee to left ankle
            (0, 14), // nose to right eye
            (14, 16), // right eye to right ear
            (0, 15), // nose to left eye
            (15, 17)  // left eye to left ear
        };

        // Event for new frame
        public event EventHandler<FrameEventArgs> NewFrame;
        
        // Event for keypoints detected
        public event EventHandler<KeypointsEventArgs> KeypointsDetected;

        public SkeletonDetector(int cameraIndex = 0)
        {
            this.cameraIndex = cameraIndex;
        }

        /// <summary>
        /// Starts the webcam and begins capturing frames
        /// </summary>
        /// <returns>True if started successfully, false otherwise</returns>
        public bool Start()
        {
            try
            {
                // Initialize video capture
                capture = new VideoCapture(cameraIndex);
                
                if (!capture.IsOpened())
                {
                    throw new Exception($"Could not open camera at index {cameraIndex}");
                }
                
                // Set resolution
                capture.Set(VideoCaptureProperties.FrameWidth, 640);
                capture.Set(VideoCaptureProperties.FrameHeight, 480);
                
                isRunning = true;
                
                // Start processing on a background thread
                System.Threading.Tasks.Task.Run(() => ProcessFrames());
                
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting camera: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops the webcam capture
        /// </summary>
        public void Stop()
        {
            isRunning = false;
            
            // Wait a bit for the processing loop to exit
            System.Threading.Thread.Sleep(100);
            
            capture?.Dispose();
            capture = null;
        }

        /// <summary>
        /// Main processing loop for webcam frames
        /// </summary>
        private void ProcessFrames()
        {
            while (isRunning && capture != null && capture.IsOpened())
            {
                try
                {
                    // Capture frame
                    using (Mat frame = new Mat())
                    {
                        if (!capture.Read(frame) || frame.Empty())
                        {
                            System.Threading.Thread.Sleep(30);
                            continue;
                        }
                        
                        // Detect skeleton keypoints
                        DetectSkeleton(frame);
                        
                        // Convert the OpenCV Mat to a bitmap for display
                        var frameBitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        
                        // Raise event with the new frame
                        OnNewFrame(new FrameEventArgs { Frame = frameBitmap });
                        
                        // Raise event with detected keypoints
                        var keypointsArray = FlattenKeypoints();
                        OnKeypointsDetected(new KeypointsEventArgs { Keypoints = keypointsArray });
                    }
                    
                    // Add a small delay to avoid maxing out the CPU
                    System.Threading.Thread.Sleep(30);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error processing frame: {ex.Message}");
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Detects skeleton keypoints in the given frame
        /// </summary>
        /// <param name="frame">Input frame from webcam</param>
        private void DetectSkeleton(Mat frame)
        {
            // In a real implementation, this would use OpenPose or another pose estimation model
            // For now, we'll generate mock data for demonstration
            // In the real implementation, this would be replaced with actual pose detection code
            
            // Clear existing keypoints
            keypoints.Clear();
            
            // For demonstration, we'll generate a simple T-pose based on frame dimensions
            int width = frame.Width;
            int height = frame.Height;
            
            // Add keypoints (this would be replaced with actual detection in production)
            // These would come from an actual skeleton detection model like OpenPose
            keypoints.Add(new Point(width / 2, height / 4)); // nose
            keypoints.Add(new Point(width / 2, height / 3)); // neck
            keypoints.Add(new Point(width / 3, height / 3)); // right shoulder
            keypoints.Add(new Point(width / 4, height / 2)); // right elbow
            keypoints.Add(new Point(width / 5, height / 2)); // right wrist
            keypoints.Add(new Point(2 * width / 3, height / 3)); // left shoulder
            keypoints.Add(new Point(3 * width / 4, height / 2)); // left elbow
            keypoints.Add(new Point(4 * width / 5, height / 2)); // left wrist
            keypoints.Add(new Point(width / 2 - 20, 2 * height / 3)); // right hip
            keypoints.Add(new Point(width / 2 - 20, 3 * height / 4)); // right knee
            keypoints.Add(new Point(width / 2 - 20, 4 * height / 5)); // right ankle
            keypoints.Add(new Point(width / 2 + 20, 2 * height / 3)); // left hip
            keypoints.Add(new Point(width / 2 + 20, 3 * height / 4)); // left knee
            keypoints.Add(new Point(width / 2 + 20, 4 * height / 5)); // left ankle
            keypoints.Add(new Point(width / 2 - 10, height / 4 - 5)); // right eye
            keypoints.Add(new Point(width / 2 + 10, height / 4 - 5)); // left eye
            keypoints.Add(new Point(width / 2 - 20, height / 4)); // right ear
            keypoints.Add(new Point(width / 2 + 20, height / 4)); // left ear
        }

        /// <summary>
        /// Flattens keypoints into a 1D array for model input (x1, y1, x2, y2, ...)
        /// </summary>
        /// <returns>Array of keypoint coordinates</returns>
        private float[] FlattenKeypoints()
        {
            float[] result = new float[keypoints.Count * 2];
            
            for (int i = 0; i < keypoints.Count; i++)
            {
                result[i * 2] = (float)keypoints[i].X;
                result[i * 2 + 1] = (float)keypoints[i].Y;
            }
            
            return result;
        }

        /// <summary>
        /// Draws the skeleton on the given canvas
        /// </summary>
        /// <param name="canvas">Canvas to draw on</param>
        /// <param name="frameWidth">Width of the webcam frame</param>
        /// <param name="frameHeight">Height of the webcam frame</param>
        public void DrawSkeleton(Canvas canvas, double frameWidth, double frameHeight)
        {
            // Clear canvas
            canvas.Children.Clear();
            
            if (keypoints.Count < KeypointCount)
                return;
                
            // Scale factors to map keypoints to canvas
            double scaleX = canvas.ActualWidth / frameWidth;
            double scaleY = canvas.ActualHeight / frameHeight;
            
            // Draw connections
            foreach (var connection in connections)
            {
                if (connection.Item1 < keypoints.Count && connection.Item2 < keypoints.Count)
                {
                    var point1 = keypoints[connection.Item1];
                    var point2 = keypoints[connection.Item2];
                    
                    Line line = new Line
                    {
                        X1 = point1.X * scaleX,
                        Y1 = point1.Y * scaleY,
                        X2 = point2.X * scaleX,
                        Y2 = point2.Y * scaleY,
                        Stroke = new SolidColorBrush(Colors.Cyan),
                        StrokeThickness = 2
                    };
                    
                    canvas.Children.Add(line);
                }
            }
            
            // Draw keypoints
            foreach (var point in keypoints)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Colors.Yellow)
                };
                
                Canvas.SetLeft(ellipse, (point.X * scaleX) - 4);
                Canvas.SetTop(ellipse, (point.Y * scaleY) - 4);
                canvas.Children.Add(ellipse);
            }
        }

        /// <summary>
        /// Raises the NewFrame event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnNewFrame(FrameEventArgs e)
        {
            NewFrame?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the KeypointsDetected event
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnKeypointsDetected(KeypointsEventArgs e)
        {
            KeypointsDetected?.Invoke(this, e);
        }

        /// <summary>
        /// Dispose resources used by the skeleton detector
        /// </summary>
        public void Dispose()
        {
            Stop();
        }
    }

    public class FrameEventArgs : EventArgs
    {
        public Bitmap Frame { get; set; }
    }

    public class KeypointsEventArgs : EventArgs
    {
        public float[] Keypoints { get; set; }
    }
}
