using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace TextToSpeech
{
    public enum AudioCodecs
    {
        MP3 = 0,
        WAV,
        AAC,
        OGG
    }

    public enum Language
    {
        Catalan = 0,
        Chinese_China,
        Chinese_HongKong,
        Chinese_Taiwan,
        Danish,
        Dutch,
        English_Australia,
        English_Canada,
        English_GreatBritain,
        English_India,
        English_UnitedStates,
        Finnish,
        French_Canada,
        French_France,
        German,
        Italian,
        Japanese,
        Korean,
        Norwegian,
        Polish,
        Portuguese_Brazil,
        Portuguese_Portugal,
        Russian,
        Spanish_Mexico,
        Spanish_Spain,
        Swedish_Sweden
    }

    public class TextToSpeechOption
    {
        public static string[] language = new string[]
        {
            "ca-es",
            "zh-cn",
            "zh-hk",
            "zh-tw",
            "da-dk",
            "nl-nl",
            "en-au",
            "en-ca",
            "en-gb",
            "en-in",
            "en-us",
            "fi-fi",
            "fr-ca",
            "fr-fr",
            "de-de",
            "it-it",
            "ja-jp",
            "ko-kr",
            "nb-no",
            "pl-pl",
            "pt-br",
            "pt-pt",
            "ru-ru",
            "es-mx",
            "es-es",
            "sv-se"
        };

        public static string[] audioCodecs = new string[]
        {
            "MP3",
            "WAV",
            "AAC",
            "OGG"
        };
    }

    public class TextToSpeechManager
    {
        public Language selectedLanguage = Language.English_UnitedStates;
        public AudioCodecs selectedAudioCodecs = AudioCodecs.OGG;
        
        private string _language;
        private string _source;
        private string _audioCodecs;
        private string _url = "https://api.voicerss.org/?key={0}&hl={1}&src={2}&c={3}";

        private string CurrentLanguage
        {
            get
            {
                _language = TextToSpeechOption.language[(int) selectedLanguage];
                return _language;
            }
        }

        private string CurrentAudioCodecs
        {
            get
            {
                _audioCodecs = TextToSpeechOption.audioCodecs[(int) selectedAudioCodecs];
                return _audioCodecs;
            }
        }
        
        public string APIKey { get; set; }

        private AudioType GetAudioType(AudioCodecs audioCodecs)
        {
            var type = AudioType.OGGVORBIS;
            switch (audioCodecs)
            {
                case AudioCodecs.AAC:
                    type = AudioType.ACC;
                    break;

                case AudioCodecs.MP3:
                    type = AudioType.MPEG;
                    break;

                case AudioCodecs.OGG:
                    type = AudioType.OGGVORBIS;
                    break;

                case AudioCodecs.WAV:
                    type = AudioType.WAV;
                    break;

                default:
                    type = AudioType.OGGVORBIS;
                    break;
            }

            return type;
        }

        private AudioType GetAudioTypeWithIndex(int index)
        {
            AudioType type = AudioType.OGGVORBIS;
            switch (index)
            {
                case 0:
                    type = AudioType.MPEG;
                    break;

                case 1:
                    type = AudioType.WAV;
                    break;

                case 2:
                    type = AudioType.ACC;
                    break;

                case 3:
                    type = AudioType.OGGVORBIS;
                    break;

                default:
                    type = AudioType.OGGVORBIS;
                    break;
            }

            return type;
        }

        public AudioType GetCurrentAudioType()
        {
            return GetAudioType(selectedAudioCodecs);
        }

        public AudioType GetCurrentAudioTypeWithIndex(int index)
        {
            return GetAudioTypeWithIndex(index);
        }

        public string GetTextToSpeechAudio(string content, Language language = Language.English_UnitedStates,
            AudioCodecs audioCodecs = AudioCodecs.OGG)
        {
            selectedLanguage = language;
            selectedAudioCodecs = audioCodecs;

            return SetupAudioURL(content, CurrentLanguage, CurrentAudioCodecs);
        }

        //Language index Default = 10 (English-US) , Audio Format Index Default = 3(OGG)
        public string GetTextToSpeechAudioWithIndex(string content, int languageIndex = 10, int audioCodecsIndex = 3, string voice = null)
        {
            string language = TextToSpeechOption.language[languageIndex];
            string audioformat = TextToSpeechOption.audioCodecs[audioCodecsIndex];

            return SetupAudioURL(content, language, audioformat, voice);
        }

        private string SetupAudioURL(string content, string language, string audioFormat, string voice = null)
        {
            Regex rgx = new Regex("\\s+");
            _source = rgx.Replace(content, "%20");

            var url = string.Format(_url, APIKey, language, _source, audioFormat);

            if (!string.IsNullOrEmpty(voice))
                url += $"&v={voice}";
            
            return url;
        }
    }
}