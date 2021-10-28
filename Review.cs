using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReviewSpoofer
{
    public class Review
    {
        public string reviewerID { get; set; }
        public string asin { get; set; }
        public string reviewerName { get; set; }
        public int[] helpful { get; set; }
        public string reviewText { get; set; }
        public double overall { get; set; }
        public string summary { get; set; }
        public int unixReviewTime { get; set; }
        public string reviewTime { get; set; }
    }
}
