using Demonixis.InMoovSharp.Services;
using System;

namespace Demonixis.InMoovSharp.Settings
{
    [Serializable]
    public class GlobalSettings
    {
        private const string GlobalSettingsFilename = "global-settings.json";
        private static GlobalSettings _instance;

        public static readonly string[] SupportedLanguages =
        {
            "en-US",
            "fr-FR"
        };

        public string Language;
        public string VoiceRSSKey;
        public string OpenAIKey;

        public GlobalSettings()
        {
            Language = SupportedLanguages[0];
            VoiceRSSKey = string.Empty;
            OpenAIKey = string.Empty;
        }

        public int GetLanguageIndex()
        {
            return Array.IndexOf(SupportedLanguages, Language);
        }

        public void SetLanguageByIndex(int index)
        {
            if (index < 0 || index >= SupportedLanguages.Length)
            {
                Robot.Log($"Error the language index is not valid, entered {index} but maximum is {SupportedLanguages.Length}");
                return;
            }

            Language = SupportedLanguages[index];
        }

        public static GlobalSettings Get()
        {
            if (_instance == null)
            {
                _instance = SaveGame.LoadData<GlobalSettings>(GlobalSettingsFilename, "Config");

                if (_instance == null)
                    _instance = new GlobalSettings();
            }

            return _instance;
        }

        public static void Save()
        {
            SaveGame.SaveData(Get(), GlobalSettingsFilename, "Config");
        }
    }

    [Serializable]
    public struct ServiceList
    {
        public string Chatbot;
        public string SpeechSynthesis;
        public string VoiceRecognition;
        public string Navigation;
        public string ComputerVision;
        public string ServoMixer;
        public string XR;

        public bool IsValid() =>
            !string.IsNullOrEmpty(Chatbot) &&
            !string.IsNullOrEmpty(SpeechSynthesis) &&
            !string.IsNullOrEmpty(VoiceRecognition) &&
            !string.IsNullOrEmpty(ServoMixer) &&
            !string.IsNullOrEmpty(XR);

        public static ServiceList New()
        {
            return new ServiceList
            {
                // Services
                Chatbot = nameof(AIMLNetService),
                VoiceRecognition = nameof(VoiceRecognitionService),
                SpeechSynthesis = nameof(SpeechSynthesisService),
                Navigation = nameof(NavigationService),
                ServoMixer = nameof(ServoMixerService),
                ComputerVision = nameof(ComputerVisionService),
                XR = nameof(XRService)
            };
        }
    }
}