using AIMLbot.Utils;

namespace AIMLbot.Normalize
{
    /// <summary>
    ///     Strips any illegal characters found within the input string. Illegal characters are referenced from
    ///     the bot's Strippers regex that is defined in the setup XML file.
    /// </summary>
    public class StripIllegalCharacters : TextTransformer
    {
        public StripIllegalCharacters(Bot bot, string inputString) : base(bot, inputString)
        {
        }

        public StripIllegalCharacters(Bot bot)
            : base(bot)
        {
        }

        protected override string ProcessChange()
        {
            return bot.Strippers.Replace(inputString, " ");
        }
    }
}