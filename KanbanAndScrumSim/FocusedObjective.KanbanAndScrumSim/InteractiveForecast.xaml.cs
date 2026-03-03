using FocusedObjective.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for InteractiveForecast.xaml
    /// </summary>
    public partial class InteractiveForecast : Window
    {
        public delegate void ParameterChangedEventHandler(object sender, EventArgs e);

        public InteractiveForecast()
        {
            InitializeComponent();
        }

        ProjectState _state = null;
        int _monteCarloCycles = 100;
        string _aggregationValue = "";

        string _originalSimML = "";
        XDocument _document = null;
        List<MoveableParameter> parameters = new List<MoveableParameter>();

        private string _dateFormatString = "yyyyMMdd";
        private string _currencyFormatString = "C0";

        // the various trial results...

        public TrialResult BaselineTrial
        {
            get { return TrialResultList.FirstOrDefault(); }
        }

        public TrialResult LastTrial
        {
            get { return TrialResultList.LastOrDefault(); }
        }

        public TrialResult SelectedTrial
        {
            get { return (TrialResult)listResults.SelectedItem; }
        }

        public TrialResult ShortestTrial
        {
            get { return TrialResultList.OrderBy(t => t.Intervals).FirstOrDefault(); }
        }

        public TrialResult EarliestForecastTrial
        {
            get { return TrialResultList.OrderBy(t => t.CalendarDaysFromNow).FirstOrDefault(); }
        }

        public TrialResult LowestCostTrial
        {
            get { return TrialResultList.OrderBy(t => t.TotalCost).FirstOrDefault(); }
        }

        internal void SetupFromModel(ProjectState state, int monteCarloCycles = 100, string aggregationValue = "")
        {
            _state = state;
            _monteCarloCycles = monteCarloCycles;
            _aggregationValue = aggregationValue;

            if (state.AlreadySimulating) 
                return;

            if (string.IsNullOrWhiteSpace(state.CurrentSimML))
            {
                panelParameterControls.Children.Add(new HowToInteractive());
                return;
            }

            _document = XDocument.Parse(state.CurrentSimML);

            _dateFormatString = MoveablePArameterHelpers.DateFormatString(_document);
            _currencyFormatString = MoveablePArameterHelpers.CurrencyFormatString(_document);

            MoveablePArameterHelpers.ExtractVariablePI(_document.FirstNode, parameters, _dateFormatString);
            addParameterControls(parameters);

            // remember the current SIMML. We will change it back....
            _originalSimML = _state.CurrentSimML;


            if (parameters != null && parameters.Count == 0)
            {
                panelParameterControls.Children.Add(new HowToInteractive());

            }
            else
            {
                //intervalsSeries.ItemsSource = TrialResultList;

                //totalCostsSeries.ItemsSource = TrialResultList;
                //totalCostsSeries.YAxis.LabelFormat = _currencyFormatString;

                //daysFromNowSeries.ItemsSource = TrialResultList;
                listResults.ItemsSource = TrialResultList;

                // get baseline.
                ExecuteForecast();
            }
        }

        private SimulatingProgress progress;

        internal void ExecuteForecast()
        {
            // set the model parameters to equal ours...
            XDocument testDoc = XDocument.Parse(_originalSimML);
            MoveablePArameterHelpers.ReplaceVariablePIWithTestValues(testDoc.FirstNode, parameters, _dateFormatString);
            _state.CurrentSimML = testDoc.ToString();

            // sim
            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, _monteCarloCycles);

            if (!_state.SimulateAdvancedCommand(
                backgroundWorkerAdvancedSim_RunWorkerCompleted,
                backgroundWorkerAdvancedSim_ReportProgress,
                worker,
                progress,
                _monteCarloCycles,
                _aggregationValue,
                true,
                Contract.ForecastPermutationsEnum.None))
            {
                // there was a problem. restore
                _state.CurrentSimML = _originalSimML;
            }
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

            // return to neutral
            _state.CurrentSimML = _originalSimML;

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

                    XElement _lastResults = result.Simulator.Result;
                    XElement _forecastResults = result.Simulator.Result.Element("forecastDate").Element("dates");

                    XElement _targetResult = null; 

                    string date = "";
                    foreach (var item in _forecastResults.Elements())
                    {
                        if (bool.Parse(item.Attribute("targetLikelihood").Value))
                        {
                            _targetResult = item;

                            date = item.Attribute("date").Value;
                            break;
                        }
                    }

                    if (_targetResult != null)
                    {
                        TrialResult t = new TrialResult
                            {
                                Kanban = _targetResult.Attribute("intervals") != null,
                                PreviousTrial = TrialResultList.LastOrDefault(),
                                Trial = lastTrial++,
                                Date = _targetResult.Attribute("date").Value,
                                Cost = double.Parse(_targetResult.Attribute("cost").Value, System.Globalization.NumberStyles.Currency),
                                CostOfDelay = double.Parse(_targetResult.Attribute("costOfDelay").Value, System.Globalization.NumberStyles.Currency),
                                Intervals = _targetResult.Attribute("intervals") != null ? int.Parse(_targetResult.Attribute("intervals").Value) : int.Parse(_targetResult.Attribute("iterations").Value),
                                DateFormat = _dateFormatString,
                                CurrencyFormat = _currencyFormatString
                            };

                        foreach (var param in parameters)
                            t.ParameterValues.Add(param, param.CurrentValue);

                        TrialResultList.Add(t);

                        // update the panels...
                        comboA_SelectionChanged(this, null);
                        comboB_SelectionChanged(this, null);
                    }
            }
            else
            {
                // report error
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates and that you have a valid <forecastDate ...> section in your model.", "Unable to simulate...");
                this.Close();
            }
        }

        private void addParameterControls(List<MoveableParameter> parameters)
        {
            foreach (var parameter in parameters)
            {

                switch (parameter.ParameterType)
                {
                    case MoveableParameterTypeEnum.Numeric:

                        NumericParameterControl c = new NumericParameterControl();
                        c.Margin = new Thickness(0, 0, 15, 0);
                        c.Parameter = parameter;
                        c.Setup();
                        c.Changed -= c_Changed;
                        c.Changed += c_Changed;
                        panelParameterControls.Children.Add(c);

                        break;

                    case MoveableParameterTypeEnum.Date:

                        DateParameterControl d = new DateParameterControl();
                        d.Margin = new Thickness(0, 0, 15, 0);
                        d.Parameter = parameter;
                        d.Setup();
                        d.Changed -= c_Changed;
                        d.Changed += c_Changed;
                        panelParameterControls.Children.Add(d);

                        break;

                    case MoveableParameterTypeEnum.List:

                        SelectionListParameterControl e = new SelectionListParameterControl();
                        e.Margin = new Thickness(0, 0, 15, 0);
                        e.Parameter = parameter;
                        e.Setup();
                        e.Changed -= c_Changed;
                        e.Changed += c_Changed;
                        panelParameterControls.Children.Add(e);
                        break;

                    default:
                        break;
                }
            }
        }

        void c_Changed(object sender, EventArgs e)
        {
            // run sim?

        }
        



        private void buttonSimulate_Click(object sender, RoutedEventArgs e)
        {
            ExecuteForecast();
        }

        public TrialResults TrialResultList = new TrialResults();
        int lastTrial = 1;


        private void SetTableValuesA(TrialResult selected)
        {
            if (selected != null)
            {
                labelTrialA.Content = selected.Trial;
                labelCostA.Content = selected.Cost.ToString(selected.CurrencyFormat);
                labelCostOfDelayA.Content = selected.CostOfDelay.ToString(selected.CurrencyFormat);
                

                
                labelForecastA.Content = string.Format("{0}, {1} days from now", selected.Date, selected.CalendarDaysFromNow);
                labelTotalCostA.Content = selected.TotalCost.ToString(selected.CurrencyFormat);

                if (selected.Kanban)
                {
                    //DaysOrIterationsAxis.Header = "Days";
                    labelDurationA.Content = string.Format("{0} days", selected.Intervals);
                }
                else
                { 
                    //DaysOrIterationsAxis.Header = "Iterations";
                    labelDurationA.Content = string.Format("{0} iterations", selected.Intervals);
                }
            }
        }

        private void SetTableValuesB(TrialResult baseTrial, TrialResult selected)
        {


            if (selected != null)
            {
                if (baseTrial == null)
                {
                    labelTrialB.Content = selected.Trial; 
                    labelCostB.Content = selected.Cost.ToString(selected.CurrencyFormat);
                    labelCostOfDelayB.Content = selected.CostOfDelay.ToString(selected.CurrencyFormat);
                    labelTotalCostB.Content = selected.TotalCost.ToString(selected.CurrencyFormat);
                    
                    labelDurationB.Content = string.Format("{0} {1}", selected.Intervals, selected.Kanban ? "days" : "iterations");   
                    labelForecastB.Content = string.Format("{0}, {1} days from now", selected.Date, selected.CalendarDaysFromNow);
                }
                else
                {
                    labelTrialB.Content = string.Format("{0} compared to trial {1}",selected.Trial, baseTrial.Trial);

                    labelCostB.Content = string.Format("{0} compared to {1} = {2} {4} ({3}%)",
                        selected.Cost.ToString(selected.CurrencyFormat),
                        baseTrial.Cost.ToString(baseTrial.CurrencyFormat),
                        Math.Abs(selected.Cost - baseTrial.Cost).ToString(baseTrial.CurrencyFormat),
                        Math.Round(((selected.Cost - baseTrial.Cost) / baseTrial.Cost) * 100, 0),
                        selected.Cost < baseTrial.Cost ? "Lower" : "Higher");

                    labelCostOfDelayB.Content = string.Format("{0} compared to {1} = {2} {4} ({3}%)",
                        selected.CostOfDelay.ToString(selected.CurrencyFormat),
                        baseTrial.CostOfDelay.ToString(baseTrial.CurrencyFormat),
                        Math.Abs(selected.CostOfDelay - baseTrial.CostOfDelay).ToString(baseTrial.CurrencyFormat),
                        Math.Round(((selected.CostOfDelay - baseTrial.CostOfDelay) / baseTrial.CostOfDelay) * 100, 0),
                        selected.CostOfDelay < baseTrial.CostOfDelay ? "Lower" : "Higher");

                    labelTotalCostB.Content = string.Format("{0} compared to {1} = {2} {4} ({3}%)",
                        selected.TotalCost.ToString(selected.CurrencyFormat),
                        baseTrial.TotalCost.ToString(baseTrial.CurrencyFormat),
                        Math.Abs(selected.TotalCost - baseTrial.TotalCost).ToString(baseTrial.CurrencyFormat),
                        Math.Round(((selected.TotalCost - baseTrial.TotalCost) / baseTrial.TotalCost) * 100, 0),
                        selected.TotalCost < baseTrial.TotalCost ? "Lower" : "Higher");



                    labelDurationB.Content = string.Format("{0} {5} compared to {1} = {2} {5} {4} ({3}%)",
                        selected.Intervals.ToString(),
                        baseTrial.Intervals.ToString(),
                        Math.Abs(selected.Intervals - baseTrial.Intervals).ToString(),
                        Math.Round(((selected.Intervals * 1.0 - baseTrial.Intervals * 1.0) / baseTrial.Intervals * 1.0) * 100.0, 0),
                        selected.Intervals < baseTrial.Intervals ? "Faster" : "Slower",
                        selected.Kanban ? "days" : "iterations");




                    labelForecastB.Content = string.Format("{0} compared to {1} = {2} days {4} ({3}%) ",
                        selected.Date,
                        baseTrial.Date,
                        Math.Abs(selected.CalendarDaysFromNow - baseTrial.CalendarDaysFromNow),
                         Math.Round(((selected.CalendarDaysFromNow * 1.0 - baseTrial.CalendarDaysFromNow * 1.0) / baseTrial.CalendarDaysFromNow * 1.0) * 100.0, 0),
                         selected.CalendarDaysFromNow < baseTrial.CalendarDaysFromNow ? "Sooner" : "Later"
                        );
                }
            }
        }


        private TrialResult panelTrialSelection(ComboBox box)
        {
            if (box != null)
            {
                switch (box.SelectedIndex)
                {
                    case 0: return BaselineTrial;
                    case 1: return SelectedTrial;
                    case 2: return LastTrial;
                    case 3: return ShortestTrial;
                    case 4: return EarliestForecastTrial;
                    case 5: return LowestCostTrial;
                    default:
                        break;
                }
            }
            
            return null;
        }

        private void listResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setTrialParameterValues((TrialResult)listResults.SelectedItem);

            // update the tables.
            SetTableValuesA(panelTrialSelection(comboA));
            SetTableValuesB(panelTrialSelection(comboA), panelTrialSelection(comboB));
        }

        private void setTrialParameterValues(TrialResult selected)
        {
            if (selected == null) return;

            foreach (var entry in selected.ParameterValues)
                entry.Key.CurrentValue = entry.Value;
        }

        private void comboA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetTableValuesA(panelTrialSelection(comboA));
            SetTableValuesB(panelTrialSelection(comboA), panelTrialSelection(comboB));
        }

        private void comboB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listResults.SelectedItem = panelTrialSelection(comboB);
        }
    
    }



    public class TrialResults : ObservableCollection<TrialResult>
    {

    }

    public class TrialResult : INotifyPropertyChanged
    {
        private Dictionary<MoveableParameter, object> _parameterValues = new Dictionary<MoveableParameter, object>();
        private TrialResult _previousResult;
        private int _trial;
        private int _intervals;
        private string _date;
        private double _cost;
        private double _costOfDelay;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public bool Kanban { get; set; }

        public string DateFormat { get; set; }
        public string CurrencyFormat { get; set; }

        public Dictionary<MoveableParameter, object> ParameterValues
        {
            get { return _parameterValues; }
        }

        public TrialResult PreviousTrial
        {
            get
            { return _previousResult; }
            set
            {
                _previousResult = value;
                OnPropertyChanged(new PropertyChangedEventArgs("PreviousTrial"));
            }
        }



        public int Trial 
        {
            get
            {
                return _trial;
            }
            set
            {
                _trial = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Trial"));
            }
        }

        public int Intervals
        {
            get
            {
                return _intervals;
            }
            set
            {
                _intervals = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Intervals"));
            }
        }


        public string Date
        {
            get
            {
                return _date;
            }
            set
            {
                _date = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Date"));
                OnPropertyChanged(new PropertyChangedEventArgs("CalendarDaysFromNow"));
            }
        }


        public double Cost
        {
            get
            {
                return _cost;
            }
            set
            {
                _cost = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Cost"));
                OnPropertyChanged(new PropertyChangedEventArgs("TotalCost"));
            }
        }

        public double CostOfDelay
        {
            get
            {
                return _costOfDelay;
            }
            set
            {
                _costOfDelay = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CostOfDelay"));
                OnPropertyChanged(new PropertyChangedEventArgs("TotalCost"));
            }
        }


        public int CalendarDaysFromNow
        {
            get
            {
                TimeSpan span = Date.ToSafeDate(DateFormat, DateTime.Now) - DateTime.Now;
                return span.Days;
            }
        }

        public double TotalCost { get { return Cost + CostOfDelay; } }

        public override string ToString()
        {
            if (PreviousTrial == null)
                return string.Format("Trial: {0} {1} {4}, ending {2} ({3} days from now)", Trial, Intervals, Date, CalendarDaysFromNow, Kanban ? "days" : "iterations");
            else
                return string.Format("Trial: {0} {1} {4}, ending {2} ({3} days from now)", Trial, Intervals, Date, CalendarDaysFromNow, Kanban ? "days" : "iterations");
        }

    }
}
