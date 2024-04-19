using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The id element tells the AIML interpreter that it should substitute the user ID.
    ///     The determination of the user ID is not specified, since it will vary by application.
    ///     A suggested default return value is "localhost".
    ///     The id element does not have any content.
    /// </summary>
    public class ID : AIMLTagHandler
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
        public ID(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "id") return user.UserID;
            return string.Empty;
        }
    }
}