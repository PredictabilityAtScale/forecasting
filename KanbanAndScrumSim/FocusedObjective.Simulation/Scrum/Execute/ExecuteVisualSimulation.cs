using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.IO;
using FocusedObjective.Common;
using System.ComponentModel;

namespace FocusedObjective.Simulation.Scrum.Execute
{
    internal class ExecuteVisualSimulation
    {
        internal static XElement AsXML(SimulationData data)
        {
            Object blank = null;

            return ExecuteVisualSimulation.AsXML(data, ref blank);
        }

        internal static XElement AsXML(SimulationData data, ref dynamic sim, BackgroundWorker workerThread = null)
        {
            Scrum.ScrumSimulation simulator = new Scrum.ScrumSimulation(data);
            if (simulator.RunSimulation())
            {
                sim = simulator;

                XElement visual = new XElement("visual");

                Scrum.SimulationResultSummary summary = new Scrum.SimulationResultSummary(simulator);
                XElement xml = summary.AsXML();
                visual.Add(xml);

                if (data.Execute.Visual.ShowVisualizer || data.Execute.Visual.GenerateVideo)
                {
                    Viewers.ScrumFlowBoard viewer = new Viewers.ScrumFlowBoard();
                    viewer.ShowSimResults(simulator);
                    viewer.Show();

                    if (data.Execute.Visual.GenerateVideo)
                    {
                        string s = viewer.SaveAnimation(simulator.SimulationData.Execute.Visual.VideoFramesPerSecond);

                        if (!string.IsNullOrEmpty(data.Execute.Visual.VideoFilename))
                        {
                            try
                            {
                                System.IO.File.Copy(s, data.Execute.Visual.VideoFilename, true);

                                // if the location on Win7 can't be accessed (not admin) then this silently fails!
                                if (File.Exists(data.Execute.Visual.VideoFilename))
                                {
                                    System.IO.File.Delete(s);
                                    s = data.Execute.Visual.VideoFilename;
                                }
                            }
                            catch (Exception e)
                            {
                                Helper.AddError(data.Errors, ErrorSeverityEnum.Warning, 35, String.Format(Strings.Error35, data.Execute.Visual.VideoFilename, e.Message));
                            }
                        }

                        System.Diagnostics.Process.Start(s);

                        visual.Add(new XElement("video",
                            new XAttribute("filename", s)));
                    }
                }

                return visual;
            }
            else
            {
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 2, string.Format(Strings.Error2, data.Execute.LimitIntervalsTo));
                return new XElement("visual",
                    new XAttribute("success", "false"));

            }
        }

        internal static Viewers.ScrumBoardUserControl AsUserControl(SimulationData data)
        {
            Scrum.ScrumSimulation simulator = new Scrum.ScrumSimulation(data);
            if (simulator.RunSimulation())
            {
                Viewers.ScrumBoardUserControl result = new Viewers.ScrumBoardUserControl();
                result.ShowSimResults(simulator);
                return result;
            }
            else
            {
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 2, string.Format(Strings.Error2, data.Execute.LimitIntervalsTo));
                return null;
            }
        }

    }
}
