using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Xml;
namespace Jarvis.Utils
{
    public class Weather
    {
        public static string Temperature;
        public static string Condition;
        public static string ConditionDesc;
        public static string Humidity;
        public static string WindSpeed;
        public static string WindDegree;
        public static string Town;
        public static string TFCloud;
        public static string TFHigh;
        public static string TFLow;
        public static string WeatherIcon;

        public static void getWeather()
        {
            var json = new WebClient().DownloadString("http://api.openweathermap.org/data/2.5/weather?id=1733047&appid=3c82f45f7cd57a519494a0c99b6a930b&units=metric&mode=json");
            var joResponse = JObject.Parse(json);
            var ojObject = joResponse["weather"][0];

            Temperature = (string)joResponse.SelectToken("main.temp");
            TFLow = (string)joResponse.SelectToken("main.temp_min");
            TFHigh = (string)joResponse.SelectToken("main.temp_max");
            Condition = ojObject["main"].ToString();
            ConditionDesc = ojObject["description"].ToString();
            WeatherIcon = ojObject["icon"].ToString();
            Humidity = (string)joResponse.SelectToken("main.humidity");
            WindSpeed = (string)joResponse.SelectToken("wind.speed");
            WindDegree = (string)joResponse.SelectToken("wind.deg");
            Town = joResponse.Last.Previous.ToString().Replace("\"", "").Replace("name: ", "");
            TFCloud = (string)joResponse.SelectToken("clouds.all");

            //string query = String.Format("http://api.openweathermap.org/data/2.5/weather?id=1733047&appid=3c82f45f7cd57a519494a0c99b6a930b&units=metric&mode=xml"); //3c82f45f7cd57a519494a0c99b6a930b
            //XmlDocument wData = new XmlDocument();
            //wData.Load(query);

            //XmlNamespaceManager manager = new XmlNamespaceManager(wData.NameTable);
            //manager.AddNamespace("current", "http://api.openweathermap.org/data/2.5/weather?id=1733047&appid=3c82f45f7cd57a519494a0c99b6a930b&units=metric&mode=xml");

            //XmlNode channel = wData.SelectSingleNode("current");
            //XmlNodeList nodes = wData.SelectNodes("/rss/channel/item/yweather:forcast", manager);

            //Temperature = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["temp"].Value;
            //Condition = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["text"].Value;
            //Humidity = channel.SelectSingleNode("yweather:atmosphere", manager).Attributes["humidity"].Value;
            //WindSpeed = channel.SelectSingleNode("yweather:wind", manager).Attributes["speed"].Value;
            //Town = channel.SelectSingleNode("yweather:location", manager).Attributes["city"].Value;
            //TFCloud = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["text"].Value;
            //TFHigh = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["high"].Value;
            //TFLow = channel.SelectSingleNode("item").SelectSingleNode("yweather:forecast", manager).Attributes["low"].Value;

        }
    }
}
