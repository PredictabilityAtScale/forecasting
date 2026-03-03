using FocusedObjective.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    [SimMLElement("visual", "Perform visual simulation. Necessary for the KanbanSim and ScrumSim visualizer application.", true, ParentElement="execute")]
    public class ExecuteVisualData : ContractDataBase, IValidate
    {
        public ExecuteVisualData()
        {
        }

        public ExecuteVisualData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private bool _generateVideo = false;
        private int _videoFramesPerSecond = 5;
        private string _videoFilename = "";
        private bool _showVisualizer = true;
        private bool _generatePositionData = false;
        private string _positionDataFilename = "";

        // public properties
        
        public bool GenerateVideo
        {
            get { return _generateVideo; }
            set { _generateVideo = value; }
        }

        public int VideoFramesPerSecond
        {
            get { return _videoFramesPerSecond; }
            set { _videoFramesPerSecond = value; }
        }

        public string VideoFilename
        {
            get { return _videoFilename; }
            set { _videoFilename = value; }
        }

        public bool ShowVisualizer
        {
            get { return _showVisualizer; }
            set { _showVisualizer = value; }
        }

        public bool GeneratePositionData
        {
            get { return _generatePositionData;  }
            set { _generatePositionData = value;  }
        }

        public string PositionDataFilename
        {
            get { return _positionDataFilename;  }
            set { _positionDataFilename = value; }
        }


        // methods
        private bool fromXML(XElement source, XElement errors)
        {

            _generateVideo = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "generateVideo",
                "FALSE", false,
                "false", false,
                "no", false,
                "TRUE", true,
                "true", true,
                "yes", true);            

            ContractCommon.ReadAttributeIntValue(
                out _videoFramesPerSecond,
                source, 
                errors,
                "videoFramesPerSecond",
                _videoFramesPerSecond, 
                false);            

            _videoFilename = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "videoFilename",
                _videoFilename,
                false);

            _showVisualizer = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "showVisualizer",
                "TRUE", true,
                "true", true,
                "yes", true,
                "FALSE", false,
                "false", false,
                "no", false);

            _generatePositionData = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "generatePositionData",
                "FALSE", false,
                "false", false,
                "no", false,
                "TRUE", true,
                "true", true,
                "yes", true);

            _positionDataFilename = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "positionDataFilename",
                _positionDataFilename,
                false);
            
            return true;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("visual");
                
            if (_generateVideo)
                result.Add(new XAttribute("generateVideo", "true"));
            else
                result.Add(new XAttribute("generateVideo", "false"));
            
            result.Add(new XAttribute("videoFramesPerSecond", _videoFramesPerSecond.ToString()));
            result.Add(new XAttribute("videoFilename", _videoFilename.ToString()));

            if (_showVisualizer)
                result.Add(new XAttribute("showVisualizer", "true"));
            else
                result.Add(new XAttribute("showVisualizer", "false"));

            if (_generatePositionData)
                result.Add(new XAttribute("generatePositionData", "true"));
            else
                result.Add(new XAttribute("generatePositionData", "false"));

            result.Add(new XAttribute("positionDataFilename", _positionDataFilename.ToString()));
            
            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;
            return success;
        }
    }


}
