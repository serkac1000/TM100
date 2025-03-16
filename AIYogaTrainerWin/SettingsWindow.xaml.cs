using System;
using System.Windows;
using Microsoft.Win32;

namespace AIYogaTrainerWin
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public Settings Settings { get; private set; }

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            Settings = new Settings
            {
                ModelPath = settings.ModelPath,
                Pose1Name = settings.Pose1Name,
                Pose2Name = settings.Pose2Name,
                Pose3Name = settings.Pose3Name,
                Pose1AudioPath = settings.Pose1AudioPath,
                Pose2AudioPath = settings.Pose2AudioPath,
                Pose3AudioPath = settings.Pose3AudioPath
            };

            // Populate form fields with current settings
            ModelPathTextBox.Text = Settings.ModelPath;
            Pose1NameTextBox.Text = Settings.Pose1Name;
            Pose2NameTextBox.Text = Settings.Pose2Name;
            Pose3NameTextBox.Text = Settings.Pose3Name;
            Pose1AudioTextBox.Text = Settings.Pose1AudioPath;
            Pose2AudioTextBox.Text = Settings.Pose2AudioPath;
            Pose3AudioTextBox.Text = Settings.Pose3AudioPath;
        }

        private void BrowseModelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Model files (*.pb;*.onnx)|*.pb;*.onnx|All files (*.*)|*.*",
                Title = "Select TensorFlow or ONNX Model"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ModelPathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowsePose1AudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*",
                Title = "Select Audio File for Pose 1"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Pose1AudioTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowsePose2AudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*",
                Title = "Select Audio File for Pose 2"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Pose2AudioTextBox.Text = openFileDialog.FileName;
            }
        }

        private void BrowsePose3AudioButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*",
                Title = "Select Audio File for Pose 3"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Pose3AudioTextBox.Text = openFileDialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate model path
            if (string.IsNullOrWhiteSpace(ModelPathTextBox.Text))
            {
                MessageBox.Show("Please select a model file.", "Missing Model", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update settings with form values
            Settings.ModelPath = ModelPathTextBox.Text;
            Settings.Pose1Name = string.IsNullOrWhiteSpace(Pose1NameTextBox.Text) ? "Pose 1" : Pose1NameTextBox.Text;
            Settings.Pose2Name = string.IsNullOrWhiteSpace(Pose2NameTextBox.Text) ? "Pose 2" : Pose2NameTextBox.Text;
            Settings.Pose3Name = string.IsNullOrWhiteSpace(Pose3NameTextBox.Text) ? "Pose 3" : Pose3NameTextBox.Text;
            Settings.Pose1AudioPath = Pose1AudioTextBox.Text;
            Settings.Pose2AudioPath = Pose2AudioTextBox.Text;
            Settings.Pose3AudioPath = Pose3AudioTextBox.Text;

            // Close dialog with success
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
            Close();
        }
    }
}
