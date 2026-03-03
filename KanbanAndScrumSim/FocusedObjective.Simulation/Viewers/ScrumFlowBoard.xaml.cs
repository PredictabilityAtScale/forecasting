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
using FocusedObjective.Contract;
using FocusedObjective.Simulation;
using FocusedObjective.Simulation.Scrum;
using System.Diagnostics;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for ScrumFlowBoard.xaml
    /// </summary>
    public partial class ScrumFlowBoard : Window
    {
        internal Image[,] positions;
        private FocusedObjective.Simulation.Scrum.ScrumSimulation _simulator;
        private List<Iteration> _simResults;
        private Dictionary<Story, StoryUserControl> _storyControls = new Dictionary<Story, StoryUserControl>();

        internal ScrumFlowBoard()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        internal void ShowSimResults(FocusedObjective.Simulation.Scrum.ScrumSimulation simulator, int initialIteration = 0)
        {
            _simulator = simulator;
            _simResults = simulator.Iterations;

            buildBoardBackground();
            buildCardControls();
            updateBoardForTimelinePosition( _simResults, 0);
            sliderTimeline.Maximum = _simResults.Count -1;
        }

        private void buildCardControls()
        {
            foreach (Story story in _simulator.AllStories)
            {
                StoryUserControl s = new StoryUserControl(story);
                s.Width = workItemWidth;
                s.Height = workItemWidth;
                _storyControls.Add(story, s);
            }
        }

        const int statusLayoutWidth = 90;
        const int statusGap = 5;
        const int workItemWidth = 70;

        private void buildBoardBackground()
        {
        }

        private void updateBoardForTimelinePosition(List<Iteration> iterations, int iterationToShow)
        {
            textBlockCurrentlyShowing.Text = string.Empty;

            if (iterationToShow < iterations.Count)
            {
                Iteration iteration = iterations[iterationToShow];

                labelBacklog.Text = string.Format("Backlog\n({0} items)", iteration.CountStoriesInBacklog);
                labelComplete.Text = string.Format("Complete\n({0} items)", iteration.CountStoriesInComplete);

                textBlockCurrentlyShowing.Text = string.Format("Currently showing iteration {0} - {1} story points allocated - Phase {2}",
                    iteration.Sequence,
                    Math.Round(iteration.Stories.Sum(s => s.GetRemainingPoints(iteration.Sequence)), 3),
                    iteration.Phase == null ? "default" : iteration.Phase.Name);

                // clear all positions
                BoardWrapPanel.Children.Clear();
                BoardWrapPanel.InvalidateVisual();

                foreach (var story in iteration.Stories)
                {
                    StoryUserControl control = _storyControls[story];
                    control.SetStyleForIteration(iteration, 0, 0);
                    BoardWrapPanel.Children.Add(control);
                    control.InvalidateVisual();
                }

            }
        }

        private void sliderTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateBoardForTimelinePosition(_simResults, (int)e.NewValue);
        }

        internal string SaveAnimation(int fps, int dpi = 96)
        {
            string filename = System.IO.Path.GetTempFileName();
            filename = System.IO.Path.ChangeExtension(filename, "avi");
            VideoProgressWindow progress = new VideoProgressWindow();
            try
            {
                progress.VideoProgressBar.Maximum = _simResults.Count;
                progress.VideoProgressBar.Minimum = 0;
                progress.VideoProgressBar.Value = 0;

                progress.Show();

                bool compress = false;

                if (scrollViewerBoard.ComputedHorizontalScrollBarVisibility == Visibility.Visible ||
                    scrollViewerBoard.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    // increase the horizontal window size until its not shown....
                    while (scrollViewerBoard.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
                    {
                        this.Width += 25;
                        this.UpdateLayout();
                    }

                    // increase the vertical window size until its not shown....
                    while (scrollViewerBoard.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                    {
                        this.Height += 25;
                        this.UpdateLayout();
                    }
                }

                // fps can be overriden using the videoFramesPerSecond element in the SimML Execute\Visual element
                // default is 5.

                int num_total_frames = fps * _simResults.Count;

                var aviManager = new AviFile.AviManager(filename, false);
                AviFile.VideoStream aviStream = null;
                for (int i = 0; i < _simResults.Count; i++)
                {
                    sliderTimeline.Value = i;

                    this.KanbanGrid.UpdateLayout();

                    string temp_bitmap = System.IO.Path.GetTempFileName();
                    temp_bitmap = System.IO.Path.ChangeExtension(temp_bitmap, "png");
                    ImageUtilities.SaveWindow(this, dpi, temp_bitmap);

                    using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(temp_bitmap))
                    {

                        if (aviStream == null)
                        {
                            aviStream = aviManager.AddVideoStream(compress, fps, bm);
                        }
                        else
                        {
                            aviStream.AddFrame(bm);

                        }
                    }

                    progress.SetProgressValue(i);
                }

                aviManager.Close();
            }
            finally
            {
                progress.Close();
                sliderTimeline.Value = 0;
            }

            return filename;
        }

    }
}
