using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Speech.Synthesis;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Speech.Recognition;
using System.Media;

namespace Jarvis.Utils
{
    public class Commands
    {
        public Jarvis_DBEntities db = new Jarvis_DBEntities();
        Thread t;
        Random random = new Random();
        Weather w = new Weather();
        SpeechSynthesizer tts = new SpeechSynthesizer();

        char keypress = (char)Console.Read();

        //for waking up jarvis from sleep
        [DllImport("user32.dll")]
        static extern IntPtr SendMessage(int hWnd, int msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);
        private const int WmSyscommand = 0x0112;
        private const int ScMonitorpower = 0xF170;
        private const int MonitorShutoff = 2;
        private const int MouseeventfMove = 0x0001;

        public static string[] greetingsInput;
        public static string[] greetingAutoResponses;
        public static string[] thankYouResponses;
        public static string[] DemandAutoResponses;
        public static List<MLGrammar> checkConversationList;
        public static List<MLGrammar> checkCommandList;
        public static string[] getCommand;
        public static string[] getConversation;
        public static List<MLGrammar> addCountList;

        #region prevent jarvis from picking up conversation
        public void processCommand(String query, MainWindow main)
        {
            query = query.ToLower();
            if (!query.Contains("jarvis")) {
                main.log.Items.Add(DateTime.Now + ": " + query);
            }

            t = new Thread(processCommand2);
            t.Start(query);

        }
        #endregion

