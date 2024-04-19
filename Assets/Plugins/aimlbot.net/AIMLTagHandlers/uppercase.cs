using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The uppercase element tells the AIML interpreter to render the contents of the element
    ///     in uppercase, as defined (if defined) by the locale indicated by the specified language
    ///     if specified).
    ///     If no character in this string has a different uppercase version, based on the Unicode
    ///     standard, then the original string is returned.
    /// </summary>
    public class Uppercase : AIMLTagHandler
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
        public Uppercase(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "uppercase") return templateNode.InnerText.ToUpper(bot.Locale);
            return string.Empty;
        }
    }
}