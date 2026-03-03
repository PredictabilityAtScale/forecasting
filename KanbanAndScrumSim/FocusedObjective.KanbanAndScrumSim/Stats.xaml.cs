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
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using LiveCharts;
using LiveCharts.Wpf;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for Stats.xaml
    /// </summary>
    public partial class StatsWindow : Window, INotifyPropertyChanged
    {
        private XElement _statsResults = null;
        private XElement _lastResults = null;
        private SimulatingProgress progress;
        public event PropertyChangedEventHandler PropertyChanged;

        private StatisticData _currentData = new StatisticData();

        public StatisticData CurrentData
        {
            get
            {
                return _currentData;
            }
            set
            {
                _currentData = value;
            }
        }
        
        public XElement StatsResults
        {
            get { return _statsResults; }
            set { _statsResults = value; }
        }


        public StatsWindow()
        {
            InitializeComponent();
            tabItemRandomNumbers.Visibility = System.Windows.Visibility.Collapsed;

            Formatter = value => value.ToString();
            SecondaryFormatter = value => value.ToString() + "%";

            DataContext = this;
        }


        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Func<double, string> SecondaryFormatter { get; set; }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonExecute_Click(object sender, RoutedEventArgs e)
        {
            FocusedObjective.Contract.ExecuteSummaryStatisticsData command = new Contract.ExecuteSummaryStatisticsData();
            command.ReturnData = true;
            
            if (textBoxData.Text.Contains("<distribution "))
            {
                XElement xmlData = XElement.Parse(textBoxData.Text);
                XElement errors = new XElement("errors");

                Contract.SetupDistributionData dist = new Contract.SetupDistributionData(xmlData, errors);

                dist.Separator = "\n";

                // workaround: trouble roundtripping a double.Min.Max value
                if (dist.LowBound == double.Parse(double.MinValue.ToString("R")))
                    dist.LowBound = int.MinValue;

                if (dist.HighBound == double.Parse(double.MaxValue.ToString("R")))
                    dist.HighBound = int.MaxValue;

                if (dist.Validate(null, errors))
                {
                    command.Distribution = dist;
                }
                else
                {
                    string errorText = "Failed to create the distribution due to the following errors:";
 
                    foreach (var error in errors.Elements("error"))
	                {
                        errorText += Environment.NewLine + string.Format("{0}",
                            error.Value);
	                }

                    MessageBox.Show(errorText, "Failed to create distribution.");
                    return;
                }
            }
            else
            {
                // raw data
                command.Distribution = null;
                command.Data = textBoxData.Text;

                XElement errors = new XElement("errors");

                if (!command.Validate(null, errors))
                {
                    string errorText = "Failed to parse sample data due to the following errors:";

                    foreach (var error in errors.Elements("error"))
                    {
                        errorText += Environment.NewLine + string.Format("{0}",
                            error.Value);
                    }

                    MessageBox.Show(errorText, "Failed to parse sample data.");
                    return;
                }
            }

            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, 1);

            // build the sim data and pass to the worker
            FocusedObjective.Contract.SimulationData simData = new Contract.SimulationData();
            Contract.ExecuteData simExecute = new Contract.ExecuteData();
            Contract.SetupData simSetup = new Contract.SetupData();
            
            simData.Execute = simExecute;
            simData.Setup = simSetup;

            simExecute.SummaryStatistics = command;

            worker.DoWork -= backgroundWorkerAdvancedSim_DoWork;
            worker.DoWork += backgroundWorkerAdvancedSim_DoWork;

            worker.RunWorkerCompleted -= backgroundWorkerAdvancedSim_RunWorkerCompleted;
            worker.RunWorkerCompleted += backgroundWorkerAdvancedSim_RunWorkerCompleted;

            worker.ProgressChanged -= backgroundWorkerAdvancedSim_ReportProgress;
            worker.ProgressChanged += backgroundWorkerAdvancedSim_ReportProgress;

            worker.RunWorkerAsync(simData);
        }

        internal void backgroundWorkerAdvancedSim_DoWork(object sender, DoWorkEventArgs e)
        {
            Contract.SimulationData data = e.Argument as Contract.SimulationData;

            FocusedObjective.Simulation.Simulator sim = 
                    new Simulation.Simulator(data.AsXML(
                    data.Execute.SimulationType).ToString());

            e.Result = sim;

            bool result = sim.Execute(false, null);
        }

        void backgroundWorkerAdvancedSim_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
        }

        void backgroundWorkerAdvancedSim_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            progress.Close();

            if (e.Error != null)
            {
                MessageBox.Show("An error occured during analysis. Check the data for formatting errors.", "Unable to analyze...");
                this.Close();
                return;
            }

            if (e.Cancelled)
            {
                MessageBox.Show("Analysis was canceled.");
                this.Close();
                return;
            }

            FocusedObjective.Simulation.Simulator sim = (FocusedObjective.Simulation.Simulator)e.Result;

            if (sim.Result != null 
                && sim.Result.Element("summaryStatistics") != null)
            {
                // bind results;
                _lastResults = sim.Result;
                _statsResults = sim.Result.Element("summaryStatistics");
                tabControl1.SelectedIndex = 2; // chart tab
                bindResults();
            }
            else
            {
                _lastResults = null;
                _statsResults = null;

                // report error
                MessageBox.Show("An error occured during analysis. Check the data for formatting errors.", "Unable to analyze..."); 
                this.Close();
            }
        }

        private void bindResults()
        {
            if (!bindHistogramAndCurrentDataPanel())
            {
                CurrentData = new StatisticData();
                //histogramSeries.ItemsSource = null;
                //histogramCDFSeries.ItemsSource = null;
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private bool bindHistogramAndCurrentDataPanel()
        {
            bool result = false;

            if (StatsResults != null)
            {
                CurrentData.FromXML(StatsResults);

                try
                {
                    //histogramChart.BeginInit();

                    //histogramSeries.ItemsSource = CurrentStatisticsData.HistogramData;
                    //histogramCDFSeries.ItemsSource = CurrentStatisticsData.HistogramData; 



                    SeriesCollection = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Histogram",
                        Values = new ChartValues<int>(CurrentData.HistogramData.Select(i => i.Count)),
                        ScalesYAt = 0
                    },
                    new LineSeries
                    {
                        Title = "Cumulative Probability",
                        Values = new ChartValues<double>(CurrentData.HistogramData.Select(i => i.CumulativePercentile)),
                         ScalesYAt = 1,
                         Fill=Brushes.Transparent
                    }
                };


                    Labels = CurrentData.HistogramData.Select(i => i.BinLabel).ToArray();


                    OnPropertyChanged(nameof(SeriesCollection));
                    OnPropertyChanged(nameof(Labels));
                    OnPropertyChanged(nameof(CurrentData.RandomNumbers));

                }
                finally
                {
                    //histogramChart.EndInit();
                }

                // random data samples
                textBoxRandomNumbers.Text = CurrentData.RandomNumbers;
                if (string.IsNullOrEmpty(CurrentData.RandomNumbers))
                    tabItemRandomNumbers.Visibility = System.Windows.Visibility.Collapsed;
                else
                    tabItemRandomNumbers.Visibility = System.Windows.Visibility.Visible;

                result = true;
            }

            return result;
        }
    }
}
