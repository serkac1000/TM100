using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TensorFlow;

namespace AIYogaTrainerWin
{
    public class PoseRecognizer : IDisposable
    {
        private TFGraph graph;
        private TFSession session;
        private bool isModelLoaded = false;

        // Event for pose recognition
        public event EventHandler<PoseRecognizedEventArgs> PoseRecognized;

        public string CurrentPose { get; private set; } = "None";
        public float ConfidenceThreshold { get; set; } = 0.5f;

        public PoseRecognizer()
        {
        }

        /// <summary>
        /// Loads a TensorFlow model from the specified path
        /// </summary>
        /// <param name="modelPath">Path to the TensorFlow model</param>
        /// <returns>True if the model was loaded successfully</returns>
        public bool LoadModel(string modelPath)
        {
            try
            {
                // Dispose of existing graph and session if they exist
                DisposeModel();

                // Create new graph and session
                graph = new TFGraph();
                graph.Import(modelPath);
                session = new TFSession(graph);
                isModelLoaded = true;

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading model: {ex.Message}");
                isModelLoaded = false;
                return false;
            }
        }

        /// <summary>
        /// Runs the pose recognition model on the provided keypoints
        /// </summary>
        /// <param name="keypoints">Array of keypoints from the skeleton detector</param>
        /// <returns>Array of confidence scores for each pose</returns>
        public float[] RunModel(float[] keypoints)
        {
            if (!isModelLoaded || session == null)
            {
                throw new InvalidOperationException("Model not loaded. Call LoadModel first.");
            }

            try
            {
                // Create input tensor from keypoints
                var tensor = new TFTensor(keypoints);
                
                // Run inference
                var runner = session.GetRunner();
                runner.AddInput(graph.OperationByName("input"), tensor);
                runner.Fetch(graph.OperationByName("output"));
                
                var output = runner.Run();
                
                // Extract confidence scores
                float[] confidenceScores = output[0].GetValue() as float[] ?? new float[3];
                
                // Check for pose transitions based on confidence threshold
                CheckPoseTransition(confidenceScores);
                
                return confidenceScores;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error running model: {ex.Message}");
                return new float[3]; // Return empty array on error
            }
        }

        /// <summary>
        /// Check for pose transitions based on confidence scores
        /// </summary>
        /// <param name="confidenceScores">Array of confidence scores from the model</param>
        private void CheckPoseTransition(float[] confidenceScores)
        {
            if (confidenceScores == null || confidenceScores.Length < 3)
                return;

            string newPose = CurrentPose;
            int mostConfidentPoseIndex = Array.IndexOf(confidenceScores, confidenceScores.Max());
            float highestConfidence = confidenceScores[mostConfidentPoseIndex];

            // Only transition if confidence exceeds threshold
            if (highestConfidence >= ConfidenceThreshold)
            {
                // Determine which pose has highest confidence
                switch (mostConfidentPoseIndex)
                {
                    case 0:
                        newPose = "Pose1";
                        break;
                    case 1:
                        newPose = "Pose2";
                        break;
                    case 2:
                        newPose = "Pose3";
                        break;
                }

                // If pose has changed, trigger event
                if (newPose != CurrentPose)
                {
                    CurrentPose = newPose;
                    OnPoseRecognized(new PoseRecognizedEventArgs 
                    { 
                        PoseName = newPose,
                        Confidence = highestConfidence
                    });
                }
            }
        }

        /// <summary>
        /// Trigger the pose recognized event
        /// </summary>
        /// <param name="args">Event arguments containing pose info</param>
        protected virtual void OnPoseRecognized(PoseRecognizedEventArgs args)
        {
            PoseRecognized?.Invoke(this, args);
        }

        /// <summary>
        /// Dispose resources used by the pose recognizer
        /// </summary>
        public void Dispose()
        {
            DisposeModel();
        }

        /// <summary>
        /// Helper method to dispose session and graph
        /// </summary>
        private void DisposeModel()
        {
            session?.Dispose();
            session = null;
            graph?.Dispose();
            graph = null;
            isModelLoaded = false;
        }
    }

    public class PoseRecognizedEventArgs : EventArgs
    {
        public string PoseName { get; set; }
        public float Confidence { get; set; }
    }
}
