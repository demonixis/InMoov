using System;

namespace Demonixis.InMoovSharp.Services
{
    public class SpeechSynthesisService : RobotService
    {
        private const string DelayAfterKey = "WordTrigger";
        public override string SerializationFilename => "voice-synthesis.json";

        private float _delayAfterSpeak = 0.5f;

        public event Action<string> SpeechStarted;
        public event Action SpeechJustFinished;
        public event Action SpeechFinishedSafe;

        public float DelayAfterSpeak
        {
            get => _delayAfterSpeak;
            set
            {
                _delayAfterSpeak = value;
                AddSetting(DelayAfterKey, _delayAfterSpeak.ToString());
            }
        }

        public override void Initialize()
        {
            if (_customSettings.TryGetValue(DelayAfterKey, out string value))
            {
                if (float.TryParse(value, out float delay))
                    _delayAfterSpeak = delay;
            }

            base.Initialize();
        }

        public virtual void SetLanguage(string culture)
        {
        }

        public virtual string[] GetVoices() => null;

        public virtual int GetVoiceIndex() => 0;

        public virtual void SetVoice(string voiceName)
        {
        }

        public virtual void SetVoice(int voiceIndex)
        {
            AddSetting("VoiceIndex", $"{voiceIndex}");
        }

        public virtual void Speak(string message)
        {
        }

        public float GetSpeakTime(string sentence, int wordsPerMinute = 100)
        {
            var words = sentence.Split(' ');
            return words.Length * 60.0f / wordsPerMinute;
        }

        protected void NotifySpeechStarted(string message)
        {
            SpeechStarted?.Invoke(message);
        }

        protected void NotifySpeechState(bool safe)
        {
            if (safe)
                SpeechFinishedSafe?.Invoke();
            else
                SpeechJustFinished?.Invoke();

        }
    }
}