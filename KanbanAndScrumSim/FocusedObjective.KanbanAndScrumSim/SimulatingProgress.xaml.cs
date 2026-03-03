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

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for SimulatingProgress.xaml
    /// </summary>
    public partial class SimulatingProgress : Window
    {
        BackgroundWorker _worker = null;
        int _monteCarloCycles;

        public SimulatingProgress(BackgroundWorker worker, int monteCarloCycles)
        {
            InitializeComponent();
            _worker = worker;
            _monteCarloCycles = monteCarloCycles;

            if (_worker == null)
                buttonCancel.Visibility = System.Windows.Visibility.Hidden;

            this.BringIntoView();
            
        }

        private string _progressMessageString = "Cycle {0} of {1} ({2}%)";

        public string ProgressMessageFormatString
        {
            get { return _progressMessageString; }
            set { _progressMessageString = value; }
        }

        public void SetProgressMessage(int percent, string message)
        {
            if (message.StartsWith("#"))
            {
                textBlockProgress.Text = message.Substring(1);
            }
            else
            {
                textBlockProgress.Text = string.Format(ProgressMessageFormatString,
                    message,
                    _monteCarloCycles,
                    percent);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_worker != null)
            {
                _worker.CancelAsync();
                buttonCancel.IsEnabled = false;
            }
        }
    }
}
