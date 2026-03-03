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


namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for Sensitivity.xaml
    /// </summary>
    public partial class Sensitivity : Window
    {

        ProjectState _state = null;
        int _monteCarloCycles = 100;
        string _aggregationValue = "";
        private XElement _sensitivityResults = null;
        private XElement _lastResults = null;
        private SimulatingProgress progress;

        public XElement SensitivityResults
        {
            get { return _sensitivityResults; }
            set { _sensitivityResults = value; }
        }

        internal Sensitivity(ProjectState state, int monteCarloCycles = 100, string aggregationValue = "")
        {
            InitializeComponent();

            _state = state;
            _monteCarloCycles = monteCarloCycles;
            _aggregationValue = aggregationValue;
        }

        private void buttonExecute_Click(object sender, RoutedEventArgs e)
        {
            FocusedObjective.Contract.ExecuteSensitivityData data = null;

            if (checkBoxUseModel.IsChecked == false)
            {
                data = new Contract.ExecuteSensitivityData();

                data.Cycles = _monteCarloCycles;
                data.EstimateMultiplier = /*estimateMultiplier.Value ??*/ 2.0;
                data.OccurrenceMultiplier = /*occurrenceMultiplier.Value ??*/ 0.5;
                data.SortOrder = comboBoxSort.SelectedIndex == 0 ? Contract.SortOrderEnum.Ascending : Contract.SortOrderEnum.Descending;

                switch (comboMeasure.SelectedIndex)
                {
                    case 0: data.SensitivityType = Contract.SensitivityTypeEnum.Intervals; break;
                    case 1: data.SensitivityType = Contract.SensitivityTypeEnum.CycleTime; break;
                    case 2: data.SensitivityType = Contract.SensitivityTypeEnum.Empty; break;
                    case 3: data.SensitivityType = Contract.SensitivityTypeEnum.Queued; break;
                    default: data.SensitivityType = Contract.SensitivityTypeEnum.Intervals; break;
                }
            }
            else
            {
                data = extractCommandFromModel();

                if (data == null)
                    this.Close();
            }

            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, _monteCarloCycles);

            progress.ProgressMessageFormatString = "{0} of {1} ({2}%)";

            _state.SimulateAdvancedCommand(
                backgroundWorkerAdvancedSim_RunWorkerCompleted,
                backgroundWorkerAdvancedSim_ReportProgress,
                worker,
                progress,
                _monteCarloCycles,
                _aggregationValue,
                false,
                Contract.ForecastPermutationsEnum.None,
                data);


        }

        private Contract.ExecuteSensitivityData extractCommandFromModel()
        {
            if (!_state.ParseCurrentSimML())
            {
                MessageBox.Show("Error in the current SimML model. Check the Errors tab for details.", "Error in Model");
                return null;
            }

            Contract.SimulationData data = new Contract.SimulationData(XDocument.Parse(_state.CurrentSimML));

            if (data != null)
            {
                if (data.Execute.Sensitivity != null)
                    return data.Execute.Sensitivity;
            }

            MessageBox.Show("Missing <sensitivity... command in the model.", "Missing Command");
            return null;
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
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates.", "Unable to simulate...");
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
                // bind results;
                _lastResults = result.Simulator.Result;
                _sensitivityResults = result.Simulator.Result.Element("sensitivity").Element("tests");
                dataGridResults.DataContext = _sensitivityResults;
                tabControl.SelectedIndex = 1;
            }
            else
            {
                // report error
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates.", "Unable to simulate...");
                this.Close();
            }
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAsHTML_Click(object sender, RoutedEventArgs e)
        {
            // as XML now


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
                MessageBox.Show("Execute an Sensitivity simulation by clicking the Execute button first.", "No Data");
            }
        }
    }
}
