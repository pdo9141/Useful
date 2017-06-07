using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1.MVP
{
    public class BorrowerMismatchPresenter
    {
        IBorrowerMismatch view;
        IBorrowerMismatchRepository repository;

        public IBorrowerMismatch View
        {
            get { return view;  }
            set
            {
                view = value;
                AttachEventsToView();
            }
        }

        public BorrowerMismatchPresenter(IBorrowerMismatchRepository repository)
        {
            this.repository = repository;
        }

        private void AttachEventsToView()
        {
            view.SearchBorrowerMismatches += OnSearchBorrowerMismatches;
            view.ApproveBorrowerMismatches += OnApproveBorrowerMismatches;
            view.CancelBorrowerMismatches += OnCancelBorrowerMismatches;      
        }

        private void OnSearchBorrowerMismatches(object sender, SearchBorrowerMismatchesEventArgs e)
        {
            //view.BorrowerMismatches = repository.SearchBorrowerMismatches(e.ClientId, e.StartDate, e.EndDate, e.Statues);
        }

        private void OnApproveBorrowerMismatches(object sender, ApproveBorrowerMismatchesEventArgs e)
        {
            //repository.ApproveBorrowerMismatches(e.SelectedIds, e.UserId, e.UserName, e.ClientId, e.PublishingDetails);
        }

        internal Dictionary<string, string> GetBorrowerMismatchStatuses()
        {
            throw new NotImplementedException();
        }

        private void OnCancelBorrowerMismatches(object sender, CancelBorrowerMismatchesEventArgs e)
        {
            //repository.CancelBorrowerMismatches(e.SelectedIds, e.UserId, e.UserName, e.ClientId, e.PublishingDetails);
        }
    }
}
