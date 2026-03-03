using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FocusedObjective.Contract;
using System.Xml.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Globalization;
using Troschuetz.Random;
using FocusedObjective.Distributions;

namespace FocusedObjective.Simulation
{
    internal class ExecuteSummaryStatisticsSimulation
    {
        internal static XElement AsXML(SimulationData data)
        {
            Stopwatch timer = new Stopwatch();
            timer.Restart();

            if (data.Execute.SummaryStatistics.Distribution == null)
            {
                // inline data
                data.Execute.SummaryStatistics.SeparatorCharacter =
                    data.Execute.SummaryStatistics.SeparatorCharacter.Replace(@"\n", char.Parse("\n").ToString());

                data.Execute.SummaryStatistics.SeparatorCharacter =
                    data.Execute.SummaryStatistics.SeparatorCharacter.Replace(@"\r", char.Parse("\r").ToString());

                if (data.Execute.SummaryStatistics.SeparatorCharacter.Length == 0)
                {
                    return new XElement("result",
                        new XAttribute("success", false),
                        new XAttribute("message", "Invalid character for summary statistics separator. use a single character, or \n for newline, \r for return.")
                        );
                }
                else
                {
                    string[] stringElements;

                    if (System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                        stringElements = data.Execute.SummaryStatistics.Data.Split(
                            new string[] { data.Execute.SummaryStatistics.SeparatorCharacter, ",", "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    else
                        stringElements = data.Execute.SummaryStatistics.Data.Split(
                            new string[] { data.Execute.SummaryStatistics.SeparatorCharacter, "|", Environment.NewLine, "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    List<double> doubleElements = new List<double>();

                    foreach (var element in stringElements)
                    {
                        double d = 0.0;
                        if (double.TryParse(element, out d))
                            doubleElements.Add(d);
                    }

                    StatisticResults<double> stats = new StatisticResults<double>(doubleElements, data.Execute.DecimalRounding, true, data.Execute.GoogleHistogramUrlFormat);
                    XElement result = stats.AsXML("summaryStatistics");

                    result.Add(
                        new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()),
                        new XAttribute("success", true));

                    return result;
                }
            }
            else
            {
                // distribution generated data

                var dist = DistributionHelper.CreateDistribution(
                    data.Execute.SummaryStatistics.Distribution,
                    data.Errors);

                if (dist != null)
                {

                    ExecuteModelDistribution.RunModelForDistributionDataIfNeeded(dist);

                    XElement result = new XElement("summaryStatistics");

                    // double
                    if (data.Execute.SummaryStatistics.Distribution.NumberType == DistributionNumberType.Double)
                    {
                        List<double> randomData = new List<double>(data.Execute.SummaryStatistics.Distribution.Count);

                        int attempts = 0;
                        while (attempts < data.Execute.SummaryStatistics.Distribution.Count)
                        {
                            randomData.Add(Math.Round(dist.GetNextDoubleForDistribution(), data.Execute.DecimalRounding));
                            attempts++;
                        }

                        StatisticResults<double> stats = new StatisticResults<double>(randomData, data.Execute.DecimalRounding, true, data.Execute.GoogleHistogramUrlFormat);
                        result = stats.AsXML("summaryStatistics");

                        if (data.Execute.SummaryStatistics.ReturnData)
                        {
                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\n", char.Parse("\n").ToString());

                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\r", char.Parse("\r").ToString());

                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\t", char.Parse("\t").ToString());
                            
                            result.Add(new XElement("data",
                                string.Join( 
                                data.Execute.SummaryStatistics.Distribution.Separator, 
                                randomData))
                                );
                        }
                    }
                    else
                    {
                        // integers
                        List<int> randomData = new List<int>(data.Execute.SummaryStatistics.Distribution.Count);

                        int attempts = 0;
                        while (attempts < data.Execute.SummaryStatistics.Distribution.Count)
                        {
                            randomData.Add((int)Math.Round(dist.GetNextDoubleForDistribution(), 0));
                            attempts++;
                        }
                        
                        StatisticResults<int> stats = new StatisticResults<int>(randomData, data.Execute.DecimalRounding, true, data.Execute.GoogleHistogramUrlFormat);
                        result = stats.AsXML("summaryStatistics");

                        if (data.Execute.SummaryStatistics.ReturnData)
                        {

                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\n", char.Parse("\n").ToString());

                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\r", char.Parse("\r").ToString());

                            data.Execute.SummaryStatistics.Distribution.Separator =
                                 data.Execute.SummaryStatistics.Distribution.Separator.Replace(@"\t", char.Parse("\t").ToString());
                            
                            result.Add(new XElement("data",
                                string.Join(
                                data.Execute.SummaryStatistics.Distribution.Separator,
                                randomData))
                                );
                        }
                    }

                    result.Add(
                        new XAttribute("elapsedTime", timer.ElapsedMilliseconds.ToString()),
                        new XAttribute("success", true));

                    return result;
                }
                else
                {
                    return new XElement("result",
                        new XAttribute("success", false),
                        new XAttribute("message", "Failed to create random number distribution.")
                        );
                }
            }
        }
    }
}
