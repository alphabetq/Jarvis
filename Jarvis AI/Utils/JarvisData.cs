using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Timers;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace Jarvis.Utils
{
    class JarvisData
    {
        public static Jarvis_DBEntities db = new Jarvis_DBEntities();
        public static String isOff;
        public static String lastTweet;

        public static void save()
        {
            //XmlTextWriter writer = new XmlTextWriter("Data/data.xml", Encoding.UTF8);

            //writer.Formatting = Formatting.Indented;

            //writer.WriteStartElement("Jarvis");

            //writer.WriteStartElement("isOff");
            //writer.WriteString(isOff);
            //writer.WriteEndElement();


            //writer.WriteStartElement("lastTweet");
            //writer.WriteString(lastTweet);
            //writer.WriteEndElement();

            //writer.WriteEndElement();
            //writer.Close();
            try
            {

                db.log_Jarvis.Where(x => x.Type == "isOff").FirstOrDefault().Type = "isOff";
                db.log_Jarvis.Where(x => x.Type == "isOff").FirstOrDefault().Name = isOff;
                db.log_Jarvis.Where(x => x.Type == "isOff").FirstOrDefault().CreatedDate = DateTime.Now;

                db.log_Jarvis.Where(x => x.Type == "lastTweet").FirstOrDefault().Type = "lastTweet";
                db.log_Jarvis.Where(x => x.Type == "lastTweet").FirstOrDefault().Name = lastTweet;
                db.log_Jarvis.Where(x => x.Type == "lastTweet").FirstOrDefault().CreatedDate = DateTime.Now;

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MainWindow.justSpeak("Sir, I have found a system error.");
                MainWindow.justSpeak(ex.InnerException.Message.ToString());
                //throw ex;
            }
        }

        public static void load()
        {
            //XmlDocument reader = new XmlDocument();
            //reader.Load("Data/data.xml");
            //isOff = reader.SelectSingleNode("Jarvis/isOff").InnerText;
            //lastTweet = reader.SelectSingleNode("Jarvis/lastTweet").InnerText;

            var load = db.log_Jarvis.Where(x => x.Type == "isOff").Select(x => x.Name).FirstOrDefault();
            var load2 = db.log_Jarvis.Where(x => x.Type == "lastTweet").Select(x => x.Name).FirstOrDefault();

            if (load != null) {
                isOff = "true";
            }
            if (load2 != null) {
                lastTweet = load2;
            }
        }
    }
}