        public void processCommand2(object query1)
        {
            string query = query1.ToString();

            int index = random.Next(Commands.greetingAutoResponses.Length);
            int index2 = random.Next(Commands.thankYouResponses.Length);
            int index3 = random.Next(Commands.DemandAutoResponses.Length);
            var greetingAutoResponses = Commands.greetingAutoResponses[index];
            var thankYouResponses = Commands.thankYouResponses[index2];
            var DemandAutoResponses = Commands.DemandAutoResponses[index3];

            var checkConversation = checkConversationList.Where(x => x.Type != "Command" && x.Name.Contains(query)).FirstOrDefault();
            var checkCommand = checkCommandList.Where(x => x.Type != "Conversation" && query.Contains(x.Name)).FirstOrDefault();

            if (query.Contains("jarvis") || greetingsInput.Contains(query))
            {
                Choices con = new Choices();
                con.Add(getCommand);
                MainWindow.list = con;

                if (JarvisData.isOff == "false")
                {
                    justSpeak(greetingAutoResponses);
                }

                else
                {
                    JarvisData.isOff = "false";
                    //wake javis from sleep
                    mouse_event(MouseeventfMove, 0, 1, 0, UIntPtr.Zero);
                    Thread.Sleep(1000);
                    mouse_event(MouseeventfMove, 0, -1, 0, UIntPtr.Zero);
                    Thread.Sleep(1000);
                    justSpeak("I'm ready for service, sir");
                }
            }

            else if (checkCommand != null)
            {
                //for typical commands
                if (checkCommand.Process != null)
                {
                    List<string> commandList = checkCommand.Process.Replace("; ", ";").Split(';').ToList();

                    foreach (var com in commandList)
                    {
                        var exe = "ExecuteCommand";
                        var speak = "justSpeak";
                        var sendkey = "SendKeys.SendWait";
                        var thread = "Thread.Sleep";
                        if (com.Contains(exe))
                        {
                            var rep = com.Replace(exe + "(", "").Replace("\")", "\"");
                            ExecuteCommand(rep);
                        }
                        else if (com.Contains(speak))
                        {
                            var rep = com.Replace(speak + "(", "").Replace(")", "");
                            if (rep.Contains("DemandAutoResponses"))
                            {
                                justSpeak(DemandAutoResponses);
                            }
                            else if (rep.Contains("thankYouResponses"))
                            {
                                justSpeak(thankYouResponses);
                            }
                            else if (rep.Contains("greetingAutoResponses"))
                            {
                                justSpeak(greetingAutoResponses);
                            }
                            else {
                                justSpeak(rep);
                            }
                        }
                        else if (com.Contains(sendkey))
                        {
                            var rep = com.Replace(sendkey + "(\"", "").Replace("\")", "");
                            SendKeys.SendWait(rep);
                        }
                        else if (com.Contains(thread))
                        {
                            var rep = com.Replace(thread + "(", "").Replace(")", "");
                            int rep1 = Convert.ToInt32(rep);
                            Thread.Sleep(rep1);
                        }
                    }
                }

                //for non-typical commands
                else
                {
                    if (query.Contains("date") || query.Contains("time"))
                    {
                        if (query.Contains("date"))
                        {
                            justSpeak(DateTime.Today.ToString("dd-MM-yyyy"));
                        }

                        if (query.Contains("time"))
                        {
                            justSpeak(DateTime.Now.ToShortTimeString());
                        }
                    }

                    else if (query.Contains("wake up"))
                    {
                        if (JarvisData.isOff == "true")
                        {
                            JarvisData.isOff = "false";
                            //JarvisData.save();
                            justSpeak(DemandAutoResponses);
                            Thread.Sleep(1000);
                            justSpeak("I'm ready for service, sir");
                        }
                        else
                        {
                            justSpeak("I always stay woke, just like Childish Gambino");
                        }
                    }

                    else if (query.Contains("sleep"))
                    {
                        justSpeak("Signing off");
                        JarvisData.isOff = "true";
                        //JarvisData.save();
                        bool turnOff = true;   //set true if you want to turn off, false if on
                        SendMessage(0xFFFF, WmSyscommand, (IntPtr)ScMonitorpower, (IntPtr)(turnOff ? 2 : -1));
                    }

                    else if (query.Contains("restart"))
                    {
                        justSpeak("restarting");
                        SendKeys.SendWait("%{TAB}");
                        Thread.Sleep(1000);
                        SendKeys.SendWait("^+{F5}");
                    }

                    //else if ((keypress.Modifiers & ConsoleModifiers.Control) != 0)
                    //{
                    //    SendKeys.SendWait("^(c)");
                    //    ExecuteCommand(@"/c ""C:/nircmd.exe speak text ~$clipboard$");
                    //}

                    else if (query.Contains("change tweet"))
                    {
                        if (Twitter.changeTweet != "Tesla")
                        {
                            if (MainWindow.tts.Voice.Name != "Microsoft Huihui Desktop")
                            {

                                MainWindow.tts.SelectVoice("Microsoft Huihui Desktop");

                            }
                            Twitter.changeTweet = "Tesla";
                        }
                        else
                        {
                            Twitter.changeTweet = "elonmusk";
                        }
                    }

                    else if (query.Contains("read tweet"))
                    {
                        if (Twitter.changeTweet == "Tesla")
                        {
                            MainWindow.tts.SelectVoice("Microsoft Huihui Desktop");
                        }
                        else
                        {
                            MainWindow.tts.SelectVoice("IVONA 2 Brian");
                        }
                        MainWindow.justSpeak(DemandAutoResponses);

                        foreach (var tweet in Twitter.LatestTweet.Take(1))
                        {
                            if (Twitter.changeTweet == "Tesla")
                            {
                                MainWindow.tts.SelectVoice("Microsoft Huihui Desktop");
                            }
                            else
                            {
                                MainWindow.tts.SelectVoice("IVONA 2 Brian");
                            }
                            MainWindow.justSpeak(tweet);
                        }
                    }

                    else if (query.Contains("play song"))
                    {
                        string search = query.ToString();
                        search = search.Replace("search ", "");

                        if (search.StartsWith("youtube"))
                        {
                            search = search.Replace("youtube ", "");
                            justSpeak(DemandAutoResponses);
                            YoutubeAPI.Search(search);

                            Choices con = new Choices();
                            con.Add(getConversation);
                            MainWindow.list = con;
                        }

                        else
                        {
                            search = search.Replace(" ", "%20");
                            justSpeak(DemandAutoResponses);
                            //ExecuteCommand(@"/c """"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"" http://www.google.com/search?q=""" + search);
                            Process.Start("http://www.google.com/search?q=" + search);
                            Thread.Sleep(2000);
                            justSpeak("The results are ready, sir.");
                        }
                    }

                    else if (query.Contains("full weather status"))
                    {
                        justSpeak("The current weather is " + Weather.Condition + ", and the condition is " +
                        Weather.ConditionDesc + ". The cloudiness is " + Weather.TFCloud + " percent. The current temperature is " + 
                        Weather.Temperature + ", the highest temperature is " + Weather.TFHigh + " and the lowest temperature is " + 
                        Weather.TFLow + ". The humidity is " + Weather.Humidity + " percent. The wind is " + Weather.WindSpeed + 
                        " meter/sec and " + Weather.WindDegree + " degrees. ");
                    }

                    else if (query.Contains("weather status"))
                    {
                        justSpeak("The current weather is " + Weather.ConditionDesc + ". The current temperature is " +
                        Weather.Temperature + ". ");
                    }

                    else if (query.Contains("good night"))
                    {
                        JarvisData.isOff = "true";
                        //JarvisData.save();
                        SoundPlayer player = new SoundPlayer("D:/projects/Jarvis FYP/Jarvis-AI/Jarvis AI/Attachment/short.wav");
                        player.Load();
                        player.Play();
                        Thread.Sleep(4000);
                        bool turnOff = true;   //set true if you want to turn off, false if on
                        SendMessage(0xFFFF, WmSyscommand, (IntPtr)ScMonitorpower, (IntPtr)(turnOff ? 2 : -1));
                    }

                    else if (query.StartsWith("search"))
                    {
                        string search = query.ToString();
                        search = search.Replace("search ", "");

                        if (search.StartsWith("youtube"))
                        {
                            search = search.Replace("youtube ", "");
                            justSpeak(DemandAutoResponses);
                            YoutubeAPI.Search(search);

                            //switch grammar from Command to Conversation
                            Choices con = new Choices();
                            con.Add(getConversation);
                            MainWindow.list = con;

                            MainWindow.gb = new GrammarBuilder(MainWindow.list);
                            Grammar finalGrammar = new Grammar(MainWindow.gb);
                            MainWindow._recognizer.UnloadAllGrammars();
                            MainWindow._recognizer.LoadGrammar(finalGrammar);

                            search = search.Replace(search, "search youtube " + search);
                        }

                        else
                        {
                            search = search.Replace(" ", "%20");
                            justSpeak(DemandAutoResponses);
                            //ExecuteCommand(@"/c """"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"" http://www.google.com/search?q=""" + search);
                            Process.Start("http://www.google.com/search?q=" + search);
                            Thread.Sleep(2000);
                            justSpeak("The results are ready, sir.");
                        }
                    }
                }
            }

            else if (checkConversation != null)
            {
                var get = db.Conversations.Where(x => x.Name == "search").Select(x => x.Result).FirstOrDefault().ToString();
                get = get.Replace(", ", ",");
                List<string> result = get.Split(',').ToList();
                if (checkConversation.Response == "youtube") {
                    Process.Start("http://www.youtube.com/watch?v=" + result[Int32.Parse(checkConversation.Process)]);
                }
                else if (checkConversation.Response == "decision")
                {
                    if (checkConversation.Name == "yes") {
                        var top = db.MLGrammars.Where(x => x.Type == "Conversation" && x.Response == "youtube").OrderByDescending(x => x.UsageTime).FirstOrDefault();
                        Process.Start("http://www.youtube.com/watch?v=" + result[Int32.Parse(top.Process)]);
                    }
                    else if (checkConversation.Name == "no")
                    {
                        MainWindow.justSpeak("Which one should I play?");
                    }

                    log_Grammar log = new log_Grammar();
                    log.Name = checkConversation.Name;
                    log.Type = checkConversation.Type;
                    log.CreatedDate = DateTime.Now;
                    db.log_Grammar.Add(log);
                    db.SaveChanges();
                }
                else
                {
                    Random random = new Random();
                    int randomNumber = random.Next(0, 9);
                    Process.Start("http://www.youtube.com/watch?v=" + result[randomNumber]);
                }

                if (checkConversation.Name == "no") {
                    //switch grammar from Conversation to Conversation but without decision
                    string[] getCommand = db.MLGrammars.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type != "Commands" && x.Response != "decision").Select(x => x.Name).ToList().ToArray();
                    Choices con = new Choices();
                    con.Add(getCommand);
                    MainWindow.list = con;

                    MainWindow.gb = new GrammarBuilder(MainWindow.list);
                    Grammar finalGrammar = new Grammar(MainWindow.gb);
                    MainWindow._recognizer.UnloadAllGrammars();
                    MainWindow._recognizer.LoadGrammar(finalGrammar);
                }
                else
                {
                    //switch grammar from Conversation to Command
                    string[] getCommand = db.MLGrammars.Where(x => string.IsNullOrEmpty(x.Name) == false && x.Type != "Conversation").Select(x => x.Name).ToList().ToArray();
                    Choices con = new Choices();
                    con.Add(getCommand);
                    MainWindow.list = con;

                    MainWindow.gb = new GrammarBuilder(MainWindow.list);
                    Grammar finalGrammar = new Grammar(MainWindow.gb);
                    MainWindow._recognizer.UnloadAllGrammars();
                    MainWindow._recognizer.LoadGrammar(finalGrammar);
                }

            }

            //save data
            if (!query.Contains("jarvis")) {
                query = query.Replace("jarvis ", "");
                var addCount = addCountList.Where(x => x.Name == query).FirstOrDefault();
                if (addCount != null)
                {
                    if (addCount.UsageTime == null)
                    {
                        addCount.UsageTime = 1;
                    }
                    else
                    {
                        addCount.UsageTime += 1;
                    }
                    db.SaveChanges();
                }
            }

            if (query.Contains("no") || query.Contains("search youtube") || query.Contains("jarvis") || greetingsInput.Contains(query))
            {
                JarvisData.isOff = "false";
                //JarvisData.save();
            }
            else
            {
                JarvisData.isOff = "true";
                //JarvisData.save();
            }
            t.Abort();
        }

        private void justSpeak(string text)
        {
            tts.Speak(text);
        }

        public static void ExecuteCommand(string Command)
        {
            System.Diagnostics.ProcessStartInfo procStartInfo =
             new System.Diagnostics.ProcessStartInfo("cmd", Command);

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
