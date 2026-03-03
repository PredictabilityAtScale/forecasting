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
using FocusedObjective.Simulation.Kanban;
using System.Diagnostics;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for Kanban.xaml
    /// </summary>
    public partial class KanbanBoard : Window
    {
        private FocusedObjective.Simulation.Kanban.KanbanSimulation _simulator;
        private List<SetupColumnData> _columnList;
        private List<TimeInterval> _simResults;
        private Dictionary<Card, CardUserControl> _cardControls = new Dictionary<Card, CardUserControl>();
        private Dictionary<SetupColumnData, BoardColumn> _columnControls = new Dictionary<SetupColumnData, BoardColumn>();

        public KanbanBoard()
        {
            InitializeComponent();
        }

        internal void ShowSimResults(FocusedObjective.Simulation.Kanban.KanbanSimulation simulator, int initialInterval = 0)
        {
            _simulator = simulator;
            _columnList = simulator.SimulationData.Setup.Columns;
            _simResults = simulator.ResultTimeIntervals;
            buildBoardBackground();
            updateBoardForTimelinePosition( _simResults, 0);
            sliderTimeline.Maximum = _simResults.Count -1;
        }

        private void buildBoardBackground()
        {
            foreach (var column in _columnList)
            {
                var control = new BoardColumn(column);

                control.CurrentWipLimit = column.WipLimit;
                control.FillEmptyWipPositions();

                _columnControls.Add(column, control);
                columnsPanel.Children.Add(control);
            }
        }

        private void updateBoardForTimelinePosition(List<TimeInterval> intervals, int intervalToShow)
        {
            textBlockCurrentlyShowing.Text = string.Empty;

            if (intervalToShow < intervals.Count)
            {
                TimeInterval interval = intervals[intervalToShow];

                labelBacklog.Text = string.Format("Backlog\n({0} items)", interval.CountCardsInBacklog);
                labelComplete.Text = string.Format("Complete\n({0} items)", interval.CountCompletedCards);

                textBlockCurrentlyShowing.Text = string.Format("Currently showing interval {0} (elapsed time: {1} {2}) - Phase: {3}",
                    interval.Sequence, interval.ElapsedTime, _simulator.SimulationData.Execute.IntervalUnit, interval.Phase == null ? "default" : interval.Phase.Name);

                // clear all the current controls
                foreach (var column in _columnControls.Keys)
                    _columnControls[column].ClearAllControls();

                foreach (var column in _columnControls.Keys)
                {
                    // apply the phase WIP's if applied
                    if (interval.Phase != null && interval.Phase.Columns.Any())
                        _columnControls[column].CurrentPhaseColumnData =
                            interval.Phase.Columns.FirstOrDefault(pc => pc.ColumnId == column.Id);
                    else
                        _columnControls[column].CurrentPhaseColumnData = null;

                    // update for time interval
                    _columnControls[column].UpdateForTimeInterval(_cardControls, interval, _simulator.SimulationData.Execute.DateFormat);
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
