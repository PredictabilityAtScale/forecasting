using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.Contract.Data
{

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Property,  AllowMultiple = true)]
    public class SimMLElement : System.Attribute
    {
        private bool _hasMandatoryAttributes = true;
        private bool _hasAnyAttributes = true;
        private string _parentElement;
        private string _parentParentElement = "";

        public SimMLElement(string name, string description, bool mandatory)
        {
            Name = name;
            Description = description;
            Mandatory = mandatory;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Mandatory { get; set; }
        public bool HasMandatoryAttributes { get { return _hasMandatoryAttributes; } set { _hasMandatoryAttributes = value; } }
        public bool HasAnyAttributes { get { return _hasAnyAttributes; } set { _hasAnyAttributes = value; } }

        public string ParentElement { get { return _parentElement; } set { _parentElement = value; } }
        public string ParentParentElement { get { return _parentParentElement; } set { _parentParentElement = value; } }
    }

    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
    public class SimMLAttribute : System.Attribute
    {
 
        public SimMLAttribute(string name, string description, bool mandatory)
        {
            Name = name;
            Description = description;
            Mandatory = mandatory;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool Mandatory{get; set; }

        public string ValidValues { get; set; }

    }
}
