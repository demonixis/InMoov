using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The srai element instructs the AIML interpreter to pass the result of processing the contents
    ///     of the srai element to the AIML matching loop, as if the input had been produced by the user
    ///     (this includes stepping through the entire input normalization process). The srai element does
    ///     not have any attributes. It may contain any AIML template elements.
    ///     As with all AIML elements, nested forms should be parsed from inside out, so embedded srais are
    ///     perfectly acceptable.
    /// </summary>
    public class Srai : AIMLTagHandler
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
        public Srai(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "srai")
                if (templateNode.InnerText.Length > 0)
                {
                    var subRequest = new Request(templateNode.InnerText, user, bot);
                    subRequest.StartedOn = request.StartedOn; // make sure we don't keep adding time to the request
                    var subQuery = bot.Chat(subRequest);
                    request.hasTimedOut = subRequest.hasTimedOut;
                    return subQuery.Output;
                }

            return string.Empty;
        }
    }
}