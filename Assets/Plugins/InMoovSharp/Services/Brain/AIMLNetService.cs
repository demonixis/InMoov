using AIMLbot;
using Demonixis.InMoovSharp.Settings;
using System;
using System.IO;

namespace Demonixis.InMoovSharp.Services
{
    public class AIMLNetService : ChatbotService
    {
        [Serializable]
        public struct AIMLNetServiceData
        {
            public string User;
            public string Language;

            public bool IsValid()
            {
                var valid = string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Language);

                if (!valid)
                {
                    User = "Unity";
                    Language = "en-US";
                }

                return true;
            }

            public static AIMLNetServiceData CreateNew()
            {
                return new AIMLNetServiceData
                {
                    User = "Unity",
                    Language = "en-US"
                };
            }
        }

        private const string SerialFilename = "aiml-data.json";
        private Bot _aimlBot;
        private User _user;
        private AIMLNetServiceData _data;
        private string _pathToUserSettings;

        public string AIMLPath => Path.Combine(SaveGame.GetSavePath("Brain"), "AIML.Net");

        public AIMLNetService()
        {
            _aimlBot = new Bot();
            _data = SaveGame.LoadData<AIMLNetServiceData>(SerialFilename, "Brain");

            if (!_data.IsValid())
                _data = AIMLNetServiceData.CreateNew();

            _user = new User(_data.User, _aimlBot);
            _pathToUserSettings = string.Empty;
        }

        protected override void SafeInitialize()
        {
            InitializeBrain();
            LoadBrain();
        }

        private void InitializeBrain()
        {
            _pathToUserSettings = Path.Combine(SaveGame.GetSavePath("Brain"), $"aiml-{_data.Language}-graphmaster.xml");
            _aimlBot.CustomResourcePath = Path.Combine(AIMLPath, _data.Language);

#if INMOOV_UNITY
            if (!Directory.Exists(_aimlBot.CustomResourcePath))
                _aimlBot.CustomResourcePath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "AIML.Net", _data.Language);
#endif

            _aimlBot.LoadSettings();
            _aimlBot.isAcceptingUserInput = false;
            _aimlBot.LoadAimlFromFiles();
            _aimlBot.isAcceptingUserInput = true;
        }

        public override void SetLanguage(string culture)
        {
            if (!IsLanguageSupported(culture))
            {
                Robot.Log($"[AIML.Net] Language {culture} is not supported.");
                return;
            }

            _data.Language = culture;
            SafeInitialize();
        }

        private bool IsLanguageSupported(string lang)
        {
            return Directory.Exists(Path.Combine(AIMLPath, lang));
        }

        public override void SubmitResponse(string inputSpeech, bool noReply = false)
        {
            if (Paused) return;

            var request = new Request(inputSpeech, _user, _aimlBot);
            var result = _aimlBot.Chat(request);

            if (!noReply)
                NotifyResponseReady(result.Output);
        }

        public override void Dispose()
        {
            SaveGame.SaveData(_data, SerialFilename, "Brain");
            SaveBrain();
        }

        public void SaveBrain()
        {
            try
            {
                _user.Predicates.DictionaryAsXML.Save(_pathToUserSettings);
                Robot.Log("Brain saved");
            }
            catch (Exception e)
            {
                Robot.Log("Brain not saved");
                Robot.Log(e.ToString());
            }
        }

        public void LoadBrain()
        {
            try
            {
                _user.Predicates.LoadSettings(_pathToUserSettings);
                Robot.Log("Brain loaded");
            }
            catch (Exception e)
            {
                Robot.Log("Brain not loaded");
                Robot.Log(e.ToString());
            }
        }
    }
}