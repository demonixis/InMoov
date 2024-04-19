
using System;

namespace Demonixis.InMoovSharp.Services
{
    public class VoiceRecognitionService : RobotService
    {
        private const string WordTriggerKey = "WordTrigger";

        private string _wordTrigger = "Robot";

        public bool IsLocked { get; protected set; }
        public bool CanListen => !IsLocked && !Paused;

        public override string SerializationFilename => "voice-recognition.json";

        public string WordTrigger
        {
            get => _wordTrigger;
            set
            {
                _wordTrigger = value.ToLower();
                AddSetting(WordTriggerKey, _wordTrigger);
            }
        }

        public event Action<bool> ListenChanged;
        public event Action<string> PhraseDetected;

        public override void Initialize()
        {
            if (_customSettings.TryGetValue(WordTriggerKey, out string value))
                _wordTrigger = value;

            if (!string.IsNullOrEmpty(_wordTrigger))
                _wordTrigger = _wordTrigger.ToLower();

            base.Initialize();
        }

        protected override void SafeInitialize()
        {
            var robot = Robot.Instance;
            var speech = robot.GetService<SpeechSynthesisService>();
            speech.SpeechStarted += _ => SetLocked(true);
            speech.SpeechFinishedSafe += () => SetLocked(false);
        }

        protected virtual void SetLocked(bool locked)
        {
            IsLocked = locked;
            ListenChanged?.Invoke(CanListen);
        }

        public override void SetPaused(bool paused)
        {
            base.SetPaused(paused);
            ListenChanged?.Invoke(CanListen);
        }

        public virtual void SetLanguage(string culture)
        {
        }

        protected void NotifyPhraseDetected(string phrase)
        {
            if (!string.IsNullOrEmpty(_wordTrigger))
            {
                if (!phrase.StartsWith(_wordTrigger)) return;
                phrase = phrase.Remove(0, _wordTrigger.Length);
            }

            PhraseDetected?.Invoke(phrase);
        }
    }
}