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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using FocusedObjective.Simulation.Viewers;
using System.Dynamic;
using System.Collections;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for ChartsUserControl.xaml
    /// </summary>
    public partial class ChartsUserControl : UserControl, INotifyPropertyChanged
    {
        private StatisticData _currentData = new StatisticData();
        private FocusedObjective.Contract.SimulationTypeEnum _lastSimulationType = Contract.SimulationTypeEnum.Kanban;
        private List<string> lastColumns = new List<string>();
        private ObservableCollection<StatisticItems> stats;
        private StatisticItems visualClassOfServiceItem;
        private StatisticItems monteCarloClassOfServiceItem;
        private StatisticItems visualColumnActivePositionsItem;
        private StatisticItems monteCarloColumnActivePositionsItem;
        private XElement _currentSimulationResults;
        private List<string> lastCOS = new List<string>();

        public string _lastChartedSimML;
        public event PropertyChangedEventHandler PropertyChanged;

        public ChartsUserControl()
        {
            InitializeComponent();

            // start with this as the default...
            buildKanbanStatisticList();


            Formatter = value => value.ToString();
            SecondaryFormatter = value => value.ToString() + "%";

            DataContext = this;
        }


        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
        public Func<double, string> SecondaryFormatter { get; set; }

        public StatisticData CurrentStatisticsData
        {
            get
            {
                return _currentData;
            }
            set
            {
                _currentData = value;
                OnPropertyChanged("CurrentData");
            }
        }

        public SeriesCollection SeriesCollectionCumulativeFlow { get; set; }
        public Func<double, string> XFormatterCumulativeFlow { get; set; }

        public Func<double, string> YFormatterCumulativeFlow { get; set; }


        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        internal void PopulateCharts(ProjectState state, bool forceRefresh = false)
        {
            if (state.LastSimulator != null && // must have been simulated
                state.LastSimulator.Result != null  // a valid result needed to be returned
             && (forceRefresh || _lastChartedSimML != state.LastSimulatedSimML) // needs to be a change in SimML
               )
            {
                _lastChartedSimML = state.LastSimulatedSimML;

                XElement results = state.LastSimulator.Result;

                // check valid here. success=true
                XAttribute successFlag = results.Attribute("success");
                if (successFlag != null && bool.Parse(successFlag.Value) == true)
                {
                    if (state.SimulationType == Contract.SimulationTypeEnum.Kanban
                        && state.LastSimulator.KanbanUserControl != null)
                    {
                        if (_lastSimulationType != Contract.SimulationTypeEnum.Kanban)
                        {
                            lastColumns.Clear();
                            lastCOS.Clear();
                            buildKanbanStatisticList();
                            _lastSimulationType = Contract.SimulationTypeEnum.Kanban;
                        }

                        //tabItemCumulativeFlow.Visibility = System.Windows.Visibility.Visible;
                        //tabItemIntervals.Visibility = System.Windows.Visibility.Visible;

                        buildCumulativeFlowChart(results);
                        buildIntervalsChart(results);
                        buildStatisticsCharts(results);
                    }
                    else
                    {
                        if (state.SimulationType == Contract.SimulationTypeEnum.Scrum
                            && state.LastSimulator.ScrumUserControl != null) if (state.LastSimulator.ScrumUserControl != null)
                            {
                                if (_lastSimulationType != Contract.SimulationTypeEnum.Scrum)
                                {
                                    lastColumns.Clear();
                                    lastCOS.Clear();
                                    buildScrumStatisticList();
                                    _lastSimulationType = Contract.SimulationTypeEnum.Scrum;
                                }

                                // move to the first tab
                                tabControlCharts.SelectedIndex = 0;
                                tabItemStatistics.Focus();

                                //tabItemCumulativeFlow.Visibility = System.Windows.Visibility.Collapsed;
                                //tabItemIntervals.Visibility = System.Windows.Visibility.Collapsed;
                                buildStatisticsCharts(results);
                            }
                    }
                }
                else
                {
                    ClearChartsAndData();
                }
            }
        }

        internal void ClearChartsAndData()
        {
            // clear the current chart data - it is now invalid
            //cflowChart.Series.Clear();

            CurrentStatisticsData.Reset();
            //histogramSeries.ItemsSource = CurrentStatisticsData.HistogramData;
            //histogramCDFSeries.ItemsSource = CurrentStatisticsData.HistogramData;

            //blockedSeries.ItemsSource = null;
            //pullTransactionsSeries.ItemsSource = null;
        }

        private void buildKanbanStatisticList()
        {
            stats = new ObservableCollection<StatisticItems>();

            stats.Add(new StatisticItems
            {
                Name = "Cycle Time (Work Cards)",
                XPath = @"./visual/statistics/cards/work/cycleTime",
                IntegerType = false,
                Description = "The total time 'work' card types spend on the Kanban board in any state. Measured from the moment they start the first column until they move to the completed work list."
            });

            stats.Add(new StatisticItems
            {
                Name = "Cycle Time (Defect Cards)",
                XPath = @"./visual/statistics/cards/defect/cycleTime",
                IntegerType = false,
                Description = "The total time 'defect' card types spend on the Kanban board in any state. Measured from the moment they start the first column until they move to the completed work list."

            });

            stats.Add(new StatisticItems
            {
                Name = "Cycle Time (Added Scope Cards)",
                XPath = @"./visual/statistics/cards/addedScope/cycleTime",
                IntegerType = false,
                Description = "The total time 'added scope' types spend on the Kanban board in any state. Measured from the moment they start the first column until they move to the completed work list."

            });

            visualClassOfServiceItem = new StatisticItems
            {
                Name = "Class of Service Cycle-Times (Visual)",
                XPath = "",
                IntegerType = true,
                Description = "Choose a class of service from the sub-list to see the Cycle-Time for work of that class of service."

            };

            stats.Add(visualClassOfServiceItem);

            visualColumnActivePositionsItem = new StatisticItems
            {
                Name = "Column Active Positions (Visual)",
                XPath = "",
                IntegerType = true,
                Description = "Choose a column from the sub-list to see the Active Positions for each time interval."

            };

            stats.Add(visualColumnActivePositionsItem);

            stats.Add(new StatisticItems
            {
                Name = "Empty Board Positions",
                XPath = @"./visual/statistics/emptyPositions",
                IntegerType = true,
                Description = "The number of un-used positions on the Kanban board. Measured as the number of total WIP positions allowed minus the number of cards on the Kanban board during each simulation interval."
            });

            stats.Add(new StatisticItems
            {
                Name = "Queued Board Positions",
                XPath = @"./visual/statistics/queuedPositions",
                IntegerType = true,
                Description = "The number of queued positions on the Kanban board. Measured as the number of cards that are completed but have no vacant position in the next column of the Kanban board during each simulation interval."

            });

            stats.Add(new StatisticItems
            {
                Name = "Blocked Board Positions",
                XPath = @"./visual/statistics/blockedPositions",
                IntegerType = true,
                Description = "The number of blocked positions on the Kanban board. Measured as the number of cards that are marked as blocked on the Kanban board during each simulation interval. Cards can be blocked because of a Blocking Event definition, or because a card was given priority during a WIP violation as part of a class of service definition."

            });

            stats.Add(new StatisticItems
            {
                Name = "Active Board Positions",
                XPath = @"./visual/statistics/activePositions",
                IntegerType = true,
                Description = "The number of active positions on the Kanban board. Measured as the number of cards that are being worked-on (not blocked or queued) on the Kanban board during each simulation interval."

            });

            stats.Add(new StatisticItems
            {
                Name = "In-Active Board Positions",
                XPath = @"./visual/statistics/inActivePositions",
                IntegerType = true,
                Description = "The number of in-active positions on the Kanban board. Measured as the number of cards that are NOT being worked-on (they are blocked or queued) on the Kanban board during each simulation interval."

            });

            stats.Add(new StatisticItems
            {
                Name = "Pull Transactions",
                XPath = @"./visual/statistics/pullTransactions",
                IntegerType = true,
                Description = "The number of pulled cards into a new position on the Kanban board. Measured as the number of cards that pulled into a column on the Kanban board during each simulation interval."

            });

            stats.Add(new StatisticItems
            {
                Name = "Intervals (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/intervals",
                IntegerType = true,
                Description = "The average number of simulation intervals taken to complete the entire backlog and any defect or added scope work. Measured as the number of simulation steps needed to empty the entire backlog, repeated multiple times as part of a Monte Carlo simulation. The unit of measure depends on your model. If you estimated in work hours, this will be hours; if you estimated in work days, this will be days. Lower values means faster delivery.",
                Default = true
            });

            stats.Add(new StatisticItems
            {
                Name = "Work Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/work/count",
                IntegerType = true,
                Description = "The average number of work cards created in the backlog. Measured as the number of backlog items created in your model's backlog section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Work Card Cycle Time (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/work/cycleTime",
                IntegerType = false,
                Description = "The average total time 'work' card types spend on the Kanban board in any state over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the average value for each simulation run for the time each work card starts the first column until it moves to the completed work list."

            });

            stats.Add(new StatisticItems
            {
                Name = "Added Scope Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/addedScope/count",
                IntegerType = true,
                Description = "The average number of added scope cards created by Added Scope event definitions over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the number of added scope items created by your model's addedScopes section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Added Scope Card Lead Time (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/addedScope/cycleTime",
                IntegerType = false,
                Description = "The average total time 'added scope' card types spend on the Kanban board in any state over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the average value for each simulation run for the time each added scope card starts the first column until it moves to the completed work list."

            });

            stats.Add(new StatisticItems
            {
                Name = "Defect Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/defect/count",
                IntegerType = true,
                Description = "The average number of defect cards created by Defect event definitions over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the number of defect items created by your model's defects section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Defect Card Cycle Time (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/defect/cycleTime",
                IntegerType = false,
                Description = "The average total time 'defect' card types spend on the Kanban board in any state over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the average value for each simulation run for the time each defect card starts in any column column until it moves to the completed work list."

            });

            monteCarloClassOfServiceItem = new StatisticItems
            {
                Name = "Class of Service Cycle-Times (Monte Carlo)",
                XPath = "",
                IntegerType = true,
                Description = "Choose a class of service from the sub-list to see the Cycle-Time for work of that class of service."
            };

            stats.Add(monteCarloClassOfServiceItem);

            stats.Add(new StatisticItems
            {
                Name = "Empty Board Positions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/emptyPositions",
                IntegerType = false,
                Description = "The average number of un-used positions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated multiple simulation cycles as part of a Monte Carlo analysis. Measured as the number of empty board positions (positions available minus cards on board) during each interval for a simulation run."

            });

            stats.Add(new StatisticItems
            {
                Name = "Queued Board Positions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/queuedPositions",
                IntegerType = false,
                Description = "The average number of complete, but waiting opening in the next column positions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated multiple simulation cycles as part of a Monte Carlo analysis."

            });

            stats.Add(new StatisticItems
            {
                Name = "Blocked Board Positions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/blockedPositions",
                IntegerType = false,
                Description = "The average number of blocked positions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated multiple simulation cycles as part of a Monte Carlo analysis."

            });

            stats.Add(new StatisticItems
            {
                Name = "Active Board Positions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/activePositions",
                IntegerType = true,
                Description = "The average number of active positions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated multiple simulation cycles as part of a Monte Carlo analysis."

            });

            stats.Add(new StatisticItems
            {
                Name = "In-Active Board Positions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/inActivePositions",
                IntegerType = true,
                Description = "The average number of in-active positions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated multiple simulation cycles as part of a Monte Carlo analysis."

            });

            stats.Add(new StatisticItems
            {
                Name = "Pull Transactions (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/pullTransactions",
                IntegerType = true,
                Description = "The average number of pull transactions on the Kanban board during each simulation interval for a full simulation cycle. This is repeated for multiple simulation cycles as part of a Monte Carlo analysis."

            });

            monteCarloColumnActivePositionsItem = new StatisticItems
            {
                Name = "Column Active Positions (Monte Carlo)",
                XPath = "",
                IntegerType = true,
                Description = "Choose a column from the sub-list to see the Active Positions of each column over many simulation iterations."
            };

            stats.Add(monteCarloColumnActivePositionsItem);
            HistogramCombo.ItemsSource = stats;
            StatisticItems selected = HistogramCombo.SelectedItem as StatisticItems;
            HistogramCombo.SelectedItem = stats.First(s => s.Default);
        }

        private void buildScrumStatisticList()
        {
            stats = new ObservableCollection<StatisticItems>();

            stats.Add(new StatisticItems
            {
                Name = "Points Allocated Per Iteration",
                XPath = @"./visual/statistics/pointsAllocatedPerIteration",
                IntegerType = false,
                Description = "The number of story points allocated each iteration."
            });

            stats.Add(new StatisticItems
            {
                Name = "Point Size (Work Cards)",
                XPath = @"./visual/statistics/cards/work/pointSize",
                IntegerType = false,
                Description = "The point-sizes chosen for 'work' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Blocking Point Size (Work Cards)",
                XPath = @"./visual/statistics/cards/work/blockPointSize",
                IntegerType = false,
                Description = "The blocking point-sizes chosen for 'work' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Point Size (Added-Scope Cards)",
                XPath = @"./visual/statistics/cards/addedScope/pointSize",
                IntegerType = false,
                Description = "The point-sizes chosen for 'added scope' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Blocking Point Size (Added-Scope Cards)",
                XPath = @"./visual/statistics/cards/addedScope/blockPointSize",
                IntegerType = false,
                Description = "The blocking point-sizes chosen for 'added scope' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Point Size (Defect Cards)",
                XPath = @"./visual/statistics/cards/defect/pointSize",
                IntegerType = false,
                Description = "The point-sizes chosen for 'defect' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Blocking Point Size (Defect Cards)",
                XPath = @"./visual/statistics/cards/defect/blockPointSize",
                IntegerType = false,
                Description = "The blocking point-sizes chosen for 'defect' card types."
            });

            stats.Add(new StatisticItems
            {
                Name = "Iterations (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/iterations",
                IntegerType = true,
                Description = "The number of simulation iterations taken to complete the entire backlog and any defect or added scope work. Measured as the number of simulation iterations needed to empty the entire backlog, repeated multiple times as part of a Monte Carlo simulation. The number of days in an iteration depends on your model (defined in the <iteration> section).",
                Default = true
            });

            stats.Add(new StatisticItems
            {
                Name = "Work Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/work/count",
                IntegerType = true,
                Description = "The average number of work cards created in the backlog. Measured as the number of backlog items created in your model's backlog section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Work Card Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/work/pointSize",
                IntegerType = false,
                Description = "The average point-sizes chosen for 'work' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            stats.Add(new StatisticItems
            {
                Name = "Work Card Blocking Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/work/blockPointSize",
                IntegerType = false,
                Description = "The average blocking point-sizes chosen for 'work' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            stats.Add(new StatisticItems
            {
                Name = "Added Scope Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/addedScope/count",
                IntegerType = true,
                Description = "The average number of 'added scope' cards created in the backlog. Measured as the number of backlog items created in your model's backlog section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Added-Scope Card Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/addedScope/pointSize",
                IntegerType = false,
                Description = "The average point-sizes chosen for 'added scope' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            stats.Add(new StatisticItems
            {
                Name = "Added Scope Card Blocking Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/addedScope/blockPointSize",
                IntegerType = false,
                Description = "The average blocking point-sizes chosen for 'added scope' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            stats.Add(new StatisticItems
            {
                Name = "Defect Card Count (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/defect/count",
                IntegerType = true,
                Description = "The average number of 'defect' cards created in the backlog. Measured as the number of backlog items created in your model's backlog section."

            });

            stats.Add(new StatisticItems
            {
                Name = "Defect Card Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/defect/pointSize",
                IntegerType = false,
                Description = "The average point-sizes chosen for 'defect' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            stats.Add(new StatisticItems
            {
                Name = "Defect Card Blocking Point Sizes (Monte Carlo)",
                XPath = @"./monteCarlo/statistics/cards/defect/blockPointSize",
                IntegerType = false,
                Description = "The average blocking point-sizes chosen for 'defect' card types over multiple simulation cycles as part of a Monte Carlo analysis."
            });

            HistogramCombo.ItemsSource = stats;
            StatisticItems selected = HistogramCombo.SelectedItem as StatisticItems;
            HistogramCombo.SelectedItem = stats.First(s => s.Default);
        }

        private void buildStatisticsCharts(XElement results)
        {
            _currentSimulationResults = results;

            refreshClassOfServiceSubEntriesIfNecessary();
            refreshColumnSubEntriesIfNecessary();

            // refresh the charts if necessary
            if (_currentSimulationResults != null && HistogramCombo.SelectedItem != null)
            {
                StatisticItems thisItem = (StatisticItems)HistogramCombo.SelectedItem;

                if (thisItem.Children.Any())
                    thisItem = (StatisticItems)HistogramSubCombo.SelectedItem;

                if (thisItem == null || !bindHistogramAndCurrentDataPanel(thisItem))
                {
                    // reset charts
                    CurrentStatisticsData.Reset();
                    //histogramSeries.ItemsSource = CurrentStatisticsData.HistogramData;
                    //histogramCDFSeries.ItemsSource = CurrentStatisticsData.HistogramData;
                }

            }
        }

        private void refreshClassOfServiceSubEntriesIfNecessary()
        {
            // class of service changes...
            XElement root = _currentSimulationResults.XPathSelectElement(@"./visual/statistics/classOfServices");
            if (root != null)
            {
                // only update if changed (or the first time)...
                bool needsUpdating = lastCOS.Count == 0;

                foreach (var cos in root.Elements("classOfService"))
                {
                    if (!lastCOS.Contains(cos.Attribute("name").Value))
                        needsUpdating = true;

                    if (needsUpdating)
                        break;
                }

                if (needsUpdating)
                {
                    // delete the current COS sub-entries...
                    visualClassOfServiceItem.Children.Clear();
                    monteCarloClassOfServiceItem.Children.Clear();

                    lastCOS.Clear();

                    foreach (var cos in root.Elements("classOfService"))
                    {
                        // to avoid unnecesary refreshes, and to keep the chart stable, remember the index and name positions...
                        lastCOS.Add(cos.Attribute("name").Value);

                        visualClassOfServiceItem.Children.Add(
                            new StatisticItems
                            {
                                Name = string.Format("'{0}' Class of Service", cos.Attribute("name").Value),
                                XPath = string.Format(@"./visual/statistics/classOfServices/classOfService[@name = ""{0}""]/cycleTime", cos.Attribute("name").Value),
                                IntegerType = false,
                                Parent = visualClassOfServiceItem,
                                Description = string.Format("The total time cards with '{0}' class of service spend on the Kanban board in any state. Measured from the moment they start the first column until they move to the completed work list.", cos.Attribute("name").Value)
                            });

                        monteCarloClassOfServiceItem.Children.Add(new StatisticItems
                        {
                            Name = string.Format("'{0}' Class of Service", cos.Attribute("name").Value),
                            XPath = string.Format(@"./monteCarlo/statistics/classOfServices/classOfService[@name = ""{0}""]/cycleTime", cos.Attribute("name").Value),
                            IntegerType = false,
                            Parent = monteCarloClassOfServiceItem,
                            Description = string.Format("The average total time cards with '{0}' class of service spend on the Kanban board in any state over multiple simulation cycles as part of a Monte Carlo analysis. Measured as the average value for each simulation run for the time each work card starts the first column until it moves to the completed work list.", cos.Attribute("name").Value)
                        });

                    }
                }
            }
            else
            {
                // delete COS sub-entries...
                visualClassOfServiceItem.Children.Clear();
                monteCarloClassOfServiceItem.Children.Clear();
                lastCOS.Clear();
            }
        }

        private void refreshColumnSubEntriesIfNecessary()
        {
            // column changes...
            XElement root = _currentSimulationResults.XPathSelectElement(@"./visual/statistics/columns");
            if (root != null)
            {
                // only update if changed (or the first time)...
                bool needsUpdating = lastColumns.Count == 0;

                foreach (var col in root.Elements("column"))
                {
                    if (!lastColumns.Contains(col.Attribute("name").Value))
                        needsUpdating = true;

                    if (needsUpdating)
                        break;
                }

                if (needsUpdating)
                {
                    // delete the current column sub-entries...
                    visualColumnActivePositionsItem.Children.Clear();
                    monteCarloColumnActivePositionsItem.Children.Clear();

                    lastColumns.Clear();

                    foreach (var col in root.Elements("column"))
                    {
                        // to avoid unnecesary refreshes, and to keep the chart stable, remember the index and name positions...
                        lastColumns.Add(col.Attribute("name").Value);

                        visualColumnActivePositionsItem.Children.Add(
                            new StatisticItems
                            {
                                Name = string.Format("'{0}' Column", col.Attribute("name").Value),
                                XPath = string.Format(@"./visual/statistics/columns/column[@name = ""{0}""]/activePositions", col.Attribute("name").Value),
                                IntegerType = false,
                                Parent = visualColumnActivePositionsItem,
                                Description = string.Format("The number of cards in the '{0}' column that are actively being worked on for each time interval. This is useful for determining how many staff are required with certain skills. Measured as the number of card that are NOT blocked or queued.", col.Attribute("name").Value)
                            });

                        monteCarloColumnActivePositionsItem.Children.Add(new StatisticItems
                        {
                            Name = string.Format("'{0}' Column", col.Attribute("name").Value),
                            XPath = string.Format(@"./monteCarlo/statistics/columns/column[@name = ""{0}""]/activePositions", col.Attribute("name").Value),
                            IntegerType = false,
                            Parent = monteCarloColumnActivePositionsItem,
                            Description = string.Format("The number of cards in the '{0}' column that are actively being worked on aggregated over many simulation cycles. This is useful for determining how many staff are required with certain skills. Measured as the number of card that are NOT blocked or queued.", col.Attribute("name").Value)
                        });

                    }
                }
            }
            else
            {
                // delete COS sub-entries...
                visualColumnActivePositionsItem.Children.Clear();
                monteCarloColumnActivePositionsItem.Children.Clear();
                lastColumns.Clear();
            }
        }



        private void buildCumulativeFlowChart(XElement results)
        {
            // try and disconnect the chart....to avoid trackball weirdness...
            //foreach (var series in cflowChart.Series)
            //    series.ItemsSource = null;

            //cflowChart.Series.Clear();


            //TODO:At present we use the CSV data in the results for the cumulative flow. I think in the future, this data deserves XML elements of their own.
            XElement csvData = results.Element("visual").Element("cumulativeFlow").Element("data");
            CsvParser parser = new CsvParser(csvData.Value);

            Dictionary<string, List<ChartPoint>> cfSeries = new Dictionary<string, List<ChartPoint>>();

            int suffixForDuplicates = 1;
            // make a series for each column
            foreach (var header in parser.Headers)
            {
                if (!cfSeries.ContainsKey(header))
                    cfSeries.Add(header, new List<ChartPoint>());
                else
                    cfSeries.Add(header + suffixForDuplicates++.ToString(), new List<ChartPoint>());
            }

            // read each line of the CSV data, and if valid, add each datapoint to the correct series

            int intervalIndex = 0;
            foreach (var interval in parser)
            {
                CsvLine csvLine = interval as CsvLine;
                if (csvLine != null && csvLine.IsValid)
                {
                    for (int i = 0; i < parser.Headers.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(csvLine[i].ToString().Trim()))
                            cfSeries[parser.Headers[i]].Add(new ChartPoint
                            {
                                Interval = intervalIndex,
                                Count = int.Parse(csvLine[i].ToString().Trim())
                            });
                    }
                }
                intervalIndex++;
            }



            SeriesCollectionCumulativeFlow = new SeriesCollection();

            foreach (var series in cfSeries.Keys.Reverse())
            {
                ChartValues<double> seriesValues = new ChartValues<double>(cfSeries[series].Select(p => (double)p.Count));

                SeriesCollectionCumulativeFlow.Add(
                    new StackedAreaSeries
                    {
                        Title = series,

                        Values = seriesValues,
                        LineSmoothness = 0,
                    }
                    );

            };

            XFormatterCumulativeFlow = val => val.ToString();
            YFormatterCumulativeFlow = val => val.ToString();


            OnPropertyChanged(nameof(SeriesCollectionCumulativeFlow));




        }

        public class StatisticItems
        {
            public StatisticItems Parent;
            public string Name;
            public bool IntegerType;
            public bool ExactValues;
            public bool COSBase;
            public bool Default = false;
            public ObservableCollection<StatisticItems> Children = new ObservableCollection<StatisticItems>();

            public string XPath { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }


        private void buildIntervalsChart(XElement results)
        {
            try
            {

                if (results == null ||
                    results.Element("visual") == null ||
                    results.Element("visual").Element("intervals") == null)
                    return;

                var intervalsData = results.Element("visual").Element("intervals").Elements("interval");

                bool useDates = false;

                //TODO:Add date support
                //if (intervalsData != null && intervalsData.FirstOrDefault() != null)
                //    useDates = !string.IsNullOrWhiteSpace(intervalsData.First().Attribute("date").Value);

                if (useDates)
                {
                    // use dates ...
                    //TODO:Add date support...
                }
                else
                {
                    // use intervals
                    List<ChartPoint> blockedSeriesData = new List<ChartPoint>();
                    List<ChartPoint> pullTransactionsSeriesData = new List<ChartPoint>();
                    List<ChartPoint> queuedSeriesData = new List<ChartPoint>();

                    foreach (var item in intervalsData)
                    {
                        blockedSeriesData.Add(new ChartPoint { Interval = int.Parse(item.Attribute("sequence").Value), Count = int.Parse(item.Attribute("blocked").Value) });
                        pullTransactionsSeriesData.Add(new ChartPoint { Interval = int.Parse(item.Attribute("sequence").Value), Count = int.Parse(item.Attribute("pullTransactions").Value) });
                        queuedSeriesData.Add(new ChartPoint { Interval = int.Parse(item.Attribute("sequence").Value), Count = int.Parse(item.Attribute("queued").Value) });
                    }



                    //blockedSeries.ItemsSource = blockedSeriesData;
                    //pullTransactionsSeries.ItemsSource = pullTransactionsSeriesData;
                    //queuedSeries.ItemsSource = queuedSeriesData;
                }
            }
            catch
            { }
        }

        private void setHistogram(StatisticItems thisItem, bool fromSubCombo)
        {
            if (!fromSubCombo && thisItem.Children.Any())
            {
                HistogramSubCombo.Visibility = System.Windows.Visibility.Visible;
                HistogramSubCombo.ItemsSource = thisItem.Children;
            }
            else
            {
                if (!fromSubCombo)
                {
                    HistogramSubCombo.Visibility = System.Windows.Visibility.Hidden;
                    HistogramSubCombo.ItemsSource = null;
                }
            }

            descriptionTextBlock.Text = thisItem.Description;

            if (!bindHistogramAndCurrentDataPanel(thisItem))
            {
                //reset everything, show error
                CurrentStatisticsData.Reset();
                // histogramSeries.ItemsSource = CurrentStatisticsData.HistogramData;
                // histogramCDFSeries.ItemsSource = CurrentStatisticsData.HistogramData;

                if (thisItem.XPath != "")
                    descriptionTextBlock.Text = "Choose a measurement from the list above to see the current data.";
            }
        }

        private void HistogramSubCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentSimulationResults != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                StatisticItems thisItem = (StatisticItems)e.AddedItems[0];
                setHistogram(thisItem, true);
            }
        }

        private void HistogramCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (_currentSimulationResults != null && e.AddedItems != null && e.AddedItems.Count > 0)
            {
                StatisticItems thisItem = (StatisticItems)e.AddedItems[0];
                setHistogram(thisItem, false);
            }
        }

        private bool bindHistogramAndCurrentDataPanel(StatisticItems thisItem)
        {
            bool result = false;

            if (thisItem.XPath != "")
            {
                XElement root = _currentSimulationResults.XPathSelectElement(thisItem.XPath);

                if (root != null)
                {
                    CurrentStatisticsData.FromXML(root);


                    System.Diagnostics.Debug.WriteLine(CurrentStatisticsData.HistogramData.Select(i => i.Count).First().ToString());

                    SeriesCollection = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = thisItem.Name,
            Values = new ChartValues<int>(CurrentStatisticsData.HistogramData.Select(i => i.Count)),
            ScalesYAt = 0
        },
        new LineSeries
        {
            Title = "Cumulative Probability",
            Values = new ChartValues<double>(CurrentStatisticsData.HistogramData.Select(i => i.CumulativePercentile)),
             ScalesYAt = 1,
             Fill=Brushes.Transparent
        }
    };


                    Labels = CurrentStatisticsData.HistogramData.Select(i => i.BinLabel).ToArray();

                    OnPropertyChanged(nameof(SeriesCollection));
                    OnPropertyChanged(nameof(Labels));

                    result = true;
                }
            }

            return result;
        }
    }

    public class ChartPoint
    {
        public int Interval { get; set; }
        public int Count { get; set; }
    }

    public class CsvLine : System.Dynamic.DynamicObject
    {
        string[] _lineContent;
        List<string> _headers;

        public bool IsValid
        {
            get { return _headers.Count == _lineContent.Count(); }
        }


        public CsvLine(string line, List<string> headers)
        {
            this._lineContent = line.Split(',');
            this._headers = headers;
        }

        public override bool TryGetMember(
        GetMemberBinder binder,
        out object result)
        {
            result = null;

            // find the index position and get the value
            int index = _headers.IndexOf(binder.Name);
            if (index >= 0 && index < _lineContent.Length)
            {
                result = _lineContent[index];
                return true;
            }

            return false;
        }

        public override bool TryGetIndex(
        GetIndexBinder binder,
        object[] indexes,
        out object result)
        {
            result = null;

            int index = (int)indexes[0];
            if (index >= 0 && index < _lineContent.Length)
            {
                result = _lineContent[index];
                return true;
            }

            return false;
        }

        public string this[int index]
        {
            get
            {
                if (index >= 0 && index < _lineContent.Length)
                {
                    return _lineContent[index];
                }
                throw new IndexOutOfRangeException("Index out of range");
            }
        }
    }

    public class CsvParser : IEnumerable
    {
        List<string> _headers;
        string[] _lines;

        public List<string> Headers
        {
            get { return _headers; }
        }

        public CsvParser(string csvContent)
        {
            _lines = csvContent.Split(new string[] { "\r\n", "\n\r" }, StringSplitOptions.RemoveEmptyEntries);

            // grab the header row and remember positions
            if (_lines.Length > 0)
                _headers = _lines[0].Split(',').ToList();
        }

        public IEnumerator GetEnumerator()
        {
            // skip the header line
            bool header = true;

            foreach (var line in _lines)
                if (header)
                    header = false;
                else
                    yield return new CsvLine(line, _headers);
        }
    }

}
