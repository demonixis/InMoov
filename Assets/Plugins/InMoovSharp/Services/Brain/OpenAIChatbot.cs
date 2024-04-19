using Demonixis.InMoovSharp.Settings;
using OpenAI;

namespace Demonixis.InMoovSharp.Services
{
    public class OpenAIChatbot : ChatbotService
    {
        private OpenAIAPI _openAIAPI;
        private bool _isValid;

        private Engine BotEngine { get; set; } = Engine.Davinci;
        public int MaxToken { get; set; } = 200;

        protected override void SafeInitialize()
        {
            var settings = GlobalSettings.Get();
            var key = settings.OpenAIKey;

            if (string.IsNullOrEmpty(key))
            {
                Robot.Log("The API key for OpenAI is missing.");
                return;
            }

            _openAIAPI = new OpenAIAPI(settings.OpenAIKey, BotEngine);
            _isValid = true;
        }

        public override void SetLanguage(string culture)
        {
        }

        public override async void SubmitResponse(string inputSpeech, bool noReply = false)
        {
            if (Paused) return;

            if (!_isValid)
            {
                SubmitResponse("OpenAI is not available");
                return;
            }

            var request = new CompletionRequestBuilder()
                .WithPrompt(inputSpeech)
                .WithMaxTokens(MaxToken)
                .Build();

            var result = await _openAIAPI.Completions.CreateCompletionAsync(request);

            if (!noReply)
                NotifyResponseReady(result.ToString());
        }
    }
}