# Jarvis-AI

<p align="center">
<img src="https://user-images.githubusercontent.com/29234246/57649435-cb9f4f80-75fa-11e9-9da9-71d141bc78bd.jpeg"/>
</p>

### Current Features:
- [`Twitter.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/Twitter.cs)
  - Retrieve tweets from a specific Twitter account.
  - Play a notification sound every time when a new tweet has been retrieved.
  - Read the tweet's content using Text-to-Speech(TTS) every time when a new tweet has been retrieved, https:// links from the tweet will not be read.
  - Display the tweets on the interface.
  - Read all the tweets using TTS by saying "read tweet".
  - Change to another predefined Twitter account by saying "change tweet". (Note: Different language other than English may need to change TTS to the specific language as well, just refer to the code on [`Commands.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/Commands.cs))

- [`JarvisData.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/JarvisData.cs)
  - Save and load data from previous session.
  
- [`Weather.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/Weather.cs)
  - Retrieve current weather conditions.
  - Read full weather report by saying "full weather status".
  - Read summary of weather report by saying "weather status".
  
- [`YoutubeAPI.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/YoutubeAPI.cs)
  - Type "search youtube _your_search_query_" to retrieve search results on Youtube.
  - Read top video title based on the search results.
  - Suggest a specific video to play based on the most used command. Answer "yes" to play the suggested video, answer "no" or just say "first song" to choose the video. 
<br /> `if ((topResult.UsageTime * 75 / 100) >= (allUsageSum/countList))`
  
- [`Commands.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/Commands.cs)
  - Initiating commands or conversaions by saying "jarvis".
  - Responds to typical and non-typical commands.
  - Ask questions and create conversations.
  
- [`MainWindow.xaml.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/MainWindow.xaml.cs)
  - Display current time.
  - Display Twitter feeds.
  - Display current weather status. 
  - Display current battery life.
  - Greet the user based on the current time.
  - Face recognition function.
  - Set alarm clock.
  - Set sleep timer randomly in between 10 minutes to 20 minutes every time Jarvis starts.

### Installation:

1. Install IVONA 2 Brian or use any other language packs.
2. Install Microsoft SQL Server Management Studio 2012 and restore the [`Jarvis_DB.bak`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis_DB.bak) database file.
3. Install Microsoft Visual Studio 2017.
4. Get your own YoutubeAPI and Twitter Developer API credentials and paste them on [`YoutubeAPI.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/YoutubeAPI.cs) and [`Twitter.cs`](https://github.com/alphabetq/Jarvis-AI/blob/master/Jarvis%20AI/Utils/Twitter.cs) respectively.

### Credits:

This project was inspired by [@nickivey](https://github.com/nickivey)'s project, check it out [here](https://github.com/nickivey/Jarvis).
