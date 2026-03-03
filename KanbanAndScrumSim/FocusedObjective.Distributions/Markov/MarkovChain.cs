using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FocusedObjective.Distributions.Markov
{
    public static class MarkovChain
    {
        public static IEnumerable<T> GenerateOccurrenceRatesBasedOnSample<T>(
            List<T> samples,
            int windowSize,
            int maximumGeneratedSamples = 10000)
        {
            if (samples == null)
                throw new ArgumentNullException("samples");

            if (samples.Count() < windowSize)
                throw new ArgumentOutOfRangeException("samples", "The samples collection must have at least the window size elements.");

            // create a chain
            var chain = new Chain<T>(samples, windowSize);

            // generate a new sequence using a starting int, and maximum return size
            return chain.Generate(
                samples.First(), 
                maximumGeneratedSamples);
        }

        internal static void Test()
        {
            // sample data set
            string seed = Tidy(@"Twinkle, twinkle, little star,
             How I wonder what you are!
             Up above the world so high,
             Like a diamond in the sky!

             When the blazing sun is gone,
             When he nothing shines upon,
             Then you show your little light,
             Twinkle, twinkle, all the night.

             Then the traveller in the dark,
             Thanks you for your tiny spark,
             He could not see which way to go,
             If you did not twinkle so.

             In the dark blue sky you keep,
             And often through my curtains peep,
             For you never shut your eye,
             Till the sun is in the sky.

             As your bright and tiny spark,
             Lights the traveller in the dark,—
             Though I know not what you are,
             Twinkle, twinkle, little star.
             ");

            // tokenise the input string
            var seedList = new List<string>(Split(seed.ToLower()));

            // create a chain with a window size of 4
            var chain = new Chain<string>(seedList, 4);

            // generate a new sequence using a starting word, and maximum return size
            var generated = new List<string>(chain.Generate("twinkle", 2000));

            // output the results to the console
            generated.ForEach(item => Console.Write("{0}", item));
        }

        // tokenise a string into words (regex definition of word)
        private static IEnumerable<string> Split(string subject)
        {
            List<string> tokens = new List<string>();
            Regex regex = new Regex(@"(\W+)");
            tokens.AddRange(regex.Split(subject));

            return tokens;
        }

        private static string Tidy(string p)
        {
            string result = p.Replace('\t', ' ');
            string compress = result;

            do
            {
                result = compress;
                compress = result.Replace(" ", " ");
            }
            while (result != compress);

            return result;
        }
    }
}


