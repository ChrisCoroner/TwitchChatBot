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
    /// Interaction logic for ErrorMessage.xaml
    /// </summary>
    public partial class ErrorMessage : UserControl, ITwitchMenuItem
    {
        public ErrorMessage(TwitchBot inBot, Popup inCurrentPopup)
        {
            Bot = inBot;
            CurrentPopup = inCurrentPopup;

            InitializeComponent();
        }

        public ErrorMessage(TwitchBot inBot, Popup inCurrentPopup, String inError)
        {
            Bot = inBot;
            CurrentPopup = inCurrentPopup;

            InitializeComponent();
           
            ErrorMessageField.Text = inError;
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            CurrentPopup.IsOpen = false;
        }

        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }
    }
}
