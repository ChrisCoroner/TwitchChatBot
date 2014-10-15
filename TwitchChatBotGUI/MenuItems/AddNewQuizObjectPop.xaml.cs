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
    /// Interaction logic for AddNewQuizObjectPop.xaml
    /// </summary>
    public partial class AddNewQuizObjectPop : System.Windows.Controls.UserControl, ITwitchMenuItem, IDataErrorInfo
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

                if (name == "NewQuizQuestion")
                {
                    if (NewQuizQuestion == null)
                    {
                        result = "Question cannot be empty";
                    }

                    else if (NewQuizQuestion.Length < 3)
                    {
                        result = "The question is too short";
                    }
                }

                if (name == "NewQuizAnswer")
                {
                    if (NewQuizAnswer == null)
                    {
                        result = "Question cannot be empty";
                    }
                    else if (NewQuizAnswer.Length < 3)
                    {
                        result = "The answer is too short";
                    }
                }
                return result;
            }
        }

        private void AddNewQuizObjectPop_Loaded(object sender, RoutedEventArgs e)
        {
            AddNewQuizObjectWindowName.DataContext = (AddNewQuizObjectPop)this;
        }

        public AddNewQuizObjectPop(TwitchBot inBot, Popup inCurrentPopup)
        {
            this.Loaded += AddNewQuizObjectPop_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            InitializeComponent();
        }

        public AddNewQuizObjectPop(TwitchBot inBot, Popup inCurrentPopup, System.Windows.Controls.DataGrid inQuizDataGrid)
        {
            this.Loaded += AddNewQuizObjectPop_Loaded;
            Grid = inQuizDataGrid;
            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            InitializeComponent();
        }

        async private void AcceptClick(object sender, RoutedEventArgs e)
        {
            if (NewQuizQuestion != null && NewQuizAnswer != null)
            {
                Bot.AddNewQuizObject(NewQuizQuestion, NewQuizAnswer);
                //Grid.Items.Add(new QuizObject(NewQuizQuestion, NewQuizAnswer));
            }

            CurrentPopup.IsOpen = false;
        }

        public string NewQuizQuestion
        {
            get;
            set;
        }

        public string NewQuizAnswer
        {
            get;
            set;
        }

        public System.Windows.Controls.DataGrid Grid { get; set; }
        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }
    }
}
