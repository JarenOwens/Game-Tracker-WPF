using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml.Serialization;
using System.Windows.Controls;
using System.Net;
using System.Drawing;

namespace Game_Tracker_1
{
    [Serializable()]
    public class videoGame : ISerializable
    {
        //game name
        [XmlElement("name")]
        public string name { get; set; }
        //whether the game has been beaten, dropped, currently playing, etc.
        [XmlElement("beaten")]
        public string beaten { get; set; }
        //whether or not the user wants to beat the game
        [XmlElement("want_to_beat")]
        public string wantToBeat { get; set; }
        //when the user started playing the game
        [XmlElement("start_date")]
        public DateTime startDate { get; set; }
        //how the start date will be displayed as a string
        [XmlElement("start_date_string")]
        public string startDateString { get; set; }
        //when the user finished the game
        [XmlElement("end_date")]
        public DateTime endDate { get; set; }
        //how the end date is displayed as a string, may be an empty string if the game hasn't been beaten
        [XmlElement("end_date_string")]
        public string endDateString { get; set; }
        //how many hours the user has played the game
        [XmlElement("hours")]
        public double hours { get; set; }
        //how the hours will be displayed, NA if hours == 0
        [XmlElement("hours_string")]
        public string hoursString { get; set; }
        [XmlElement("image_link")]
        public string image_link { get; set; }
        public System.Drawing.Image image { get; set; }

        public videoGame()
        {
            name = "";
            beaten = "No";
            wantToBeat = "Yes";
            startDate = DateTime.Now;
            startDateString = startDate.Date.ToShortDateString();
            endDate = DateTime.Now;
            endDateString = "";
            hours = 0;
            hoursString = "NA";
            image_link = "";
            image = null;

            update_image();
        }

        public videoGame(string name, string beaten, string wantToBeat, DateTime startDate, DateTime endDate, double hours, string image_link)
        {
            this.name = name;
            this.beaten = beaten;
            this.wantToBeat = wantToBeat;
            this.startDate = startDate;
            startDateString = startDate.Date.ToShortDateString();
            this.endDate = endDate;
            switch (beaten) //format the endDateString based on whether or not the game has been beaten
            {
                case "Yes":
                    endDateString = endDate.Date.ToShortDateString();
                    break;
                default:
                    endDateString = "";
                    break;
            }

            if (this.hours > 0) //format the hoursString based on whether or not the hours are more than 0
            {
                hoursString = this.hours.ToString();
            }
            else
            {
                hoursString = "NA";
            }
            this.image_link = image_link;

            update_image();
        }

        public void update_image()
        {
            if (image_link != "")
            {
                try
                {
                    var request = WebRequest.Create(image_link);
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        image = Bitmap.FromStream(stream);
                    }
                }
                catch (Exception)
                {
                    image = null;
                }
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) //object data to be serialized to a file
        {
            info.AddValue("name", name);
            info.AddValue("beaten", beaten);
            info.AddValue("wantToBeat", wantToBeat);
            info.AddValue("startDate", startDate);
            info.AddValue("startDateString", startDateString);
            info.AddValue("endDate", endDate);
            info.AddValue("endDateString", endDateString);
            info.AddValue("hours", hours);
            info.AddValue("hoursString", hoursString);
            info.AddValue("image_link", image_link);
        }

        public videoGame(SerializationInfo info, StreamingContext context)
        {
            name = (string)info.GetValue("name", typeof(string));
            beaten = (string)info.GetValue("beaten", typeof(string));
            wantToBeat = (string)info.GetValue("wantToBeat", typeof(string));
            startDate = (DateTime)info.GetValue("startDate", typeof(DateTime));
            startDateString = (string)info.GetValue("startDateString", typeof(string));
            endDate = (DateTime)info.GetValue("endDate", typeof(DateTime));
            endDateString = (string)info.GetValue("endDateString", typeof(string));
            try //any new properties added need to be try/catched incase the user's file is using an older version of the object
            {
                hours = (double)info.GetValue("hours", typeof(double));
                hoursString = (string)info.GetValue("hoursString", typeof(string));
                image_link = (string)info.GetValue("image_link", typeof(string));
            }
            catch (Exception)
            {

            }
        }
    }
}
