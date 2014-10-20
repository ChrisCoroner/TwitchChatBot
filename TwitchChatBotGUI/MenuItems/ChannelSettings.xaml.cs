using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Globalization;

namespace TwitchChatBotGUI.MenuItems
{
    /// <summary>
    /// Interaction logic for ChannelSettings.xaml
    /// </summary>
    public partial class ChannelSettings : UserControl, ITwitchMenuItem
    {

        private void ChannelSettings_Loaded(object sender, RoutedEventArgs e)
        {
            TwitchChannel.Text = Bot.TwitchChannel;
        }

        public ChannelSettings(TwitchBot inBot, Popup inCurrentPopup)
        {
            this.Loaded += ChannelSettings_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;

            InitializeComponent();
        }

        public ChannelSettings(TwitchBot inBot, Popup inCurrentPopup, MetroWindow inWindow)
        {
            
            this.Loaded += ChannelSettings_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;
            mWindow = inWindow;

            InitializeComponent();
        }

        async Task WaitABit()
        {
            await Task.Delay(30000);
        }

        private async void AcceptClick(object sender, RoutedEventArgs e)
        {

            Bot.TwitchChannel = TwitchChannel.Text;
            if (Bot.Connected)
            {
                Bot.JoinTwitchChannel(TwitchChannel.Text);
            }
            CurrentPopup.IsOpen = false;
        }

        MetroWindow mWindow;
        public Popup CurrentPopup { get; set; }
        public TwitchBot Bot { get; set; }
    }
}
