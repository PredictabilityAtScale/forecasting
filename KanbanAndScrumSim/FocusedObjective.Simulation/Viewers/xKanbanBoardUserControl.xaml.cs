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
    /// Interaction logic for KanbanBoardUserControl.xaml
    /// </summary>

    public partial class xKanbanBoardUserControl : UserControl
    {
        internal Image[,] positions;
        private FocusedObjective.Simulation.Kanban.KanbanSimulation _simulator;
        private List<SetupColumnData> _columnList;
        private List<TimeInterval> _simResults;
        private Dictionary<Card, CardUserControl> _cardControls = new Dictionary<Card, CardUserControl>();

        public xKanbanBoardUserControl()
        {
            InitializeComponent();
        }

        internal List<TimeInterval> SimResults
        {
            get { return _simResults; }
        }

        internal void ShowSimResults(FocusedObjective.Simulation.Kanban.KanbanSimulation simulator, int initialInterval = 0)
        {
            _simulator = simulator;
            _columnList = simulator.SimulationData.Setup.Columns;
            _simResults = simulator.ResultTimeIntervals;

            buildBoardBackground();
            buildCardControls();
            updateBoardForTimelinePosition(_columnList, _simResults, 0);
            sliderTimeline.Maximum = _simResults.Count - 1;
        }

        private void buildCardControls()
        {
            foreach (Card card in _simulator.AllCardsList)
            {
                CardUserControl c = new CardUserControl(card);

                c.Width = workItemWidth;
                c.Height = workItemWidth;

                // place off screen
                Canvas.SetTop(c, -500);
                Canvas.SetLeft(c, -500);

                BoardCanvas.Children.Add(c);
                _cardControls.Add(card, c);

            }
        }

        const int statusLayoutWidth = 90;
        const int statusGap = 5;
        const int workItemWidth = 70;

        Dictionary<SetupColumnData, Label> columnHeaderLabels = new Dictionary<SetupColumnData, Label>();

        private void buildBoardBackground()
        {
            List<SetupColumnData> columnList = _columnList;

            positions = new Image[columnList.Count, columnList.Max(s => s.HighestWipLimit)];

            // build the status columns
            for (int status = 0; status < columnList.Count; status++)
            {
                StatusGridHolder.ColumnDefinitions.Add(
                    new ColumnDefinition { Width = new GridLength(statusGap + statusLayoutWidth + statusGap) });

                // add the headers
                Label header = new Label();
                header.Name = "label_status_" + status.ToString();
                this.RegisterName(header.Name, header);
                Grid.SetColumn(header, status);
                Grid.SetRow(header, 0);


                header.Content = String.Format("{0}\n(limit: {1})", columnList[status].Name, columnList[status].WipLimit);

                StatusGridHolder.Children.Add(header);
                columnHeaderLabels.Add(columnList[status], header);

                // add the empty positions on the board
                for (int position = 0; position < columnList[status].HighestWipLimit; position++)
                {
                    Image emptyPosition = new Image();

                    var uriSource = new Uri("/FocusedObjective.Simulation;component/Viewers/images/empty.png", UriKind.Relative);

                    if (columnList[status].IsBuffer)
                        uriSource = new Uri("/FocusedObjective.Simulation;component/Viewers/images/empty_buffer.png", UriKind.Relative);

                    emptyPosition.Source = new BitmapImage(uriSource);
                    emptyPosition.Width = workItemWidth;
                    emptyPosition.Height = workItemWidth;
                    emptyPosition.Name = "position_" + status.ToString() + "_" + position.ToString();
                    this.RegisterName(emptyPosition.Name, emptyPosition);
                    Canvas.SetTop(emptyPosition, ((position * (statusLayoutWidth)) + statusGap));
                    Canvas.SetLeft(emptyPosition, statusGap + (status * (statusGap + statusLayoutWidth + statusGap)));
                    BoardCanvas.Children.Add(emptyPosition);

                    positions[status, position] = emptyPosition;
                }
            }
        }

        private void updateBoardForTimelinePosition(List<SetupColumnData> columnDefinitions, List<TimeInterval> intervals, int intervalToShow)
        {
            textBlockCurrentlyShowing.Text = string.Empty;

            if (intervalToShow < intervals.Count)
            {
                TimeInterval interval = intervals[intervalToShow];

                labelBacklog.Text = string.Format("Backlog\n({0} items)", interval.CountCardsInBacklog);
                labelComplete.Text = string.Format("Complete\n({0} items)", interval.CountCompletedCards);

                textBlockCurrentlyShowing.Text = string.Format("Currently showing interval {0} (elapsed time: {1} {2}) - Phase: {3}",
                    interval.Sequence, interval.ElapsedTime, _simulator.SimulationData.Execute.IntervalUnit, interval.Phase == null ? "default" : interval.Phase.Name);

                for (int colIndex = 0; colIndex < columnDefinitions.Count; colIndex++)
                {
                    FocusedObjective.Contract.SetupColumnData column = columnDefinitions[colIndex];


                    //TODO! this changes
                    Label l = null;
                    if (columnHeaderLabels.ContainsKey(column))
                        l = columnHeaderLabels[column];

                    // apply the phase WIP's if applied
                    if (interval.Phase != null && interval.Phase.Columns.Any())
                    {
                        SetupPhaseColumnData col = interval.Phase.Columns.FirstOrDefault(pc => pc.ColumnId == column.Id);

                        int showWips = column.WipLimit;

                        if (col != null)
                        {
                            l.Content = String.Format("{0}\n(phase wip: {1})", column.Name, col.WipLimit);
                            showWips = col.WipLimit;
                        }

                        // hide empty positions not in this phase
                        for (int ei = 0; ei < column.HighestWipLimit; ei++)
                        {
                            Image emptyImage = positions[colIndex, ei];

                            if (ei < showWips)
                                emptyImage.Visibility = System.Windows.Visibility.Visible;
                            else
                                emptyImage.Visibility = System.Windows.Visibility.Hidden;
                        }

                    }
                    else
                    {
                        l.Content = String.Format("{0}\n(limit: {1})", column.Name, column.WipLimit);

                        // show empty positions up to the column's WIP limit
                        // hide empty positions not in this phase
                        for (int ei = 0; ei < column.HighestWipLimit; ei++)
                        {
                            Image emptyImage = positions[colIndex, ei];

                            if (ei < column.WipLimit)
                                emptyImage.Visibility = System.Windows.Visibility.Visible;
                            else
                                emptyImage.Visibility = System.Windows.Visibility.Hidden;
                        }
                    }

                    for (int position = 0; position < column.HighestWipLimit; position++)
                    {
                        Card card = interval.GetCardInPositionForColumn(column, position);

                        // remove old card if there is one in this position
                        var oldControls = from cont in _cardControls.Values
                                          where positions[colIndex, position] != null &&
                                          Canvas.GetTop(cont) == Canvas.GetTop(positions[colIndex, position]) &&
                                          Canvas.GetLeft(cont) == Canvas.GetLeft(positions[colIndex, position])
                                          select cont;

                        foreach (var cont in oldControls)
                        {
                            if (card == null || card != cont.Card)
                            {
                                Canvas.SetLeft(cont, -500);
                                Canvas.SetTop(cont, -500);
                                cont.InvalidateVisual();
                            }
                        }

                        if (card == null)
                        {
                        }
                        else
                        {
                            CardUserControl control = _cardControls[card];

                            // we use the same top and left as the button placeholder
                            Image b = positions[colIndex, position];

                            //XXX
                            //control.SetStyleForTimeInterval(interval, column, position);
                            Canvas.SetTop(control, Canvas.GetTop(b));
                            Canvas.SetLeft(control, Canvas.GetLeft(b));
                            control.InvalidateVisual();

                        }

                    }
                }
            }
        }

        private void sliderTimeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updateBoardForTimelinePosition(_columnList, _simResults, (int)e.NewValue);
        }

        //internal string SaveAnimation(int fps, int dpi = 96)
        //{
        //    string filename = System.IO.Path.GetTempFileName();
        //    filename = System.IO.Path.ChangeExtension(filename, "avi");
        //    VideoProgressWindow progress = new VideoProgressWindow();
        //    try
        //    {
        //        progress.VideoProgressBar.Maximum = _simResults.Count;
        //        progress.VideoProgressBar.Minimum = 0;
        //        progress.VideoProgressBar.Value = 0;

        //        progress.Show();

        //        bool compress = false;

        //        if (scrollViewerBoard.ComputedHorizontalScrollBarVisibility == Visibility.Visible ||
        //            scrollViewerBoard.ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //        {
        //            // increase the horizontal window size until its not shown....
        //            while (scrollViewerBoard.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
        //            {
        //                this.Width += 25;
        //                this.UpdateLayout();
        //            }

        //            // increase the vertical window size until its not shown....
        //            while (scrollViewerBoard.ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //            {
        //                this.Height += 25;
        //                this.UpdateLayout();
        //            }
        //        }

        //        // fps can be overriden using the videoFramesPerSecond element in the SimML Execute\Visual element
        //        // default is 5.

        //        int num_total_frames = fps * _simResults.Count;

        //        var aviManager = new AviFile.AviManager(filename, false);
        //        AviFile.VideoStream aviStream = null;
        //        for (int i = 0; i < _simResults.Count; i++)
        //        {
        //            sliderTimeline.Value = i;

        //            this.KanbanGrid.UpdateLayout();

        //            string temp_bitmap = System.IO.Path.GetTempFileName();
        //            temp_bitmap = System.IO.Path.ChangeExtension(temp_bitmap, "png");
        //            ImageUtilities.SaveWindow(this, dpi, temp_bitmap);

        //            using (System.Drawing.Bitmap bm = new System.Drawing.Bitmap(temp_bitmap))
        //            {

        //                if (aviStream == null)
        //                {
        //                    aviStream = aviManager.AddVideoStream(compress, fps, bm);
        //                }
        //                else
        //                {
        //                    aviStream.AddFrame(bm);

        //                }
        //            }

        //            progress.SetProgressValue(i);
        //        }

        //        aviManager.Close();
        //    }
        //    finally
        //    {
        //        progress.Close();
        //        sliderTimeline.Value = 0;
        //    }

        //    return filename;
        //}

    }
}
