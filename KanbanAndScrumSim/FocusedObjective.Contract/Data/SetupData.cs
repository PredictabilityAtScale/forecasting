using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("setup", "The section of the model file where the project is defined for simulation.", true, HasAnyAttributes=false, HasMandatoryAttributes = false)]
    public class SetupData : ContractDataBase, IValidate
    {
        public SetupData()
        {
        }
        
        public SetupData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private SetupBacklogData _setupBacklogData = null;
        private List<SetupDefectData> _setupDefectData = new List<SetupDefectData>();
        private List<SetupAddedScopeData> _setupAddedScopeData = new List<SetupAddedScopeData>();
        private List<SetupColumnData> _setupColumnData = new List<SetupColumnData>();
        private List<SetupBlockingEventData> _setupBlockingEventData = new List<SetupBlockingEventData>();
        private SetupIterationData _setupIterationData = null;
        private List<SetupDistributionData> _setupDistributionData = new List<SetupDistributionData>();
        private SetupPhasesData _phases = null;
        private List<SetupClassOfServiceData> _classOfServices = new List<SetupClassOfServiceData>();
        private ForecastDateData _forecastDateData = null;

        // public properties
        [SimMLElement("backlog", "Contains the initial work to be simulated as deliverables or individual items.", true, HasMandatoryAttributes = false)]
        public SetupBacklogData Backlog
        {
            get { return _setupBacklogData; }
            set { _setupBacklogData = value; }
        }

        [SimMLElement("defects", "Contains the defect definitions. Defects are work that is added to the backlog due to finding defects in completed work.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupDefectData> Defects
        {
            get { return _setupDefectData; }
        }

        [SimMLElement("addedScopes", "Contains the addedScope definitions. Added scope entries represent work added to the backlog once a project has started.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupAddedScopeData> AddedScopes
        {
            get { return _setupAddedScopeData; }
        }

        [SimMLElement("columns", "Contains the column definitions (Kanban models only). Columns represent the workflow items flow through during a project.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupColumnData> Columns
        {
            get { return _setupColumnData; }
        }

        [SimMLElement("blockingEvents", "Contains the blockingEvent definitions. Blocking events represent impediments to completing work items once its started.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupBlockingEventData> BlockingEvents
        {
            get { return _setupBlockingEventData; }
        }

        [SimMLElement("iteration", "Contains details about iteration length and scope (Scrum models only).", false)]
        public SetupIterationData Iteration
        {
            get { return _setupIterationData; }
            set { _setupIterationData = value; }
        }

        [SimMLElement("forecastDate", "Contains details about forecasting. Not required for visual simulation, but needed whenever a forecast of a date is performed.", false)]
        public ForecastDateData ForecastDate
        {
            get { return _forecastDateData; }
            set { _forecastDateData = value; }
        }

        [SimMLElement("distributions", "Contains any probability distribution definitions. Distributions allow historical data or know patterns of data to be used during simulation.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupDistributionData> Distributions
        {
            get { return _setupDistributionData; }
        }

        [SimMLElement("phases", "Contains any project phases definitions. Phases are used to divide a project timeline into multiple sections. Different parts of the model can target speciic phases to simulate project ramp-up, staffing arrival, etc.", false, HasMandatoryAttributes = false)]
        public SetupPhasesData Phases
        {
            get { return _phases; }
        }

        [SimMLElement("classOfServices", "Contains any class of service definitions (Kanban models only). Class of service's allow different types of work to have specific simulation properties. Often used to indicate urgency or work items that get fast-tracked.", false, HasAnyAttributes = false, HasMandatoryAttributes = false)]
        public List<SetupClassOfServiceData> ClassOfServices
        {
            get { return _classOfServices; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            // sub elements
            _setupBacklogData = ContractCommon.ReadElement(source, typeof(SetupBacklogData), "backlog", errors, false);
            _setupIterationData = ContractCommon.ReadElement(source, typeof(SetupIterationData), "iteration", errors, false);
            _forecastDateData = ContractCommon.ReadElement(source, typeof(ForecastDateData), "forecastDate", errors, false);

            // add the defect data
            XElement defects = source.Element("defects");
            if (defects != null)
            {
                foreach (XElement def in defects.Elements("defect"))
                {
                    _setupDefectData.Add(
                        new SetupDefectData(def, errors));
                }
            }

            // add the added scope data
            XElement addedScopes = source.Element("addedScopes");
            if (addedScopes != null)
            {
                foreach (XElement add in addedScopes.Elements("addedScope"))
                {
                    _setupAddedScopeData.Add(
                        new SetupAddedScopeData(add, errors));
                }
            }

            // add the columns data
            XElement columns = source.Element("columns");
            if (columns != null)
            {
                foreach (XElement col in columns.Elements("column"))
                {
                    _setupColumnData.Add(
                        new SetupColumnData(col, errors));
                }

                // set the sequence of columns from left to right
                int i = 1;
                foreach (var col in _setupColumnData.OrderBy(c => c.Id))
                {
                    col.Sequence = i;
                    i++;
                }
            }

            // add the blocking event data
            XElement blockingEvents = source.Element("blockingEvents");
            if (blockingEvents != null)
            {
                foreach (XElement blk in blockingEvents.Elements("blockingEvent"))
                {
                    _setupBlockingEventData.Add(
                        new SetupBlockingEventData(blk, errors));
                }
            }

            // add the distribution data
            XElement distributions = source.Element("distributions");
            if (distributions != null)
            {
                foreach (XElement dis in distributions.Elements("distribution"))
                {
                    _setupDistributionData.Add(
                        new SetupDistributionData(dis, errors));
                }
            }

            _phases = ContractCommon.ReadElement(source, typeof(SetupPhasesData), "phases", errors, false);

            if (_phases == null)
                _phases = new SetupPhasesData();

            XElement classOfServices = source.Element("classOfServices");
            if (classOfServices != null)
            {
                foreach (XElement cos in classOfServices.Elements("classOfService"))
                {
                    _classOfServices.Add(
                        new SetupClassOfServiceData(cos, errors));
                }
            }

            return true;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("setup");

            if (_setupIterationData != null)
                result.Add(_setupIterationData.AsXML(simType));

            if (_forecastDateData != null)
                result.Add(_forecastDateData.AsXML(simType));

            if (_setupColumnData != null && _setupColumnData.Any())
            {
                XElement columns = new XElement("columns");

                foreach (var column in _setupColumnData)
                    columns.Add(column.AsXML(simType));

                result.Add(columns);
            }

            if (_setupBacklogData != null)
                result.Add(_setupBacklogData.AsXML(simType));

            if (_setupDefectData != null && _setupDefectData.Any())
            {
                XElement defects = new XElement("defects");

                foreach (var defect in _setupDefectData)
                    defects.Add(defect.AsXML(simType));

                result.Add(defects);
            }

            if (_setupBlockingEventData != null && _setupBlockingEventData.Any())
            {
                XElement blockingEvents = new XElement("blockingEvents");

                foreach (var bevent in _setupBlockingEventData)
                    blockingEvents.Add(bevent.AsXML(simType));

                result.Add(blockingEvents);
            }

            if (_setupAddedScopeData != null && _setupAddedScopeData.Any())
            {
                XElement addedScopes = new XElement("addedScopes");

                foreach (var scope in _setupAddedScopeData)
                    addedScopes.Add(scope.AsXML(simType));

                result.Add(addedScopes);
            }

            if (_setupDistributionData != null && _setupDistributionData.Any())
            {
                XElement distributions = new XElement("distributions");

                foreach (var dist in _setupDistributionData)
                    distributions.Add(dist.AsXML(simType));

                result.Add(distributions);
            }


            if (_phases != null && _phases.Any())
                result.Add(_phases.AsXML(simType));

            if (_classOfServices != null && _classOfServices.Any())
            {
                XElement classOfServices = new XElement("classOfServices");

                foreach (var cos in _classOfServices)
                    classOfServices.Add(cos.AsXML(simType));

                result.Add(classOfServices);
            }

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            bool onlyExecuteAllowed = false;

           
            if (data.Execute.SummaryStatistics != null || data.Execute.Ballot != null)
                if (data.Execute.Visual == null && 
                    data.Execute.MonteCarlo == null && 
                    data.Execute.Sensitivity == null && 
                    data.Execute.ForecastDate == null && 
                    data.Execute.AddStaff == null)
                        onlyExecuteAllowed = true;

            // columns is needed if this is a Kanban simulation
            if (!onlyExecuteAllowed && data.Execute.SimulationType == SimulationTypeEnum.Kanban)
            {
                if (this.Backlog == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 3, string.Format(Strings.Error3, "backlog"), Source);
                }

                if (this.Columns.Count == 0)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 6, Strings.Error6, Source);
                }
                else
                {
                    // each column id must be unique
                    bool invalid = (from col in this.Columns
                                    where this.Columns.Count(c => c.Id == col.Id) > 1
                                    select col)
                                   .Any();

                    if (invalid)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 8, Strings.Error8, Source);
                    }
                }
            }

            // validate each column
            foreach (var col in Columns)
                success &= col.Validate(data, errors);

            // validate each added scope
            foreach (var asc in AddedScopes)
                success &= asc.Validate(data, errors);

            // validate each blocking event
            foreach (var blk in BlockingEvents)
                success &= blk.Validate(data, errors);

            // validate each defect
            foreach (var def in Defects)
                success &= def.Validate(data, errors);

            // validate backlog
            if (Backlog != null)
                success &= Backlog.Validate(data, errors);

            // validate each distribution
            foreach (var dis in Distributions)
                success &= dis.Validate(data, errors);

            // validate each phase
            foreach (var phase in Phases)
                success &= phase.Validate(data, errors);

            /* p   start 10 end 20
             * p2  start 15 end 18, 25
             * 
             *   p2 start <= p end && p2 start >= p start
            */
            if (success)
            {
                // check for overlapping phases
                var phaseOverlaps = from p in Phases
                                    let overlaps = Phases.Where(p2 =>
                                        (p2 != p) &&
                                        (p2.Start <= p.End && p2.Start >= p.Start))
                                    where overlaps.Count() > 0
                                    select new { original = p, overlaps = overlaps };

                if (phaseOverlaps.Count() > 0)
                {
                    success = false;
                    foreach (var p in phaseOverlaps)
                        foreach (var overlap in p.overlaps)
                            Helper.AddError(errors, ErrorSeverityEnum.Error, 51, string.Format(Strings.Error51, overlap.Name, p.original.Name), Source);
                }
            }

            // validate each class of service
            foreach (var cos in ClassOfServices)
                success &= cos.Validate(data, errors);

            // check that a default COS is specified
            if (success && ClassOfServices != null && ClassOfServices.Any())
            {
                if (! ClassOfServices.Any(c => c.Default))
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 53, Strings.Error53, Source);
                }
            }

            // for Scrum simulation, an iteration element is needed.
            if (!onlyExecuteAllowed && data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (this.Backlog == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 3, string.Format(Strings.Error3, "backlog"), Source);
                }

                if (Iteration == null)
                {
                    success = false;
                    Helper.AddError(errors, ErrorSeverityEnum.Error, 3, string.Format(Strings.Error3, "iteration"), Source);
                }
                else
                {
                    success &= Iteration.Validate(data, errors);
                }
            }

            if (ForecastDate != null)
                success &= ForecastDate.Validate(data, errors);

            return success;
        }


    }


}
