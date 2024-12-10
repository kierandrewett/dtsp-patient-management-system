using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PMS.Controllers
{
    public class SynthesizerController
    {
        private readonly object _SpeechLock = new object();
        private WindowManager WindowManager { get; set; }
        private SpeechSynthesizer Synthesizer { get; set; }
        private SettingsController SettingsController { get; set; }

        public Prompt CurrentPrompt
        {
            get => Synthesizer.GetCurrentlySpokenPrompt();
        }

        private string _CurrentPromptText;
        public string CurrentPromptText
        {
            get => _CurrentPromptText;
        }

        public Dictionary<string, string> SubstitutionLexicon = new()
        {
            { "Dr", "Doctor" },
            { "Dr.", "Doctor." },
        };

        public SynthesizerController(WindowManager wm)
        {
            WindowManager = wm;
            SettingsController = new SettingsController();

            InitSynthesizer();
        }

        private void InitSynthesizer()
        {
            Synthesizer = new SpeechSynthesizer();

            Synthesizer.SetOutputToDefaultAudioDevice();
            Synthesizer.SpeakStarted += Synthesizer_SpeakStarted;
            Synthesizer.SpeakCompleted += Synthesizer_SpeakCompleted;
        }

        private string SubstituteLexiconIntoPrompt(string promptText)
        {
            string[] splitPromptText = promptText.Split(' ');

            for (int i = 0; i < splitPromptText.Length; i++)
            {
                string word = splitPromptText[i];
                string wordTrimmed = word.Trim().ToLower();

                splitPromptText[i] = SubstitutionLexicon.FirstOrDefault(l => l.Key.Trim().Equals(word, StringComparison.CurrentCultureIgnoreCase)).Value ?? word;
            }

            return string.Join(' ', splitPromptText);
        }

        public Prompt? Speak(string promptText, int volume = 100, bool cancelCurrent = true)
        {
            promptText = SubstituteLexiconIntoPrompt(promptText);

            // The speak method makes use of locking via the Threading .NET API to ensure
            // that exclusive access is granted to speech playback/cancellation functionalities
            // while inside the locked state.
            //
            // TLDR; This means that no part of the controller can access these APIs while locked.
            //
            // References: https://learn.microsoft.com/en-us/dotnet/api/system.threading.lock

            lock (_SpeechLock)
            {
                if (_CurrentPromptText == promptText)
                {
                    return null;
                }
                _CurrentPromptText = promptText;

                Synthesizer.Volume = volume;
                Synthesizer.Rate = SettingsController.StoredSettings.TTSSpeechRate ?? 5;

                return Synthesizer.SpeakAsync(promptText);
            }
        }

        public void CancelSpeech(Prompt? cancelPrompt = null)
        {
            lock (_SpeechLock)
            {
                if (cancelPrompt != null)
                {
                    Synthesizer.SpeakAsyncCancel(cancelPrompt);
                }
                else
                {
                    Synthesizer.SpeakAsyncCancelAll();
                }
            }
        }

        private void Synthesizer_SpeakStarted(object? sender, SpeakStartedEventArgs e)
        {
            LogController.WriteLine(
                $"Now announcing: {CurrentPromptText}",
                LogCategory.SynthesizerController
            );
        }

        private void Synthesizer_SpeakCompleted(object? sender, SpeakCompletedEventArgs e)
        {
            LogController.WriteLine(
                $"Announcement cancelled: {CurrentPromptText}",
                LogCategory.SynthesizerController
            );
            _CurrentPromptText = null;
        }
    }
}
