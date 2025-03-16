using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Handles visualization of skeleton keypoints on a canvas
    /// </summary>
    public class SkeletonVisualizer
    {
        private Canvas canvas;
        private const double POINT_RADIUS = 5;
        private const double LINE_THICKNESS = 3;
        
        // Define skeleton connections (pairs of keypoint indices that should be connected by lines)
        private readonly int[][] connections = new int[][]
        {
            // Head to shoulders
            new int[] { 0, 1 },
            new int[] { 0, 2 },
            
            // Shoulders to elbows
            new int[] { 1, 3 },
            new int[] { 2, 4 },
            
            // Elbows to wrists
            new int[] { 3, 5 },
            new int[] { 4, 6 },
            
            // Shoulders to hips
            new int[] { 1, 7 },
            new int[] { 2, 8 },
            
            // Hips to knees
            new int[] { 7, 9 },
            new int[] { 8, 10 },
            
            // Knees to ankles
            new int[] { 9, 11 },
            new int[] { 10, 12 }
        };
        
        /// <summary>
        /// Initializes a new instance of the SkeletonVisualizer class
        /// </summary>
        /// <param name="canvas">The canvas to draw on</param>
        public SkeletonVisualizer(Canvas canvas)
        {
            this.canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));
        }
        
        /// <summary>
        /// Draws a skeleton on the canvas based on detected keypoints
        /// </summary>
        /// <param name="keypoints">Array of normalized keypoint coordinates (x1, y1, x2, y2, ...)</param>
        public void DrawSkeleton(float[] keypoints)
        {
            if (keypoints == null || keypoints.Length < 2)
                return;
                
            // Clear previous skeleton
            canvas.Children.Clear();
            
            // Calculate scale to fit the canvas
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;
            
            // Draw connections first (so points are drawn on top)
            foreach (var connection in connections)
            {
                if (connection.Length == 2)
                {
                    int index1 = connection[0];
                    int index2 = connection[1];
                    
                    // Check if indices are valid
                    if (index1 * 2 + 1 < keypoints.Length && index2 * 2 + 1 < keypoints.Length)
                    {
                        // Get keypoint coordinates and scale to canvas size
                        double x1 = keypoints[index1 * 2] * canvasWidth;
                        double y1 = keypoints[index1 * 2 + 1] * canvasHeight;
                        double x2 = keypoints[index2 * 2] * canvasWidth;
                        double y2 = keypoints[index2 * 2 + 1] * canvasHeight;
                        
                        // Draw line
                        Line line = new Line
                        {
                            X1 = x1,
                            Y1 = y1,
                            X2 = x2,
                            Y2 = y2,
                            StrokeThickness = LINE_THICKNESS,
                            Stroke = new LinearGradientBrush
                            {
                                StartPoint = new Point(0, 0),
                                EndPoint = new Point(1, 1),
                                GradientStops = new GradientStopCollection
                                {
                                    new GradientStop { Color = Colors.Cyan, Offset = 0 },
                                    new GradientStop { Color = Colors.Magenta, Offset = 1 }
                                }
                            }
                        };
                        
                        canvas.Children.Add(line);
                    }
                }
            }
            
            // Draw keypoints
            for (int i = 0; i < keypoints.Length / 2; i++)
            {
                // Get keypoint coordinates and scale to canvas size
                double x = keypoints[i * 2] * canvasWidth;
                double y = keypoints[i * 2 + 1] * canvasHeight;
                
                // Draw circle
                Ellipse ellipse = new Ellipse
                {
                    Width = POINT_RADIUS * 2,
                    Height = POINT_RADIUS * 2,
                    Fill = Brushes.Cyan
                };
                
                // Add drop shadow effect
                ellipse.Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Cyan,
                    ShadowDepth = 0,
                    BlurRadius = 10,
                    Opacity = 0.8
                };
                
                // Position ellipse
                Canvas.SetLeft(ellipse, x - POINT_RADIUS);
                Canvas.SetTop(ellipse, y - POINT_RADIUS);
                
                canvas.Children.Add(ellipse);
            }
        }
    }
}
