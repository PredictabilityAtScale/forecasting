using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.IO;
using FocusedObjective.Common;
using System.Windows.Controls;
using System.ComponentModel;
using FocusedObjective.Distributions;
using Troschuetz.Random;

namespace FocusedObjective.Simulation.Kanban.Execute
{
    internal class ExecuteVisualSimulation
    {
        

        internal static XElement AsXML(SimulationData data, ref dynamic kanbanSimulator, BackgroundWorker workerThread = null)
        {

            // if any distributions need modelling, so them now
            foreach (var distData in data.Setup.Distributions)
            {
                if (distData.Shape.ToLower() == "model")
                {
                    var dist = DistributionHelper.CreateDistribution(distData);
                    ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(dist, workerThread);

                    // change to a SIP
                    distData.Shape = "sip";
                    distData.Data = ((ModelDistribution)dist).Sip.ToString();
                }
            }


            kanbanSimulator = new Kanban.KanbanSimulation(data);
            if (kanbanSimulator.RunSimulation())
            {
                XElement xml = Kanban.ResultsVisual.AsXML(kanbanSimulator);

                // launch the viewer
                if (data.Execute.Visual.ShowVisualizer || data.Execute.Visual.GenerateVideo)
                    GenerateVideo(data, kanbanSimulator, xml);

                return xml;
            }
            else
            {
                Helper.AddError(data.Errors, ErrorSeverityEnum.Error, 2, string.Format(Strings.Error2, data.Execute.LimitIntervalsTo));
                return new XElement("visual",
                    new XAttribute("success", "false"));
            }
            
        }

        internal static void GenerateVideo(SimulationData data, dynamic kanbanSimulator, XElement xml)
        {
            Viewers.KanbanBoard viewer = new Viewers.KanbanBoard();
            viewer.ShowSimResults(kanbanSimulator);
            viewer.Show();

            if (data.Execute.Visual.GenerateVideo)
            {
                string s = viewer.SaveAnimation(kanbanSimulator.SimulationData.Execute.Visual.VideoFramesPerSecond);

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
                    catch (IOException e)
                    {
                        Helper.AddError(data.Errors, ErrorSeverityEnum.Warning, 35, String.Format(Strings.Error35, data.Execute.Visual.VideoFilename, e.Message));
                    }
                }

                System.Diagnostics.Process.Start(s);

                xml.Add(new XElement("video",
                    new XAttribute("filename", s)));
            }
        }

        internal static Viewers.KanbanBoardUserControl AsUserControl(SimulationData data)
        {
            Kanban.KanbanSimulation simulator = new Kanban.KanbanSimulation(data);
            if (simulator.RunSimulation())
            {
                Viewers.KanbanBoardUserControl result = new Viewers.KanbanBoardUserControl();
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
