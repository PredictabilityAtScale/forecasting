using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Contract;
using System.Diagnostics;
using FocusedObjective.Common;
using System.ComponentModel;

namespace FocusedObjective.Simulation
{
    public class Simulator
    {
        string _input;
        SimulationData _data;
        XElement _rootResult;

        private dynamic _simulator = null;
        
        public object TheSimulator
        {
            get { return _simulator; }
        }


        public Simulator(string input)
        {
            _input = input;
        }

        public XElement Result
        {
            get
            {
                return _rootResult;
            }
        }

        public XElement Errors
        {
            get
            {
                if (_data == null)
                    return _rootResult;
                else if (_rootResult == null)
                    return _data.Errors;
                else
                    return _rootResult.Element("errors");
            }
        }

        public Viewers.KanbanBoardUserControl KanbanUserControl
        {
            get
            {
                if (_simulator != null && _simulator is FocusedObjective.Simulation.Kanban.KanbanSimulation)
                {
                     Viewers.KanbanBoardUserControl _kanbanUserControl = new Viewers.KanbanBoardUserControl();
                    _kanbanUserControl.ShowSimResults(_simulator);
                    return _kanbanUserControl;
                }

                return null;
            }
        }

        public Viewers.ScrumBoardUserControl ScrumUserControl
        {
            get
            {
                if (_simulator != null && _simulator is FocusedObjective.Simulation.Scrum.ScrumSimulation)
                {
                    Viewers.ScrumBoardUserControl _scrumUserControl = new Viewers.ScrumBoardUserControl();
                    _scrumUserControl.ShowSimResults(_simulator);
                    return _scrumUserControl;
                }

                return null;
            }
        }

        public bool Execute(bool justValidate = false, BackgroundWorker workerThread = null)
        {

            bool result = false;

            // parse the input xml string into an XDocument first...
            try
            {
                XDocument inputDocument = XDocument.Parse(_input, LoadOptions.SetLineInfo);
                _data = new SimulationData(inputDocument, SyncfusionComplexEval.GetEngineSyncfusionInstance());

                // put the current thread on the right locale
                _data.SetCurrentThreadsCulture();


                if (_data.Validate())
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Restart();

                    // valid SimXML - loop the executes
                    _rootResult = new XElement("results");
                    _rootResult.Add(new XAttribute("locale", _data.Locale.ToString()));

                    if (!justValidate)
                    {


                            //if (DateTime.Now < _data.LicenseSettings.ExpiryDate)
                            //{
                                if (_data.Execute.Visual != null)
                                    _rootResult.Add(ExecuteVisualSimulation.AsXML(_data, ref _simulator, workerThread));

                                if (_data.Execute.Ballot != null)
                                    _rootResult.Add(ExecuteBallot.AsXML(_data));

                                // an optimization. forecast date reuses this monte carlo runs simulations. if its set.
                                if (_data.Execute.MonteCarlo != null)
                                    _rootResult.Add(ExecuteMonteCarloSimulation.AsXML(_data, workerThread));

                                // an optimization. monte carlo runs are recycled if monte carlo and forecast date are combined
                                if (_data.Execute.ForecastDate != null && _data.Execute.MonteCarlo == null)
                                    _rootResult.Add(ExecuteForecastDateSimulation.AsXML(_data, workerThread)); 

                                if (_data.Execute.AddStaff != null)
                                    _rootResult.Add(ExecuteAddStaffSimulation.AsXML(_data, workerThread));

                                if (_data.Execute.Sensitivity != null)
                                    _rootResult.Add(ExecuteSensitivitySimulation.AsXML(_data, workerThread));

                                if (_data.Execute.SummaryStatistics != null)
                                    _rootResult.Add(ExecuteSummaryStatisticsSimulation.AsXML(_data));

                                if (_data.Execute.ModelAudit != null)
                                    _rootResult.Add(ExecuteModelAuditSimulation.AsXML(_data));
                            //}
                            //else
                            //{
                            //    Helper.AddError(
                            //        _data.Errors,
                            //        ErrorSeverityEnum.Error,
                            //        17,
                            //        Strings.Error17);
                            //}
                        
                    }

                    _rootResult.Add(_data.Errors);

                    if (_data.Errors.Elements("error").Count() == 0)
                        _rootResult.Add(new XAttribute("success", "true"));
                    else
                        _rootResult.Add(new XAttribute("success", "false"));
                    
                    _rootResult.Add(new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()));
                    _rootResult.Add(new XAttribute("simulationType", _data.Execute.SimulationType.ToString()));

                    result = true;
                }
                else
                {
                    XElement _rootResult = new XElement("results",
                        new XAttribute("success", "false"),
                        new XAttribute("errorMessage", "Invalid SimXML, see specific errors listed"),
                        _data.Errors
                        );

                }
            }
            catch (Exception e)
            {
                _rootResult = new XElement("results",
                    new XAttribute("success", "false"),
                    new XAttribute("errorMessage", e.Message));

            }

            return result;
        }
    }
}
