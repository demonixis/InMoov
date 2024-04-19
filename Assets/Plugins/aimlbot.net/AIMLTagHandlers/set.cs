using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The set element instructs the AIML interpreter to set the value of a predicate to the result
    ///     of processing the contents of the set element. The set element has a required attribute name,
    ///     which must be a valid AIML predicate name. If the predicate has not yet been defined, the AIML
    ///     interpreter should define it in memory.
    ///     The AIML interpreter should, generically, return the result of processing the contents of the
    ///     set element. The set element must not perform any text formatting or other "normalization" on
    ///     the predicate contents when returning them.
    ///     The AIML interpreter implementation may optionally provide a mechanism that allows the AIML
    ///     author to designate certain predicates as "return-name-when-set", which means that a set
    ///     operation using such a predicate will return the name of the predicate, rather than its
    ///     captured value. (See [9.2].)
    ///     A set element may contain any AIML template elements.
    /// </summary>
    public class Set : AIMLTagHandler
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
        public Set(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "set")
                if (bot.GlobalSettings.Count > 0)
                    if (templateNode.Attributes.Count == 1)
                        if (templateNode.Attributes[0].Name.ToLower() == "name")
                        {
                            if (templateNode.InnerText.Length > 0)
                            {
                                user.Predicates.AddSetting(templateNode.Attributes[0].Value, templateNode.InnerText);
                                return user.Predicates.GrabSetting(templateNode.Attributes[0].Value);
                            }

                            // remove the predicate
                            user.Predicates.RemoveSetting(templateNode.Attributes[0].Value);
                            return string.Empty;
                        }

            return string.Empty;
        }
    }
}