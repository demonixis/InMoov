using Demonixis.InMoovSharp.Utils;
using System.Collections;
using System.Diagnostics;

namespace Demonixis.InMoovSharp.Services
{
    public class CmdSpeechSynthesis : SpeechSynthesisService
    {
        public string SpeechCommandName = "say";

        public override void Speak(string message)
        {
            if (Paused) return;
            StartCoroutine(SpeechLoop(message));
        }

        private IEnumerator SpeechLoop(string message)
        {
            NotifySpeechStarted(message);

            var waitTime = GetSpeakTime(message);
            Process.Start(SpeechCommandName, message);

            yield return CoroutineFactory.WaitForSeconds(waitTime);

            NotifySpeechState(false);

            yield return CoroutineFactory.WaitForSeconds(DelayAfterSpeak);

            NotifySpeechState(true);
        }
    }
}