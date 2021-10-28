using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Markov;

namespace ReviewSpoofer.Services
{
    public class MarkovSingleton
    {
        public MarkovChain<char> reviewIDChain;
        public MarkovChain<string> reviewerNameChain;
        public MarkovChain<string> reviewTextChain;
        public MarkovChain<string> summaryChain;
        public Dictionary<string, int> asinValues;
        public int dataCount = 0;
        public int smallestUnixTime = Int32.MaxValue;
        public int largestUnixTime = Int32.MinValue;

        public MarkovSingleton()
        {
            Ingest();
        }

        private void Ingest()
        {
            reviewIDChain = new MarkovChain<char>(1);
            reviewerNameChain = new MarkovChain<string>(1);
            reviewTextChain = new MarkovChain<string>(1);
            summaryChain = new MarkovChain<string>(1);
            asinValues = new Dictionary<string, int>();

            using (StreamReader r = new StreamReader("Musical_Instruments_5.json"))
            {
                while (!r.EndOfStream)
                {
                    Review review = JsonSerializer.Deserialize<Review>(r.ReadLine());
                    dataCount++;
                    if (asinValues.ContainsKey(review.asin))
                    {
                        asinValues[review.asin] = asinValues[review.asin] + 1;
                    }
                    else
                    {
                        asinValues.Add(review.asin, 1);
                    }
                    reviewIDChain.Add(review.reviewerID);
                    if (review.reviewerName != null)
                        reviewerNameChain.Add(review.reviewerName.Trim().Split(' '));
                    reviewTextChain.Add(review.reviewText.Trim().Split(' '));
                    summaryChain.Add(review.summary.Trim().Split(' '));
                    smallestUnixTime = Math.Min(review.unixReviewTime, smallestUnixTime);
                    largestUnixTime = Math.Max(review.unixReviewTime, largestUnixTime);
                }
            }
        }
    }
}
