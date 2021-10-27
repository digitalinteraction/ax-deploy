using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeployLib
{
    public class Configuration
    {
        public Configuration(uint sessionId, DateTime start, int duration, int rate, int range)
        {
            this.SessionId = sessionId;
            this.Start = start;
            this.Duration = duration; // 24 * 7 * 60 * 60;  // 604800
            this.Rate = rate;
            this.Range = range;
        }

        public Configuration() : this(0, DateTime.MinValue, 24 * 7 * 60 * 60, 100, 8) { }

        public int Rate { get; set; }

        public int Range { get; set; }

        public int Duration { get; set; }

        public string Metadata { get { return ""; } }

        public DateTime Start { get; set; }

        public DateTime End
        {
            get
            {
                return Start.AddSeconds(Duration);
            }
            set
            {
                Duration = (int)((value - Start).TotalSeconds);
            }
        }

        public uint SessionId { get; set; }

        public bool Valid
        {
            get
            {
                if (Duration <= 0) { return false; }
                if (Start.Year < 2000) { return false; }
                if (End <= Start) { return false; }
                int[] ValidRates = { 6, 12, 25, 50, 100, 200, 400, 800, 1600, 3200 };
                if (!ValidRates.Contains(Rate)) { return false; }
                int[] ValidRanges = { 16, 8, 4, 2 };
                if (!ValidRanges.Contains(Range)) { return false; }
                return true;
            }
        }

        public int Within
        {
            get
            {
                DateTime now = DateTime.Now;
                if (End < now)
                {
                    return 1;
                }
                if (Start < now)
                {
                    return 0;
                }
                return -1;
            }
        }

        public override string ToString()
        {
            return "CONFIG #" + SessionId + " " + Start.ToString("yyyy-MM-dd HH\\:mm\\:ss") + " to " + End.ToString("yyyy-MM-dd HH\\:mm\\:ss") + " (" + (Duration / 60 / 60) + " hours) @" + Rate + "Hz +/-" + Range + "g";
        }
    }
}
