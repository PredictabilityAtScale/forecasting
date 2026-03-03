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

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for ForecastDate.xaml
    /// </summary>
    public partial class ForecastDateScrum : Window
    {
        ProjectState _state = null;
        int _monteCarloCycles = 100;
        string _aggregationValue = "";
        private XElement _forecastResults = null;
        private XElement _lastResults = null;
        private SimulatingProgress progress;
        private bool doingPermutations = false;

        public XElement ForecastResults
        {
            get { return _forecastResults; }
            set { _forecastResults = value; }
        }

        internal ForecastDateScrum(ProjectState state, int monteCarloCycles = 100, string aggregationValue = "")
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
            BackgroundWorker worker = new BackgroundWorker();
            progress = new SimulatingProgress(worker, _monteCarloCycles);

            _state.SimulateAdvancedCommand(
                backgroundWorkerAdvancedSim_RunWorkerCompleted,
                backgroundWorkerAdvancedSim_ReportProgress,
                worker,
                progress,
                _monteCarloCycles,  
                _aggregationValue,
                true);
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
                    comboBoxPermutationResult.DataContext = result.Simulator.Result.Element("forecastDatePermutations");

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

                    dataGridPermutationResults.DataContext = null;

                    labelPermutation.IsEnabled = false;
                    comboBoxPermutationResult.IsEnabled = false;
                }
            }
            else
            {
               // report error
                MessageBox.Show("An error occured during simulation. Check the current model visually simulates and that you have a valid <forecastDate ...> section in your model.", "Unable to simulate...");
                this.Close();
            }
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
                GenerateAndLaunchHTMLReport();
            }
            else
            {
                MessageBox.Show("Simulation failed.", "No Data");
            }
        }

        private string GenerateAndLaunchHTMLReport()
        {
            string filenames = "";

            // set the folder
            string folder = "Scrum";

            try
            {
                if (_forecastResults != null)
                    filenames += publishForecastResultsHTML(folder, _forecastResults);
            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format("Error creating the HTML report. Message: {0}", e.Message));
            }

            return filenames;
        }

        private string publishForecastResultsHTML(string folder, XElement data)
        {
            string html = processRazorTemplate(folder, "ForecastDateResults.cshtml");
            string htmlFilename = System.IO.Path.GetTempFileName();
            htmlFilename = System.IO.Path.ChangeExtension(htmlFilename, "html");
            System.IO.File.WriteAllText(htmlFilename, html);
            System.Diagnostics.Process.Start(htmlFilename);

            return htmlFilename;
        }

        private string processRazorTemplate(string folder, string templateFilename)
        {
            string result = null;

            try
            {
                string htmlFilename = System.IO.Path.GetTempFileName();
                htmlFilename = System.IO.Path.ChangeExtension(htmlFilename, "html");

                string f = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    "Templates",
                    folder,
                    templateFilename);


                string template = System.IO.File.ReadAllText(f);
                /*
                 * var engine = new RazorEngine<RazorTemplateBase>();
                var context = _lastResults.ToString();

                string output = engine.RenderTemplate(
                    template,
                    new string[] { "System.Xml.Linq.dll", "System.Xml.dll" },
                    context);

                if (output == null)
                    result = "*** ERROR:\r\n" + engine.ErrorMessage;
                else
                    result = output;
                */

            }
            catch (Exception e)
            {
                result = "*** ERROR:\r\n" + e.Message;
            }

            return result;
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

    }
}
