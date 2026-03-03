using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    public class ContractDataBase
    {
        /// <summary>
        /// Gets or sets the XML source element (the XElement instance). 
        /// This is used for reporting line and position information of validation errors.
        /// </summary>
        public XElement Source
        {
            get;
            set;
        }
       
    }
}