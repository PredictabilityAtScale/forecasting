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
using FocusedObjective.Common;

namespace FocusedObjective.KanbanSim
{
    /// <summary>
    /// Interaction logic for DateParameterControl.xaml
    /// </summary>
    public partial class DateParameterControl : UserControl, IParameterControl
    {
        public DateParameterControl()
        {
            InitializeComponent();
        }

        public MoveableParameter Parameter { get; set; }

        public void Setup()
        {
            labelTitle.Content = Parameter.ToString();
            
            setCurrentValue((DateTime)Parameter.CurrentValue);
            Parameter.PropertyChanged -= Parameter_PropertyChanged; 
            Parameter.PropertyChanged += Parameter_PropertyChanged;

        }

        void Parameter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            setCurrentValue((DateTime)Parameter.CurrentValue);
        }


        private void setCurrentValue(DateTime value)
        {
            labelValue.Content = value.ToSafeDateString(Parameter.FormatString);

            datePicker.SelectedDate = value;

            if ((DateTime)Parameter.CurrentValue == (DateTime)Parameter.DefaultValue)
                labelValue.FontWeight = FontWeights.Regular;
            else
                labelValue.FontWeight = FontWeights.Bold;

        }


        private void datePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Parameter != null)
            {
                Parameter.CurrentValue = datePicker.SelectedDate;
                labelValue.Content = Parameter.CurrentValueAsString;

                if ((DateTime)Parameter.CurrentValue == (DateTime)Parameter.DefaultValue)
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
