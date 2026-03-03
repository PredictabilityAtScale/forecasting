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
    /// Interaction logic for AddStaff.xaml
    /// </summary>
    public partial class AddStaff : Window
    {

        ProjectState _state = null;
        int _monteCarloCycles = 100;
        string _aggregationValue = "";
        private XElement _addStaffResults = null;
        private XElement _lastResults = null;

        private SimulatingProgress progress;

        public XElement AddStaffResults
        {
            get { return _addStaffResults; }
            set { _addStaffResults = value; }
        }

        internal AddStaff(ProjectState state, int monteCarloCycles = 100, string aggregationValue = "")
        {
            InitializeComponent();

            _state = state;
            _monteCarloCycles = monteCarloCycles;
            _aggregationValue = aggregationValue;
        }

        private void buttonExecute_Click(object sender, RoutedEventArgs e)
        {
            FocusedObjective.Contract.ExecuteAddStaffData data = null;

            if (checkBoxUseModel.IsChecked == false)
            {
                data = new Contract.ExecuteAddStaffData();

                data.Cycles = _monteCarloCycles;
                data.Count = (int)Math.Round(upDownCount.Value ?? 3.0, 0);

                switch (comboLowest.SelectedIndex)
                {
                    case 0: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.Intervals; break;
                    case 1: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.CycleTime; break;
                    case 2: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.Empty; break;
                    case 3: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.Queued; break;
                    case 4: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.QueuedAndEmpty; break;
                    default: data.OptimizeForLowest = Contract.OptimizeForLowestEnum.Intervals; break;
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
                null,
                data);
            
        }

        private Contract.ExecuteAddStaffData extractCommandFromModel()
        {
            if (!_state.ParseCurrentSimML())
            {
                MessageBox.Show("Error in the current SimML model. Check the Errors tab for details.", "Error in Model");
                return null;
            }

            Contract.SimulationData data = new Contract.SimulationData(XDocument.Parse(_state.CurrentSimML));

            if (data != null)
            {
                if (data.Execute.AddStaff != null)
                    return data.Execute.AddStaff;
            }

            MessageBox.Show("Missing <addStaff... command in the model.", "Missing Command");
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
                _addStaffResults = result.Simulator.Result.Element("addStaff");
                dataGridResults.DataContext = _addStaffResults;
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
                MessageBox.Show("Execute an Add Staff simulation by clicking the Execute button first.", "No Data");
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_state.SimulationType == Contract.SimulationTypeEnum.Scrum)
            {
                MessageBox.Show("This simulation type currently only supports Kanban/Lean project types.");
                this.Close();
            }

        }


    }
}
