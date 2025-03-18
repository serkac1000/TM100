using System;
using System.Collections.Generic;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Handles visualizing skeleton data in console application
    /// with enhanced match percentage visualization
    /// </summary>
    public class SkeletonVisualizer
    {
        private const int ConsoleWidth = 60;
        private const int ConsoleHeight = 20;
        private const char SkeletonPointChar = '●';
        private const char SkeletonLineChar = '═';
        
        private char[,] canvas;
        
        public SkeletonVisualizer()
        {
            canvas = new char[ConsoleHeight, ConsoleWidth];
            ClearCanvas();
        }
        
        /// <summary>
        /// Clear the visualization canvas
        /// </summary>
        public void ClearCanvas()
        {
            for (int y = 0; y < ConsoleHeight; y++)
            {
                for (int x = 0; x < ConsoleWidth; x++)
                {
                    canvas[y, x] = ' ';
                }
            }
        }
        
        /// <summary>
        /// Draw the skeleton based on the provided keypoints
        /// </summary>
        public void DrawSkeleton(PoseKeypoints keypoints)
        {
            ClearCanvas();
            
            // Draw all the points
            DrawPoint(keypoints.Head, "Head");
            DrawPoint(keypoints.Neck, "Neck");
            DrawPoint(keypoints.RightShoulder, "RSh");
            DrawPoint(keypoints.RightElbow, "REl");
            DrawPoint(keypoints.RightWrist, "RWr");
            DrawPoint(keypoints.LeftShoulder, "LSh");
            DrawPoint(keypoints.LeftElbow, "LEl");
            DrawPoint(keypoints.LeftWrist, "LWr");
            DrawPoint(keypoints.RightHip, "RHi");
            DrawPoint(keypoints.RightKnee, "RKn");
            DrawPoint(keypoints.RightAnkle, "RAn");
            DrawPoint(keypoints.LeftHip, "LHi");
            DrawPoint(keypoints.LeftKnee, "LKn");
            DrawPoint(keypoints.LeftAnkle, "LAn");
            
            // Draw lines connecting the points
            DrawLine(keypoints.Head, keypoints.Neck);
            DrawLine(keypoints.Neck, keypoints.RightShoulder);
            DrawLine(keypoints.Neck, keypoints.LeftShoulder);
            DrawLine(keypoints.RightShoulder, keypoints.RightElbow);
            DrawLine(keypoints.RightElbow, keypoints.RightWrist);
            DrawLine(keypoints.LeftShoulder, keypoints.LeftElbow);
            DrawLine(keypoints.LeftElbow, keypoints.LeftWrist);
            DrawLine(keypoints.Neck, keypoints.RightHip);
            DrawLine(keypoints.Neck, keypoints.LeftHip);
            DrawLine(keypoints.RightHip, keypoints.RightKnee);
            DrawLine(keypoints.RightKnee, keypoints.RightAnkle);
            DrawLine(keypoints.LeftHip, keypoints.LeftKnee);
            DrawLine(keypoints.LeftKnee, keypoints.LeftAnkle);
            DrawLine(keypoints.RightHip, keypoints.LeftHip);
            
            // Render the canvas to the console
            Render();
        }
        
        /// <summary>
        /// Draw the comparison between reference and detected skeletons with match percentages
        /// </summary>
        public void DrawSkeletonComparison(PoseKeypoints reference, PoseKeypoints detected, Dictionary<string, double> matchPercentages)
        {
            ClearCanvas();
            
            // Draw header
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', ConsoleWidth) + "╗");
            Console.WriteLine("║" + "ENHANCED SKELETON VISUALIZATION".PadLeft((ConsoleWidth + "ENHANCED SKELETON VISUALIZATION".Length) / 2).PadRight(ConsoleWidth) + "║");
            Console.WriteLine("╚" + new string('═', ConsoleWidth) + "╝");
            Console.ResetColor();
            
            // Draw reference skeleton
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("REFERENCE POSE (Perfect Form):");
            Console.ResetColor();
            
            // Draw all reference points in yellow
            Console.ForegroundColor = ConsoleColor.Yellow;
            DrawColoredPoint(reference.Head, "Head");
            DrawColoredPoint(reference.Neck, "Neck");
            DrawColoredPoint(reference.RightShoulder, "RSh");
            DrawColoredPoint(reference.RightElbow, "REl");
            DrawColoredPoint(reference.RightWrist, "RWr");
            DrawColoredPoint(reference.LeftShoulder, "LSh");
            DrawColoredPoint(reference.LeftElbow, "LEl");
            DrawColoredPoint(reference.LeftWrist, "LWr");
            DrawColoredPoint(reference.RightHip, "RHi");
            DrawColoredPoint(reference.RightKnee, "RKn");
            DrawColoredPoint(reference.RightAnkle, "RAn");
            DrawColoredPoint(reference.LeftHip, "LHi");
            DrawColoredPoint(reference.LeftKnee, "LKn");
            DrawColoredPoint(reference.LeftAnkle, "LAn");
            Console.ResetColor();
            
            Console.WriteLine();
            
            // Draw detected skeleton
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("DETECTED POSE (Your Current Position):");
            Console.ResetColor();
            
            // Draw all detected points in blue
            Console.ForegroundColor = ConsoleColor.Blue;
            DrawColoredPoint(detected.Head, "Head");
            DrawColoredPoint(detected.Neck, "Neck");
            DrawColoredPoint(detected.RightShoulder, "RSh");
            DrawColoredPoint(detected.RightElbow, "REl");
            DrawColoredPoint(detected.RightWrist, "RWr");
            DrawColoredPoint(detected.LeftShoulder, "LSh");
            DrawColoredPoint(detected.LeftElbow, "LEl");
            DrawColoredPoint(detected.LeftWrist, "LWr");
            DrawColoredPoint(detected.RightHip, "RHi");
            DrawColoredPoint(detected.RightKnee, "RKn");
            DrawColoredPoint(detected.RightAnkle, "RAn");
            DrawColoredPoint(detected.LeftHip, "LHi");
            DrawColoredPoint(detected.LeftKnee, "LKn");
            DrawColoredPoint(detected.LeftAnkle, "LAn");
            Console.ResetColor();
            
            Console.WriteLine();
            
            // Display match percentages with color coding
            DisplayMatchPercentages(matchPercentages);
        }
        
        /// <summary>
        /// Display match percentages with color coding for better visualization
        /// </summary>
        private void DisplayMatchPercentages(Dictionary<string, double> matchPercentages)
        {
            Console.WriteLine("╔" + new string('═', ConsoleWidth) + "╗");
            Console.WriteLine("║" + "KEYPOINT MATCH PERCENTAGES".PadLeft((ConsoleWidth + "KEYPOINT MATCH PERCENTAGES".Length) / 2).PadRight(ConsoleWidth) + "║");
            Console.WriteLine("╠" + new string('═', ConsoleWidth) + "╣");
            
            // Group keypoints by body section for more organized display
            Dictionary<string, List<KeyValuePair<string, double>>> bodyGroups = new Dictionary<string, List<KeyValuePair<string, double>>>()
            {
                { "Upper Body", new List<KeyValuePair<string, double>>() },
                { "Core", new List<KeyValuePair<string, double>>() },
                { "Lower Body", new List<KeyValuePair<string, double>>() }
            };
            
            foreach (var kvp in matchPercentages)
            {
                string keypoint = kvp.Key;
                double percentage = kvp.Value;
                
                if (keypoint.Contains("Head") || keypoint.Contains("Neck") || 
                    keypoint.Contains("Shoulder") || keypoint.Contains("Elbow") || 
                    keypoint.Contains("Wrist"))
                {
                    bodyGroups["Upper Body"].Add(kvp);
                }
                else if (keypoint.Contains("Hip"))
                {
                    bodyGroups["Core"].Add(kvp);
                }
                else
                {
                    bodyGroups["Lower Body"].Add(kvp);
                }
            }
            
            // Output match percentages by body section
            foreach (var section in bodyGroups)
            {
                Console.WriteLine("║" + $" {section.Key}:".PadRight(ConsoleWidth) + "║");
                Console.WriteLine("║" + new string('─', ConsoleWidth) + "║");
                
                foreach (var kvp in section.Value)
                {
                    string keypoint = GetFriendlyKeypointName(kvp.Key);
                    double percentage = kvp.Value;
                    
                    // Color code based on match percentage
                    ConsoleColor textColor = GetMatchPercentageColor(percentage);
                    
                    Console.Write("║ ");
                    Console.ForegroundColor = textColor;
                    Console.Write($"{keypoint}: ".PadRight(25));
                    
                    // Draw progress bar
                    DrawMatchPercentageBar(percentage, 20);
                    
                    Console.ResetColor();
                    Console.WriteLine(" ║");
                }
                
                Console.WriteLine("║" + new string(' ', ConsoleWidth) + "║");
            }
            
            Console.WriteLine("╚" + new string('═', ConsoleWidth) + "╝");
        }
        
        /// <summary>
        /// Draw a colored visual representation of the match percentage
        /// </summary>
        private void DrawMatchPercentageBar(double percentage, int barLength)
        {
            int filledLength = (int)(percentage * barLength / 100);
            
            // Color-coded bar based on percentage
            ConsoleColor barColor = GetMatchPercentageColor(percentage);
            Console.ForegroundColor = barColor;
            
            Console.Write("[");
            for (int i = 0; i < barLength; i++)
            {
                if (i < filledLength)
                    Console.Write("█");
                else
                    Console.Write(" ");
            }
            Console.Write($"] {percentage:F1}%");
            Console.ResetColor();
        }
        
        /// <summary>
        /// Get color based on match percentage
        /// </summary>
        private ConsoleColor GetMatchPercentageColor(double percentage)
        {
            if (percentage >= 85)
                return ConsoleColor.Green;
            else if (percentage >= 70)
                return ConsoleColor.Yellow;
            else if (percentage >= 50)
                return ConsoleColor.DarkYellow;
            else
                return ConsoleColor.Red;
        }
        
        /// <summary>
        /// Get user-friendly keypoint name
        /// </summary>
        private string GetFriendlyKeypointName(string technicalName)
        {
            switch (technicalName)
            {
                case "Head": return "Head Position";
                case "Neck": return "Neck Alignment";
                case "RightShoulder": return "Right Shoulder";
                case "LeftShoulder": return "Left Shoulder";
                case "RightElbow": return "Right Elbow";
                case "LeftElbow": return "Left Elbow";
                case "RightWrist": return "Right Wrist";
                case "LeftWrist": return "Left Wrist";
                case "RightHip": return "Right Hip";
                case "LeftHip": return "Left Hip";
                case "RightKnee": return "Right Knee";
                case "LeftKnee": return "Left Knee";
                case "RightAnkle": return "Right Ankle";
                case "LeftAnkle": return "Left Ankle";
                default: return technicalName;
            }
        }
        
        /// <summary>
        /// Draw a point on the canvas at the normalized coordinate with a label
        /// </summary>
        private void DrawPoint((double x, double y) point, string label)
        {
            int canvasX = (int)(point.x * (ConsoleWidth - 1));
            int canvasY = (int)(point.y * (ConsoleHeight - 1));
            
            if (canvasX >= 0 && canvasX < ConsoleWidth && canvasY >= 0 && canvasY < ConsoleHeight)
            {
                canvas[canvasY, canvasX] = SkeletonPointChar;
                
                // Draw label if it fits
                if (canvasX + label.Length < ConsoleWidth)
                {
                    for (int i = 0; i < label.Length; i++)
                    {
                        canvas[canvasY, canvasX + 1 + i] = label[i];
                    }
                }
            }
        }
        
        /// <summary>
        /// Draw a colored point directly to console with a label
        /// </summary>
        private void DrawColoredPoint((double x, double y) point, string label)
        {
            int canvasX = (int)(point.x * (ConsoleWidth - 1));
            int canvasY = (int)(point.y * (ConsoleHeight - 1));
            
            if (canvasX >= 0 && canvasX < ConsoleWidth)
            {
                Console.Write("".PadLeft(canvasX));
                Console.Write(SkeletonPointChar);
                Console.Write($" {label}");
                Console.WriteLine();
            }
        }
        
        /// <summary>
        /// Draw a line between two points on the canvas
        /// </summary>
        private void DrawLine((double x1, double y1) point1, (double x2, double y2) point2)
        {
            int x1 = (int)(point1.x1 * (ConsoleWidth - 1));
            int y1 = (int)(point1.y1 * (ConsoleHeight - 1));
            int x2 = (int)(point2.x2 * (ConsoleWidth - 1));
            int y2 = (int)(point2.y2 * (ConsoleHeight - 1));
            
            // Simple line drawing algorithm
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                if (x1 >= 0 && x1 < ConsoleWidth && y1 >= 0 && y1 < ConsoleHeight)
                {
                    if (canvas[y1, x1] != SkeletonPointChar)
                    {
                        canvas[y1, x1] = SkeletonLineChar;
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
        /// Render the canvas to the console
        /// </summary>
        private void Render()
        {
            Console.WriteLine(new string('-', ConsoleWidth + 2));
            
            for (int y = 0; y < ConsoleHeight; y++)
            {
                Console.Write("|");
                for (int x = 0; x < ConsoleWidth; x++)
                {
                    Console.Write(canvas[y, x]);
                }
                Console.WriteLine("|");
            }
            
            Console.WriteLine(new string('-', ConsoleWidth + 2));
        }
        
        /// <summary>
        /// Print pose guidance text with severity-based color coding
        /// </summary>
        public void DisplayPoseGuidance(string poseName, string[] adjustments, double accuracy)
        {
            // Add header with box border
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + new string('═', ConsoleWidth) + "╗");
            Console.WriteLine("║" + $" POSE GUIDANCE: {poseName} ".PadRight(ConsoleWidth) + "║");
            Console.WriteLine("╠" + new string('═', ConsoleWidth) + "╣");
            Console.ResetColor();
            
            // Show accuracy with color coding
            Console.Write("║ Overall Accuracy: ");
            ConsoleColor accuracyColor = GetMatchPercentageColor(accuracy);
            Console.ForegroundColor = accuracyColor;
            Console.Write($"{accuracy:F1}%");
            Console.ResetColor();
            Console.WriteLine("".PadRight(ConsoleWidth - 22) + "║");
            
            Console.WriteLine("║" + new string('─', ConsoleWidth) + "║");
            Console.WriteLine("║ Suggested Adjustments:".PadRight(ConsoleWidth) + "║");
            
            // Process adjustments with severity indicators
            foreach (var adjustment in adjustments)
            {
                string text = adjustment;
                ConsoleColor textColor = ConsoleColor.White;
                
                // Check for severity indicators and assign appropriate colors
                if (adjustment.StartsWith("[CRITICAL]"))
                {
                    text = adjustment.Replace("[CRITICAL]", "").Trim();
                    textColor = ConsoleColor.Red;
                    text = "⚠️ " + text;
                }
                else if (adjustment.StartsWith("[MAJOR]"))
                {
                    text = adjustment.Replace("[MAJOR]", "").Trim();
                    textColor = ConsoleColor.Yellow;
                    text = "⚡ " + text;
                }
                else if (adjustment.StartsWith("[MINOR]"))
                {
                    text = adjustment.Replace("[MINOR]", "").Trim();
                    textColor = ConsoleColor.Green;
                    text = "• " + text;
                }
                else
                {
                    text = "• " + text;
                }
                
                Console.Write("║ ");
                Console.ForegroundColor = textColor;
                Console.Write(text.PadRight(ConsoleWidth - 2));
                Console.ResetColor();
                Console.WriteLine("║");
            }
            
            Console.WriteLine("╚" + new string('═', ConsoleWidth) + "╝");
        }
    }
}