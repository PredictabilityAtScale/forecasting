using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows.Threading;
using FocusedObjective.Simulation.Viewers;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;

namespace FocusedObjective.KanbanSim
{
    internal class ProjectState
    {
        internal ProjectState()
        {
        }

        private bool _alreadySimulating = false;

        private string _lastSavedSimML = "";
        private string _currentFileName = "";

        internal string CurrentFileName
        {
            get 
            { 
                return _currentFileName;  
            }
            set 
            { 
                _currentFileName = value;
                ModelEditorUserControl.UpdateFileNameAndStatus();
            }
        }

        internal bool AlreadySimulating
        {
            get { return _alreadySimulating; }
        }

        
        internal bool HasChangedSinceLastSave
        {
            get
            {
                if (_lastSavedSimML == "")
                    return false;

                return _lastSavedSimML != CurrentSimML;
            }
        }

        internal string LastSimulatedSimML { get; set; }
        
        internal FocusedObjective.Simulation.Simulator LastSimulator 
        { 
            get; set; 
        }

        internal Exception LastException { get; set; }
        
        internal KanbanBoardUserControl KanbanBoardUserControl { get; set; }
        internal ScrumBoardUserControl ScrumBoardUserControl { get; set; }
        internal ChartsUserControl ChartUserControl { get; set; }
        internal ModelEditorUserControl ModelEditorUserControl { get; set; }
        
        internal string CurrentSimML 
        {
            get
            {
                return this.ModelEditorUserControl.CurrentSimML;
            }
            set
            {
                this.ModelEditorUserControl.CurrentSimML = value;
            }
        }

        internal bool LoadExampleFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                if (ConfirmUnsavedWork())
                {
                    this.CurrentFileName = file;
                    this.CurrentSimML = System.IO.File.ReadAllText(this.CurrentFileName);
                    _lastSavedSimML = this.CurrentSimML;
                    return true;
                }
            }

