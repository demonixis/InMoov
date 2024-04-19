using System;
using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The star element indicates that an AIML interpreter should substitute the value "captured"
    ///     by a particular wildcard from the pattern-specified portion of the match path when returning
    ///     the template.
    ///     The star element has an optional integer index attribute that indicates which wildcard to use.
    ///     The minimum acceptable value for the index is "1" (the first wildcard), and the maximum
    ///     acceptable value is equal to the number of wildcards in the pattern.
    ///     An AIML interpreter should raise an error if the index attribute of a star specifies a wildcard
    ///     that does not exist in the category element's pattern. Not specifying the index is the same as
    ///     specifying an index of "1".
    ///     The star element does not have any content.
    /// </summary>
    public class Star : AIMLTagHandler
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
        public Star(AIMLbot.Bot bot,
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
            if (templateNode.Name.ToLower() == "star")
            {
                if (query.InputStar.Count > 0)
                {
                    if (templateNode.Attributes.Count == 0)
                        // return the first (latest) star in the List<>
                        return query.InputStar[0];
                    if (templateNode.Attributes.Count == 1)
                        if (templateNode.Attributes[0].Name.ToLower() == "index")
                            try
                            {
                                var index = Convert.ToInt32(templateNode.Attributes[0].Value);
                                index--;
                                if ((index >= 0) & (index < query.InputStar.Count))
                                    return query.InputStar[index];
                                bot.WriteToLog("InputStar out of bounds reference caused by input: " +
                                               request.rawInput);
                            }
                            catch
                            {
                                bot.WriteToLog(
                                    "Index set to non-integer value whilst processing star tag in response to the input: " +
                                    request.rawInput);
                            }
                }
                else
                {
                    bot.WriteToLog(
                        "A star tag tried to reference an empty InputStar collection when processing the input: " +
                        request.rawInput);
                }
            }

            return string.Empty;
        }
    }
}