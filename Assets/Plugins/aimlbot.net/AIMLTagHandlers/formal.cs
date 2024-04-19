using System.Text;
using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The formal element tells the AIML interpreter to render the contents of the element
    ///     such that the first letter of each word is in uppercase, as defined (if defined) by
    ///     the locale indicated by the specified language (if specified). This is similar to methods
    ///     that are sometimes called "Title Case".
    ///     If no character in this string has a different uppercase version, based on the Unicode
    ///     standard, then the original string is returned.
    /// </summary>
    public class Formal : AIMLTagHandler
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
        public Formal(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "formal")
            {
                var result = new StringBuilder();
                if (templateNode.InnerText.Length > 0)
                {
                    var words = templateNode.InnerText.ToLower().Split();
                    foreach (var word in words)
                    {
                        var newWord = word.Substring(0, 1);
                        newWord = newWord.ToUpper();
                        if (word.Length > 1) newWord += word.Substring(1);
                        result.Append(newWord + " ");
                    }
                }

                return result.ToString().Trim();
            }

            return string.Empty;
        }
    }
}