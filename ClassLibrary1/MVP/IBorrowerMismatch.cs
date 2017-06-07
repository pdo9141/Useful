using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.MVP
{
    public interface IBorrowerMismatch : IView
    {
        IEnumerable<BorrowerMismatch> BorrowerMismatches { get; set; }
        event EventHandler<SearchBorrowerMismatchesEventArgs> SearchBorrowerMismatches;
        event EventHandler<ApproveBorrowerMismatchesEventArgs> ApproveBorrowerMismatches;
        event EventHandler<CancelBorrowerMismatchesEventArgs> CancelBorrowerMismatches;
    }
}
