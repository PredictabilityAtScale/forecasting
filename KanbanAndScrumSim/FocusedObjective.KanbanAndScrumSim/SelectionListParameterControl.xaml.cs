using FocusedObjective.Common;
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

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for NumericParameterControl.xaml
    /// </summary>
    public partial class SelectionListParameterControl : UserControl, IParameterControl
    {
        public SelectionListParameterControl()
        {
            InitializeComponent();
        }

        public MoveableParameter Parameter { get; set; }


        private bool _isSetup = false;

        private List<string> _displayList;
        //private List<string> _valueList;

        public void Setup()
        {
            labelTitle.Content = Parameter.ToString();

            _displayList = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(Parameter.DisplayList))
                _displayList.AddRange(Parameter.DisplayList.Split(new char[] {'|'}));

            // if the current value isn't in the list, add it!
            int index = _displayList.IndexOf(Parameter.OriginalValueString);
            if (index == -1)
                _displayList.Add(Parameter.OriginalValueString);

            // not use yet.
            //_valueList = new List<string>();
            //if (!string.IsNullOrWhiteSpace(Parameter.ValueList))
            //    _valueList.AddRange(Parameter.ValueList.Split(new char[] { '|' }));

            comboList.ItemsSource = _displayList;

            setCurrentValue((string)Parameter.CurrentValue);

            Parameter.PropertyChanged -= Parameter_PropertyChanged;
            Parameter.PropertyChanged += Parameter_PropertyChanged;

            _isSetup = true;
        }

        void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setCurrentValue((string)Parameter.CurrentValue);
        }

        private void setCurrentValue(string value)
        {
            if (!string.IsNullOrWhiteSpace(value) )
            {
                int index = _displayList.IndexOf(value);

                comboList.SelectedIndex = index;

                if ((string)Parameter.CurrentValue == (string)Parameter.DefaultValue)
                    comboList.FontWeight = FontWeights.Regular;
                else
                    comboList.FontWeight = FontWeights.Bold;
            }
        }


        public event FocusedObjective.KanbanSim.InteractiveForecast.ParameterChangedEventHandler Changed;

        // Invoke the Changed event; called whenever value changes
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        private void comboList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSetup && Parameter != null && (string)e.AddedItems[0] != (string)Parameter.CurrentValue)
            {
                Parameter.CurrentValue = (string)e.AddedItems[0];
                

                if ((string)Parameter.CurrentValue == (string)Parameter.DefaultValue)
                    comboList.FontWeight = FontWeights.Regular;
                else
                    comboList.FontWeight = FontWeights.Bold;

                OnChanged(EventArgs.Empty);
            }
        }

    }
}
