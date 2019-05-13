/*
 * Copyright 2015 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 *
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Jarvis.Utils
{
    /// <summary>
    /// YouTube Data API v3 sample: search by keyword.
    /// Relies on the Google APIs Client Library for .NET, v1.7.0 or higher.
    /// See https://code.google.com/p/google-api-dotnet-client/wiki/GettingStarted
    ///
    /// Set ApiKey to the API key value from the APIs & auth > Registered apps tab of
    ///   https://cloud.google.com/console
    /// Please ensure that you have enabled the YouTube Data API for your project.
    /// </summary>
    /// 

    class YoutubeAPI
    {
        public static Jarvis_DBEntities db = new Jarvis_DBEntities();

        public static async Task Search(string query)
        {
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "example_id",
                ApplicationName = "Google.Apis.YouTube.Samples.Search"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = query; // Replace with your search term.
            searchListRequest.MaxResults = 10;

            // Call the search.list method to retrieve results matching the specified query term.
            var searchListResponse = searchListRequest.ExecuteAsync();

            //List<string> videos = new List<string>();
            //List<string> channels = new List<string>();
            //List<string> playlists = new List<string>();
            List<string> titles = new List<string>();
            List<string> vids = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Result.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        //videos.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.VideoId));
                        titles.Add(searchResult.Snippet.Title);
                        vids.Add(searchResult.Id.VideoId);
                        break;

                        //case "youtube#channel":
                        //  channels.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.ChannelId));
                        //  break;

                        //case "youtube#playlist":
                        //  playlists.Add(String.Format("{0} ({1})", searchResult.Snippet.Title, searchResult.Id.PlaylistId));
                        //  break;
                }
            }

            string alltitles = string.Join(", ", titles.ToArray());
            string allvids = string.Join(", ", vids.ToArray());
            var conversation = db.Conversations.Where(x => x.Name == "search" && x.Type == "youtube").FirstOrDefault();
            conversation.Remarks = alltitles;
            conversation.Result = allvids;
            db.SaveChanges();

            var yesno = db.MLGrammars.Where(x => x.Type != "Command" && x.Response == "decision").Select(x => x.UsageTime).ToList();
            var topResult = db.MLGrammars.Where(x => x.Type != "Command" && x.Response == "youtube").OrderByDescending(x => x.UsageTime).FirstOrDefault();
            var countList = db.MLGrammars.Where(x => x.Type != "Command" && x.Response == "youtube" && x.Name != topResult.Name).ToList().Count();
            var allUsageSum = db.MLGrammars.Where(x => x.Type != "Command" && x.Response == "youtube" && x.Name != topResult.Name).Select(x => x.UsageTime).ToList().Sum();

            MainWindow.justSpeak("The results are ready. The top result is " + titles.FirstOrDefault());
            if ((topResult.UsageTime * 75 / 100) >= (allUsageSum/countList)) {
                MainWindow.justSpeak("Would you like to play the " + topResult.Name + ", sir? The title is " + titles[int.Parse(topResult.Process)] + ".");
            }
            else {
                MainWindow.justSpeak("Which one should I play?");
            }
            //foreach (var speak in speakResults) {
            //    MainWindow.justSpeak(speak + ". ");
            //}

            //Console.WriteLine(String.Format("Videos:\n{0}\n", string.Join("\n", videos)));
            //Console.WriteLine(String.Format("Channels:\n{0}\n", string.Join("\n", channels)));
            //Console.WriteLine(String.Format("Playlists:\n{0}\n", string.Join("\n", playlists)));

            JarvisData.isOff = "true";
            //JarvisData.save();
        }

        public static async Task Playlist(string query)
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for read-only access to the authenticated 
                    // user's account, but not other types of account access.
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("Google.Apis.YouTube.Samples.Search")
                );
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Google.Apis.YouTube.Samples.Playlists"
            });

            #region get all playlists
            // var channelsListRequest = youtubeService.Channels.List("contentDetails");
            var playlistListRequest = youtubeService.Playlists.List("snippet");
            playlistListRequest.Mine = true;

            // Retrieve the contentDetails part of the channel resource for the authenticated user's channel.
            var playlistListResponse = await playlistListRequest.ExecuteAsync();

            List<string> titles = new List<string>();
            List<string> vids = new List<string>();

            foreach (var playlist in playlistListResponse.Items)
            {
                // From the API response, extract the playlist ID that identifies the list
                // of videos uploaded to the authenticated user's channel.
                titles.Add(playlist.Snippet.Title);
                vids.Add(playlist.Id);
            }

            string alltitles = string.Join(", ", titles.ToArray());
            string allvids = string.Join(", ", vids.ToArray());
            var conversation = db.Conversations.Where(x => x.Name == "playlist" && x.Type == "youtube").FirstOrDefault();
            conversation.Remarks = alltitles;
            conversation.Result = allvids;
            db.SaveChanges();
            var speakResults = titles.Take(3);
            MainWindow.justSpeak("The results are ready. The results are ");
            foreach (var title in titles)
            {
                if (title == titles.LastOrDefault())
                {
                    MainWindow.justSpeak("and " + title + ", ");
                }
                else {
                    MainWindow.justSpeak(title + ", ");
                }
            }
            #endregion

            #region get all vids in a playlist

            #endregion
        }
    }
}
