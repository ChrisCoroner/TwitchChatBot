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
    /// Interaction logic for Info.xaml
    /// </summary>
    public partial class Info : System.Windows.Controls.UserControl, ITwitchMenuItem
    {
        public Info(TwitchBot inBot, Popup inCurrentPopup)
        {
            Bot = inBot;
            
            CurrentPopup = inCurrentPopup;
            InitializeComponent();
        }

        private void PayPalButtonClick(object sender, RoutedEventArgs e)
        {
            Bot.PayPalSupport();
        }

        private void GitHubButtonClick(object sender, RoutedEventArgs e)
        {
            Bot.GitHubForkMe();
        }

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }




    }
}