            return false;
        }

        internal bool LoadFileCommand()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Title = "Open file...";
            dlg.FileName = "";
            dlg.DefaultExt = ".simML";
            dlg.Filter = "SimML Files (.simML)|*.simML|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (dlg.ShowDialog() == true)
            {
                if (ConfirmUnsavedWork())
                {
                    this.CurrentFileName = dlg.FileName;
                    this.CurrentSimML = System.IO.File.ReadAllText(this.CurrentFileName);
                    _lastSavedSimML = this.CurrentSimML;
                    return true;
                }
                
            }

            return false;
        }

        internal bool SaveFileCommand()
        {
            if (string.IsNullOrWhiteSpace(CurrentFileName))
                return SaveAsFileCommand();

            try
            {
                LastException = null;
                System.IO.File.WriteAllText(CurrentFileName, this.CurrentSimML);
                _lastSavedSimML = this.CurrentSimML;
                ModelEditorUserControl.UpdateFileNameAndStatus();
                return true;
            }
            catch (Exception exc)
            {
                LastException = exc;
                SetError(exc);
            }

            return false;
        }

        internal bool NewFileCommand()
        {
            // does the current file need saving?
            if (ConfirmUnsavedWork())
            {
                // clean up this project instance
                this.KanbanBoardUserControl = null;
                this.ScrumBoardUserControl = null;
                this.CurrentFileName = "";
                this.CurrentSimML = "";
                _lastSavedSimML = this.CurrentSimML; 
                this.LastException = null;
                this.LastSimulatedSimML = "";
                this.LastSimulator = null;

                this.ChartUserControl.ClearChartsAndData();

                return true;
            }
            else
            {
                SetError("'New' command aborted", "INFO"); 
                return false;
            }
        }

        internal bool ConfirmUnsavedWork()
        {
            bool safeToContinue = true;

            if (HasChangedSinceLastSave)
            {
                MessageBoxResult result = MessageBox.Show(
                    "** WARNING: Current model file is unsaved **\n\nDo you want to save it now?\n\nClick Yes to Save\nClick No to continue WITHOUT saving\nClick Cancel to abort.",
                    "Save Confirmation", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    if (SaveFileCommand())
                        safeToContinue = true;
                    else
                        safeToContinue = false; // unable to save for some reason.
                }
                else
                {
                    if (result == MessageBoxResult.Cancel)
                        safeToContinue = false;
                    else // no
                        safeToContinue = true;
                }
            }

            return safeToContinue;
        }

        internal bool SaveAsFileCommand()
        {
            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Title = "Save current file as...";

            if (!string.IsNullOrWhiteSpace(CurrentFileName) && System.IO.File.Exists(CurrentFileName))
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(CurrentFileName);
                dlg.FileName = System.IO.Path.GetFileName(CurrentFileName);
            }

            dlg.DefaultExt = ".simML";
            dlg.Filter = "SimML Files (.simML)|*.simML|XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            dlg.CheckPathExists = true;
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;

            if (dlg.ShowDialog() == true)
            {
                CurrentFileName = dlg.FileName;
                return SaveFileCommand();
            }

            return false;
        }

        internal bool ParseCurrentSimML()
        {
            bool result = false;

            try
            {
                FocusedObjective.Simulation.Simulator sim = new Simulation.Simulator(this.CurrentSimML);
                result = sim.Execute(true);
                this.LastSimulator = sim;
            }
            catch(Exception e)
            {
                this.LastException = e;
                SetError(e);
            }

            return result;
        }

        internal class SimInput
        {
            internal XDocument Model { get; set; }
            internal int MonteCarloCycles { get; set; }
            internal bool ForceRefresh { get; set; }

            internal string AggregationValue { get; set; }

            internal bool ForecastDateCommand { get; set; }

            internal BackgroundWorker Worker { get; set; }


            internal SimulatingProgress ProgressWindow { get; set; }

            public Contract.ExecuteSensitivityData SensitivityCommand { get; set; }

            public Contract.ExecuteAddStaffData AddStaffCommand { get; set; }

            public Contract.ForecastPermutationsEnum ForecastDatePermutation { get; set; }
        }

        internal class SimResult
        {
            internal FocusedObjective.Simulation.Simulator Simulator { get; set; }
            internal string DateFormat { get; set; }
            internal bool Result { get; set; }
            internal bool ForceRefresh { get; set; }

            public bool Canceled { get; set; }
        }

        private SimulatingProgress progress = null;
        
        internal void SimulateCurrentSimML(int monteCarloCycles = 0, string aggregationValue = "", bool forceRefresh = false, bool executeInCloud = false)
        {
            // if already simulating, exit now
            if (_alreadySimulating)
                return;

            // if no change....
            if (!forceRefresh && LastSimulatedSimML == CurrentSimML)
                return;

            ClearError(); 
            
            XDocument model = null;
            this.LastException = null;
            bool proceed = false;

            try
            {
                if (this.ModelEditorUserControl.CurrentlyValidSimML)
                {
                    // set the correct commands.... Visual, Monte-carlo at a minimum
                    model = XDocument.Parse(this.CurrentSimML.Trim(), LoadOptions.SetLineInfo);
                    
                    // add license...
                    addLicenseElement(model);

                    proceed = true;                    
                }
                else
                {
                    // current simml in text editor not valid....
                    SetError("Failed to simulate. The current Model text is invalid. Check the 'Errors' tab on the model page.");
                }
            }
            catch(Exception e)
            {
                this.LastException = e;
                SetError(e);
            }

            if (!proceed)
                return;

            // let sim
            BackgroundWorker worker = new BackgroundWorker();
            
            progress = new SimulatingProgress(worker, monteCarloCycles);

            SimInput inputState = new SimInput {
                Model = model,
                MonteCarloCycles = monteCarloCycles,
                AggregationValue = aggregationValue,
                ForceRefresh = forceRefresh,
                Worker = worker,
            };

            // Set up the Background Worker Events
            worker.DoWork -= backgroundWorker_DoWork; 
            worker.DoWork += backgroundWorker_DoWork;
            
            worker.RunWorkerCompleted -=
                             backgroundWorker_RunWorkerCompleted;
            worker.RunWorkerCompleted +=
                             backgroundWorker_RunWorkerCompleted;

            worker.ProgressChanged -= backgroundWorker_ReportProgress; 
            worker.ProgressChanged += backgroundWorker_ReportProgress;
            worker.WorkerReportsProgress = true;

            worker.WorkerSupportsCancellation = true;

            progress.Show();

            _alreadySimulating = true;

            worker.RunWorkerAsync(inputState);
        }

        internal void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            SimInput state = e.Argument as SimInput;
            
            SimResult result = new SimResult();
            e.Result = result;
            

            // pass this on...
            result.ForceRefresh = state.ForceRefresh;
            
            FocusedObjective.Contract.SimulationData data = new Contract.SimulationData(
               state.Model);


            // add a visual command if there isn't one...
            if (data.Execute.Visual == null)
            {
                data.Execute.Visual = new Contract.ExecuteVisualData();
                data.Execute.Visual.GenerateVideo = false;
                data.Execute.Visual.GeneratePositionData = true;
            }

            data.Execute.Visual.ShowVisualizer = false;


            if (state.MonteCarloCycles <= 0)
            {
                data.Execute.MonteCarlo = null;
            }
            else
            {
                data.Execute.MonteCarlo = new Contract.ExecuteMonteCarloData();
                data.Execute.MonteCarlo.Cycles = state.MonteCarloCycles;
            }

            data.Execute.ModelAudit = new Contract.ExecuteModelAuditData();

            data.Execute.AddStaff = null;
            data.Execute.ForecastDate = null;
            data.Execute.Sensitivity = null;

            if (!string.IsNullOrWhiteSpace(state.AggregationValue))
            {
                data.Execute.AggregationValue =
                    (Contract.AggregationValueEnum)Enum.Parse(
                        typeof(Contract.AggregationValueEnum),
                        state.AggregationValue,
                        true);
            }

            FocusedObjective.Simulation.Simulator sim = new Simulation.Simulator(
                data.AsXML(data.Execute.SimulationType).ToString());

            result.Result = sim.Execute(false, state.Worker);
            result.Simulator = sim;
            result.DateFormat = data.Execute.DateFormat;

            if (state.Worker.CancellationPending)
                result.Canceled = true;
            
        }


        void backgroundWorker_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
             progress.SetProgressMessage(e.ProgressPercentage, (string)e.UserState);
        }

        // Completed Method
        void backgroundWorker_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            _alreadySimulating = false;
            progress.Close();

            if (e.Error != null)
            {
                SetError(e.Error);
                return;
            }

            if (e.Cancelled || (e.Result != null && ((SimResult)e.Result).Canceled))
            {
                SetError("Simulation cancelled at user request. Partial results returned.", "INFO");
            }

            SimResult result = (SimResult)e.Result;

            if (result.Result)
            {
                this.LastSimulator = result.Simulator;
                this.LastSimulatedSimML = this.CurrentSimML;

                this.ModelEditorUserControl.CurrentResults = this.LastSimulator.Result != null ? this.LastSimulator.Result.ToString() : string.Empty;

                if (result.Simulator.KanbanUserControl != null)
                {
                    this.SimulationType = FocusedObjective.Contract.SimulationTypeEnum.Kanban;
                    this.KanbanBoardUserControl = result.Simulator.KanbanUserControl;
                    BoardScrollViewer.Content = this.KanbanBoardUserControl;
                    this.KanbanBoardUserControl.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                    this.KanbanBoardUserControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                }
                else
                {
                    if (result.Simulator.ScrumUserControl != null)
                    {
                        this.SimulationType = FocusedObjective.Contract.SimulationTypeEnum.Scrum;
                        this.ScrumBoardUserControl = result.Simulator.ScrumUserControl;
                        BoardScrollViewer.Content = this.ScrumBoardUserControl;
                        this.ScrumBoardUserControl.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
                        this.ScrumBoardUserControl.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    }
                    else
                    {
                        // add the placeholder instruction panel with buttons
                    }
                }

                // chart tab
                this.ChartUserControl.PopulateCharts(this, result.ForceRefresh);
            }
            else
            {
                if (result.Simulator.Result != null)
                {
                    XAttribute errorMessage = result.Simulator.Result.Attribute("errorMessage");
                    if (errorMessage != null)
                        SetError(string.Format("Failed to complete simulation. Error message: {0}", errorMessage.Value));
                    else
                        SetError("Failed to complete simulation. Often because the 'limitIntervalsTo' setting in the model is set too low. Increase it and try again.");
                }
            }
        }

        public TextBlock ErrorTextBlock { get; set; }

        public void SetError(string message, string severity = "ERROR")
        {
            ErrorTextBlock.Text = string.Format("{0}: {1} - [click to clear error]", severity, message);
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        public void SetError(Exception e)
        {
            ErrorTextBlock.Text = string.Format("ERROR: {0} - {1} - [click to clear error]", e.GetType().ToString(), e.Message);
            ErrorTextBlock.Visibility = Visibility.Visible;
        }

        public void ClearError()
        {
            ErrorTextBlock.Text = "";
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }

        public ScrollViewer BoardScrollViewer { get; set; }
        public DockPanel BoardDockPanel { get; set; }

        private void addLicenseElement(XDocument currentSimML)
        {
            try
            {
                XElement licElement = null;
                XElement simElement = currentSimML.Element("simulation");

                if (simElement != null)
                {
                    // 1. is there an element in the current SimML, just use it. 
                    licElement = simElement.Element("license");
                    if (licElement == null)
                    {
                        // 2. is there license text in the settings?
                        if (Settings.Default.LicenseText != null &&
                            !string.IsNullOrWhiteSpace(Settings.Default.LicenseText))
                        {
                            licElement = XElement.Parse(Settings.Default.LicenseText);
                            if (licElement != null)
                                simElement.Add(licElement);
                        }
                        else
                        {
                            // 3. is there a license.lic file in the app folder
                            string file =
                                System.IO.Path.Combine(
                                System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                                "license.lic");

                            if (System.IO.File.Exists(file))
                            {
                                XDocument licDoc = XDocument.Load(file);
                                if (licDoc != null && licDoc.Element("license") != null)
                                {
                                    licElement = licDoc.Element("license");
                                    simElement.Add(licElement);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                
                // report the error...
                SetError("Built-in license being used which may reduce simulation functionality. Unable to read a valid license key from the License Manager (see the Help tab) or from the license.lic file.", "INFO");
            }
        }

        internal bool SimulateAdvancedCommand(
            RunWorkerCompletedEventHandler onComplete, 
            ProgressChangedEventHandler onProgress,
            BackgroundWorker worker,
            SimulatingProgress progress,
            int monteCarloCycles = 0,
            string aggregationValue = "", 
            bool forecastDateCommand = false,
            Contract.ForecastPermutationsEnum permutations = Contract.ForecastPermutationsEnum.None,
            Contract.ExecuteSensitivityData sensitivityData = null,
            Contract.ExecuteAddStaffData addStaffData = null)
        {
            if (worker.IsBusy)
                return false;

            ClearError();

            XDocument model = null;
            this.LastException = null;
            bool proceed = false;

            try
            {
                if (this.ModelEditorUserControl.CurrentlyValidSimML)
                {
                    // set the correct commands.... Visual, Monte-carlo at a minimum
                    model = XDocument.Parse(this.CurrentSimML.Trim(), LoadOptions.SetLineInfo);

                    // add license...
                    addLicenseElement(model);

                    proceed = true;
                }
                else
                {
                    // current simml in text editor not valid....
                    SetError("Failed to simulate. The current Model text is invalid. Check the 'Errors' tab on the model page.");
                }
            }
            catch (Exception e)
            {
                this.LastException = e;
                SetError(e);
            }

            if (!proceed)
                return proceed;

            // lets sim


            SimInput inputState = new SimInput
            {
                Model = model,
                MonteCarloCycles = monteCarloCycles,
                AggregationValue = aggregationValue,
                ForceRefresh = true,
                ForecastDateCommand = forecastDateCommand,
                ForecastDatePermutation = permutations,
                SensitivityCommand = sensitivityData,
                AddStaffCommand = addStaffData,
                Worker = worker
            };

            // Set up the Background Worker Events
            worker.DoWork -= backgroundWorkerAdvancedSim_DoWork;
            worker.DoWork += backgroundWorkerAdvancedSim_DoWork;

            worker.RunWorkerCompleted -= onComplete;
            worker.RunWorkerCompleted += onComplete;

            if (onProgress != null)
            {
                worker.ProgressChanged -= onProgress;
                worker.ProgressChanged += onProgress;
                worker.WorkerReportsProgress = true;
            }
            else
            {
                worker.WorkerReportsProgress = false;
            }

            worker.WorkerSupportsCancellation = true;

            progress.Show();
            worker.RunWorkerAsync(inputState);

            return true;
        }

        internal void backgroundWorkerAdvancedSim_DoWork(object sender, DoWorkEventArgs e)
        {
            SimInput state = e.Argument as SimInput;

            

            SimResult result = new SimResult();
            e.Result = result;

            FocusedObjective.Contract.SimulationData data = new Contract.SimulationData(
               state.Model);

            data.Execute.Visual = null;
            data.Execute.MonteCarlo = null;
            data.Execute.ModelAudit = null;

            if (state.AddStaffCommand != null)
            {
                data.Execute.AddStaff = state.AddStaffCommand;
                data.Execute.AddStaff.Cycles = state.MonteCarloCycles;
            }

            if (state.SensitivityCommand != null)
            {
                data.Execute.Sensitivity = state.SensitivityCommand;
                data.Execute.Sensitivity.Cycles = state.MonteCarloCycles;
            }

            if (state.ForecastDateCommand && data.Setup.ForecastDate != null)
            {
                data.Execute.ForecastDate = new Contract.ExecuteForecastDateData();
                data.Execute.ForecastDate.ReturnProgressData = true; 
                data.Execute.ForecastDate.Cycles = state.MonteCarloCycles;
                data.Execute.ForecastDate.Permutations = state.ForecastDatePermutation;
            }
            else
            {
                data.Execute.ForecastDate = null;
            }

            if (!string.IsNullOrWhiteSpace(state.AggregationValue))
            {
                data.Execute.AggregationValue =
                    (Contract.AggregationValueEnum)Enum.Parse(
                        typeof(Contract.AggregationValueEnum),
                        state.AggregationValue,
                        true);
            }

            FocusedObjective.Simulation.Simulator sim = 
                new Simulation.Simulator(data.AsXML(
                    data.Execute.SimulationType).ToString());

            result.Result = sim.Execute(false, state.Worker);
            result.Simulator = sim;
            result.DateFormat = data.Execute.DateFormat;

            if (state.Worker.CancellationPending)
                result.Canceled = true;

        }


        public Contract.SimulationTypeEnum SimulationType { get; set; }
    }

}
