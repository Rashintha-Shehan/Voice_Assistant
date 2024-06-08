using System;
using System.IO;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows.Forms;

namespace VoiceRecognition
{
    public partial class Form1 : Form
    {
        private readonly SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
        private readonly SpeechSynthesizer Mary = new SpeechSynthesizer();
        private readonly SpeechRecognitionEngine startlistening = new SpeechRecognitionEngine();
        private readonly Random rnd = new Random();
        private int RecTimeOut = 0;
        private DateTime lastHelloTime = DateTime.MinValue;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(File.ReadAllLines(@"defaultCommand.txt")))));
            _recognizer.SpeechRecognized += Default_SpeechRecognized;
            _recognizer.SpeechDetected += _recognizer_SpeechRecognized;
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);

            startlistening.SetInputToDefaultAudioDevice();
            startlistening.LoadGrammarAsync(new Grammar(new GrammarBuilder(new Choices(File.ReadAllLines(@"defaultCommand.txt")))));
            startlistening.SpeechRecognized += startlistening_SpeechRecognized;
        }

        private void Default_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text.ToLower();

            switch (speech)
            {
                case "hello":
                    if ((DateTime.Now - lastHelloTime).TotalSeconds > 5)
                    {
                        Mary.SpeakAsync("Hello! How can I help you?");
                        lastHelloTime = DateTime.Now;
                    }
                    break;
                case "how are you":
                    Mary.SpeakAsync("I'm doing well, thank you for asking!");
                    break;
                case "what is the time now":
                    Mary.SpeakAsync(DateTime.Now.ToString("h mm tt"));
                    break;
                case "date":
                    Mary.SpeakAsync(DateTime.Now.ToString("MMMM d, yyyy"));
                    break;
                case "month":
                    Mary.SpeakAsync(DateTime.Now.ToString("MMMM"));
                    break;
                case "year":
                    Mary.SpeakAsync(DateTime.Now.Year.ToString());
                    break;
                case "wake up":
                    // Check for similar phrases
                    if (speech.Contains("wake up"))
                    {
                        startlistening.RecognizeAsyncCancel();
                        Mary.SpeakAsync("Yes, I'm here.");
                        _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                    }
                    break;
                case "stop talking":
                    Mary.SpeakAsyncCancelAll();
                    Mary.SpeakAsync("Yes sir.");
                    break;
                case "stop listening":
                    Mary.SpeakAsync("Okay. Just call me if you need anything.");
                    _recognizer.RecognizeAsyncCancel();
                    startlistening.RecognizeAsync(RecognizeMode.Multiple);
                    break;
                case "show commands":
                    DisplayCommands();
                    break;
                case "hide commands":
                    LstCommand.Visible = false;
                    break;
                default:
                    Mary.SpeakAsync("Sorry, I didn't understand. Can you please repeat?");
                    break;
            }
        }

        private void startlistening_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            string speech = e.Result.Text.ToLower();

            if (speech == "wake up")
            {
                startlistening.RecognizeAsyncCancel();
                Mary.SpeakAsync("Yes, I'm here.");
                _recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private void _recognizer_SpeechRecognized(object sender, SpeechDetectedEventArgs e)
        {
            RecTimeOut = 0;
        }

        private void DisplayCommands()
        {
            string[] commands = File.ReadAllLines(@"defaultCommand.txt");
            LstCommand.Items.Clear();
            LstCommand.SelectionMode = SelectionMode.None;
            LstCommand.Visible = true;
            foreach (string command in commands)
            {
                LstCommand.Items.Add(command);
            }
        }

        private void TimeSpeek_Tick(object sender, EventArgs e)
        {
            if (RecTimeOut == 10)
            {
                _recognizer.RecognizeAsyncCancel();
            }
            else if (RecTimeOut == 11)
            {
                TimeSpeek.Stop();
                startlistening.RecognizeAsync(RecognizeMode.Multiple);
                RecTimeOut = 0;
            }
        }
    }
}
