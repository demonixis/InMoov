using Demonixis.InMoovSharp.Settings;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Demonixis.InMoovSharp.Services
{
    public sealed class BrainWorldContext : IDisposable
    {
        private ChatbotService _chatbot;
        private SpeechSynthesisService _speechSynthesis;
        private VoiceRecognitionService _voiceRecognition;

        [NonSerialized] public List<string> DetectedObjects = new();
        [NonSerialized] public List<string> DetectedPersons = new();

        private string CurrentLanguage
        {
            get
            {
                var settings = GlobalSettings.Get();
                return settings.Language;
            }
        }

        public event Action<int, Vector3> NewPersonDetected;

        public void SetSpeechSynthesis(SpeechSynthesisService speechSynthesis)
        {
            _speechSynthesis = speechSynthesis;
            _speechSynthesis.SetLanguage(CurrentLanguage);
        }

        public void SetVoiceRecognition(VoiceRecognitionService voiceRecognition)
        {
            if (_voiceRecognition != null)
                _voiceRecognition.PhraseDetected -= OnVoiceRecognized;

            _voiceRecognition = voiceRecognition;
            _voiceRecognition.SetLanguage(CurrentLanguage);
            voiceRecognition.PhraseDetected += OnVoiceRecognized;
        }

        public void SetChatbot(ChatbotService chatBot)
        {
            if (_chatbot != null)
                _chatbot.ResponseReady -= OnChatbotResponse;

            _chatbot = chatBot;
            _chatbot.SetLanguage(CurrentLanguage);
            _chatbot.ResponseReady += OnChatbotResponse;
        }

        public void Setup(
            Robot robot,
            ChatbotService chatbotService,
            VoiceRecognitionService voiceRecognition,
            SpeechSynthesisService speechSynthesis)
        {
            _chatbot = chatbotService;
            _voiceRecognition = voiceRecognition;
            _speechSynthesis = speechSynthesis;

            _chatbot.ResponseReady += OnChatbotResponse;
            _voiceRecognition.PhraseDetected += OnVoiceRecognized;

            var settings = GlobalSettings.Get();
            SetLanguage(settings.Language);

            robot.ServiceChanged += RobotOnServiceChanged;
        }

        private void RobotOnServiceChanged(RobotService previousService, RobotService newService)
        {
            if (newService is ChatbotService chatBotService)
                SetChatbot(chatBotService);
            else if (newService is SpeechSynthesisService speechService)
                SetSpeechSynthesis(speechService);
            else if (newService is VoiceRecognitionService voiceRecognition)
                SetVoiceRecognition(voiceRecognition);
        }

        public void SetLanguage(string language)
        {
            _chatbot.SetLanguage(language);
            _voiceRecognition.SetLanguage(language);
            _speechSynthesis.SetLanguage(language);
        }

        private void OnChatbotResponse(string response)
        {
            _speechSynthesis.Speak(string.IsNullOrEmpty(response) ? "I don't understand" : response);
        }

        private void OnVoiceRecognized(string phrase)
        {
            _chatbot.SubmitResponse(phrase);
        }

        public void Dispose()
        {
            _chatbot.ResponseReady -= OnChatbotResponse;
            _voiceRecognition.PhraseDetected -= OnVoiceRecognized;
        }
    }
}