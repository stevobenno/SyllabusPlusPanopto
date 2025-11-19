using System;

namespace SyllabusPlusPanopto.Transform.ApiWrappers
{
    public class SessionFilter
    {
        #region Internals

        private double _views = 0;
        private double _minutes = 0;
        private double _visitors = 0;
        private DateTime? _startDate = null;
        private DateTime? _endDate = null;

        #endregion

        #region Constructors

        public SessionFilter()
        { 
        }

        public SessionFilter(DateTime? start, DateTime? end, double numberOfViews, double minsViewed, double numberOfVisitors)
        {
            _startDate = start;
            _endDate = end;
            _views = numberOfViews;
            _minutes = minsViewed;
            _visitors = numberOfViews;
        }

        #endregion

        #region Properties

        public double NumberOfViews
        {
            get 
            {
                return _views;
            }
            set
            {
                _views = value;
            }
        }

        public double MinutesViewed
        {
            get 
            {
                return _minutes;
            }
            set
            {
                _minutes = value;
            }
        }

        public double NumberOfVisitors
        {
            get
            {
                return _visitors;
            }
            set
            {
                _visitors = value;
            }
        }

        public DateTime? StartDate
        {
            get
            {
                return _startDate;
            }
            set
            {
                _startDate = value;
            }
        }

        public DateTime? EndDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                _endDate = value;
            }
        }

        #endregion
    }
}

