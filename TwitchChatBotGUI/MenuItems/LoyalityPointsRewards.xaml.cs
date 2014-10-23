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
    /// Interaction logic for LoyalityPointsRewards.xaml
    /// </summary>
    public partial class LoyalityPointsRewards : System.Windows.Controls.UserControl, ITwitchMenuItem, IDataErrorInfo
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

                if (name == "mLoyalityCommand")
                {
                    if (String.IsNullOrEmpty(tempLoyalityCommand))
                    {

                    }
                    if (!(String.IsNullOrEmpty(tempLoyalityCommand)) && tempLoyalityCommand[0] != '!')
                    {
                        result = "Command should start with exclamation sign (!)";
                        return result;
                    }
                    if (!(tempLoyalityCommand.Contains("*UserName*")))
                    {
                        result = "Command should contain *UserName* keyword";
                        return result;
                    }
                }

                return result;
            }
        }

        public string mLoyalityCommand
        {
            get
            {
                return tempLoyalityCommand;
            }
            set
            {
                tempLoyalityCommand = value;
            }
        }

        string tempLoyalityCommand;

        private void LoyalityPointsRewards_Loaded(object sender, RoutedEventArgs e)
        {
            tempLoyalityCommand = Bot.LoyalityCommand;

            LoyalityCommand.Text = tempLoyalityCommand;

            LoyalityWindowName.DataContext = (LoyalityPointsRewards)this;
        }

        public LoyalityPointsRewards(TwitchBot inBot, Popup inCurrentPopup)
        {
            this.Loaded += LoyalityPointsRewards_Loaded;
            Bot = inBot;
            CurrentPopup = inCurrentPopup;

            InitializeComponent();
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            Bot.LoyalityCommand = tempLoyalityCommand;
            CurrentPopup.IsOpen = false;
        }

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }
    }
}
