using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using FocusedObjective.Common;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for ForecastDate.xaml
    /// </summary>
    public partial class ForecastDate : Window
    {

        ProjectState _state = null;
        int _monteCarloCycles = 100;
        string _aggregationValue = "";
        private XElement _forecastResults = null;
        private XElement _progressResults = null;
        private XElement _lastResults = null;
        private SimulatingProgress progress;
        private bool doingPermutations = false;

        public ObservableCollection<DateStatisticData> ProgressForecastDetails { get; set; }

        public XElement ForecastResults
        {
            get { return _forecastResults; }
            set { _forecastResults = value; }
        }

        public XElement ProgressResults
        {
            get { return _progressResults; }
            set { _progressResults = value; }
        }
        
        
        internal ForecastDate(ProjectState state, int monteCarloCycles = 100, string aggregationValue = "")
        {
            InitializeComponent();

            _state = state;
            _monteCarloCycles = monteCarloCycles;
            _aggregationValue = aggregationValue;

            labelPermutation.IsEnabled = false;
            comboBoxPermutationResult.IsEnabled = false;
        }

        private void buttonExecute_Click(object sender, RoutedEventArgs e)
        {
            doingPermutations = false;
            progressChartBuilt = false;

            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, _monteCarloCycles);

            _state.SimulateAdvancedCommand(
                backgroundWorkerAdvancedSim_RunWorkerCompleted,
                backgroundWorkerAdvancedSim_ReportProgress,
                worker,
                progress,
                _monteCarloCycles, 
                _aggregationValue,
                true,
                Contract.ForecastPermutationsEnum.None);
        }


        void backgroundWorkerAdvancedSim_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            progress.SetProgressMessage(e.ProgressPercentage, (string)e.UserState);
        }

        void backgroundWorkerAdvancedSim_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            progress.Close();
           
            if (e.Error != null)
            {
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates and that you have a valid <forecastDate ...> section in your model.", "Unable to simulate...");
                this.Close();
                return;
            }

            if (e.Cancelled || (e.Result != null && ((FocusedObjective.KanbanSim.ProjectState.SimResult)e.Result).Canceled))
            {
                MessageBox.Show("Simulation was canceled.");
                this.Close();
                return;
            }

            FocusedObjective.KanbanSim.ProjectState.SimResult result = (FocusedObjective.KanbanSim.ProjectState.SimResult)e.Result;

            if (result.Result)
            {
                if (doingPermutations)
                {
                    // bind the perutation results to combo box
                    comboBoxPermutationResult.DataContext =  result.Simulator.Result.Element("forecastDatePermutations");

                    labelPermutation.IsEnabled = true;
                    comboBoxPermutationResult.IsEnabled = true;

                    // select the first as the default
                    if (comboBoxPermutationResult.HasItems)
                        comboBoxPermutationResult.SelectedIndex = 0;

                    // clear the forecast grid
                    dataGridResults.DataContext = null;
                }
                else
                {
                    // bind results;
                    _lastResults = result.Simulator.Result;
                    _forecastResults = result.Simulator.Result.Element("forecastDate").Element("dates");
                    dataGridResults.DataContext = _forecastResults;

                    //forecastChart.DataContext = _forecastResults;
                    //forecastSeries.DataContext = _forecastResults;
                    //forecastChart.DataContext = _forecastResults;
                    //forecastSeries.ItemsSource = _forecastResults.Elements("date");


                    dataGridPermutationResults.DataContext = null;

                    labelPermutation.IsEnabled = false;
                    comboBoxPermutationResult.IsEnabled = false;

                    _progressResults = result.Simulator.Result.Element("forecastDate").Element("progress");
                   
                }
            }
            else
            {
               // report error
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates and that you have a valid <forecastDate ...> section in your model.", "Unable to simulate...");
                this.Close();
            }
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //tabItemVariance.Header = "Completion Progress";
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            //processProgressChart();
        }

        /*
        private void processProgressChart()
        {

            //tabItemVariance.Header = "Variance (building...)";

            Dispatcher.Invoke(new Action(()=> tabItemVariance.Header = "Tracking progress (building...)"), null);

            //TODO: Need to do a do events here...

            try
            {
                if (_progressResults != null)
                {
                    if (_progressResults.Attribute("dateFormat") != null)
                        dateAxis.LabelFormat = _progressResults.Attribute("dateFormat").Value;

                    ProgressForecastDetails = new ObservableCollection<DateStatisticData>();

                    foreach (var d in _progressResults.Elements("date"))
                    {
                        var data = new DateStatisticData();
                        data.FromXML(d.Element("forecast"));

                        data.DateData = d.Attribute("date").Value.ToSafeDate(dateAxis.LabelFormat, null) ?? DateTime.Today;

                        var actual = d.Attribute("actual");
                        if (actual != null)
                            data.ActualCompletedCount = double.Parse(d.Attribute("actual").Value);
                        else
                            data.ActualCompletedCount = double.NaN;

                        var annotation = d.Attribute("annotation");
                        if (annotation != null && !string.IsNullOrWhiteSpace(annotation.Value))
                            data.Annotation = annotation.Value;
                        else
                            data.Annotation = null;

                        var likelihood = d.Attribute("likelihood");
                        if (likelihood != null && !string.IsNullOrWhiteSpace(likelihood.Value))
                            data.Likelihood = double.Parse(likelihood.Value);
                        else
                            data.Likelihood = double.NaN;

                        var targetLikelihood = d.Attribute("targetLikelihood");
                        if (targetLikelihood != null && !string.IsNullOrWhiteSpace(targetLikelihood.Value))
                            data.TargetLikelihood = bool.Parse(targetLikelihood.Value);


                        ProgressForecastDetails.Add(data);
                    }

                    // find the boundaries
                    //int firstAboveZero          = ProgressForecastDetails.IndexOf(ProgressForecastDetails.OrderBy(pfd => pfd.Likelihood).First(pfd => pfd.Likelihood > 0.0));
                    //int firstAboveFifty         = ProgressForecastDetails.IndexOf(ProgressForecastDetails.OrderBy(pfd => pfd.Likelihood).First(pfd => pfd.Likelihood > 0.5));
                    //int firstAboveSeventyFive   = ProgressForecastDetails.IndexOf(ProgressForecastDetails.OrderBy(pfd => pfd.Likelihood).First(pfd => pfd.Likelihood > 0.75));

                    //ProgressForecastDetails.OrderBy(pfd => pfd.Likelihood).First(pfd => pfd.Likelihood > 0.0).DateData

                    //lowestStripline.Start = firstAboveZero;
                    //lowestStripline.Width = firstAboveFifty - firstAboveZero;
                    //TODO: LowMedStripline.Offset = firstAboveFifty;
                    //TODO: LowMedStripline.Width = firstAboveSeventyFive - firstAboveFifty;
                    //TODO: HighMedStripline.Offset = firstAboveSeventyFive;
                    //TODO: HighMedStripline.Width = ProgressForecastDetails.Count - firstAboveSeventyFive;

                    
                    var targEntry = ProgressForecastDetails.OrderBy(pfd => pfd.Likelihood).FirstOrDefault(pfd => pfd.TargetLikelihood == true);

                    if (targEntry != null)
                    {
                        markerLine.X1 = targEntry.DateData;
                        

                        //markerLabel.X1 = targEntry.DateData;
                        //markerLabel.Y1 = 5;
 
                        //markerLabel.Text = string.Format("{0}%", targEntry.Likelihood * 100);
                    }


                    forecastSeriesHighLow.ItemsSource = ProgressForecastDetails;
                    forecastSeriesMedian.ItemsSource = ProgressForecastDetails;
                    actualSeries.ItemsSource = ProgressForecastDetails;
                }
            }
            finally
            {
                tabItemVariance.Header = "Tracking progress";
            }
        }
        */

        public class DateStatisticData : StatisticData
        {
            public DateTime DateData { get; set; }
            public double ActualCompletedCount { get; set; }
            public string Annotation { get; set; }
            public double Likelihood { get; set; }
            public bool TargetLikelihood { get; set; }
        }
            
        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            buttonExecute_Click(sender, e);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void buttonAsHTML_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResults != null)
            {
                // the data first
                string temp = System.IO.Path.GetTempFileName();
                temp = System.IO.Path.ChangeExtension(temp, "xml");
                System.IO.File.WriteAllText(temp, _lastResults.ToString());
                System.Diagnostics.Process.Start(temp);

                // now do the html templates for the whole file....
                //GenerateAndLaunchHTMLReport();
            }
            else
            {
                MessageBox.Show("Simulation failed.", "No Data");
            }
        }


        private void buttonPermutationExecute_Click(object sender, RoutedEventArgs e)
        {
            doingPermutations = true; 

            Contract.ForecastPermutationsEnum permutation = Contract.ForecastPermutationsEnum.None;

            // get the perm type as an enum
            switch (comboBoxPermutations.SelectedIndex)
            {
                case 0: permutation = Contract.ForecastPermutationsEnum.SequentialBacklog; break;
                case 1: permutation = Contract.ForecastPermutationsEnum.SequentialDeliverables; break;
                case 2: permutation = Contract.ForecastPermutationsEnum.Deliverables; break;
                default:
                    break;
            }

            // launch the command
            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, _monteCarloCycles);

            _state.SimulateAdvancedCommand(
                backgroundWorkerAdvancedSim_RunWorkerCompleted,
                backgroundWorkerAdvancedSim_ReportProgress,
                worker,
                progress,
                _monteCarloCycles,
                _aggregationValue,
                true,
                permutation);

        }

        private void comboBoxPermutationResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // bind to this result
            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var q = e.AddedItems[0] as XElement;

                if (q != null)
                {
                    dataGridPermutationResults.DataContext = q.Element("forecastDate").Element("dates");
                }
            }

        }

        private void buttonTargetDateExecute_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool progressChartBuilt = false;

        private void tabItemVariance_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!progressChartBuilt)
            {
                progressChartBuilt = true;
                //processProgressChart();
            }
        }

    }
}
