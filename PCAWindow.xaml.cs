using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PCAGenerator
{
    /// <summary>
    /// Interaction logic for PCAWindow.xaml
    /// </summary>
    public partial class PCAWindow : Window
    {

        private string outputDir;
        private PCA pCA;
        private List<string> samples;

        public PCAWindow(string outputDirectory, PCA pCAObject, List<string> allSamples)
        {
            InitializeComponent();
            outputDir = outputDirectory;
            pCA = pCAObject;
            samples = allSamples;
            ValuesListBox.ItemsSource = samples;
            LoadPlot();


            var preselect = pCA.ExcludingSamples ?? new List<string>();

            foreach (var item in preselect)
            {
                var match = ValuesListBox.Items.Cast<string>()
                    .FirstOrDefault(x => x == item);
                if (match != null)
                    ValuesListBox.SelectedItems.Add(match);
            }

            // Update dropdown button content with selected values
            var selected = ValuesListBox.SelectedItems.Cast<string>().ToList();
            DropDownButton.Content = selected.Count > 0
                ? string.Join(", ", selected)
                : "Click Here!";

        }

        private void LoadPlot()
        {

            PCAPlotImage.Source = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();


            if (File.Exists(outputDir))
            {
                using (var stream = new FileStream(outputDir, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    PCAPlotImage.Source = bitmap;
                }
            }
            else
            {
                ResultsTitle.Text = "PCA plot not found.";
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            string outputFolderPath = System.IO.Path.GetDirectoryName(outputDir);

            if (Directory.Exists(outputFolderPath))
            {
                Process.Start("explorer.exe", outputFolderPath);
            }
        }

        private void Modify_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Show();   // show the already existing one
                this.Close();        // close the PCA window
            }
        }

        private void Regenerate_Click(object sender, RoutedEventArgs e)
        {
            
            this.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            pCA.ExcludingSamples = ValuesListBox.SelectedItems.Cast<string>().ToList();
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

                    outputDir = result[0].ToString();
                    List<string> usedSamples = new List<string>();
                    foreach (var s in result[1])
                        usedSamples.Add(s.ToString());
                    samples = new List<string>();
                    foreach (var s in result[2])
                        samples.Add(s.ToString());

                    this.Close();

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

        private void DropDownButton_Checked(object sender, RoutedEventArgs e)
        {
            DropDownPopup.IsOpen = true;
        }

        private void DropDownButton_Unchecked(object sender, RoutedEventArgs e)
        {
            DropDownPopup.IsOpen = false;

            // Update button text with selected items
            var selected = ValuesListBox.SelectedItems.Cast<string>().ToList();
            DropDownButton.Content = selected.Count > 0 ? string.Join(", ", selected) : "Click Here!";
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX *= 1.2; // increase by 20%
            ZoomTransform.ScaleY *= 1.2;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX /= 1.2; // decrease by 20%
            ZoomTransform.ScaleY /= 1.2;
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            ZoomTransform.ScaleX = 1.0;
            ZoomTransform.ScaleY = 1.0;
        }

    }
}
