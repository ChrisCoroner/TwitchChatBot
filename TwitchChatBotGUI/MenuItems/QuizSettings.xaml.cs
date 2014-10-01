using System;
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
using TwitchChatBot;

namespace TwitchChatBotGUI.MenuItems
{
    /// <summary>
    /// Interaction logic for QuizSettings.xaml
    /// </summary>
    public partial class QuizSettings : UserControl, ITwitchMenuItem
    {

        private void QuizSettings_Loaded(object sender, RoutedEventArgs e)
        {
            TimeBetweenQuestions.Text = String.Format("{0}",Bot.TimeBetweenQuestions);
            TimeBetweenHints.Text = String.Format("{0}", Bot.TimeBetweenHints);
            QuizFile.Text = Bot.QuizFile;
        }

        public QuizSettings(TwitchBot inBot, Popup inCurrentPopup)
        {
            this.Loaded += QuizSettings_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            
            InitializeComponent();
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            Bot.TimeBetweenQuestions = Int32.Parse(TimeBetweenQuestions.Text);
            Bot.TimeBetweenHints = Int32.Parse(TimeBetweenHints.Text);
            Bot.QuizFile = QuizFile.Text;
            CurrentPopup.IsOpen = false;
        } 

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }


    }


}
