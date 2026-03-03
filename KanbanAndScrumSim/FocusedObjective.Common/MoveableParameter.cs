using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Common
{

    public enum MoveableParameterTypeEnum
    {
        Numeric,
        Date,
        List
    }

    public class MoveableParameter : INotifyPropertyChanged
    {
        public string Name;
        public string FormatString;
        public object LowestValue;
        public object DefaultValue;
        public string DisplayList;
        public string ValueList;
        public MoveableParameterTypeEnum ParameterType;

        private object _currentValue;

        public object CurrentValue
        {
            get { return _currentValue; }
            set
            {
                _currentValue = value;
                OnPropertyChanged(new PropertyChangedEventArgs("CurrentValue"));
            }
        }

        public object HighestValue;
        public XProcessingInstruction Instruction;
        public string OriginalValueString;
        public object StepSize;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        public override string ToString()
        {
            if (CurrentValue is DateTime)
                return string.Format("{0} ({3})", Name, LowestValue, HighestValue, ((DateTime)DefaultValue).ToSafeDateString(FormatString));
            else
                return string.Format("{0} ({3})", Name, LowestValue, HighestValue, DefaultValue);
        }

        public string CurrentValueAsString
        {

            get
            {
                if (CurrentValue is double)
                    return CurrentValue.ToString();
                else if (CurrentValue is DateTime)
                    return ((DateTime)CurrentValue).ToSafeDateString(FormatString);

                else
                    return CurrentValue.ToString();
            }
        }

    }
}
