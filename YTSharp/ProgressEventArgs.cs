using System;

namespace YTSharp
{
    public class ProgressEventArgs : EventArgs
    {
        public object ProcessObject { get; set; }
        public decimal Percentage { get; set; }
        public string Error { get; set; }

        public ProgressEventArgs() :
            base()
        {

        }

    }

    public class DownloadEventArgs : EventArgs
    {
        public object ProcessObject { get; set; }
        public DownloadEventArgs() :
            base()
        {

        }

    }
}