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
using System.Windows.Navigation;
using System.Windows.Shapes;
using FocusedObjective.Contract;
using FocusedObjective.Simulation;
using FocusedObjective.Simulation.Scrum;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for ScrumBoardUserControl.xaml
    /// </summary>
    public partial class ScrumBoardUserControl : UserControl
    {
        internal Image[,] positions;
        private FocusedObjective.Simulation.Scrum.ScrumSimulation _simulator;
        private List<Iteration> _simResults;
        private Dictionary<Story, StoryUserControl> _storyControls = new Dictionary<Story, StoryUserControl>();

        public ScrumBoardUserControl()
        {
            InitializeComponent();
        }

        internal void ShowSimResults(FocusedObjective.Simulation.Scrum.ScrumSimulation simulator, int initialIteration = 0)
        {
            _simulator = simulator;
            _simResults = simulator.Iterations;

            buildBoardBackground();
            buildCardControls();
            updateBoardForTimelinePosition(_simResults, 0);
            sliderTimelineTop.Maximum = _simResults.Count - 1;
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
        const int workItemWidth = 125;

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


                if (iteration.CurrentDate == null)
                {
                    textBlockCurrentlyShowing.Text = string.Format("Currently showing iteration {0} - {1} story points allocated - Phase {2}. Value Delivered: {3}",
                        iteration.Sequence,
                        Math.Round(iteration.Stories.Sum(s => s.GetRemainingPoints(iteration.Sequence)), 3),
                        iteration.Phase == null ? "default" : iteration.Phase.Name,
                        iteration.ValueDeliveredSoFar.ToString(_simulator.SimulationData.Execute.CurrencyFormat));
                }
                else
                {
                    // show date instead
                    textBlockCurrentlyShowing.Text = string.Format("Currently showing iteration {0} (start: {4}) - {1} story points allocated - Phase {2}. Value Delivered: {3}",
                        iteration.Sequence,
                        Math.Round(iteration.Stories.Sum(s => s.GetRemainingPoints(iteration.Sequence)), 3),
                        iteration.Phase == null ? "default" : iteration.Phase.Name,
                        iteration.ValueDeliveredSoFar.ToString(_simulator.SimulationData.Execute.CurrencyFormat),
                        iteration.CurrentDate.Value.ToString(_simulator.SimulationData.Execute.DateFormat)
                        );

                }

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

    }
}
