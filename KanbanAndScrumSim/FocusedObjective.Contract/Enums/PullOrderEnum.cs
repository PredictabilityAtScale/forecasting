using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.Contract
{
    public enum PullOrderEnum
    {
        randomAfterOrdering,
        random,
        indexSequence,
        FIFO,
        FIFOStrict
    }
}
