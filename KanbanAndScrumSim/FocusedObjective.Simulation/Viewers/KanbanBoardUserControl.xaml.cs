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

    public partial class KanbanBoardUserControl : UserControl
    {
        private FocusedObjective.Simulation.Kanban.KanbanSimulation _simulator;
        private List<SetupColumnData> _columnList;
        private List<TimeInterval> _simResults;
        private Dictionary<Card, CardUserControl> _cardControls = new Dictionary<Card, CardUserControl>();
        private Dictionary<SetupColumnData, BoardColumn> _columnControls = new Dictionary<SetupColumnData, BoardColumn>();

        public KanbanBoardUserControl()
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
            sliderTimelineTop.Maximum = _simResults.Count -1;
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

                if (interval.CurrentDate == null)
                {
                    textBlockCurrentlyShowing.Text = string.Format("Currently showing interval {0} (elapsed time: {1} {2}) - Phase: {3}. Value Delivered: {4}",
                        interval.Sequence,
                        interval.ElapsedTime,
                        _simulator.SimulationData.Execute.IntervalUnit,
                        interval.Phase == null ? "default" : interval.Phase.Name,
                         interval.ValueDeliveredSoFar.ToString(_simulator.SimulationData.Execute.CurrencyFormat));
                }
                else
                {
                    // show date instead
                    textBlockCurrentlyShowing.Text = string.Format("Currently showing interval {0} ({1}) - Phase: {2}. Value Delivered: {3}",
                        interval.Sequence,
                        interval.CurrentDate.Value.ToString(_simulator.SimulationData.Execute.DateFormat),
                        interval.Phase == null ? "default" : interval.Phase.Name,
                        interval.ValueDeliveredSoFar.ToString(_simulator.SimulationData.Execute.CurrencyFormat));
                }
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


    }
}
