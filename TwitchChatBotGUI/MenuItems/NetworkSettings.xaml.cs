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
    /// Interaction logic for NetworkSettings.xaml
    /// </summary>
    public partial class NetworkSettings : UserControl
    {
        private void NetworkSettings_Loaded(object sender, RoutedEventArgs e)
        {
            if (Bot.Proxy != null)
            {
                ProxyAddress.Text = Bot.Proxy.EndpointAddress;
                ProxyPort.Text = String.Format("{0}", Bot.Proxy.EndpointPort);
            }
        }

        public NetworkSettings(TwitchBot inBot, Popup inCurrentPopup) 
        {
            this.Loaded += NetworkSettings_Loaded;

            Bot = inBot;
            CurrentPopup = inCurrentPopup;


            InitializeComponent();
        }

        private void AcceptClick(object sender, RoutedEventArgs e)
        {
            if (ProxyAddress.Text != "" && ProxyPort.Text != "")
            {
                Bot.Proxy = new Endpoint();
                Bot.Proxy.EndpointAddress = ProxyAddress.Text;
                Bot.Proxy.EndpointPort = Int32.Parse(ProxyPort.Text);
            }
            else if (ProxyAddress.Text == "" && ProxyPort.Text == "")
            {
                Bot.Proxy = null;
            }

            CurrentPopup.IsOpen = false;
        }

        Popup CurrentPopup { get; set; }
        TwitchBot Bot { get; set; } 
    }
}
