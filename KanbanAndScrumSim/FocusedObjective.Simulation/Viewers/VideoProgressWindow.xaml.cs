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

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class VideoProgressWindow : Window
    {
        public VideoProgressWindow()
        {
            InitializeComponent();
        }

        private delegate void UpdateProgressBarDelegate(
                System.Windows.DependencyProperty dp, Object value);

        private UpdateProgressBarDelegate updatePbDelegate = null;

        internal void SetProgressValue(int value)
        {
                if (updatePbDelegate == null)
                    updatePbDelegate = new UpdateProgressBarDelegate(videoProgressBar.SetValue);

                Dispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, (double)(value * 1.0) });
        }


        internal ProgressBar VideoProgressBar
        {
            get { return videoProgressBar; }
        }
    }
}
