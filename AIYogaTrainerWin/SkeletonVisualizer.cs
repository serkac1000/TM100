using System;
using System.Collections.Generic;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Handles visualizing skeleton data in console application
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
        /// Print pose guidance text
        /// </summary>
        public void DisplayPoseGuidance(string poseName, string[] adjustments, double accuracy)
        {
            Console.WriteLine($"Current Pose: {poseName}");
            Console.WriteLine($"Accuracy: {accuracy:F1}%");
            
            Console.WriteLine("\nGuidance:");
            foreach (var adjustment in adjustments)
            {
                Console.WriteLine($"- {adjustment}");
            }
            
            Console.WriteLine();
        }
    }
}