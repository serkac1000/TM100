using System;
using System.IO;
using System.Linq;
using OpenCvSharp;
using Tensorflow;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Handles pose detection and model inference
    /// </summary>
    public class PoseDetector : IDisposable
    {
        private Graph graph;
        private Session session;
        private bool isDisposed = false;
        
        // The number of keypoints in the pose model (Teachable Machine uses 17 keypoints)
        private const int NUM_KEYPOINTS = 17;
        
        /// <summary>
        /// Initializes a new instance of the PoseDetector class
        /// </summary>
        /// <param name="modelPath">Path to the TensorFlow/ONNX model file</param>
        public PoseDetector(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Model file not found", modelPath);
            }

            try
            {
                // Initialize TensorFlow graph and session
                graph = new Graph();
                graph.Import(modelPath);
                session = new Session(graph);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load model: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Detects skeleton keypoints from an image frame
        /// </summary>
        /// <param name="frame">The input frame from webcam</param>
        /// <returns>Array of keypoint coordinates (x1, y1, x2, y2, ...)</returns>
        public float[] DetectSkeleton(Mat frame)
        {
            try
            {
                // This is a simplified implementation
                // In a real application, we would use a pose estimation model
                // such as PoseNet or MoveNet to extract precise keypoints
                
                // For demonstration purposes, we'll create a simulated set of keypoints
                // based on basic image processing techniques
                
                // Convert to grayscale
                Mat gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                
                // Apply GaussianBlur
                Cv2.GaussianBlur(gray, gray, new Size(5, 5), 0);
                
                // Use Canny edge detection
                Mat edges = new Mat();
                Cv2.Canny(gray, edges, 50, 150);
                
                // Find contours
                Point[][] contours;
                HierarchyIndex[] hierarchy;
                Cv2.FindContours(edges, out contours, out hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                
                // Sort contours by area (largest first)
                var sortedContours = contours.OrderByDescending(c => Cv2.ContourArea(c)).ToArray();
                
                // Generate simulated keypoints
                // In a real implementation, this would be replaced by actual pose estimation
                float[] keypoints = new float[NUM_KEYPOINTS * 2];
                
                if (sortedContours.Length > 0)
                {
                    // Get bounding rectangle of the largest contour
                    Rect boundingRect = Cv2.BoundingRect(sortedContours[0]);
                    
                    // Draw bounding rectangle (for visualization)
                    Cv2.Rectangle(frame, boundingRect, Scalar.Red, 2);
                    
                    // Create simulated keypoints based on the bounding rectangle
                    // These are just placeholder values for demonstration
                    // Head
                    keypoints[0] = boundingRect.X + boundingRect.Width / 2;
                    keypoints[1] = boundingRect.Y + boundingRect.Height / 6;
                    
                    // Shoulders
                    keypoints[2] = boundingRect.X + boundingRect.Width / 4;
                    keypoints[3] = boundingRect.Y + boundingRect.Height / 3;
                    keypoints[4] = boundingRect.X + 3 * boundingRect.Width / 4;
                    keypoints[5] = boundingRect.Y + boundingRect.Height / 3;
                    
                    // Elbows
                    keypoints[6] = boundingRect.X + boundingRect.Width / 8;
                    keypoints[7] = boundingRect.Y + boundingRect.Height / 2;
                    keypoints[8] = boundingRect.X + 7 * boundingRect.Width / 8;
                    keypoints[9] = boundingRect.Y + boundingRect.Height / 2;
                    
                    // Wrists
                    keypoints[10] = boundingRect.X + boundingRect.Width / 10;
                    keypoints[11] = boundingRect.Y + 2 * boundingRect.Height / 3;
                    keypoints[12] = boundingRect.X + 9 * boundingRect.Width / 10;
                    keypoints[13] = boundingRect.Y + 2 * boundingRect.Height / 3;
                    
                    // Hips
                    keypoints[14] = boundingRect.X + boundingRect.Width / 3;
                    keypoints[15] = boundingRect.Y + 2 * boundingRect.Height / 3;
                    keypoints[16] = boundingRect.X + 2 * boundingRect.Width / 3;
                    keypoints[17] = boundingRect.Y + 2 * boundingRect.Height / 3;
                    
                    // Knees
                    keypoints[18] = boundingRect.X + boundingRect.Width / 3;
                    keypoints[19] = boundingRect.Y + 4 * boundingRect.Height / 5;
                    keypoints[20] = boundingRect.X + 2 * boundingRect.Width / 3;
                    keypoints[21] = boundingRect.Y + 4 * boundingRect.Height / 5;
                    
                    // Ankles
                    keypoints[22] = boundingRect.X + boundingRect.Width / 3;
                    keypoints[23] = boundingRect.Y + boundingRect.Height - 5;
                    keypoints[24] = boundingRect.X + 2 * boundingRect.Width / 3;
                    keypoints[25] = boundingRect.Y + boundingRect.Height - 5;
                    
                    // Fill the rest with placeholder zeros
                    for (int i = 26; i < NUM_KEYPOINTS * 2; i++)
                    {
                        keypoints[i] = 0;
                    }
                    
                    // Normalize keypoints to the range [0, 1]
                    for (int i = 0; i < keypoints.Length; i += 2)
                    {
                        keypoints[i] /= frame.Width;
                        keypoints[i + 1] /= frame.Height;
                    }
                }
                
                return keypoints;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting skeleton: {ex.Message}");
                return new float[NUM_KEYPOINTS * 2];
            }
        }

        /// <summary>
        /// Runs the pose detection model with the given keypoints
        /// </summary>
        /// <param name="keypoints">Array of normalized keypoint coordinates</param>
        /// <returns>Array of confidence scores for each pose</returns>
        public float[] RunModel(float[] keypoints)
        {
            try
            {
                // Prepare input tensor
                // Reshape keypoints to match the expected input shape
                var inputShape = new long[] { 1, keypoints.Length };
                var inputTensor = new TFTensor(keypoints.Select(x => (float)x).ToArray(), inputShape);
                
                // Run inference
                // Note: Input and output tensor names may vary based on the actual model
                var runner = session.GetRunner();
                runner.AddInput("serving_default_input_1:0", inputTensor); // Adjust tensor name to match your model
                runner.Fetch("StatefulPartitionedCall:0"); // Adjust tensor name to match your model
                
                var output = runner.Run();
                
                // Process output
                var outputTensor = output[0];
                var outputArray = outputTensor.GetValue() as float[];
                
                // Ensure we always return at least 3 confidence values (one for each pose)
                if (outputArray == null || outputArray.Length < 3)
                {
                    return new float[] { 0.0f, 0.0f, 0.0f };
                }
                
                // Return the first 3 values as pose confidences
                return new float[] { outputArray[0], outputArray[1], outputArray[2] };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running model: {ex.Message}");
                // Return zero confidence if model inference fails
                return new float[] { 0.0f, 0.0f, 0.0f };
            }
        }

        /// <summary>
        /// Releases resources used by the PoseDetector
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the PoseDetector
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    session?.Dispose();
                    graph?.Dispose();
                }

                // Clean up unmanaged resources

                isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizer for PoseDetector
        /// </summary>
        ~PoseDetector()
        {
            Dispose(false);
        }
    }
}
