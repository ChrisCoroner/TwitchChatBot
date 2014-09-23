using System;
using System.Collections.Generic;
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

using TwitchChatBot;

namespace TwitchChatBotGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bot = new TwitchBot();
            bot.Destination = new Endpoint();
            bot.Destination.EndpointAddress = "irc.twitch.tv";
            bot.Destination.EndpointPort = 6667;
        }

        private void ProxyAddressChanged(object sender, TextChangedEventArgs e)
        {

        }

        

        private void ProxyPortChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        TwitchBot bot;

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            if(ProxyAddress.Text != "" && ProxyPort.Text != ""){
                bot.Proxy = new Endpoint();
                bot.Proxy.EndpointAddress = ProxyAddress.Text;
                bot.Proxy.EndpointPort = Int32.Parse(ProxyPort.Text);
            }
            bot.Connect();

            bot.SendMessage("PASS oauth:lxubjjlsavkv1o3ih44d3csztfpw7vu\r\n");
            bot.SendMessage("NICK sovietmade\r\n");
            bot.SendMessage("JOIN #sovietmade\r\n");
            bot.SendMessage("PRIVMSG #sovietmade :test\r\n");
        }


        
    }
}
