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
using MahApps.Metro.Controls.Dialogs;
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
        void LogError(string inString)
        {
            string proxy = "";
            if (bot.Proxy != null)
            {
                proxy = "http://" + bot.Proxy.ToString();
            }
            GoogleFormLogger gfl = new GoogleFormLogger(proxy);
            gfl.PostToGoogleForm(inString);

        }

        private void OnSourceUpdated(Object sender, DataTransferEventArgs args)
        {


        }

        void UnhandledExceptionsHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            
            string errorMessage = "[UnhandledExceptionsHandler]" + e.ToString();
            ExLogger.ExLog(errorMessage);
        }

        void UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            string errorMessage = "[UnobservedTaskException]" + e.ToString();
            ExLogger.ExLog(errorMessage);
        }

        void DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = "[DispatcherUnhandledException]" + e.ToString();
            ExLogger.ExLog(errorMessage);
        }

        public MainWindow()
        {  
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionsHandler;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;
            Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
            try
            {
                bot = new TwitchBot();
                bot.NotifyAboutNotices += OpenErrorMessageFromDifThread;
                bot.Destination = new Endpoint();
                bot.Destination.EndpointAddress = "irc.twitch.tv";
                bot.Destination.EndpointPort = 6667;
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
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
                    OpenErrorMessage("Something went wrong - connection is not established (check your internet connection/server availability)");
                    return;
                }
                //bot.Connect();
                bot.SendMessage("PASS oauth:" + bot.Auth.AuthKey + "\r\n" + "NICK " + bot.Auth.AuthName + "\r\n" + "JOIN #" + bot.Auth.AuthName + "\r\n");
                bot.TwitchChannel = bot.Auth.AuthName;
                //bot.SendMessage( );
            }
            else 
            {
                OpenErrorMessage("You should authorize first");
            }
        }

        private void DisconnectClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bot.Disconnect();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void SendClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string messageToSend = MessageBox.Text;
                //IrcCommand ic = new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#"+bot.TwitchChannel, false), new IrcCommandParameter(messageToSend, true));
                //bot.SendMessage(ic.ToString() + "\r\n");
                bot.SendMessageToCurrentChannel(messageToSend);
                MessageBox.Text = "";
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }





        private void StartQuizButtonClick(object sender, RoutedEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void StopQuizButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bot.StopQuiz();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
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
            try
            {
                bot.PreviousQuestion();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void ForwardQuizButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                bot.NextQuestion();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void QuickAddQuestion(object sender, RoutedEventArgs e)
        {
            try
            {
                bot.AddNewQuizObject("", "");
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void DropTheList(object sender, RoutedEventArgs e)
        {
            try
            {
                bot.DropTheQuizList();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void DropTheItemFromList(object sender, RoutedEventArgs e)
        {
            try
            {
                QuizObject qo = (QuizObject)QuizDataGrid.SelectedItem;
                if (qo != null)
                {
                    bot.DropTheItemFromList(qo);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void AskSelectedQuestion(object sender, RoutedEventArgs e)
        {
            try
            {
                QuizObject qo = (QuizObject)QuizDataGrid.SelectedItem;
                if (qo != null)
                {
                    bot.AskScpecifiedQuestion(qo);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        private void SelectionChangedOccured(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                QuizObject qo = (QuizObject)QuizDataGrid.SelectedItem;
                if (qo != null)
                {
                    QuizDataGrid.ScrollIntoView(qo);
                }
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                string errorMessage = String.Format("[FatalError]{0}", ex.ToString());
                ExLogger.ExLog(ex.ToString());
                OpenErrorMessage(errorMessage);
                throw;
            }
        }

        CancellationTokenSource StartAuthorize;

        TwitchBot bot;




        #region functionality for menu-driven UI

        private void OpenErrorMessageFromDifThread(String inErrorMessage)
        {
            this.Dispatcher.Invoke((Action<string>)OpenErrorMessage, inErrorMessage);
        }



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

        private async void ChannelButtonClick(object sender, RoutedEventArgs e)
        {
            double progress = 0;
            var controller = await this.ShowProgressAsync("Please wait...", "Quiz bot is preparing to switch location,stay tuned!");
            controller.SetCancelable(false);
            progress += 0.1;
            controller.SetProgress(progress);
            while (progress < 1)
            {
                await Task.Delay(10000);
                progress += 0.3;
                controller.SetProgress(progress);
            }
            await controller.CloseAsync();

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
            FlySettingsNew.IsOpen = true;
        }

        private void FlyoutQuiz(object sender, RoutedEventArgs e)
        {
            FlyQuiz.IsOpen = true;
        }

        #endregion












    }
}
