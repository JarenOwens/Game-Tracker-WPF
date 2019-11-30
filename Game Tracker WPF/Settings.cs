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

        //saves window size
        public coordinates window_size { get; set; }
        //saves window position
        public coordinates window_position { get; set; }
        //saves window state (maximized, minimized, normal)
        public WindowState window_state { get; set; }
        //saves path to last opened file
        public string xml_file_path { get; set; }
        //saves last selected sort setting
        public string sort_setting { get; set; }

        //load settings from xml file
        private void load_settings()
        {
            try
            {
                //check if file exists
                if (File.Exists("settings.xml")) {
                    //load xml into XElement
                    XElement settings_file = XElement.Load(@"settings.xml");

                    //read window size setting from xml
                    window_size.x = Convert.ToDouble(settings_file.Element("window_size").Element("x").Value);
                    window_size.y = Convert.ToDouble(settings_file.Element("window_size").Element("y").Value);

                    //read window position from xml
                    window_position.x = Convert.ToDouble(settings_file.Element("window_position").Element("x").Value);
                    window_position.y = Convert.ToDouble(settings_file.Element("window_position").Element("y").Value);

                    //read window sate from xml
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

                    //read last opened file
                    xml_file_path = settings_file.Element("xml_file_path").Value;
                    //read last sort setting
                    sort_setting = settings_file.Element("sort_setting").Value;
                }
            }catch{
                //do nothing
            }
        }

        //save settings to xml
        public void save_settings()
        {
            //create new XDocument
            XDocument settings_doc = new XDocument(
                //root node for settings
                new XElement("root",
                    //window size node
                    new XElement("window_size", 
                        new XElement("x", window_size.x),
                        new XElement("y", window_size.y)
                    ),
                    //window position node
                    new XElement("window_position", 
                        new XElement("x", window_position.x),
                        new XElement("y", window_position.y)
                    ),
                    //window state node
                    new XElement("window_state", window_state),
                    //last opened file
                    new XElement("xml_file_path", xml_file_path),
                    //last sort setting
                    new XElement("sort_setting", sort_setting)
                )
            );
            //save XDocument to settings.xml
            settings_doc.Save("settings.xml");
        }
    }

    //simple coordinates class used to save window size and position
    class coordinates
    {
        [XmlElement("x")]
        public double x { get; set; }
        [XmlElement("y")]
        public double y { get; set; }
    }
}
