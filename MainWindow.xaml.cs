using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PCAGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog
        {
            Filter = "CSV Files|*.csv|Excel Files|*.xlsx|All Files|*.*"
        };

        if (ofd.ShowDialog() == true)
        {
            DataFileTextBox.Text = ofd.FileName;
        }
    }

    private void RunPCA_Click(object sender, RoutedEventArgs e)
    {
       /* if (string.IsNullOrWhiteSpace(DataFileTextBox.Text) || !File.Exists(DataFileTextBox.Text))
        {
            MessageBox.Show("Please select a valid input file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string inputFile = DataFileTextBox.Text;
        string groupVar = GroupVariableTextBox.Text;
        string shapeVar = ShapeVariableTextBox.Text;
        bool showNames = ShowSampleNamesCheckBox.IsChecked ?? false;

        try
        {
            // Call Python PCA function
            string outputDir = PythonRunner.RunPCA(inputFile, groupVar, shapeVar, showNames);

            // Open PCA results window
            PCAWindow pcaWindow = new PCAWindow(outputDir);
            pcaWindow.Show();
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error running PCA: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
       */
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}