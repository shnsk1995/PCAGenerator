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
using Python.Runtime;
using System.IO;
using System;
using System.IO.Enumeration;
using System.Windows.Controls.Primitives;

namespace PCAGenerator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        PCA_by_Group.IsChecked = true;
        CategorizebyColor.IsChecked = true;
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

    

    private void Category_Checked(object sender, RoutedEventArgs e)
    {
        
    }

    private void RunPCA_Click(object sender, RoutedEventArgs e)
    {

        /*[System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetDllDirectory(string lpPathName);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string pythonDir = System.IO.Path.Combine(baseDir, "Python");
        Console.WriteLine(pythonDir);
        SetDllDirectory(pythonDir);
        Runtime.PythonDLL = System.IO.Path.Combine(pythonDir, "python312.dll");
        Environment.SetEnvironmentVariable("TCL_LIBRARY", System.IO.Path.Combine(pythonDir, "tcl", "tcl8.6"));
        Environment.SetEnvironmentVariable("TK_LIBRARY", System.IO.Path.Combine(pythonDir, "tcl", "tk8.6"));

        /*if (string.IsNullOrWhiteSpace(DataFileTextBox.Text) || !File.Exists(DataFileTextBox.Text))
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

            PythonEngine.PythonHome = pythonDir;
            PythonEngine.PythonPath = string.Join(";", new[]{
    pythonDir,
    System.IO.Path.Combine(pythonDir, "Lib"),
    System.IO.Path.Combine(pythonDir, "Lib", "site-packages")
});

            // Extra safety: clear PYTHONHOME and PYTHONPATH env vars 
            // so it doesn’t fall back to global Python
            Environment.SetEnvironmentVariable("PYTHONHOME", pythonDir);
            Environment.SetEnvironmentVariable("PYTHONPATH", PythonEngine.PythonPath);
            PythonEngine.Initialize();

            Console.WriteLine("Looking for Python at: " + Runtime.PythonDLL);
            Console.WriteLine("PythonHome: " + PythonEngine.PythonHome);
            Console.WriteLine("PythonPath: " + PythonEngine.PythonPath);

            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                Console.WriteLine("Python executable: " + sys.executable);
                Console.WriteLine("Python version: " + sys.version);
                Console.WriteLine("Sys.path: " + sys.path);
                sys.path.append(System.IO.Path.Combine(pythonDir, "DLLs"));
                sys.path.append(System.IO.Path.Combine(pythonDir, "Lib"));
                sys.path.append(System.IO.Path.Combine(pythonDir, "Lib", "site-packages"));

                // Add the PythonScripts folder to sys.path
                string scriptPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python");
                sys.path.append(scriptPath);

                // Import your script
                dynamic myscript = Py.Import("PCA");
                string outputDir = myscript.pca_plot();

                // Open PCA results window
                PCAWindow pcaWindow = new PCAWindow(outputDir);
                pcaWindow.Show();
            }





            this.Close();
        }
        catch (PythonException ex)
        {

            string errorMessage = $"Python error:\n{ex.Message}\n\n{ex.StackTrace}";

            // Log to file
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PythonErrors.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMessage}\n\n");

            // Python errors
            MessageBox.Show($"Python error:\n{ex.Message}\n\n{ex.StackTrace}");


        }
        catch (Exception ex)
        {
            string errorMessage = $".NET error:\n{ex.Message}\n\n{ex.StackTrace}";

            // Log to file
            string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NETErrors.log");
            File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMessage}\n\n");

            // .NET errors
            MessageBox.Show($".NET error:\n{ex.Message}\n\n{ex.StackTrace}");
        }
        finally
        {
            if (PythonEngine.IsInitialized)
                PythonEngine.Shutdown();
        }
        */

        if (string.IsNullOrWhiteSpace(DataFileTextBox.Text) || !File.Exists(DataFileTextBox.Text))
        {
            MessageBox.Show("Please select a valid input file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        PCA pCA = new PCA();

        pCA.InputFile = DataFileTextBox.Text;

        if (PCA_by_Group.IsChecked == true)
        {
            if (CategorizebyColor.IsChecked == true)
            {
                pCA.ColorVar = "Group";
                pCA.ShapeVar = null;
            }
            else if (CategorizebyShape.IsChecked == true)
            {
                pCA.ColorVar = null;
                pCA.ShapeVar = "Group";
            }
        }
        else if (PCA_by_GroupAndCondition.IsChecked == true)
        {
            if (CategorizebyColorAndShape.IsChecked == true)
            {
                pCA.ColorVar = "Group";
                pCA.ShapeVar = "Condition";
            }
            else if (CategorizebyShapeAndColor.IsChecked == true)
            {
                pCA.ColorVar = "Condition";
                pCA.ShapeVar = "Group";
            }
        }
        
        pCA.ShowNames = ShowSampleNamesCheckBox.IsChecked ?? false;
        pCA.PlotTitle = PlotTitleTextBox.Text;
        pCA.FileName = FileNameTextBox.Text;

        pCA.SampleNameFontSize = int.Parse(((ComboBoxItem)SampleNameFontSize.SelectedItem).Content.ToString());
        string sampleNameOffset = (SampleNameOffset.SelectedItem as ComboBoxItem).Content.ToString();
        string[] parts = sampleNameOffset.Trim('(', ')').Split(',');
        pCA.Offset = parts.Select(int.Parse).ToArray();

        pCA.PointSize = int.Parse(((ComboBoxItem)PointSize.SelectedItem).Content.ToString());
        pCA.PointTransparency = float.Parse(((ComboBoxItem)PointTransparency.SelectedItem).Content.ToString(), System.Globalization.CultureInfo.InvariantCulture);

        pCA.TitleFontSize = int.Parse(((ComboBoxItem)TitleFontSize.SelectedItem).Content.ToString());
        pCA.AxisTitleFontSize = int.Parse(((ComboBoxItem)AxisTitleFontSize.SelectedItem).Content.ToString());
        pCA.AxisLabelFontSize = int.Parse(((ComboBoxItem)AxisLabelFontSize.SelectedItem).Content.ToString());
        pCA.LegendTitleFontSize = int.Parse(((ComboBoxItem)LegendTitleFontSize.SelectedItem).Content.ToString());
        pCA.LegendFontSize = int.Parse(((ComboBoxItem)LegendFontSize.SelectedItem).Content.ToString());

        pCA.ShowGrid = ShowGridCheckBox.IsChecked ?? false;
        pCA.GridStyle = (GridStyle.SelectedItem as ComboBoxItem).Content.ToString();
        pCA.GridTransparency = float.Parse(((ComboBoxItem)GridTransparency.SelectedItem).Content.ToString(), System.Globalization.CultureInfo.InvariantCulture);

        pCA.PlotWidth = int.Parse(((ComboBoxItem)PlotWidth.SelectedItem).Content.ToString());
        pCA.PlotHeight = int.Parse(((ComboBoxItem)PlotHeight.SelectedItem).Content.ToString());
        pCA.Dpi = int.Parse(((ComboBoxItem)DPI.SelectedItem).Content.ToString());


            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string pythonDir = System.IO.Path.Combine(baseDir, "Python");

            try
            {
                // Tell pythonnet where Python is
                Runtime.PythonDLL = System.IO.Path.Combine(pythonDir, "python312.dll");
                PythonEngine.PythonHome = pythonDir;
                PythonEngine.PythonPath = string.Join(";", new[] {
                pythonDir,
                System.IO.Path.Combine(pythonDir, "Lib"),
                System.IO.Path.Combine(pythonDir, "Lib", "site-packages"),
                System.IO.Path.Combine(pythonDir, "DLLs")
            });

                PythonEngine.Initialize();

                Console.WriteLine("Looking for Python at: " + Runtime.PythonDLL);
                Console.WriteLine("PythonHome: " + PythonEngine.PythonHome);
                Console.WriteLine("PythonPath: " + PythonEngine.PythonPath);

                using (Py.GIL())
                {
                    dynamic sys = Py.Import("sys");

                    // Add custom script folder
                    string scriptPath = System.IO.Path.Combine(baseDir, "Python");
                    sys.path.append(scriptPath);

                    
                    // Import and run your PCA script
                    dynamic pcaScript = Py.Import("PCA");
                    var result = pcaScript.pca_plot(
                        pCA.InputFile,
                        pCA.ColorVar, 
                        pCA.ShapeVar, 
                        pCA.ShowNames,
                        pCA.PlotTitle,
                        pCA.FileName,
                        pCA.SampleNameFontSize,
                        pCA.Offset,
                        pCA.PointTransparency,
                        pCA.PointSize,
                        pCA.TitleFontSize,
                        pCA.AxisTitleFontSize,
                        pCA.AxisLabelFontSize,
                        pCA.LegendTitleFontSize,
                        pCA.LegendFontSize,
                        pCA.PlotWidth,
                        pCA.PlotHeight,
                        pCA.Dpi,
                        pCA.ShowGrid,
                        pCA.GridStyle,
                        pCA.GridTransparency,
                        pCA.ExcludingSamples
                        );

                string outputDir = result[0].ToString();
                List<string> usedSamples = new List<string>();
                foreach (var s in result[1])
                    usedSamples.Add(s.ToString());
                List<string> samples = new List<string>();
                foreach (var s in result[2])
                    samples.Add(s.ToString());

                // Open PCA results window
                PCAWindow pcaWindow = new PCAWindow(outputDir, pCA, samples);
                pcaWindow.Show();
                }
            }
            catch (PythonException ex)
            {
                HandleError("Python", ex.Message, ex.StackTrace);
            }
            catch (Exception ex)
            {
                HandleError(".NET", ex.Message, ex.StackTrace);
            }
            finally
            {
                if (PythonEngine.IsInitialized)
                    PythonEngine.Shutdown();
            }


    }

    private void HandleError(string type, string message, string stackTrace)
    {
        string errorMessage = $"{type} error:\n{message}\n\n{stackTrace}";
        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{type}Errors.log");

        File.AppendAllText(logPath, $"[{DateTime.Now}] {errorMessage}\n\n");
        MessageBox.Show(errorMessage);
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void OutputDirectory_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new Microsoft.Win32.OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select this folder"
        };

        if (ofd.ShowDialog() == true)
        {
            string folderPath = System.IO.Path.GetDirectoryName(ofd.FileName);
            DataFileTextBox.Text = folderPath;
        }

    }

    private void ShowGridCheckBoxTriggered(object sender, RoutedEventArgs e)
    {
        GridStyle.IsEnabled = ShowGridCheckBox.IsChecked == true;
        GridTransparency.IsEnabled = ShowGridCheckBox.IsChecked == true;
    }

    private void ShowSampleNamesCheckBoxTriggered(object sender, RoutedEventArgs e)
    {
        SampleNameFontSize.IsEnabled = ShowSampleNamesCheckBox.IsChecked == true;
        SampleNameOffset.IsEnabled = ShowSampleNamesCheckBox.IsChecked == true;
    }

    private void PCA_by_Group_Triggered(object sender, RoutedEventArgs e)
    {
        GroupOnly.IsEnabled = PCA_by_Group.IsChecked == true;
        if(GroupOnly.IsEnabled == true)
        {
            CategorizebyColor.IsChecked = true;
        }
    }

    private void PCA_by_GroupAndCondition_Triggered(object sender, RoutedEventArgs e)
    {
        GroupAndCondition.IsEnabled = PCA_by_GroupAndCondition.IsChecked == true;
        if (GroupAndCondition.IsEnabled == true)
        {
            CategorizebyColorAndShape.IsChecked = true;
        }
    }
}