using Game_Tracker_1;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
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

            update_status_bar();
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

            update_status_bar();
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

                    //display 0 hours as NA
                    if(numberbox_hours_played.Text != "0")
                    {
                        selected_game.hoursString = numberbox_hours_played.Text;
                    }
                    else
                    {
                        selected_game.hoursString = "NA";
                    }
                    
                }
                else
                {
                    //if the game isn't beaten the end date displays as "NA" and hours is set to 0 and shown as "NA"
                    selected_game.endDateString = "";
                    selected_game.endDate = DateTime.Now;
                    selected_game.hoursString = "NA";
                    selected_game.hours = 0;
                }

                selected_game.image_link = @"images\" + selected_game.name + ".png";
                try
                {
                    SaveImage(texbox_image_url.Text, @"images\" + selected_game.name + ".png", ImageFormat.Png);
                }
                catch (Exception)
                {

                }
                selected_game.update_image();

                if(checkbox_rating_type.IsChecked == false)
                {
                    selected_game.rating_type = false;
                    switch (combobox_rating_numbers.SelectedIndex)
                    {
                        case 0:
                            selected_game.rating = "0";
                            break;
                        case 1:
                            selected_game.rating = "1";
                            break;
                        case 2:
                            selected_game.rating = "2";
                            break;
                        case 3:
                            selected_game.rating = "3";
                            break;
                        case 4:
                            selected_game.rating = "4";
                            break;
                        case 5:
                            selected_game.rating = "5";
                            break;
                        case 6:
                            selected_game.rating = "6";
                            break;
                        case 7:
                            selected_game.rating = "7";
                            break;
                        case 8:
                            selected_game.rating = "8";
                            break;
                        case 9:
                            selected_game.rating = "9";
                            break;
                        case 10:
                            selected_game.rating = "10";
                            break;
                        default:
                            selected_game.rating = "NA";
                            break;
                    }
                }
                else
                {
                    selected_game.rating_type = true;
                    switch (combobox_rating_letters.SelectedIndex)
                    {
                        case 0:
                            selected_game.rating = "F";
                            break;
                        case 1:
                            selected_game.rating = "D";
                            break;
                        case 2:
                            selected_game.rating = "C";
                            break;
                        case 3:
                            selected_game.rating = "B";
                            break;
                        case 4:
                            selected_game.rating = "A";
                            break;
                        default:
                            selected_game.rating = "NA";
                            break;
                    }
                }
                //save changes to file
                save_games();
                //refresh datagrid to reflect changes
                datagrid_games.Items.Refresh();

                update_status_bar();
            }
            catch (Exception)
            {
                //do nothing
            }
            
        }

        //retrieved from https://stackoverflow.com/questions/24797485/how-to-download-image-from-url
        public void SaveImage(string imageUrl, string filename, ImageFormat format)
        {
            WebClient client = new WebClient();
            Stream stream = client.OpenRead(imageUrl);
            Bitmap bitmap; bitmap = new Bitmap(stream);

            if (bitmap != null)
            {
                bitmap.Save(filename, format);
            }

            stream.Flush();
            stream.Close();
            client.Dispose();
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

            update_status_bar();
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

            settings.image_column_visibility = checkbox_image.IsChecked ?? true;
            settings.name_column_visibility = checkbox_name.IsChecked ?? true;
            settings.beaten_column_visibility = checkbox_beaten.IsChecked ?? true;
            settings.wanttobeat_column_visibility = checkbox_want_to_beat.IsChecked ?? true;
            settings.startdate_column_visibility = checkbox_start_date.IsChecked ?? true;
            settings.enddate_column_visibility = checkbox_end_date.IsChecked ?? true;
            settings.hoursplayed_column_visibility = checkbox_hours_played.IsChecked ?? true;
            settings.rating_column_visibility = checkbox_rating.IsChecked ?? true;

            settings.header_color = color_picker_header.SelectedColorText;
            settings.table_color = color_picker_table.SelectedColorText;
            settings.text_color = color_picker_text.SelectedColorText;

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

                //load last opened file
                xml_file_path = settings.xml_file_path;
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(xml_file_path);

                checkbox_image.IsChecked = settings.image_column_visibility;
                checkbox_name.IsChecked = settings.name_column_visibility;
                checkbox_beaten.IsChecked = settings.beaten_column_visibility;
                checkbox_want_to_beat.IsChecked = settings.wanttobeat_column_visibility;
                checkbox_start_date.IsChecked = settings.startdate_column_visibility;
                checkbox_end_date.IsChecked = settings.enddate_column_visibility;
                checkbox_hours_played.IsChecked = settings.hoursplayed_column_visibility;
                checkbox_rating.IsChecked = settings.rating_column_visibility;

                System.Windows.Media.Color header_color;
                System.Windows.Media.Color table_color;
                System.Windows.Media.Color text_color;
                try
                {
                    
                    header_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.header_color);
                    table_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.table_color);
                    text_color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(settings.text_color);

                    color_picker_header.SelectedColor = header_color;
                    color_picker_table.SelectedColor = table_color;
                    color_picker_text.SelectedColor = text_color;
                }
                catch (Exception)
                {

                }
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
                        new XElement("image_link", game.image_link),
                        new XElement("rating", game.rating),
                        new XElement("rating_type", game.rating_type)
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

            disable_gui_elements();
            
            //select sort setting in combobox found in settings file
            foreach(ComboBoxItem item in combobox_sort.Items)
            {
                if(item.Content.ToString() == settings.sort_setting)
                {
                    combobox_sort.SelectedIndex = combobox_sort.Items.IndexOf(item);
                    break;
                }
            }

            update_status_bar();

            switch (settings.window_state)
            {
                case WindowState.Maximized:
                    Game_Tracker.WindowState = WindowState.Maximized;
                    break;
                default:
                    Game_Tracker.WindowState = WindowState.Normal;
                    break;
            }
        }

        private void enable_gui_elements()
        {
            textbox_name.IsEnabled = true;
            combobox_beaten.IsEnabled = true;
            combobox_want_to_beat.IsEnabled = true;
            datepicker_start_date.IsEnabled = true;
            texbox_image_url.IsEnabled = true;
            combobox_rating_letters.IsEnabled = true;
            combobox_rating_numbers.IsEnabled = true;
            checkbox_rating_type.IsEnabled = true;
        }

        private void disable_gui_elements()
        {
            textbox_name.IsEnabled = false;
            combobox_beaten.IsEnabled = false;
            combobox_want_to_beat.IsEnabled = false;
            datepicker_start_date.IsEnabled = false;
            texbox_image_url.IsEnabled = false;
            combobox_rating_letters.IsEnabled = false;
            combobox_rating_numbers.IsEnabled = false;
            checkbox_rating_type.IsEnabled = false;
        }

        //fill gui with data of selected game
        private void datagrid_games_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //return if nothing is selected
            if(datagrid_games.SelectedItems.Count < 1)
            {
                disable_gui_elements();
                return;
            }

            //re-enable gui elements
            enable_gui_elements();

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
            datepicker_start_date.DisplayDate = current_game.startDate;
            //disable end date datepicker and hours numberbox if game isn't beaten
            if(current_game.beaten != "Yes")
            {
                datepicker_end_date.IsEnabled = false;
                datepicker_end_date.SelectedDate = DateTime.Now;
                datepicker_end_date.DisplayDate = DateTime.Now;
                numberbox_hours_played.IsEnabled = false;
                numberbox_hours_played.Text = "0";
            }
            else
            {
                //if game is beaten re-enable end date datepicker and hours numberbox and set values
                datepicker_end_date.IsEnabled = true;
                datepicker_end_date.SelectedDate = current_game.endDate;
                datepicker_end_date.DisplayDate = current_game.endDate;

                numberbox_hours_played.IsEnabled = true;
                numberbox_hours_played.Text = Convert.ToString(current_game.hours);
            }

            texbox_image_url.Text = current_game.image_link;

            if (!current_game.rating_type)
            {
                checkbox_rating_type.IsChecked = false;
                combobox_rating_letters.Visibility = Visibility.Collapsed;
                combobox_rating_numbers.Visibility = Visibility.Visible;
                
                try
                {
                    int rating = int.Parse(current_game.rating);
                    switch (rating)
                    {
                        case 0:
                            combobox_rating_numbers.SelectedIndex = 0;
                            break;
                        case 1:
                            combobox_rating_numbers.SelectedIndex = 1;
                            break;
                        case 2:
                            combobox_rating_numbers.SelectedIndex = 2;
                            break;
                        case 3:
                            combobox_rating_numbers.SelectedIndex = 3;
                            break;
                        case 4:
                            combobox_rating_numbers.SelectedIndex = 4;
                            break;
                        case 5:
                            combobox_rating_numbers.SelectedIndex = 5;
                            break;
                        case 6:
                            combobox_rating_numbers.SelectedIndex = 6;
                            break;
                        case 7:
                            combobox_rating_numbers.SelectedIndex = 7;
                            break;
                        case 8:
                            combobox_rating_numbers.SelectedIndex = 8;
                            break;
                        case 9:
                            combobox_rating_numbers.SelectedIndex = 9;
                            break;
                        case 10:
                            combobox_rating_numbers.SelectedIndex = 10;
                            break;
                        default:
                            combobox_rating_numbers.SelectedIndex = 11;
                            break;
                    }
                }
                catch (Exception)
                {
                    combobox_rating_numbers.SelectedIndex = 11;
                }
                combobox_rating_letters.SelectedIndex = 5;
            }
            else
            {
                checkbox_rating_type.IsChecked = true;
                combobox_rating_numbers.Visibility = Visibility.Collapsed;
                combobox_rating_letters.Visibility = Visibility.Visible;
                switch (current_game.rating)
                {
                    case "F":
                        combobox_rating_letters.SelectedIndex = 0;
                        break;
                    case "D":
                        combobox_rating_letters.SelectedIndex = 1;
                        break;
                    case "C":
                        combobox_rating_letters.SelectedIndex = 2;
                        break;
                    case "B":
                        combobox_rating_letters.SelectedIndex = 3;
                        break;
                    case "A":
                        combobox_rating_letters.SelectedIndex = 4;
                        break;
                    default:
                        combobox_rating_letters.SelectedIndex = 5;
                        break;
                }
                combobox_rating_numbers.SelectedIndex = 11;
            }
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

            update_status_bar();
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

        private void sort_rating_ascending()
        {
            try
            {
                ObservableCollection<videoGame> zero_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> one_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> two_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> three_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> four_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> five_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> six_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> seven_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> eight_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> nine_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> ten_collection = new ObservableCollection<videoGame>();

                ObservableCollection<videoGame> F_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> D_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> C_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> B_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> A_collection = new ObservableCollection<videoGame>();

                ObservableCollection<videoGame> NA_collection = new ObservableCollection<videoGame>();
                foreach (videoGame game in game_list)
                {
                    if(game.rating_type == false && game.rating != "NA")
                    {
                        switch (Int32.Parse(game.rating))
                        {
                            case 0:
                                zero_collection.Add(game);
                                break;
                            case 1:
                                one_collection.Add(game);
                                break;
                            case 2:
                                two_collection.Add(game);
                                break;
                            case 3:
                                three_collection.Add(game);
                                break;
                            case 4:
                                four_collection.Add(game);
                                break;
                            case 5:
                                five_collection.Add(game);
                                break;
                            case 6:
                                six_collection.Add(game);
                                break;
                            case 7:
                                seven_collection.Add(game);
                                break;
                            case 8:
                                eight_collection.Add(game);
                                break;
                            case 9:
                                nine_collection.Add(game);
                                break;
                            case 10:
                                ten_collection.Add(game);
                                break;
                            default:
                                NA_collection.Add(game);
                                break;
                        }
                    }
                    else
                    {
                        switch (game.rating)
                        {
                            case "F":
                                F_collection.Add(game);
                                break;
                            case "D":
                                D_collection.Add(game);
                                break;
                            case "C":
                                C_collection.Add(game);
                                break;
                            case "B":
                                B_collection.Add(game);
                                break;
                            case "A":
                                A_collection.Add(game);
                                break;
                            default:
                                NA_collection.Add(game);
                                break;
                        }
                    }
                }

                ObservableCollection<videoGame> sorted_list = new ObservableCollection<videoGame>();
                foreach(videoGame game in zero_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in one_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in two_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in three_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in four_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in five_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in six_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in seven_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in eight_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in nine_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in ten_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in F_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in D_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in C_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in B_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in A_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in NA_collection)
                {
                    sorted_list.Add(game);
                }

                game_list = sorted_list;
                datagrid_games.ItemsSource = null;
                datagrid_games.ItemsSource = game_list;
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {

            }
        }

        private void sort_rating_descending()
        {
            try
            {
                ObservableCollection<videoGame> zero_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> one_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> two_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> three_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> four_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> five_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> six_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> seven_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> eight_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> nine_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> ten_collection = new ObservableCollection<videoGame>();

                ObservableCollection<videoGame> F_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> D_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> C_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> B_collection = new ObservableCollection<videoGame>();
                ObservableCollection<videoGame> A_collection = new ObservableCollection<videoGame>();

                ObservableCollection<videoGame> NA_collection = new ObservableCollection<videoGame>();
                foreach (videoGame game in game_list)
                {
                    if (game.rating_type == false && game.rating != "NA")
                    {
                        switch (Int32.Parse(game.rating))
                        {
                            case 0:
                                zero_collection.Add(game);
                                break;
                            case 1:
                                one_collection.Add(game);
                                break;
                            case 2:
                                two_collection.Add(game);
                                break;
                            case 3:
                                three_collection.Add(game);
                                break;
                            case 4:
                                four_collection.Add(game);
                                break;
                            case 5:
                                five_collection.Add(game);
                                break;
                            case 6:
                                six_collection.Add(game);
                                break;
                            case 7:
                                seven_collection.Add(game);
                                break;
                            case 8:
                                eight_collection.Add(game);
                                break;
                            case 9:
                                nine_collection.Add(game);
                                break;
                            case 10:
                                ten_collection.Add(game);
                                break;
                            default:
                                NA_collection.Add(game);
                                break;
                        }
                    }
                    else
                    {
                        switch (game.rating)
                        {
                            case "F":
                                F_collection.Add(game);
                                break;
                            case "D":
                                D_collection.Add(game);
                                break;
                            case "C":
                                C_collection.Add(game);
                                break;
                            case "B":
                                B_collection.Add(game);
                                break;
                            case "A":
                                A_collection.Add(game);
                                break;
                            default:
                                NA_collection.Add(game);
                                break;
                        }
                    }
                }

                ObservableCollection<videoGame> sorted_list = new ObservableCollection<videoGame>();
                foreach (videoGame game in A_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in B_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in C_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in D_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in F_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in ten_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in nine_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in eight_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in seven_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in six_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in five_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in four_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in three_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in two_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in one_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in zero_collection)
                {
                    sorted_list.Add(game);
                }
                foreach (videoGame game in NA_collection)
                {
                    sorted_list.Add(game);
                }

                game_list = sorted_list;
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
            sort();
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
                    numberbox_hours_played.IsEnabled = true;
                    break;
                default:
                    datepicker_end_date.IsEnabled = false;
                    numberbox_hours_played.IsEnabled = false;
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
                case 8:
                    sort_rating_descending();
                    break;
                case 9:
                    sort_rating_ascending();
                    break;
                default:
                    break;
            }
        }

        private void checkbox_rating_type_Unchecked(object sender, RoutedEventArgs e)
        {
            combobox_rating_letters.Visibility = Visibility.Collapsed;
            combobox_rating_numbers.Visibility = Visibility.Visible;
        }

        private void checkbox_rating_type_Checked(object sender, RoutedEventArgs e)
        {
            combobox_rating_numbers.Visibility = Visibility.Collapsed;
            combobox_rating_letters.Visibility = Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            popup_customize.IsOpen = true;
        }

        private void button_default_Click(object sender, RoutedEventArgs e)
        {
            ColorConverter converter = new ColorConverter();

            color_picker_header.SelectedColor = System.Windows.Media.Color.FromArgb(255, 187, 226, 255);
            color_picker_table.SelectedColor = System.Windows.Media.Color.FromRgb(255, 255, 255);
            color_picker_text.SelectedColor = System.Windows.Media.Color.FromRgb(0,0,0);
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            popup_customize.IsOpen = false;
        }

        private void update_status_bar()
        {
            int total = 0;
            double total_hours = 0;
            int beaten = 0;
            int not_beaten = 0;
            int currently_playing = 0;
            int dropped = 0;

            foreach(videoGame videoGame in game_list)
            {
                total += 1;
                total_hours += videoGame.hours;
                switch (videoGame.beaten)
                {
                    case "Yes":
                        beaten += 1;
                        break;
                    case "No":
                        not_beaten += 1;
                        break;
                    case "Currently Playing":
                        currently_playing += 1;
                        break;
                    case "Dropped":
                        dropped += 1;
                        break;
                    default:
                        break;
                }
            }

            textblock_total.Text = total.ToString();
            textblock_totalhours.Text = total_hours.ToString();
            textblock_beaten.Text = beaten.ToString();
            textblock_notbeaten.Text = not_beaten.ToString();
            textblock_currentlyplaying.Text = currently_playing.ToString();
            textblock_dropped.Text = dropped.ToString();
        }
    }


}
