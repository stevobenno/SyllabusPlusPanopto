namespace SyllabusPlusPanopto.Integration.ApiWrappers
{
    public class SessionUsage
    {
        #region Internals

        private double _views = 0;
        private double _minutes = 0;
        private double _visitors = 0;
        private bool _ok = false;

        #endregion

        #region Constructors

        public SessionUsage()
        {
        }

        #endregion

        #region Properties

        public bool IsOk
        {
            get
            {
                return _ok;
            }
            set
            {
                _ok = value;
            }
        }

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

        #endregion
    }
}
