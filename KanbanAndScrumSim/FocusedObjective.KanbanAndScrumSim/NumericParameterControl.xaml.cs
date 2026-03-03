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
    public partial class NumericParameterControl : UserControl, IParameterControl
    {
        public NumericParameterControl()
        {
            InitializeComponent();
        }

        public MoveableParameter Parameter { get; set; }


        private bool _isSetup = false;

        public void Setup()
        {
            labelTitle.Content = Parameter.ToString();
            labelMin.Content = Parameter.LowestValue;
            labelMax.Content = Parameter.HighestValue;

            slider.Minimum = (double)Parameter.LowestValue;
            slider.Maximum = (double)Parameter.HighestValue;
            slider.SmallChange = (double)Parameter.StepSize;
            slider.TickFrequency = (double)Parameter.StepSize;

            setCurrentValue((double)Parameter.CurrentValue);

            Parameter.PropertyChanged -= Parameter_PropertyChanged;
            Parameter.PropertyChanged += Parameter_PropertyChanged;

            _isSetup = true;
        }

        void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setCurrentValue((double)Parameter.CurrentValue);
        }

        private void setCurrentValue(double value)
        {
            if (value >= (double)Parameter.LowestValue && value <= (double)Parameter.HighestValue)
            {
                
                slider.Value = value;
                labelValue.Content = Parameter.CurrentValueAsString;

                if ((double)Parameter.CurrentValue == (double)Parameter.DefaultValue)
                    labelValue.FontWeight = FontWeights.Regular;
                else
                    labelValue.FontWeight = FontWeights.Bold;
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isSetup && Parameter != null && (double)e.OldValue != (double)e.NewValue)
            {
                Parameter.CurrentValue = e.NewValue;
                labelValue.Content = Parameter.CurrentValueAsString;

                if ((double)Parameter.CurrentValue == (double)Parameter.DefaultValue)
                    labelValue.FontWeight = FontWeights.Regular;
                else
                    labelValue.FontWeight = FontWeights.Bold;

                OnChanged(EventArgs.Empty);
            }
        }

        public event FocusedObjective.KanbanSim.InteractiveForecast.ParameterChangedEventHandler Changed;

        // Invoke the Changed event; called whenever value changes
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

    }
}
