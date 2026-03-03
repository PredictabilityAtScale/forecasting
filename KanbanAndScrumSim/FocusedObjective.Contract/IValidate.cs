using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public interface IValidate
    {
        bool Validate(SimulationData data, XElement errors);
    }
}
