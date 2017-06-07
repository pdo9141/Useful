using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.MVP
{
    public class BorrowerMismatchUI : IBorrowerMismatch
    {
        private BorrowerMismatchPresenter presenter;
        public IEnumerable<BorrowerMismatch> BorrowerMismatches { get; set; }
        public IBorrowerMismatchRepository Repository { get; set; }
                
        public event EventHandler<ApproveBorrowerMismatchesEventArgs> ApproveBorrowerMismatches;
        public event EventHandler<CancelBorrowerMismatchesEventArgs> CancelBorrowerMismatches;
        public event EventHandler<SearchBorrowerMismatchesEventArgs> SearchBorrowerMismatches;

        public void PageLoad()
        {
            // Refactor once we integrate DI container, inject dependecies thru property injection (necessary for web forms) from composition root
            Repository = new BorrowerMismatchRepository();
            presenter = new BorrowerMismatchPresenter(Repository);
            presenter.View = this;
        }

        private void LoadStatuses()
        {
            Dictionary<string, string> borrowerMismatchStatuse = presenter.GetBorrowerMismatchStatuses();
            //load dropdown with statuses
        }

        private void Search()
        {
            SearchBorrowerMismatches(this, new SearchBorrowerMismatchesEventArgs());
            //rgResults.DataSource = this.BorrowerMismatches;
            //rgResults.DataBind();
        }

        private void Approve()
        {
            ApproveBorrowerMismatches(this, new ApproveBorrowerMismatchesEventArgs());
        }

        private void Cancel()
        {
            CancelBorrowerMismatches(this, new CancelBorrowerMismatchesEventArgs());
        }
    }
}
