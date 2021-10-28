using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReviewSpoofer.Services;

namespace ReviewSpoofer.Controllers
{
    [ApiController]
    [Route("api/generate")]
    public class GenerateController : ControllerBase
    {
        private readonly MarkovSingleton _markovSingleton;
        private readonly ILogger<GenerateController> _logger;

        public GenerateController(MarkovSingleton markovSingleton, ILogger<GenerateController> logger)
        {
            _markovSingleton = markovSingleton;
            _logger = logger;
        }

        [HttpGet]
        public string Get()
        {
            Review spoof = new Review();
            Random rand = new Random();

            //ReviewerID appears to be 14 digiti alphanumeric. Always starts with A though!  Was going to just do random gen, but used markov.  Guaranteed 14 digits!
            //const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            //spoof.reviewerID = $"A{new String(Enumerable.Repeat(chars, 13).Select(s => s[rand.Next(s.Length)]).ToArray())}";
            while (string.IsNullOrEmpty(spoof.reviewerID) || spoof.reviewerID.Length < 14)
            {
                spoof.reviewerID += new String(_markovSingleton.reviewIDChain.Chain(rand).ToArray());
            }
            spoof.reviewerID = spoof.reviewerID.Substring(0,14);

            //Asin is the product key, I'm guessing?  Pulling from a dictionary.  Dictionary was made with ability to weigh keys by popularity, but not implemented.
            List<string> keys = new List<string>(_markovSingleton.asinValues.Keys);
            spoof.asin = keys[rand.Next(keys.Count)];

            //Reviewer Names can be generate with markov chains.
            spoof.reviewerName = String.Join(" ", _markovSingleton.reviewerNameChain.Chain(rand));

            //Helpful appears to be an int array of two values.  Guessing its "2/3 people found this review helpful" and the "2/3" are pulled from this array.
            //This super simple algo favors small responses, but with a majority of positive responses
            int second = FlipTilYouMiss(rand);
            int first = second - FlipTilYouMiss(rand, second);
            spoof.helpful = new int[2]{ first, second };

            //Here's another usecase for markov.  Its having issues with punctuation.
            spoof.reviewText = String.Join(" ", _markovSingleton.reviewTextChain.Chain(rand));

            //Overall will just be random distribution, but due to rand not making exact max, gonna do a trick to slightly weight to 5.0
            spoof.overall = Math.Round(Math.Clamp(rand.NextDouble() * 5.1, 0.0, 5.0), 1);

            //Summary is another markov use case.
            spoof.summary = String.Join(" ", _markovSingleton.summaryChain.Chain(rand));

            //Review times are related values, so I cant fudge them as I'd like.  Also must be within time range.
            //Grab the difference between the earliest and latest times we saw, get a random number between those dates, and add that to the earliest date.
            int dif = _markovSingleton.largestUnixTime - _markovSingleton.smallestUnixTime;
            spoof.unixReviewTime = (int)(rand.NextDouble() * dif) + _markovSingleton.smallestUnixTime;

            //Convert that date to datetime.  Sample data was formated with trailing zero in month but not day.
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(spoof.unixReviewTime);
            spoof.reviewTime = $"{dateTime.ToString("MM d")}, {dateTime.Year}";

            return JsonSerializer.Serialize(spoof);
        }

        private int FlipTilYouMiss(Random rand, int max = Int32.MaxValue)
        {
            int x = 0;
            while(rand.Next(2) == 0 && x < max)
            {
                x++;
            }
            return x;
        }
    }
}
