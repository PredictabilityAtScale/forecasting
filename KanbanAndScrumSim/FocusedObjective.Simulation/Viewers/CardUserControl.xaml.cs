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
using FocusedObjective.Simulation.Kanban;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for CardUserControl.xaml
    /// </summary>
    public partial class CardUserControl : UserControl
    {
        Random _random = new Random();

        internal CardUserControl(FocusedObjective.Simulation.Kanban.Card card)
        {
            InitializeComponent();
            _card = card;

            textAreaHeader.Text = _card.Name;
        }

        internal CardUserControl(FocusedObjective.Contract.SetupColumnData column)
        {
            InitializeComponent();
            
            // make a blank background card
            cardCanvas.Visibility = System.Windows.Visibility.Hidden;

            emptyBufferImage.Visibility = System.Windows.Visibility.Hidden;
            emptyImage.Visibility = System.Windows.Visibility.Hidden;

            if (column.IsBuffer)
                emptyBufferImage.Visibility = System.Windows.Visibility.Visible;
            else
                emptyImage.Visibility = System.Windows.Visibility.Visible;
        }

        private FocusedObjective.Simulation.Kanban.Card _card = null;
        private int _lastOrientation = 1;

        internal FocusedObjective.Simulation.Kanban.Card Card
        {
            get { return _card; }
        }

        internal void SetStyleForTimeInterval(FocusedObjective.Simulation.Kanban.TimeInterval interval, FocusedObjective.Contract.SetupColumnData column, bool hideBackground, CardPosition position)
        {
            // set the correct wip position image
            emptyBufferImage.Visibility = System.Windows.Visibility.Hidden;
            emptyImage.Visibility = System.Windows.Visibility.Hidden;

            if (!hideBackground && !position.HasViolatedWIP)
            {
                if (column.IsBuffer)
                    emptyBufferImage.Visibility = System.Windows.Visibility.Visible;
                else
                    emptyImage.Visibility = System.Windows.Visibility.Visible;
            }

            string detailText = string.Empty;

            // detail text
            string detailFormat = "Time for column:{0}";

            detailText = string.Format(detailFormat,
                Math.Round( _card.CalculatedRandomWorkTimeForColumn(column), 3)  );

            //find the correct background image to show
            string sourceImageName = string.Empty;

            switch (_card.CardType)
            {
                case CardTypeEnum.AddedScope: 
                    sourceImageName += "green";  
                    break;
                case CardTypeEnum.Defect: 
                    sourceImageName += "blue"; 
                    break;
                default: // work
                    sourceImageName += "yellow";
                    break;
            }

            if (_card.StatusHistory.ContainsKey(interval.Sequence))
            {
                switch (_card.StatusHistory[interval.Sequence])
                {
                    case CardStatusEnum.NewStatusThisInterval: 
                        sourceImageName += "_initial"; 
                        _lastOrientation = _random.Next(1,4);
                        break;
                    case CardStatusEnum.SameStatusThisInterval: sourceImageName += "_same"; break;
                    case CardStatusEnum.CompletedButWaitingForFreePosition: sourceImageName += "_queued"; break;
                    case CardStatusEnum.Blocked: sourceImageName += "_blocked"; break;
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
            double angle = 0;
            if (_lastOrientation < 2)
                angle = -8.0;
            else
                if (_lastOrientation > 2)
                    angle = +8.0;

            textAreaDetail.Text = detailText;

            // COS star
            if (Card.ClassOfService != null && !Card.ClassOfService.Default)
            {
                cosImage.Visibility = System.Windows.Visibility.Visible;

                StringBuilder builder = new StringBuilder();
                builder.AppendLine(string.Format("Class of service: {0}", Card.ClassOfService.Name));

                if (position.HasViolatedWIP)
                    builder.AppendLine("Has Violated WIP limit");

                if (Card.CustomBacklog != null && !string.IsNullOrWhiteSpace(Card.CustomBacklog.DueDate))
                    builder.AppendLine(string.Format("Due Date: {0}", Card.CustomBacklog.DueDate));

                if (Card.ClassOfService.SkipPercentage > 0.0)
                    builder.AppendLine(string.Format("Skip percentage: {0}", Card.ClassOfService.SkipPercentage));

                cosImage.ToolTip = builder.ToString();
            }
            else
            {
                cosImage.Visibility = System.Windows.Visibility.Hidden;
            }

            cardCanvas.RenderTransform = new RotateTransform(angle, this.ActualWidth/2, this.ActualHeight/2);
        }

        internal void ClearEmptyWipBackground()
        {
            emptyBufferImage.Visibility = System.Windows.Visibility.Hidden;
            emptyImage.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
