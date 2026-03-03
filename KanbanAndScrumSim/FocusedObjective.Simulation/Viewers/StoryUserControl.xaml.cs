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
using FocusedObjective.Simulation.Enums;
using FocusedObjective.Simulation;
using FocusedObjective.Simulation.Scrum;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for StoryUserControl.xaml
    /// </summary>
    public partial class StoryUserControl : UserControl
    {
        Random _random = new Random();

        internal StoryUserControl(FocusedObjective.Simulation.Scrum.Story story)
        {
            InitializeComponent();
            _story = story;

            textAreaHeader.Text = _story.Name;
        }

        private FocusedObjective.Simulation.Scrum.Story _story = null;
        private int _lastOrientation = 1;

        internal FocusedObjective.Simulation.Scrum.Story Story
        {
            get { return _story; }
        }

        internal void SetStyleForIteration(FocusedObjective.Simulation.Scrum.Iteration iteration, int column, int position)
        {
            string detailText = string.Empty;

            // detail text
            string detailFormat = "It:{1}\nSize:{0}\n{2}";
            //string detailFormat = "rp:{2}\n\nrb:{4}";

            detailText = string.Format(detailFormat,
                //Story.Name,
                Math.Round( _story.CalculatedStorySize, 3),
                Math.Round( _story.GetRemainingPoints(iteration.Sequence), 3),
                Story.CustomBacklog == null ? string.Empty : Story.CustomBacklog.Name
                //,
                //Math.Round( _story.GetRemainingBlockedPoints(iteration.Sequence), 3)
                );

            //find the correct background image to show
            string sourceImageName = string.Empty;

            switch (_story.StoryType)
            {
                case StoryTypeEnum.AddedScope: 
                    sourceImageName += "green";  
                    break;
                case StoryTypeEnum.Defect: 
                    sourceImageName += "blue"; 
                    break;
                default: // work
                    sourceImageName += "yellow";
                    break;
            }

            if (_story.StatusHistory.ContainsKey(iteration.Sequence))
            {
                switch (_story.StatusHistory[iteration.Sequence])
                {
                    case StoryStatusEnum.Doing: 
                        sourceImageName += "_initial"; 
                        break;
                    case StoryStatusEnum.StillDoing:
                        sourceImageName += "_same";
                        break;
                    case StoryStatusEnum.Blocked: sourceImageName += "_blocked"; break;
                    default:
                        sourceImageName += "_initial";
                        break;
                }

                // keep the same orientation unless it is in a new position this interval
                sourceImageName += "_2";
            }
            else
            {
                //TODO:Defect: defects arent storing status in status history...
                sourceImageName += "_initial_2";
            }

            sourceImageName = string.Format("/FocusedObjective.Simulation;component/Viewers/images/{0}.png", sourceImageName);
            var uriSource = new Uri(sourceImageName, UriKind.Relative); 
            backgroundImage.Source = new BitmapImage(uriSource);

            // apply rotation
            _lastOrientation = _random.Next(1, 4); 
            
            double angle = 0;
            if (_lastOrientation < 2)
                angle = -8.0;
            else
                if (_lastOrientation > 2)
                    angle = +8.0;

            textAreaDetail.Text = detailText;

            cardCanvas.RenderTransform = new RotateTransform(angle, this.ActualWidth/2, this.ActualHeight/2);

        }
    }
}
