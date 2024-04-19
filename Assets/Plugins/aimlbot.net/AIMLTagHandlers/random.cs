using System;
using System.Collections.Generic;
using System.Xml;
using AIMLbot.Utils;

namespace AIMLbot.AIMLTagHandlers
{
    /// <summary>
    ///     The random element instructs the AIML interpreter to return exactly one of its contained li
    ///     elements randomly. The random element must contain one or more li elements of type
    ///     defaultListItem, and cannot contain any other elements.
    /// </summary>
    public class Random : AIMLTagHandler
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
        public Random(AIMLbot.Bot bot,
            User user,
            SubQuery query,
            Request request,
            Result result,
            XmlNode templateNode)
            : base(bot, user, query, request, result, templateNode)
        {
            isRecursive = false;
        }

        protected override string ProcessChange()
        {
            if (templateNode.Name.ToLower() == "random")
                if (templateNode.HasChildNodes)
                {
                    // only grab <li> nodes
                    var listNodes = new List<XmlNode>();
                    foreach (XmlNode childNode in templateNode.ChildNodes)
                        if (childNode.Name == "li")
                            listNodes.Add(childNode);
                    if (listNodes.Count > 0)
                    {
                        var r = new global::System.Random();
                        var chosenNode = listNodes[r.Next(listNodes.Count)];
                        return chosenNode.InnerXml;
                    }
                }

            return string.Empty;
        }
    }
}