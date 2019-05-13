using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TweetSharp;

namespace Jarvis.Utils
{
    class Twitter
    {
        //public static string LatestTweet = "";
        public static List<string> LatestTweet = new List<string>();
        public static String firstLoad;
        public static String changeTweet = "alphabetqqq";
        public static String language = "en";

        public void TwitterLoad(MainWindow main)
        {

            var twitterApp = new TwitterService("example_id", "example_id");
            twitterApp.AuthenticateWith("example_id", "example_id");

            IEnumerable<TwitterStatus> tweets = twitterApp.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions { ScreenName = changeTweet, Count = 100, IncludeRts = true, ExcludeReplies = true });
            if (tweets != null)
            {

                if (LatestTweet.FirstOrDefault() != tweets.FirstOrDefault().Text)
                {
                    changeTweet = tweets.FirstOrDefault().Author.ScreenName;
                    LatestTweet.Clear();
                    main.tweetlog.Items.Clear();
                    foreach (var tweet in tweets)
                    {
                        var source = tweet.RawSource;
                        dynamic data = JObject.Parse(source);
                        LatestTweet.Add(tweet.Text);
                        main.tweetlog.Items.Add(tweet.CreatedDate.ToLocalTime() + ": " + tweet.Text + " ❤️ " + data.favorite_count);
                    }
                    if (firstLoad != null && firstLoad != tweets.FirstOrDefault().Text)
                    {
                        string[] stringSeparators = new string[] { "https://" };
                        string[] firstNames = Twitter.LatestTweet.FirstOrDefault().Split(stringSeparators, StringSplitOptions.None);
                        SoundPlayer player = new SoundPlayer("D:/projects/Jarvis FYP/Jarvis-AI/Jarvis AI/Attachment/Twitter - Sound.wav");
                        player.Load();
                        player.Play();
                        Thread.Sleep(1000);
                        MainWindow.justSpeak("New Twitter Feed: " + firstNames.FirstOrDefault());
                    }
                }

                else if (firstLoad == null)
                {
                    if (JarvisData.lastTweet != tweets.FirstOrDefault().Text)
                    {
                        string[] stringSeparators = new string[] { "https://" };
                        string[] firstNames = Twitter.LatestTweet.FirstOrDefault().Split(stringSeparators, StringSplitOptions.None);
                        SoundPlayer player = new SoundPlayer("D:/projects/Jarvis FYP/Jarvis-AI/Jarvis AI/Attachment/Twitter - Sound.wav");
                        player.Load();
                        player.Play();
                        Thread.Sleep(1000);
                        MainWindow.justSpeak("New Twitter Feed: " + firstNames.FirstOrDefault());
                    }

                    firstLoad = tweets.FirstOrDefault().Text;
                }
            }
        }

        public void TwitterSave()
        {
            var twitterApp = new TwitterService("example_id", "example_id");
            twitterApp.AuthenticateWith("example_id", "example_id");
            TwitterUser user = twitterApp.VerifyCredentials(new VerifyCredentialsOptions());

            // Step 5 - Send Tweet to User TimeLine
            SendTweetOptions options = new SendTweetOptions();

            string URL = "file:\\C:\\Users\\<User>\\Desktop\\test.jpg";
            string path = new Uri(URL).LocalPath;

            // Sending with Media
            twitterApp.SendTweetWithMedia(new SendTweetWithMediaOptions
            {
                Status = "<status>"
            });

            var responseText = twitterApp.Response.StatusCode;

            if (responseText.ToString() == "OK")
            {
                //ViewBag.Message = "Twitter Status: Tweet Successful";
            }
            else
            {
                //ViewBag.Message = "Twitter Status: Tweet Unsuccessful";
            }
        }
    }
}
