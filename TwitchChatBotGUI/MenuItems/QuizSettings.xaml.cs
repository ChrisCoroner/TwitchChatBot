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
using System.Windows.Forms;
using Microsoft.TeamFoundation.MVVM;
using TwitchChatBot;

namespace TwitchChatBotGUI.MenuItems
{




    /// <summary>
    /// Interaction logic for QuizSettings.xaml
    /// </summary>
    public partial class QuizSettings : System.Windows.Controls.UserControl, ITwitchMenuItem
    {

        private string selectedPath = string.Empty;

        public string SelectedPath
        {
            get { return this.selectedPath; }
            set { selectedPath = value; }
        }

        public ICommand BrowseFileCommand
        {
            get {return new RelayCommand(BrowseFileAction); }
        }

        public void BrowseFileAction()
        {
            var dialog = new OpenFileDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.SelectedPath = dialog.FileName;
            }
        }

        private void QuizSettings_Loaded(object sender, RoutedEventArgs e)
        {
            TimeBetweenQuestions.Text = String.Format("{0}",Bot.TimeBetweenQuestions);
            TimeBetweenHints.Text = String.Format("{0}", Bot.TimeBetweenHints);
            QuizFile.Text = Bot.QuizFile;
            QuizWindowName.DataContext = (QuizSettings)this;
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
