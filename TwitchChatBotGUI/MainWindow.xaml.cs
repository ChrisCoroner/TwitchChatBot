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
using TwitchChatBotGUI.MenuItems;
using TwitchChatBot;
using MahApps.Metro.Controls;
using System.Globalization;

namespace TwitchChatBotGUI
{

    public class WidthToHalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return ((double)value / 2);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return ((double)value * 2);
        }
    }

    public class IntToDoubleOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int tick = (int)value;
            if (tick % 2 == 0)
            {
                return (0.5);
            }
            else {
                return 1.0;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return ((double)value * 2);
        }
    }

    public class IntToDoubleWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            int tick = (int)value;
            if (tick % 2 == 0)
            {
                return 55.0;
            }
            else
            {
                return 45.0;
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return ((double)value * 2);
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private void OnSourceUpdated(Object sender, DataTransferEventArgs args)
        {


        }

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


            MainWindowName.DataContext = bot;
        }

        private void ConnectClick(object sender, RoutedEventArgs e)
        {
            if (bot.Authorized)
            {
                try
                {
                    bot.Connect();
                }
                catch (TimeoutException ex)
                {
                    OpenErrorMessage("Something went wrong - connection is not established");
                    return;
                }
                bot.SendMessage("PASS oauth:" + bot.Auth.AuthKey + "\r\n");
                bot.SendMessage("NICK " + bot.Auth.AuthName + "\r\n");
            }
            else 
            {
                OpenErrorMessage("You should authorize first");
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            bot.Disconnect();
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            string messageToSend = MessageBox.Text;
            //IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#"+bot.TwitchChannel, false), new IrcCommandParameter(messageToSend, true));
            //bot.SendMessage(ic.ToString() + "\r\n");
            bot.SendMessageToCurrentChannel(messageToSend);
            MessageBox.Text = "";
        }





        private void StartQuizButtonClick(object sender, RoutedEventArgs e)
        {
            if (bot.QuizList.Count() == 0)
            {
                OpenErrorMessage("You should specify file containing a quiz or manually add some questions!");
            }
            else
            {
                bot.SendMessageToCurrentChannel("Quiz is starting!");

                bot.StartQuiz();
            }
        }

        private void StopQuizButtonClick(object sender, RoutedEventArgs e)
        {
            bot.StopQuiz();
        }

        async Task GetAuth(CancellationToken ct)
        {
            bot.TwitchAuthorize();
            while(bot.Auth.AuthName == ""){
                await Task.Delay(100);
            }
        }

        async private void Authorize_Click(object sender, RoutedEventArgs e)
        {               
            StartAuthorize = new CancellationTokenSource();
            await GetAuth(StartAuthorize.Token); 
        }

        private void AddNewQuizObject(object sender, RoutedEventArgs e)
        {
            OpenAddNewQuizObjectPop(null, null);
        }



        private void PreviousQuizButtonClick(object sender, RoutedEventArgs e)
        {
            bot.PreviousQuestion();
        }

        private void ForwardQuizButtonClick(object sender, RoutedEventArgs e)
        {
            bot.NextQuestion();
        }

        private void QuickAddQuestion(object sender, RoutedEventArgs e)
        {
            bot.AddNewQuizObject("", "");
        }

        private void DropTheList(object sender, RoutedEventArgs e)
        {
            bot.DropTheQuizList();
        }

        private void SelectionChangedOccured(object sender, SelectionChangedEventArgs e)
        {
            QuizObject qo =  (QuizObject)QuizDataGrid.SelectedItem;
            if (qo != null)
            {
                QuizDataGrid.ScrollIntoView(qo);
            }
            Console.WriteLine();
        }

        CancellationTokenSource StartAuthorize;

        TwitchBot bot;




        #region functionality for menu-driven UI

        private void OpenErrorMessage(String inErrorMessage)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new ErrorMessage(bot, Pop, inErrorMessage));
        }

        private void OpenAddNewQuizObjectPop(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new AddNewQuizObjectPop(bot, Pop, QuizDataGrid));
        }

        private void OpenNetworkSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new NetworkSettings(bot, Pop));
        }

        private void OpenQuizSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new QuizSettings(bot, Pop));
        }

        private void OpenChannelSettings(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new ChannelSettings(bot, Pop));
        }

        private void OpenInfo(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new Info(bot, Pop));
        }

        private void ChannelButtonClick(object sender, RoutedEventArgs e)
        {
            Popup Pop = new Popup();
            ShowPopupWithUserControl(new ChannelSettings(bot, Pop));
        }

        private void ShowPopupWithUserControl(ITwitchMenuItem inControl)
        {
            inControl.CurrentPopup.MinHeight = 200;
            inControl.CurrentPopup.MinWidth = 200;
            inControl.CurrentPopup.PlacementTarget = this;
            inControl.CurrentPopup.Placement = PlacementMode.Center;
            inControl.CurrentPopup.Child = (inControl as UserControl);
            inControl.CurrentPopup.IsOpen = true;
            inControl.CurrentPopup.StaysOpen = false;
        }

        private void LogOutButtonClick(object sender, RoutedEventArgs e)
        {
            bot.TwitchLogOut();
        }

        private void FlyoutSettings(object sender, RoutedEventArgs e)
        {
            FlySettings.IsOpen = true;
        }

        private void FlyoutQuiz(object sender, RoutedEventArgs e)
        {
            FlyQuiz.IsOpen = true;
        }

        #endregion












    }
}
