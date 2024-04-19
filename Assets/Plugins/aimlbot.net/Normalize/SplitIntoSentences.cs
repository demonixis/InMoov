using System;
using System.Collections.Generic;

namespace AIMLbot.Normalize
{
    /// <summary>
    ///     Splits the raw input into its constituent sentences. Split using the tokens found in
    ///     the bots Splitters string array.
    /// </summary>
    public class SplitIntoSentences
    {
        /// <summary>
        ///     The bot this sentence splitter is associated with
        /// </summary>
        private readonly Bot bot;

        /// <summary>
        ///     The raw input string
        /// </summary>
        private string inputString;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="bot">The bot this sentence splitter is associated with</param>
        /// <param name="inputString">The raw input string to be processed</param>
        public SplitIntoSentences(Bot bot, string inputString)
        {
            this.bot = bot;
            this.inputString = inputString;
        }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="bot">The bot this sentence splitter is associated with</param>
        public SplitIntoSentences(Bot bot)
        {
            this.bot = bot;
        }

        /// <summary>
        ///     Splits the supplied raw input into an array of strings according to the tokens found in
        ///     the bot's Splitters List<>
        /// </summary>
        /// <param name="inputString">The raw input to split</param>
        /// <returns>An array of strings representing the constituent "sentences"</returns>
        public string[] Transform(string inputString)
        {
            this.inputString = inputString;
            return Transform();
        }

        /// <summary>
        ///     Splits the raw input supplied via the ctor into an array of strings according to the tokens
        ///     found in the bot's Splitters List<>
        /// </summary>
        /// <returns>An array of strings representing the constituent "sentences"</returns>
        public string[] Transform()
        {
            var tokens = bot.Splitters.ToArray();
            var rawResult = inputString.Split(tokens, StringSplitOptions.RemoveEmptyEntries);
            var tidyResult = new List<string>();
            foreach (var rawSentence in rawResult)
            {
                var tidySentence = rawSentence.Trim();
                if (tidySentence.Length > 0) tidyResult.Add(tidySentence);
            }

            return tidyResult.ToArray();
        }
    }
}