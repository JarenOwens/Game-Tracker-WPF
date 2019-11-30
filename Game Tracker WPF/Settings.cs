using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameTracker
{
    class Settings
    {
        public Settings()
        {
            window_size = new coordinates();
            window_position = new coordinates();
            window_state = new WindowState();

            window_size.x = 986;
            window_size.y = 350;

            window_position.x = 0;
            window_position.y = 0;

            window_state = WindowState.Normal;

            xml_file_path = @"xml\" + DateTime.Now.Year.ToString() +".xml";

            sort_setting = "Alphabetically Descending";

            load_settings();
        }

        public coordinates window_size { get; set; }
        public coordinates window_position { get; set; }
        public WindowState window_state { get; set; }
        public string xml_file_path { get; set; }

        public string sort_setting { get; set; }

        private void load_settings()
        {
            try
            {
                if (File.Exists("settings.xml")) {
                    XElement settings_file = XElement.Load(@"settings.xml");

                    window_size.x = Convert.ToDouble(settings_file.Element("window_size").Element("x").Value);
                    window_size.y = Convert.ToDouble(settings_file.Element("window_size").Element("y").Value);

                    window_position.x = Convert.ToDouble(settings_file.Element("window_position").Element("x").Value);
                    window_position.y = Convert.ToDouble(settings_file.Element("window_position").Element("y").Value);

                    switch (settings_file.Element("window_state").Value)
                    {
                        case "Maximized":
                            window_state = WindowState.Maximized;
                            break;
                        case "Minimized":
                            window_state = WindowState.Minimized;
                            break;
                        default:
                            window_state = WindowState.Normal;
                            break;
                    }

                    xml_file_path = settings_file.Element("xml_file_path").Value;
                    sort_setting = settings_file.Element("sort_setting").Value;
                }
            }catch{

            }
        }

        public void save_settings()
        {
            XDocument settings_doc = new XDocument(
                new XElement("root",
                    new XElement("window_size", 
                        new XElement("x", window_size.x),
                        new XElement("y", window_size.y)
                    ),
                    new XElement("window_position", 
                        new XElement("x", window_position.x),
                        new XElement("y", window_position.y)
                    ),
                    new XElement("window_state", window_state),
                    new XElement("xml_file_path", xml_file_path),
                    new XElement("sort_setting", sort_setting)
                )
            );

            settings_doc.Save("settings.xml");
        }
    }

    class coordinates
    {
        [XmlElement("x")]
        public double x { get; set; }
        [XmlElement("y")]
        public double y { get; set; }
    }
}
