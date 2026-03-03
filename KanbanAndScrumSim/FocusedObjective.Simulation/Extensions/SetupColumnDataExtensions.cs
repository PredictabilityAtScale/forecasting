using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace FocusedObjective.Simulation.Extensions
{
    internal static class SetupColumnDataExtensions
    {
        internal static int FindMaximumColumnWip(this FocusedObjective.Contract.SetupColumnData column, List<FocusedObjective.Contract.SetupPhaseData> phases)
        {
            int highestPhaseWip = 0;

            var phaseLimits = 
                    phases
                    .Where(p => p.Columns.Where(pc => pc.ColumnId == column.Id).Any())
                    .Select(p => p.Columns.Where(pc => pc.ColumnId == column.Id).First().WipLimit);

            if (phaseLimits.Any())
                highestPhaseWip = phaseLimits.Max();

            return Math.Max(
                column.WipLimit,
                highestPhaseWip
                );
        }

        internal static int FindMinimumColumnWip(this FocusedObjective.Contract.SetupColumnData column, List<FocusedObjective.Contract.SetupPhaseData> phases)
        {
            int lowestPhaseWip = int.MaxValue;

            var phaseLimits =
                    phases
                    .Where(p => p.Columns.Where(pc => pc.ColumnId == column.Id).Any())
                    .Select(p => p.Columns.Where(pc => pc.ColumnId == column.Id).First().WipLimit);

            if (phaseLimits.Any())
                lowestPhaseWip = phaseLimits.Min();

            return Math.Min(
                column.WipLimit,
                lowestPhaseWip
                );
        }

    }
}
