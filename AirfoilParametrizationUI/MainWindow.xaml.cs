using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using OxyPlot;
using AirfoilParametrizationLibrary;
using Cureos.Numerics.Optimizers;
using System.Linq;
using System.Text;
using System.Globalization;

namespace AirfoilParametrizationUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel viewModel;
        MainViewModel outViewModel;
        string currentPlotFile;

        public MainWindow()
        {
            InitializeComponent();

            currentPlotFile = GetDefaultFilePath();
            selectPathTextBox.Text = GetDefaultFilePath();
            outSelectPathTextBox.Text = Environment.CurrentDirectory + @"\airfoil_out.dat";

            viewModel = new MainViewModel(currentPlotFile);
            plot.Model = viewModel.AirfoilModel;

            outViewModel = new MainViewModel(outSelectPathTextBox.Text);
            outPlot.Model = outViewModel.AirfoilModel;
        }

        private void selectPathButton_Click(object sender, RoutedEventArgs e)
        {
            string filepath = GetDefaultFilePath();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Data files | *.dat";
            dialog.DefaultExt = "dat";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.ShowDialog();

            if (dialog.FileName != null && dialog.FileName.Length > 0)
            {
                filepath = Path.GetFullPath(dialog.FileName);
            }
            selectPathTextBox.Text = filepath;

            viewModel = new MainViewModel(selectPathTextBox.Text);
            plot.Model = viewModel.AirfoilModel;
        }

        public string GetDefaultFilePath()
        {
            return Environment.CurrentDirectory + @"\2412.dat";
        }

        private void methodComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (inputMethodComboBox.SelectedIndex)
            {
                case 0:
                    fromFilePanel.Visibility = Visibility.Visible;
                    nacaFourDigitPanel.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    fromFilePanel.Visibility = Visibility.Collapsed;
                    nacaFourDigitPanel.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            };
        }

        private void nacaFourDigitsTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            AirfoilGenerator.CreateNacaFourDigits(nacaFourDigitsTextBox.Text, Convert.ToInt32(panelsTextBox.Text));
            currentPlotFile = Environment.CurrentDirectory + @"\airfoil.dat";
            viewModel = new MainViewModel(currentPlotFile);
            plot.Model = viewModel.AirfoilModel;
        }

        private void saveFileCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            outPathPanel.Visibility = Visibility.Visible;
        }

        private void saveFileCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            outPathPanel.Visibility = Visibility.Collapsed;
        }

        private void outSelectPathButton_Click(object sender, RoutedEventArgs e)
        {
            string filepath = GetDefaultFilePath();

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Data files | *.dat";
            dialog.DefaultExt = "dat";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog.ShowDialog();

            if (dialog.FileName != null && dialog.FileName.Length > 0)
            {
                filepath = Path.GetFullPath(dialog.FileName);
            }
            outSelectPathTextBox.Text = filepath;
        }

        private void outputMethodComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            switch (outputMethodComboBox.SelectedIndex)
            {
                case 0:
                    outCstPanel.Visibility = Visibility.Visible;
                    outParsecPanel.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    outCstPanel.Visibility = Visibility.Collapsed;
                    outParsecPanel.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            };
        }

        private void findParametersButton_Click(object sender, RoutedEventArgs e)
        {
            double[] wL = new double[] { -0.99, -0.99, -0.99, -0.99 };
            double[] wU = new double[] { 0.99, 0.99, 0.99, 0.99 };

            var (xL, xU) = FileProcessor.GetXCoordFromFile(currentPlotFile);

            double[] w = wL.Concat(wU).ToArray();

            AirfoilGenerator.CreateCST(w, xL, xU);

            var optimizer = new Bobyqa(8, calcfc, new double[] { -1, -1, -1, -1, -1, -1, -1, -1}, new double[] { 1, 1, 1, 1, 1, 1, 1, 1 });
            var result = optimizer.FindMinimum(w);

            double[] outwL = new double[result.X.Length / 2];
            double[] outuL = new double[result.X.Length / 2];

            for (int i = 0; i < result.X.Length / 2; i++)
            {
                outuL[i] = result.X[i];
            }
            for (int i = result.X.Length / 2; i < result.X.Length; i++)
            {
                outwL[i - result.X.Length / 2] = result.X[i];
            }

            StringBuilder sb = new StringBuilder();
            foreach (double d in outuL)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:0.###}", d) + " ");
            }
            outCstUpperTextBox.Text = sb.ToString();
            sb.Clear();
            foreach (double d in outwL)
            {
                sb.Append(string.Format(CultureInfo.InvariantCulture, "{0:0.###}", d) + " ");
            }
            outCstLowerTextBox.Text = sb.ToString();

            outViewModel = new MainViewModel(Environment.CurrentDirectory + @"\airfoil_out.dat");
            outPlot.Model = outViewModel.AirfoilModel;

            if (saveFileCheckBox.IsChecked == true)
            {
                AirfoilGenerator.CreateCST(result.X, xL, xU, outSelectPathTextBox.Text);
            }
        }

        double calcfc(int n, double[] x)
        {
            var (xL, xU) = FileProcessor.GetXCoordFromFile(currentPlotFile);
            AirfoilGenerator.CreateCST(x, xL, xU);
            double error = AirfoilGenerator.FindError(currentPlotFile, Environment.CurrentDirectory + @"\airfoil_out.dat");
            return error;
        }
    }
}
