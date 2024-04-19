using System.Xml;
using AIMLbot.Normalize;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The atomic version of the person element is a shortcut for:
    ///     <person>
    ///         <star />
    ///     </person>
    ///     The atomic person does not have any content.
    ///     The non-atomic person element instructs the AIML interpreter to:
    ///     1. replace words with first-person aspect in the result of processing the contents of the
    ///     person element with words with the grammatically-corresponding third-person aspect; and
    ///     2. replace words with third-person aspect in the result of processing the contents of the
    ///     person element with words with the grammatically-corresponding first-person aspect.
    ///     The definition of "grammatically-corresponding" is left up to the implementation.
    ///     Historically, implementations of person have dealt with pronouns, likely due to the fact that
    ///     most AIML has been written in English. However, the decision about whether to transform the
    ///     person aspect of other words is left up to the implementation.
    /// </summary>
    public class Person : AIMLTagHandler
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
        public Person(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "person")
            {
                if (templateNode.InnerText.Length > 0)
                    // non atomic version of the node
                    return ApplySubstitutions.Substitute(bot, bot.PersonSubstitutions,
                        templateNode.InnerText);

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