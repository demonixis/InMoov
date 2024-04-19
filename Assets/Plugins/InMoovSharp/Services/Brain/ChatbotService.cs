using System;

namespace Demonixis.InMoovSharp.Services
{
    public abstract class ChatbotService : RobotService
    {
        private int _minWordsInPhrase = 1;
        private int _minLettersInWord = 2;

        public event Action<string> ResponseReady;

        public abstract void SetLanguage(string culture);

        public abstract void SubmitResponse(string phrase, bool noReply = false);

        internal protected void NotifyResponseReady(string response)
        {
            ResponseReady?.Invoke(response);
        }

        public bool IsInputPhraseValid(string inputPhrase)
        {
            var tmp = inputPhrase.Split(' ');

            if (tmp.Length < _minLettersInWord ||
                tmp.Length == 0 && inputPhrase.Length < _minLettersInWord)
                return false;

            return true;
        }
    }
}