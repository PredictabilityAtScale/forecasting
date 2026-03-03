using FocusedObjective.Simulation.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Simulation.Kanban
{
    internal static class ResultsVisual
    {
        internal static XElement AsXML(KanbanSimulation simulator)
        {
            if (simulator == null)
                return null;

            XElement visual = new XElement("visual");
            
            SimulationResultSummary summary = new SimulationResultSummary(simulator) ;
            visual.Add(summary.AsXML());
            visual.Add(cumulativeFlowData(simulator));
            visual.Add(intervalsData(simulator));

            if (simulator.SimulationData.Execute.Visual.GeneratePositionData)
                visual.Add(cardFlow(simulator));

            return visual;
        }

        private static XElement intervalsData(KanbanSimulation simulator)
        {
            XElement result = new XElement("intervals");

            result.Add( simulator.ResultTimeIntervals
                    .Select(t => new 
                        XElement("interval",
                            new XAttribute("sequence", t.Sequence),
                            new XAttribute("date", t.CurrentDate.HasValue ? t.CurrentDate.Value.ToString(simulator.SimulationData.Execute.DateFormat) : ""),
                            new XAttribute("elapsedTime", t.ElapsedTime),
                            new XAttribute("phase", t.Phase != null ? t.Phase.Name : "default"),
                            new XAttribute("backlog",  t.CountCardsInBacklog),
                            new XAttribute("completed",  t.CountCompletedCards),
                            new XAttribute("queued",  t.Sequence > 0 ? t.CountCardsOnBoard(c => c.StatusHistory[t.Sequence] == Enums.CardStatusEnum.CompletedButWaitingForFreePosition) : 0),
                            new XAttribute("blocked",  t.Sequence > 0 ? t.CountCardsOnBoard(c => c.StatusHistory[t.Sequence] == Enums.CardStatusEnum.Blocked) : 0),
                            new XAttribute("work",  t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.Work)),
                            new XAttribute("defects",  t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.Defect)),
                            new XAttribute("addedScope",  t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.AddedScope)),
                            new XAttribute("pullTransactions", t.Sequence > 0 ? t.CountCardsOnBoard( c => c.StatusHistory[t.Sequence] == Enums.CardStatusEnum.NewStatusThisInterval) : 0),
                            new XElement("columns", simulator.SimulationData.Setup.Columns.Where(col => t.CardPositions.ContainsKey(col)).Select( c => 
                                new XElement("column", 
                                    new XAttribute("name", c.Name),
                                    new XAttribute("buffer", c.IsBuffer.ToString()),
                                    new XAttribute("wipLimit", (t.Phase != null && t.Phase.Columns != null && t.Phase.Columns.FirstOrDefault(pc => pc.ColumnId == c.Id) != null) ? t.Phase.Columns.FirstOrDefault(pc => pc.ColumnId == c.Id).WipLimit : c.WipLimit),
                                        t.CardPositions[c].Select(card => 
                                            new XElement("card", 
                                                new XAttribute("name", card.Card.Name),
                                                new XAttribute("status", card.Card.StatusHistoryForInterval(t.Sequence).ToString()),
                                                new XAttribute("position", card.Position),
                                                new XAttribute("hasViolatedWip", card.HasViolatedWIP),
                                                new XAttribute("cardType", card.Card.CardType.ToString()),
                                                new XAttribute("classOfService", card.Card.ClassOfServiceName ?? "default"),
                                                new XAttribute("timeForColumn", Math.Round(card.Card.CalculatedRandomWorkTimeForColumn(c),3))
                                                )
                                            ) 
                                     )
                                )
                            )
                        )      
                    )
                );

            return result;
        }

        private static XElement cardFlow(KanbanSimulation simulator)
        {

            XElement result = new XElement("cardProgress");

            XElement resultCSV = new XElement("csv");
            result.Add(resultCSV);

            StringBuilder csvCard = new StringBuilder();
            csvCard.AppendLine("Card Id,Card Type,Class of Service,Card Title");

            StringBuilder csvMovements = new StringBuilder();
            csvMovements.AppendLine("Card Id,From Lane,To Lane,Date,Interval");
            try
            {

                foreach (var card in simulator.AllCardsList)
                {
                    bool cardHasCompletedInCSVAlready = false;
                    string lastColumnName = "";
                    XElement cardElement = new XElement("card");
                    cardElement.Add(new XAttribute("id", card.Index));
                    cardElement.Add(new XAttribute("name", card.Name));
                    cardElement.Add(new XAttribute("type", card.CardType.ToString()));
                    cardElement.Add(new XAttribute("classOfService", card.ClassOfServiceName ?? ""));
                    result.Add(cardElement);


                    csvCard.AppendLine(String.Join(",", card.Index, card.CardType.ToString(), card.ClassOfServiceName ?? "", card.Name));


                    for (int i = 1; i < simulator.ResultTimeIntervals.Count; i++)
                    {
                        var thisInterval = simulator.ResultTimeIntervals[i];
                        XElement thisIntervalElement = new XElement("interval");
                        cardElement.Add(thisIntervalElement);

                        thisIntervalElement.Add(new XAttribute("date", thisInterval.CurrentDate != null ? thisInterval.CurrentDate.ToString() : ""));
                        thisIntervalElement.Add(new XAttribute("elapsed", thisInterval.ElapsedTime));

                        switch (card.StatusHistoryForInterval(i))
                        {
                            case CardStatusEnum.InBacklog:
                                thisIntervalElement.Add(new XAttribute("column", "backlog"));
                                break;
                            case CardStatusEnum.NewStatusThisInterval:
                            case CardStatusEnum.SameStatusThisInterval:
                            case CardStatusEnum.Blocked:
                            case CardStatusEnum.CompletedButWaitingForFreePosition:

                                foreach (var column in simulator.SimulationData.Setup.Columns.OrderBy(c => c.Id))
                                {
                                    if (thisInterval.CardPositions.ContainsKey(column))
                                    {

                                        var position = thisInterval.CardPositions[column].FirstOrDefault(c => c.Card == card);

                                        if (position != null)
                                        {
                                            thisIntervalElement.Add(new XAttribute("column", column.Name));
                                            thisIntervalElement.Add(new XAttribute("position", position.Position));
                                            thisIntervalElement.Add(new XAttribute("hasViolatedWip", position.HasViolatedWIP.ToString()));
                                            thisIntervalElement.Add(new XAttribute("blocked", card.StatusHistoryForInterval(i) == CardStatusEnum.Blocked));
                                            thisIntervalElement.Add(new XAttribute("queued", card.StatusHistoryForInterval(i) == CardStatusEnum.CompletedButWaitingForFreePosition));
                                            thisIntervalElement.Add(new XAttribute("newColumnThisInterval", card.StatusHistoryForInterval(i) == CardStatusEnum.NewStatusThisInterval));

                                            break;
                                        }
                                    }

                                }
                                break;


                            case CardStatusEnum.Completed:
                                thisIntervalElement.Add(new XAttribute("column", "completed"));
                                break;
                            case CardStatusEnum.None:
                                break;
                            default:
                                break;
                        }

                        // csv
                        if (card.StatusHistoryForInterval(i) == CardStatusEnum.NewStatusThisInterval)
                        {
                            // find the column this card is now in.
                            foreach (var column in simulator.SimulationData.Setup.Columns.OrderBy(c => c.Id))
                            {
                                if (thisInterval.CardPositions.ContainsKey(column))
                                {
                                    var position = thisInterval.CardPositions[column].FirstOrDefault(c => c.Card == card);

                                    if (position != null)
                                    {

                                        csvMovements.AppendLine(string.Join(",",card.Index, lastColumnName == "" ? "Backlog" : lastColumnName, column.Name, thisInterval.CurrentDate != null ? thisInterval.CurrentDate.ToString() : "", thisInterval.ElapsedTime));
                                        lastColumnName = column.Name;
                                        break;
                                    }
                                }
                            }
                        }

                        if (card.StatusHistoryForInterval(i) == CardStatusEnum.Completed)
                        {
                            if (cardHasCompletedInCSVAlready == false)
                            {
                                cardHasCompletedInCSVAlready = true;
                                csvMovements.AppendLine(string.Join(",", card.Index, lastColumnName, "Completed", thisInterval.CurrentDate != null ? thisInterval.CurrentDate.ToString() : "", thisInterval.ElapsedTime));
                            }
                        }
                    }


                }

                resultCSV.Add(new XElement("cardData", new XCData(csvCard.ToString())));
                resultCSV.Add(new XElement("cardMovements", new XCData(csvMovements.ToString())));

                if (!string.IsNullOrWhiteSpace(simulator.SimulationData.Execute.Visual.PositionDataFilename))
                    result.Save(simulator.SimulationData.Execute.Visual.PositionDataFilename);

            }
            catch (Exception e)
            {
                var m = e.Message;


            }

            return result;
        }

        private static XElement cumulativeFlowData(KanbanSimulation simulator)
        {
            XElement result = new XElement("cumulativeFlow");

            XElement chartData = new XElement("chart");
            XElement rawCSVData = new XElement("data");

            result.Add(rawCSVData);
            result.Add(chartData);

            StringBuilder data = new StringBuilder();
            StringBuilder csv = new StringBuilder();

            data.AppendLine( "var columns = [");
            data.Append("'Complete',");
            
            csv.Append("Backlog,");

            data.Append(string.Join(" ,", simulator.SimulationData.Setup.Columns.Select(c => "'" + c.Name + "'").Reverse().ToArray()));
            
            csv.Append(string.Join(" ,", simulator.SimulationData.Setup.Columns.Select(c => c.Name).ToArray()));

            data.AppendLine(",'Backlog'");
            csv.AppendLine(",Complete");//,Queued,Empty,Blocked,Work,Defects,AddedScope");

            
            
            data.AppendLine( "];");
            data.AppendLine();
            data.AppendLine("var intervals = [");
            data.AppendLine(string.Join(" ,", simulator.ResultTimeIntervals.Select(t => "'" + t.ElapsedTime.ToString() + "'").ToArray()));
            data.AppendLine("];");
            data.AppendLine();
            data.AppendLine("var countByTimeInterval = [");

            var qcsv = simulator.ResultTimeIntervals.Select(t =>
                    t.CountCardsInBacklog + "," +
                    string.Join(",", simulator.SimulationData.Setup.Columns.Select(c => t.CountCardsInPositionForColumn(c)).ToArray()) +
                    "," + t.CountCompletedCards);
            /*+
                    "," + t.CountCardsOnBoard(c => c.StatusHistory[t.Sequence] == Enums.CardStatusEnum.CompletedButWaitingForFreePosition).ToString() +
                  CHECK!  "," + t.CountCardsOnBoard(c => t.TotalWipLimitBoardPositions - c.StatusHistory[t.Sequence] != Enums.CardStatusEnum.None).ToString() +
                    "," + t.CountCardsOnBoard(c => c.StatusHistory[t.Sequence] == Enums.CardStatusEnum.Blocked).ToString() +
                    "," + t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.Work).ToString() +
                    "," + t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.Defect).ToString() +
                    "," + t.CountCardsOnBoard(c => c.CardType == Enums.CardTypeEnum.AddedScope));
            */
            foreach (string d in qcsv)
                csv.AppendLine(d);

            data.AppendLine( "[" +
                string.Join(",", simulator.ResultTimeIntervals.Select(t => t.CountCompletedCards).ToArray())
                + "],");

            var qcht = simulator.SimulationData.Setup.Columns.AsEnumerable().Reverse().Select(c =>
                        string.Join(",", simulator.ResultTimeIntervals.Select(t => t.CountCardsInPositionForColumn(c)).ToArray()));

            foreach (string d in qcht)
                data.AppendLine("[" + d + "],");

            data.AppendLine( "[" + 
                string.Join(",", simulator.ResultTimeIntervals.Select(t => t.CountCardsInBacklog).ToArray())
                + "]");

            data.AppendLine("];");

            chartData.Add(new XCData(data.ToString()));
            rawCSVData.Add(new XCData(csv.ToString()));

            return result;
        }

    }
}
