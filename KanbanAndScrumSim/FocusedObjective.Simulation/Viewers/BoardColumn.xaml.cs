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
using FocusedObjective.Simulation.Kanban;

namespace FocusedObjective.Simulation.Viewers
{
    /// <summary>
    /// Interaction logic for BoardColumn.xaml
    /// </summary>

    public partial class BoardColumn : UserControl
    {
        SetupColumnData _column;

        public BoardColumn(SetupColumnData column)
        {
            InitializeComponent();

            mainGrid.MinWidth = column.DisplayWidth * 120;
            mainGrid.MaxWidth = column.DisplayWidth * 125;
            
            _column = column;

            columnName.Text = _column.Name;
        }

        public SetupColumnData Column
        {
            get
            {
                return _column;
            }
        }

        private int _currentWipLimit;
        
        public int CurrentWipLimit 
        {
            set 
            {
                _currentWipLimit = value;

                if (_currentPhaseColumnData == null)
                {
                    wipLimit.Text =
                        string.Format("(limit: {0})", _currentWipLimit);
                }
                else
                {
                    wipLimit.Text =
                        string.Format("(phase wip: {0})", _currentWipLimit);
                }
            } 
        }

        private SetupPhaseColumnData _currentPhaseColumnData;

        public SetupPhaseColumnData CurrentPhaseColumnData {
            set
            {
                _currentPhaseColumnData = value;

                if (_currentPhaseColumnData != null)
                    CurrentWipLimit = _currentPhaseColumnData.WipLimit;
                else
                    CurrentWipLimit = _column.WipLimit;
            }
        }

        public void FillEmptyWipPositions()
        {
            // if infinite column WIP, don't draw empty
            if (_currentWipLimit <= 0)
                return;

            // add blank wip positions
            while (normalCardPositions.Children.Count < _currentWipLimit)
            {
                CardUserControl c = new CardUserControl(_column);
                normalCardPositions.Children.Add(c);
            }
        }

        internal void ClearAllControls()
        {
            normalCardPositions.Children.Clear();
            violateWipCardPositions.Children.Clear();
        }

        internal void UpdateForTimeInterval(Dictionary<Card, CardUserControl> cardControls, TimeInterval interval, string dateFormat)
        {
            var positions = new List<CardPosition>();

            if (interval.CardPositions.ContainsKey(_column))
                positions = interval.CardPositions[_column];

            foreach (var p in positions.OrderBy(cp => cp.Card, CardPriorityComparer.Instance))
            {
                if (p.Card != null)
                {
                    // create the card control if necessary
                    if (!cardControls.ContainsKey(p.Card))
                    {
                        CardUserControl c = new CardUserControl(p.Card);
                        c.Margin = new Thickness(5);
                        cardControls.Add(p.Card, c);
                    }

                    CardUserControl control = cardControls[p.Card];

                    if (p.HasViolatedWIP)
                    {
                        control.SetStyleForTimeInterval(interval, _column, true, p);
                        violateWipCardPositions.Children.Add(control);
                    }
                    else
                    {
                        control.SetStyleForTimeInterval(interval, _column, normalCardPositions.Children.Count >= _currentWipLimit, p);

                        if (control.Parent != null)
                            ((WrapPanel)control.Parent).Children.Remove(control);

                        normalCardPositions.Children.Add(control);
                    }
                }
            }

            FillEmptyWipPositions();
        }
    }
}
