using Game_Tracker_1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        private Settings settings;
        public ObservableCollection<videoGame> game_list;
        private int datagrid_selected_index = -1;
        private string xml_file_path = "";

        public MainWindow()
        {
            InitializeComponent();

            settings = new Settings();
            game_list = new ObservableCollection<videoGame>();
            game_list = get_game_list();
            datagrid_games.ItemsSource = game_list;
        }

        private ObservableCollection<videoGame> get_game_list()
        {
            ObservableCollection<videoGame> list = new ObservableCollection<videoGame>();

            if (File.Exists(xml_file_path))
            {
                XmlRootAttribute xmlRoot = new XmlRootAttribute();
                xmlRoot.ElementName = "list";
                xmlRoot.IsNullable = true;

                XmlSerializer serializer = new XmlSerializer(typeof(ObservableCollection<videoGame>), xmlRoot);
                XmlReader reader = XmlReader.Create(xml_file_path);
                
                list = (ObservableCollection<videoGame>)serializer.Deserialize(reader);
            }

            return list;
        }

        private void print_game_names_to_console()
        {
            foreach (videoGame game in game_list)
            {
                Debug.WriteLine(game.name);
            }
        }

        private void load_xml_files()
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(@"xml\", "*.xml"))
                {
                    combobox_year.Items.Add(file);
                }
            }
            catch (Exception)
            {

            }
            
        }

/*        private void add_games_to_datagrid()
        {
            foreach (videoGame game in game_list)
            {
                datagrid_games.Items.Add(game);
            }
        }
*/
        private void button_new_game_Click(object sender, RoutedEventArgs e)
        {
            game_list.Add(new videoGame());
            datagrid_games.ItemsSource = null;
            datagrid_games.ItemsSource = game_list;
            datagrid_games.Items.Refresh();
            datagrid_games.SelectedIndex = datagrid_games.Items.Count - 1;
        }

        private void button_save_changes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                videoGame selected_game = (videoGame)datagrid_games.SelectedItem;
                selected_game.name = textbox_name.Text;
                selected_game.beaten = combobox_beaten.Text;
                selected_game.wantToBeat = combobox_want_to_beat.Text;
                selected_game.startDate = datepicker_start_date.DisplayDate;
                selected_game.startDateString = datepicker_start_date.SelectedDate.Value.Date.ToShortDateString();
                if (combobox_beaten.SelectedIndex == 0)
                {
                    selected_game.endDate = datepicker_end_date.DisplayDate;
                    selected_game.endDateString = datepicker_end_date.SelectedDate.Value.Date.ToShortDateString();

                    selected_game.hours = Convert.ToDouble(numberbox_hours_played.Text);
                    selected_game.hoursString = numberbox_hours_played.Text;
                }
                else
                {
                    selected_game.endDateString = "";
                    selected_game.endDate = DateTime.Now;
                    selected_game.hoursString = "NA";
                    selected_game.hours = 0;
                }

                save_games();
                datagrid_games.Items.Refresh();
            }
            catch (Exception)
            {
                Console.WriteLine("Could not save changes. Is a game selected?");
            }
            
        }

        private void button_delete_selected_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_games.SelectedItems.Count < 1)
            {
                return;
            }
            game_list.Remove((videoGame) datagrid_games.SelectedItem);
            datagrid_games.Items.Refresh();
        }

        private void Game_Tracker_Closed(object sender, EventArgs e)
        {
            
        }

        private void save_settings()
        {
            settings.window_size.x = Application.Current.MainWindow.Width;
            settings.window_size.y = Application.Current.MainWindow.Height;

            settings.window_position.x = Application.Current.MainWindow.Left;
            settings.window_position.y = Application.Current.MainWindow.Top;

            settings.window_state = Application.Current.MainWindow.WindowState;

            settings.xml_file_path = xml_file_path;

            settings.sort_setting = combobox_sort.Text;

            settings.save_settings();
        }

        private void load_settings()
        {
            try
            {
                Application.Current.MainWindow.Width = settings.window_size.x;
                Application.Current.MainWindow.Height = settings.window_size.y;

                Application.Current.MainWindow.Left = settings.window_position.x;
                Application.Current.MainWindow.Top = settings.window_position.y;

                Application.Current.MainWindow.WindowState = settings.window_state;

                xml_file_path = settings.xml_file_path;
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(xml_file_path);
            }
            catch (Exception e)
            {

            }
            
        }

        private void Game_Tracker_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            save_games();
            save_settings();
        }

        private void save_games()
        {
            if(xml_file_path == "" || datagrid_games.Items.Count < 1)
            {
                return;
            }
            if (File.Exists(xml_file_path))
            {
                File.Delete(xml_file_path);
            }

            XDocument doc = new XDocument(new XElement("list"));

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
                        new XElement("hours_string", game.hoursString)
                    )
                );
            }
            try
            {
                doc.Save(xml_file_path);
            }
            catch (Exception)
            {
                //Couldn't save file
            }
            
        }

        private void Game_Tracker_Loaded(object sender, RoutedEventArgs e)
        {
            load_xml_files();
            load_settings();
            
            foreach(ComboBoxItem item in combobox_sort.Items)
            {
                if(item.Content.ToString() == settings.sort_setting)
                {
                    combobox_sort.SelectedIndex = combobox_sort.Items.IndexOf(item);
                    break;
                }
            }
            //combobox_sort.SelectedIndex = combobox_sort.Items.IndexOf(settings.sort_setting);
        }

        private void datagrid_games_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(datagrid_games.SelectedItems.Count < 1)
            {
                return;
            }

            textbox_name.IsEnabled = true;
            combobox_beaten.IsEnabled = true;
            combobox_want_to_beat.IsEnabled = true;
            datepicker_start_date.IsEnabled = true;

            datagrid_selected_index = datagrid_games.SelectedIndex;

            videoGame current_game = (videoGame) datagrid_games.SelectedItem;
            textbox_name.Text = current_game.name;

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

            datepicker_start_date.SelectedDate = current_game.startDate;
            if(current_game.beaten != "Yes")
            {
                datepicker_end_date.IsEnabled = false;
                datepicker_end_date.SelectedDate = DateTime.Now;
                numberbox_hours_played.IsEnabled = false;
                numberbox_hours_played.Text = "0";
            }
            else
            {
                datepicker_end_date.IsEnabled = true;
                datepicker_end_date.SelectedDate = current_game.endDate;

                numberbox_hours_played.IsEnabled = true;
                numberbox_hours_played.Text = Convert.ToString(current_game.hours);
            }
        }

        private void combobox_year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            save_games();

            xml_file_path = (string) combobox_year.SelectedValue;
            game_list = get_game_list();

            sort();
            datagrid_games.Items.Refresh();
        }

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

        private void button_new_file_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(@"xml\" + DateTime.Now.Year.ToString() + ".xml"))
            {
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
            }
            else
            {
                combobox_year.Items.Add(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
                combobox_year.SelectedIndex = combobox_year.Items.IndexOf(@"xml\" + DateTime.Now.Year.ToString() + ".xml");
            }
        }

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

        private void datagrid_games_Loaded(object sender, RoutedEventArgs e)
        {
            //sort();
        }

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
