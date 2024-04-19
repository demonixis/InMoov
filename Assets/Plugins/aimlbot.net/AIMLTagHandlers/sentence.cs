using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The sentence element tells the AIML interpreter to render the contents of the element
    ///     such that the first letter of each sentence is in uppercase, as defined (if defined) by
    ///     the locale indicated by the specified language (if specified). Sentences are interpreted
    ///     as strings whose last character is the period or full-stop character .. If the string does
    ///     not contain a ., then the entire string is treated as a sentence.
    ///     If no character in this string has a different uppercase version, based on the Unicode
    ///     standard, then the original string is returned.
    /// </summary>
    public class Sentence : AIMLTagHandler
    {
        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="bot">The bot involved in this request</param>
        /// <param name="user">The user making the request</param>
        /// <param name="query">The query that originated this node</param>
        /// <param name="request">The request inputted into the system</param>
        /// <param name="result">The result to be passed to the user</param>
        /// <param name="templateNode">The node to be processed</param>
        public Sentence(AIMLbot.Bot bot,
            User user,
            SubQuery query,
            Request request,
            Result result,
            XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
        }

        protected override string ProcessChange()
        {
            if (templateNode.Name.ToLower() == "sentence")
            {
                if (templateNode.InnerText.Length > 0)
                {
                    var result = new StringBuilder();
                    var letters = templateNode.InnerText.Trim().ToCharArray();
                    var doChange = true;
                    for (var i = 0; i < letters.Length; i++)
                    {
                        var letterAsString = Convert.ToString(letters[i]);
                        if (bot.Splitters.Contains(letterAsString)) doChange = true;

                        var lowercaseLetter = new Regex("[a-zA-Z]");

                        if (lowercaseLetter.IsMatch(letterAsString))
                        {
                            if (doChange)
                            {
                                result.Append(letterAsString.ToUpper(bot.Locale));
                                doChange = false;
                            }
                            else
                            {
                                result.Append(letterAsString.ToLower(bot.Locale));
                            }
                        }
                        else
                        {
                            result.Append(letterAsString);
                        }
                    }

                    return result.ToString();
                }

                // atomic version of the node
                var starNode = GetNode("<star/>");
                var recursiveStar = new Star(bot, user, query, request, result, starNode);
                templateNode.InnerText = recursiveStar.Transform();
                if (templateNode.InnerText.Length > 0)
                    return ProcessChange();
                return string.Empty;
            }

            return string.Empty;
        }
    }
}