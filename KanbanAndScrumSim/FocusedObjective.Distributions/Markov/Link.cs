using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FocusedObjective.Distributions.Markov
{
    /// <summary>
    /// parts of a chain (markcov)
    /// </summary>
    /// <typeparam name="T">link type</typeparam>
    internal class Link<T>
    {
        T data;
        int count;

        // following links
        Dictionary<T, Link<T>> links;

        private Link()
        {
        }

        /// <summary>
        /// create a new link
        /// </summary>
        /// <param name="data">value of the item in sequence</param>
        internal Link(T data)
        {
            this.data = data;
            this.count = 0;

            links = new Dictionary<T, Link<T>>();
        }

        /// <summary>
        /// process the input in window sized chunks
        /// </summary>
        /// <param name="input">the sample set</param>
        /// <param name="length">size of sequence window</param>
        public void Process(IEnumerable<T> input, int length)
        {
            // holds the current window
            Queue<T> window = new Queue<T>(length);

            // process the input, a window at a time (overlapping)
            foreach (T part in input)
            {
                if (window.Count == length)
                    window.Dequeue();

                window.Enqueue(part);

                ProcessWindow(window);
            }
        }

        /// <summary>
        /// process the window to construct the chain
        /// </summary>
        /// <param name="window"></param>
        private void ProcessWindow(Queue<T> window)
        {
            Link<T> link = this;

            foreach (T part in window)
                link = link.Process(part);
        }

        /// <summary>
        /// process an item following us
        /// keep track of how many times
        /// we are followed by each item
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        internal Link<T> Process(T part)
        {
            Link<T> link = Find(part);

            // not been followed by this
            // item before
            if (link == null)
            {
                link = new Link<T>(part);
                links.Add(part, link);
            }

            link.Seen();

            return link;
        }

        private void Seen()
        {
            count++;
        }

        public T Data
        {
            get
            {
                return data;
            }
        }

        public int Occurrences
        {
            get
            {
                return count;
            }
        }

        /// <summary>
        /// Total number of incidents after this link
        /// </summary>
        public int ChildOccurrences
        {
            get
            {
                // sum all followers
                int result = links.Sum(link => link.Value.Occurrences);

                return result;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} ({1})", data, count);
        }

        /// <summary>
        /// find a follower of this link
        /// </summary>
        /// <param name="start">item to be found</param>
        /// <returns></returns>
        internal Link<T> Find(T follower)
        {
            Link<T> link = null;

            if (links.ContainsKey(follower))
                link = links[follower];

            return link;
        }

        static System.Random rand = new System.Random();
        /// <summary>
        /// select a random follower weighted
        /// towards followers that followed us
        /// more often in the sample set
        /// </summary>
        /// <returns></returns>
        public Link<T> SelectRandomLink()
        {
            Link<T> link = null;

            int universe = this.ChildOccurrences;

            // select a random probability
            int rnd = rand.Next(1, universe + 1);

            // match the probability by treating
            // the followers as bands of probability
            int total = 0;
            foreach (Link<T> child in links.Values)
            {
                total += child.Occurrences;

                if (total >= rnd)
                {
                    link = child;
                    break;
                }
            }

            return link;
        }

        /// <summary>
        /// find a window of followers that
        /// are after this link, returns where
        /// the last link if found, or null if
        /// this window never occured after this link
        /// </summary>
        /// <param name="window">the sequence to look for</param>
        /// <returns></returns>
        private Link<T> Find(Queue<T> window)
        {
            Link<T> link = this;

            foreach (T part in window)
            {
                link = link.Find(part);

                if (link == null)
                    break;
            }

            return link;
        }

        /// <summary>
        /// a generated set of followers based
        /// on the likelyhood of sequence steps
        /// seen in the sample data
        /// </summary>
        /// <param name="start">a seed value to start the sequence</param>
        /// <param name="length">how big a window to use for sequence steps</param>
        /// <param name="max">maximum size of the set produced</param>
        /// <returns></returns>
        internal IEnumerable<Link<T>> Generate(T start, int length, int max)
        {
            var window = new Queue<T>(length);

            window.Enqueue(start);

            for (Link<T> link = Find(window); link != null && max != 0; link = Find(window), max--)
            {
                var next = link.SelectRandomLink();

                yield return link;

                if (window.Count == length - 1)
                    window.Dequeue();
                if (next != null)
                    window.Enqueue(next.Data);
            }
        }
    }
}
