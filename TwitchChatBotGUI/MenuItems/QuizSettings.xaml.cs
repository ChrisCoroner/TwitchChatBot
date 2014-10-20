using System;
using System.IO;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Microsoft.TeamFoundation.MVVM;
using TwitchChatBot;


namespace TwitchChatBotGUI.MenuItems
{




    /// <summary>
    /// Interaction logic for QuizSettings.xaml
    /// </summary>
    public partial class QuizSettings : System.Windows.Controls.UserControl, ITwitchMenuItem, IDataErrorInfo
    {
        public string Error
        {
            get
            {
                return null;
            }
        }

        public string this[string name]
        {
            get
            {
                string result = null;

                if (name == "mTimeBetweenQuestions")
                {

                    if (tempTimeBetweenQuestions < 0)
                    {
                        result = "The value cannot be negative!";
                        tempTimeBetweenQuestions = Bot.TimeBetweenQuestions;
                    }
                }

                if (name == "mTimeBetweenHints")
                {
                    if (tempTimeBetweenHints < 0)
                    {
                        result = "The value cannot be negative!";
                    }
                }
                return result;
            }
        }

        public int mTimeBetweenQuestions
        {
            get
            {
                return tempTimeBetweenQuestions;
            }
            set
            {
                tempTimeBetweenQuestions = value;
            }
        }

        public int mTimeBetweenHints
        {
            get
            {
                return tempTimeBetweenHints;
            }
            set
            {
                tempTimeBetweenHints = value;
            }
        }

        int tempTimeBetweenQuestions;
        int tempTimeBetweenHints;
        

        private void QuizSettings_Loaded(object sender, RoutedEventArgs e)
        {
            tempTimeBetweenQuestions = Bot.TimeBetweenQuestions;
            tempTimeBetweenHints = Bot.TimeBetweenHints;

            TimeBetweenQuestions.Text = String.Format("{0}", tempTimeBetweenQuestions);
            TimeBetweenHints.Text = String.Format("{0}", tempTimeBetweenHints);

            if (tempPath != null)
            {
                QuizFile.Text = tempPath;
            }
            else
            {
                QuizFile.Text = Bot.QuizFile;
            }
            RandomizeSwitch.IsChecked = Bot.IsRandom;
            QuizWindowName.DataContext = (QuizSettings)this;
        }

        public QuizSettings(TwitchBot inBot, Popup inCurrentPopup)
        {
            this.Loaded += QuizSettings_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            
            InitializeComponent();
        }

        async private void AcceptClick(object sender, RoutedEventArgs e)
        {
            //int ParsedTimeBetweenQuestions;
            //bool resultOfParseTimeBetweenQuestions = Int32.TryParse(TimeBetweenQuestions.Text, out ParsedTimeBetweenQuestions);
            //if (resultOfParseTimeBetweenQuestions)
            //{
            //    Bot.TimeBetweenQuestions = ParsedTimeBetweenQuestions;
            //}

            //int ParsedTimeBetweenHints;
            //bool resultOfParseTimeBetweenHints = Int32.TryParse(TimeBetweenHints.Text, out ParsedTimeBetweenHints);
            //if (resultOfParseTimeBetweenHints)
            //{
            //    Bot.TimeBetweenHints = Int32.Parse(TimeBetweenHints.Text);
            //}

            Bot.TimeBetweenQuestions = tempTimeBetweenQuestions;
            Bot.TimeBetweenHints = tempTimeBetweenHints;
            try
            {
                if (!String.IsNullOrEmpty(QuizFile.Text) && (Bot.QuizFile != QuizFile.Text) && File.Exists(QuizFile.Text))
                {
                    Bot.QuizFile = QuizFile.Text;
                    await Bot.ProcessQuizFile();
                }
            }
            catch (Exception ex)
            {
                ExLogger.ExLog(ex.ToString());
                throw;
            }
            Bot.IsRandom = (bool)RandomizeSwitch.IsChecked;
            
            CurrentPopup.IsOpen = false;
        }

        private void OpenFileClick(object sender, RoutedEventArgs e)
        {

            CurrentPopup.IsOpen = false;

            var dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();

            //Bot.QuizFile = dialog.FileName;
            tempPath = dialog.FileName;
            Console.WriteLine(dialog.FileName);
          
            CurrentPopup.IsOpen = true;
            CurrentPopup.StaysOpen = true;
           
            
            //CurrentPopup.IsOpen = true;
        }

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }

        String tempPath;


    }


}
