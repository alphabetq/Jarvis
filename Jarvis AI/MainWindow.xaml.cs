using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Speech.Recognition;
using System.Speech.Synthesis;
//using System.Windows.Forms;
using System.Runtime.InteropServices;
using Jarvis.Utils;
using System.Globalization;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace Jarvis
{

    public partial class MainWindow : Window
    {
        public Jarvis_DBEntities db = new Jarvis_DBEntities();

        private Capture capture;
        private HaarCascade haarCascade;
        DispatcherTimer timer;

        //private Timer autoSave = new Timer();
        private System.Windows.Forms.Timer clockTimer = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer TwitterCheck = new System.Windows.Forms.Timer();
        private System.Windows.Forms.Timer sleepTimer = new System.Windows.Forms.Timer();
        Commands command = new Commands();
        Twitter t1 = new Twitter();

        // for Jarvis command
        public static Choices list = new Choices();
        public static GrammarBuilder gb = new GrammarBuilder();
        JarvisData data = new JarvisData();
        public static SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
        public static SpeechSynthesizer tts = new SpeechSynthesizer();
        public static PromptBuilder cultures = new PromptBuilder();

        //for putting Jarvis to sleep and waking up jarvis
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(int hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);
        private const int WmSyscommand = 0x0112;
        private const int ScMonitorpower = 0xF170;
        private const int MonitorShutoff = 2;
        private const int MouseeventfMove = 0x0001;

        public static void justSpeak(string text)
        {
            tts.Speak(text);

            if (tts.Voice.Name != "IVONA 2 Brian")
            {
                tts.SelectVoice("IVONA 2 Brian");

            }
        }

        public MainWindow()
        {
            InitializeComponent();

            JarvisData.load();
            // to prevent jarvis taking up commands
            //System.Windows.Application.Current.Properties["SessionListening"] = "false";

            string[] getCommand = db.MLGrammars.Where(x => x.Type != "Conversation").Select(x => x.Name).ToList().ToArray();
            list.Add(getCommand);
            dictateSpeech();

            clockTimer.Tick += clockTimer_Tick;
            clockTimer.Interval = 1000;
            clockTimer.Start();
            t1.TwitterLoad(this);
            TwitterCheck.Tick += TwitterCheck_Tick;
            TwitterCheck.Interval = 2000;
            TwitterCheck.Start();
            sleepTimer.Tick += sleepTimer_Tick;
            refresh_sleepTimer();
            sleepTimer.Start();

            var MLlist = db.MLGrammars.Where(x => x.Type != null).ToList();
            justSpeak("initializing data");

            Weather.getWeather();
            High.Content = "High: " + Weather.TFHigh + "°C";
            Low.Content = "Low: " + Weather.TFLow + "°C";
            Current.Content = "Current: " + Weather.Condition + ", " + Weather.Temperature + "°C";
            var imgpath = "http://openweathermap.org/img/w/" + Weather.WeatherIcon + ".png";
            Uri imageUri = new Uri(imgpath);
            BitmapImage imageBitmap = new BitmapImage(imageUri);
            WeatherIcon.Source = imageBitmap;

            Commands.greetingsInput = db.MLGrammars.Where(x => x.Type == null).Select(x => x.Name).ToList().ToArray();
            Commands.greetingAutoResponses = db.Responses.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type == "Greetings").Select(x => x.Name + ", sir").ToList().ToArray();
            Commands.thankYouResponses = db.Responses.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type == "ThankYou").Select(x => x.Name + ", sir").ToList().ToArray();
            Commands.DemandAutoResponses = db.Responses.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type == "Demand").Select(x => x.Name + ", sir").ToList().ToArray();
            Commands.checkCommandList = db.MLGrammars.Where(x => x.Type != "Conversation").ToList();
            Commands.checkConversationList = db.MLGrammars.Where(x => x.Type != "Command").ToList();
            Commands.getCommand = db.MLGrammars.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type != "Conversation").Select(x => x.Name).ToList().ToArray();
            Commands.getConversation = db.MLGrammars.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type != "Command").Select(x => x.Name).ToList().ToArray();
            Commands.addCountList = db.MLGrammars.Where(x => x.Name != "Jarvis").ToList();

            System.Windows.Forms.PowerStatus p = System.Windows.Forms.SystemInformation.PowerStatus;
            int a = (int)(p.BatteryLifePercent * 100);
            battery.Content = a + "%";

            foreach (var ml in MLlist)
            {
                //for yes and no
                if (ml.Type == "Conversation" && ml.Response == "decision")
                {
                    var result = db.log_Grammar.Where(x => x.Name == ml.Name && x.Type == ml.Type).ToList().Count();
                    var list = db.log_Grammar.ToList().Count();
                    ml.MLRate = (decimal?)result / (decimal?)list;
                }

                else
                {
                    var totalUsageSum = db.MLGrammars.Where(x => x.Type == ml.Type && x.Response == ml.Response).Select(x => x.UsageTime).ToList().Sum();
                    if (ml.UsageTime == null) { ml.UsageTime = 0; }
                    ml.MLRate = (decimal?)ml.UsageTime / (decimal?)totalUsageSum;
                }
            }
            db.SaveChanges();

            var time = DateTime.Now;
            var tell = DateTime.Now.ToShortTimeString();
            if (time.Hour < 6)
            {
                justSpeak("good morning, sir. it's " + tell);
            }

            else if (time.Hour > 12 && time.Hour < 18)
            {
                justSpeak("good afternoon, sir. it's " + tell);
            }

            else
            {
                justSpeak("good evening, sir. it's " + tell);
            }

            Input.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture = new Capture(1);
            haarCascade = new HaarCascade("haarcascade_frontalface_default.xml");
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 0);
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Image<Bgr, Byte> currentFrame = capture.QueryFrame();
            Image<Gray, Byte> grayFrame = currentFrame.Convert<Gray, Byte>();
            Image<Gray, Byte> smallGrayFrame = grayFrame.PyrDown();
            Image<Gray, Byte> smoothedGrayFrame = smallGrayFrame.PyrUp();

            if (currentFrame != null)
            {
                var detectedFaces = smoothedGrayFrame.DetectHaarCascade(haarCascade)[0];

                foreach (var face in detectedFaces)
                {
                    MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 1.5d, 1.5d);
                    font.thickness = 3;
                    currentFrame.Draw("Detected face", ref font, new System.Drawing.Point((face.rect.X + face.rect.X) / 2, face.rect.Y - 20), new Bgr(Color.Green));
                    currentFrame.Draw(face.rect, new Bgr(Color.Green), 6);
                }

                image1.Source = ToBitmapSource(currentFrame);
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }

        public void dictateSpeech()
        {
            gb = new GrammarBuilder(list);
            Grammar finalGrammar = new Grammar(gb);

            cultures = new PromptBuilder(new CultureInfo("en-US"));
            tts.SelectVoice("IVONA 2 Brian");
            _recognizer = new SpeechRecognitionEngine((new CultureInfo("en-US")));
            gb.Culture = new CultureInfo("en-US");

            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.RequestRecognizerUpdate();
            _recognizer.LoadGrammar(finalGrammar);
            _recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Command);
            _recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void refresh_sleepTimer() //to refresh timer after every command
        {
            sleepTimer.Stop();
            Random random = new Random();
            int randomNumber = random.Next(600000, 1200000); //between 10min to 20min
            sleepTimer.Interval = randomNumber;
            sleepTimer.Start();
        }

        private void sleepTimer_Tick(object sender, EventArgs e)
        {
            JarvisData.isOff = "true";
            //JarvisData.save();
            this.log.Items.Add("Jarvis has left the chat.");
            justSpeak("I'm tired, sir. Signing off");
            sleepTimer.Stop();
            bool turnOff = true;   //set true if you want to turn off, false if on
            SendMessage(0xFFFF, WmSyscommand, (IntPtr)ScMonitorpower, (IntPtr)(turnOff ? 2 : -1));
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void clockTimer_Tick(object sender, EventArgs e)
        {
            /*if (DateTime.Now.ToString("hh:mm:ss tt") == "06:25:00 AM")
            {
                if (alarm1 == true)
                {
                    //Play Sound turn on stereo etc

                }
            }
            */
            this.Time.Content = DateTime.Now.ToString("hh:mm:ss tt");
        }

        private void TwitterCheck_Tick(object sender, EventArgs e)
        {
            if (log.Items.Count > 10)
            {
                log.Items.Clear();
            }

            t1.TwitterLoad(this); //Tweet to command

            if (JarvisData.lastTweet != Twitter.LatestTweet.FirstOrDefault())
            {
                this.log.Items.Add("New Twitter Feed: " + Twitter.LatestTweet.FirstOrDefault());
                JarvisData.lastTweet = Twitter.LatestTweet.FirstOrDefault();
                JarvisData.save();
            }
        }

        void Command(object sender, SpeechRecognizedEventArgs e) //Voice to command
        {
            if (log.Items.Count > 10)
            {
                log.Items.Clear();
            }

            string[] greetingsInput = db.MLGrammars.Where(x => x.Type == null).Select(x => x.Name).ToList().ToArray();
            if (e.Result.Text.StartsWith("jarvis")/* || greetingsInput.Contains(e.Result.Text)*/)
            {
                sleepTimer.Start();
                JarvisData.isOff = "false";
                //SendKeys.SendWait("\t");
            }

            if (JarvisData.isOff == "false") {
                command.processCommand(e.Result.Text, this);
            }

            refresh_sleepTimer();
        }
       
        public void restart()
        {
            this.Close();
            System.Windows.Forms.Application.Restart();
        }

        private void inputButton_Click(object sender, RoutedEventArgs e)
        {
            String input = this.Input.Text;
            command.processCommand(input, this);
            this.Input.Text = "";
            refresh_sleepTimer();
        }

        private void OnKeyDownHandler(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                command.processCommand(this.Input.Text, this);
                this.Input.Text = "";
            }
            refresh_sleepTimer();
        }

        public static void ExecuteCommand(string Command)
        {
            System.Diagnostics.ProcessStartInfo procStartInfo =
             new System.Diagnostics.ProcessStartInfo("cmd", "/c " + Command);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window  .
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.Close();
        }

    }
}
