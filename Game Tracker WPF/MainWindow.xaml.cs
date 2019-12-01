using Game_Tracker_1;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace GameTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //settings object
        private Settings settings;
        //list of games
        public ObservableCollection<videoGame> game_list;
        //file path
        private string xml_file_path = "";

        public MainWindow()
        {
            InitializeComponent();

            //create a new settings and game_list object
            settings = new Settings();
            game_list = new ObservableCollection<videoGame>();
            //fill game list from file
            game_list = get_game_list();
            //set datagrid source to the new list;
            datagrid_games.ItemsSource = game_list;
        }

        //retrieve games from xml file
        private ObservableCollection<videoGame> get_game_list()
        {
            ObservableCollection<videoGame> list = new ObservableCollection<videoGame>();

            //check if file exists
            if (File.Exists(xml_file_path))
            {
                //create new root node with name "list"
                XmlRootAttribute xmlRoot = new XmlRootAttribute();
                xmlRoot.ElementName = "list";
                xmlRoot.IsNullable = true;

                //create XmlSerializer to read into ObservableCollection with a root named "list"
                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<videoGame>), xmlRoot);
                //create an XmlReader to read the xml file
                XmlReader reader = XmlReader.Create(xml_file_path);
                
                //deserialize xml file in XmlReader to an ObservableCollection
                list = (ObservableCollection<videoGame>)serializer.Deserialize(reader);

                foreach(videoGame game in list)
                {
                    game.update_image();
                }
            }

            //return the new ObservableCollection
            return list;
        }

        //load files found in xml folder into the combobox
        private void load_xml_files()
        {
            try
            {
                //gather file paths for each file in xml folder with extension .xml
                foreach (string file in Directory.EnumerateFiles(@"xml\", "*.xml"))
                {
                    combobox_year.Items.Add(file);
                }
            }
            catch (Exception)
            {
                //do nothing
            }
            
        }

        //create new game in datagrid when button is pressed
        private void button_new_game_Click(object sender, RoutedEventArgs e)
        {
            //add game to list
            game_list.Add(new videoGame());
            //reset the datagrid itemssource to game_list and refresh
            datagrid_games.ItemsSource = null;
            datagrid_games.ItemsSource = game_list;
            datagrid_games.Items.Refresh();
            //change datagrid selected index to the new game
            datagrid_games.SelectedIndex = datagrid_games.Items.Count - 1;
        }

        //save changes made to game when button is pressed
        private void button_save_changes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //get game object selected in datagrid
                videoGame selected_game = (videoGame)datagrid_games.SelectedItem;
                //save values from gui into the object
                selected_game.name = textbox_name.Text;
                selected_game.beaten = combobox_beaten.Text;
                selected_game.wantToBeat = combobox_want_to_beat.Text;
                selected_game.startDate = datepicker_start_date.DisplayDate;
                selected_game.startDateString = datepicker_start_date.SelectedDate.Value.Date.ToShortDateString();
                //if the game is beaten the end date and hours should be updated
                if (combobox_beaten.SelectedIndex == 0)
                {
                    selected_game.endDate = datepicker_end_date.DisplayDate;
                    selected_game.endDateString = datepicker_end_date.SelectedDate.Value.Date.ToShortDateString();

                    selected_game.hours = Convert.ToDouble(numberbox_hours_played.Text);
                    selected_game.hoursString = numberbox_hours_played.Text;
                }
                else
                {
                    //if the game isn't beaten the end date displays as "NA" and hours is set to 0 and shown as "NA"
                    selected_game.endDateString = "";
                    selected_game.endDate = DateTime.Now;
                    selected_game.hoursString = "NA";
                    selected_game.hours = 0;
                }

                selected_game.image_link = texbox_image_url.Text;
                selected_game.update_image();

                //save changes to file
                save_games();
                //refresh datagrid to reflect changes
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {
                //do nothing
            }
            
        }

        //delete selected item when pressed
        private void button_delete_selected_Click(object sender, RoutedEventArgs e)
        {
            //return if nothing is selected
            if (datagrid_games.SelectedItems.Count < 1)
            {
                return;
            }
            //remove selected game from list and refresh datagrid
            game_list.Remove((videoGame) datagrid_games.SelectedItem);
            datagrid_games.Items.Refresh();
        }

        //save settings to xml file
        private void save_settings()
        {
            //record current settings and save
            settings.window_size.x = Application.Current.MainWindow.Width;
            settings.window_size.y = Application.Current.MainWindow.Height;

            settings.window_position.x = Application.Current.MainWindow.Left;
            settings.window_position.y = Application.Current.MainWindow.Top;

            settings.window_state = Application.Current.MainWindow.WindowState;

            settings.xml_file_path = xml_file_path;

            settings.sort_setting = combobox_sort.Text;

            settings.save_settings();
        }

        //load settings from xml file
        private void load_settings()
        {
            try
            {
                //load settings found in settings object to current window
                Application.Current.MainWindow.Width = settings.window_size.x;
                Application.Current.MainWindow.Height = settings.window_size.y;

                Application.Current.MainWindow.Left = settings.window_position.x;
                Application.Current.MainWindow.Top = settings.window_position.y;

                Application.Current.MainWindow.WindowState = settings.window_state;

                //load last opened file
                xml_file_path = settings.xml_file_path;
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(xml_file_path);
            }
            catch (Exception)
            {
                //do nothing
            }
            
        }

        //save games and settings to xml when window is closing
        private void Game_Tracker_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            save_games();
            save_settings();
        }

        //save games to xml file
        private void save_games()
        {
            //if file isn't set or no games are found in list return
            if(xml_file_path == "" || datagrid_games.Items.Count < 1)
            {
                return;
            }
            //if the file already exists delete it
            if (File.Exists(xml_file_path))
            {
                File.Delete(xml_file_path);
            }

            //create new XDocument with "list" node as root
            XDocument doc = new XDocument(new XElement("list"));

            //add each game to the XDocument as a child of "list" node
            foreach (videoGame game in game_list)
            {
                doc.Descendants("list").Last().Add(
                    new XElement("videoGame",
                        new XElement("name", game.name),
                        new XElement("beaten", game.beaten),
                        new XElement("want_to_beat", game.wantToBeat),
                        new XElement("start_date", game.startDate),
                        new XElement("start_date_string", game.startDateString),
                        new XElement("end_date", game.endDate),
                        new XElement("end_date_string", game.endDateString),
                        new XElement("hours", game.hours),
                        new XElement("hours_string", game.hoursString),
                        new XElement("image_link", game.image_link)
                    )
                );
            }
            try
            {
                //save new xml to file
                doc.Save(xml_file_path);
            }
            catch (Exception)
            {
                //Couldn't save file
            }
            
        }

        //load found files and settings when application is opened
        private void Game_Tracker_Loaded(object sender, RoutedEventArgs e)
        {
            //load found files
            load_xml_files();
            //load settings into application
            load_settings();
            
            //select sort setting in combobox found in settings file
            foreach(ComboBoxItem item in combobox_sort.Items)
            {
                if(item.Content.ToString() == settings.sort_setting)
                {
                    combobox_sort.SelectedIndex = combobox_sort.Items.IndexOf(item);
                    break;
                }
            }
        }

        //fill gui with data of selected game
        private void datagrid_games_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //return if nothing is selected
            if(datagrid_games.SelectedItems.Count < 1)
            {
                return;
            }
            
            //re-enable gui elements
            textbox_name.IsEnabled = true;
            combobox_beaten.IsEnabled = true;
            combobox_want_to_beat.IsEnabled = true;
            datepicker_start_date.IsEnabled = true;
            texbox_image_url.IsEnabled = true;

            //selected game in datagrid
            videoGame current_game = (videoGame) datagrid_games.SelectedItem;
            //update textbox name
            textbox_name.Text = current_game.name;

            //change combobox_beaten selection
            switch (current_game.beaten)
            {
                case "Yes":
                    combobox_beaten.SelectedIndex = 0;
                    break;
                case "No":
                    combobox_beaten.SelectedIndex = 1;
                    break;
                case "Currently Playing":
                    combobox_beaten.SelectedIndex = 2;
                    break;
                case "Dropped":
                    combobox_beaten.SelectedIndex = 3;
                    break;
                default:
                    break;
            }

            //change combobox_want_to_beat selection
            switch (current_game.wantToBeat)
            {
                case "Yes":
                    combobox_want_to_beat.SelectedIndex = 0;
                    break;
                case "No":
                    combobox_want_to_beat.SelectedIndex = 1;
                    break;
                default:
                    break;
            }

            //update start date datepicker
            datepicker_start_date.SelectedDate = current_game.startDate;
            //disable end date datepicker and hours numberbox if game isn't beaten
            if(current_game.beaten != "Yes")
            {
                datepicker_end_date.IsEnabled = false;
                datepicker_end_date.SelectedDate = DateTime.Now;
                numberbox_hours_played.IsEnabled = false;
                numberbox_hours_played.Text = "0";
            }
            else
            {
                //if game is beaten re-enable end date datepicker and hours numberbox and set values
                datepicker_end_date.IsEnabled = true;
                datepicker_end_date.SelectedDate = current_game.endDate;

                numberbox_hours_played.IsEnabled = true;
                numberbox_hours_played.Text = Convert.ToString(current_game.hours);
            }

            texbox_image_url.Text = current_game.image_link;
        }

        //if selected file is changed, save current file and open new file
        private void combobox_year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //save current file
            save_games();

            //update file path to new file
            xml_file_path = (string) combobox_year.SelectedValue;
            //get new list of games
            game_list = get_game_list();

            //sort based on current selection
            sort();
            //refresh datagrid
            datagrid_games.Items.Refresh();
        }

        //sort datagrid using various options
        private void sort_alphabetically_ascending()
        {
            game_list = new ObservableCollection<videoGame>(game_list.OrderBy(i => i.name));
            datagrid_games.ItemsSource = null;
            datagrid_games.ItemsSource = game_list;
            datagrid_games.Items.Refresh();
        }

        private void sort_alphabetically_descending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderByDescending(i => i.name));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_start_date_ascending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderBy(i => i.startDate));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_start_date_descending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderByDescending(i => i.startDate));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_end_date_ascending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderBy(i => i.endDate));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_end_date_descending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderByDescending(i => i.endDate));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_hours_ascending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderBy(i => i.hours));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_hours_descending()
        {
            try
            {
                game_list = new ObservableCollection<videoGame>(game_list.OrderByDescending(i => i.hours));
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        //if combobox_sort is changed sort by appropiate selection
        private void combobox_sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (combobox_sort.SelectedIndex)
            {
                case 0:
                    sort_alphabetically_descending();
                    break;
                case 1:
                    sort_alphabetically_ascending();
                    break;
                case 2:
                    sort_start_date_descending();
                    break;
                case 3:
                    sort_start_date_ascending();
                    break;
                case 4:
                    sort_end_date_descending();
                    break;
                case 5:
                    sort_end_date_ascending();
                    break;
                case 6:
                    sort_hours_descending();
                    break;
                case 7:
                    sort_hours_ascending();
                    break;
                default:
                    break;
            }
        }

        //if new file button is pressed create new file using the year if it doesn't already exist, otherwise load that file
        private void button_new_file_Click(object sender, RoutedEventArgs e)
        {
            //if file already exists
            if (File.Exists(@"xml\" + DateTime.Now.Year.ToString() + ".xml"))
            {
                //change selection to current year's file
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
            }
            else
            {
                //else create new file for the year
                combobox_year.Items.Add(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
            }
        }

        //re-enable or disable end date datepicker based on whether or not the game is beaten
        private void combobox_beaten_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (combobox_beaten.SelectedIndex)
            {
                case 0:
                    datepicker_end_date.IsEnabled = true;
                    break;
                default:
                    datepicker_end_date.IsEnabled = false;
                    break;
            }
        }

        //sort based on combobox_sort selection
        private void sort()
        {
            switch (combobox_sort.SelectedIndex)
            {
                case 0:
                    sort_alphabetically_descending();
                    break;
                case 1:
                    sort_alphabetically_ascending();
                    break;
                case 2:
                    sort_start_date_descending();
                    break;
                case 3:
                    sort_start_date_ascending();
                    break;
                case 4:
                    sort_end_date_descending();
                    break;
                case 5:
                    sort_end_date_ascending();
                    break;
                case 6:
                    sort_hours_descending();
                    break;
                case 7:
                    sort_hours_ascending();
                    break;
                default:
                    break;
            }
        }
    }


}
