using System;
using System.Collections.Generic;
using System.Text;
using AIMLbot.Utils;

namespace AIMLbot
{
    /// <summary>
    ///     Encapsulates information about the result of a request to the bot
    /// </summary>
    public class Result
    {
        /// <summary>
        ///     The individual sentences that constitute the raw input from the user
        /// </summary>
        public readonly List<string> InputSentences = new();

        /// <summary>
        ///     The individual sentences produced by the bot that form the complete response
        /// </summary>
        public readonly List<string> OutputSentences = new();

        /// <summary>
        ///     The subQueries processed by the bot's graphmaster that contain the templates that
        ///     are to be converted into the collection of Sentences
        /// </summary>
        public readonly List<SubQuery> SubQueries = new();

        /// <summary>
        ///     The bot that is providing the answer
        /// </summary>
        public Bot bot;

        /// <summary>
        ///     The amount of time the request took to process
        /// </summary>
        public TimeSpan Duration;

        /// <summary>
        ///     The normalized sentence(s) (paths) fed into the graphmaster
        /// </summary>
        public List<string> NormalizedPaths = new();

        /// <summary>
        ///     The request from the user
        /// </summary>
        public Request request;

        /// <summary>
        ///     The user for whom this is a result
        /// </summary>
        public User user;

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="user">The user for whom this is a result</param>
        /// <param name="bot">The bot providing the result</param>
        /// <param name="request">The request that originated this result</param>
        public Result(User user, Bot bot, Request request)
        {
            this.user = user;
            this.bot = bot;
            this.request = request;
            this.request.result = this;
        }

        /// <summary>
        ///     The raw input from the user
        /// </summary>
        public string RawInput => request.rawInput;

        /// <summary>
        ///     The result from the bot with logging and checking
        /// </summary>
        public string Output
        {
            get
            {
                if (OutputSentences.Count > 0) return RawOutput;

                if (request.hasTimedOut)
                {
                    return bot.TimeOutMessage;
                }

                var paths = new StringBuilder();
                foreach (var pattern in NormalizedPaths) paths.Append(pattern + Environment.NewLine);
                bot.WriteToLog("The bot could not find any response for the input: " + RawInput +
                               " with the path(s): " + Environment.NewLine + paths +
                               " from the user with an id: " + user.UserID);
                return string.Empty;
            }
        }

        /// <summary>
        ///     Returns the raw sentences without any logging
        /// </summary>
        public string RawOutput
        {
            get
            {
                var result = new StringBuilder();
                foreach (var sentence in OutputSentences)
                {
                    var sentenceForOutput = sentence.Trim();
                    if (!CheckEndsAsSentence(sentenceForOutput)) sentenceForOutput += ".";
                    result.Append(sentenceForOutput + " ");
                }

                return result.ToString().Trim();
            }
        }

        /// <summary>
        ///     Returns the raw output from the bot
        /// </summary>
        /// <returns>The raw output from the bot</returns>
        public override string ToString()
        {
            return Output;
        }

        /// <summary>
        ///     Checks that the provided sentence ends with a sentence splitter
        /// </summary>
        /// <param name="sentence">the sentence to check</param>
        /// <returns>True if ends with an appropriate sentence splitter</returns>
        private bool CheckEndsAsSentence(string sentence)
        {
            foreach (var splitter in bot.Splitters)
                if (sentence.Trim().EndsWith(splitter))
                    return true;
            return false;
        }
    }
}