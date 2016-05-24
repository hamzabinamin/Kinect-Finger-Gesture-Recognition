using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Speech.Synthesis;
using System.Timers;
using System.Threading.Tasks;

namespace DTWGestureRecognition
{
    public  class SpeechSpeaker
    {
        private static SpeechSynthesizer synth = new SpeechSynthesizer();
        private static Timer timer;

        private static bool repeatSpeech;
        private static bool enableSpeech;
        private static string currentWord;
        //private static Prompt toBeSpeak;
        //private static Prompt lastSpoken;

        public static bool RepeatSpeech
        {
            get { return SpeechSpeaker.repeatSpeech; }
            set
            {
                SpeechSpeaker.repeatSpeech = value;
                if (!value)
                {
                    lock (synth)
                    {
                        synth.SpeakAsyncCancelAll();
                    }
                }
            }
        }

        public static bool EnableSpeech
        {
            get { return SpeechSpeaker.enableSpeech; }
            set { SpeechSpeaker.enableSpeech = value; }
        }

        public static void InitializeSpeechSpeaker()
        {
            timer = new Timer(4500);
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;

            enableSpeech = true;
            currentWord = string.Empty;
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (synth)
            {
                synth.SpeakAsyncCancelAll();
                /*if (toBeSpeak != null &&
                    lastSpoken == null)
                {
                    synth.SpeakAsync(toBeSpeak);
                }*/
            }
        }

        public static void SayIt(string text)
        {
            lock (synth) //currentWord
            {
                var validation = repeatSpeech ? true : !currentWord.Equals(text);
                if (validation && !String.IsNullOrWhiteSpace(text) && enableSpeech)
                {
                    currentWord = text;
                    //synth.SpeakAsyncCancelAll();
                    //setSpeakingFlow();
                    synth.SpeakAsync(currentWord);
                    if (repeatSpeech)
                    {
                        currentWord = string.Empty;
                    }
                }
            }
        }
    }
}
