using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwitchChatBot
{
    class CustomHost : MarshalByRefObject
    {
        public void parse(string page, string query, ref StreamWriter sw)
        {
            SimpleWorkerRequest swr = new SimpleWorkerRequest(page, query, sw);
            HttpRuntime.ProcessRequest(swr);
        }
        
        ~CustomHost()
        {
            HttpRuntime.Close();
        }
    }



    public class TwitchBot : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class TwitchAuthorizationPart
        {
            public string[] scopes { get; set; }
            public string created_at { get; set; }
            public string updated_at { get; set; }
        }

        public class TwitchToken
        {
            public bool valid { get; set; }
            public TwitchAuthorizationPart authorization { get; set; }
            public string user_name { get; set; }
        }

        public class TwitchUserInfo
        {
            public Dictionary<string,string> _links {get;set;}
            public TwitchToken token { get; set; }
        }

        public class TwitchAuthorization
        {

            public TwitchAuthorization()
            {
                AuthKey = Properties.Settings.Default.authKey;
                AuthName = Properties.Settings.Default.authName;

            }

            public void TwitchAuthorize()
            {
                Process.Start("https://api.twitch.tv/kraken/oauth2/authorize?response_type=token&client_id=amoyxo9a7agc0e1gjpcawa1rqb2ciy4&redirect_uri=http://localhost:6555/Auth.aspx&scope=chat_login+channel_editor+user_read");
            }

            public string AuthKey
            {
                set
                {
                    authKey = value;
                    Properties.Settings.Default.authKey = value;
                    Properties.Settings.Default.Save();
                }
                get
                {
                    return authKey;
                }
            }

            public string AuthName
            {
                set
                {
                    authName = value;
                    Properties.Settings.Default.authName = value;
                    Properties.Settings.Default.Save();
                }
                get
                {
                    return authName;
                }
            }

            //public TwitchUserInfo UserInfo { get; set; }

            String authKey;
            String authName;

        }

		public TwitchBot ()
		{
            
			mTcpConnection = new TcpConnection();
			mTcpConnection.DataReceived += ProccessMessageData;
			mIrcCommandAnalyzer = new SimpleTwitchBotIrcCommandAnalyzer();
			//string[] Quiz = new string[]{"{Question}hey?{Answer}hey there!"};
            //string Quiz = @"C:\Users\ёрий\Documents\GitHub\TwitchChatBot\Quiz.txt";
			//mQE = new QuizEngine(Quiz);
            //mQE.SendMessage = SendMessage;
            //mQE.StartQuiz();

            host = (CustomHost)ApplicationHost.CreateApplicationHost(typeof(CustomHost), "/",@"C:\Users\ёрий\Documents\GitHub\TwitchChatBot");

            StartHttpListener();
            mQE = new QuizEngine();
            //mQE.SendMessage = SendMessage;
            mQE.SendMessage = SendMessageToCurrentChannel;

            TwitchChannel = "NONE";
		}

        public void StartHttpListener()
        {
            mListener = new HttpListener();
            mListener.Prefixes.Add("http://localhost:6555/");
            mListener.Start();
            mListener.BeginGetContext(ListenerCallback, mListener);
        }

        void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext context = mListener.EndGetContext(result);
            HttpListenerResponse response = context.Response;
 
            StreamReader reader = new StreamReader(context.Request.InputStream);
            string postRequest = reader.ReadToEnd();
            if (postRequest.Contains("{\"x\":\"#access_token="))
            {
                string cutRequest = postRequest.Replace("{\"x\":\"#access_token=", "");
                cutRequest = cutRequest.Substring(0, cutRequest.IndexOf('&'));
                Console.WriteLine("Data received:" + cutRequest);
                TA.AuthKey = cutRequest;
                WebRequest request = WebRequest.Create ("https://api.twitch.tv/kraken?oauth_token=" + cutRequest);
                HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse ();
                Stream dataStream = httpWebResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader dataReader = new StreamReader(dataStream);
                // Read the content. 
                string responseFromServer = dataReader.ReadToEnd();
                TwitchUserInfo twitchinfo = new JavaScriptSerializer().Deserialize<TwitchUserInfo>(responseFromServer);
                TA.AuthName = twitchinfo.token.user_name;
                AuthorizedName = AuthorizedName;
            }
            

            StreamWriter sw = new StreamWriter(response.OutputStream);

            string lp = context.Request.Url.LocalPath.Substring(1);
            string queryUrl = context.Request.Url.Query;
            if (context.Request.Url.Query.Length > 0 && context.Request.Url.Query[0] == '?')
            {
                queryUrl = queryUrl.Substring(1);
            }
            host.parse(lp, queryUrl, ref sw);
            sw.Flush();
            response.Close();
            
            mListener.BeginGetContext(ListenerCallback, mListener);
        }

		public Endpoint Proxy {
			get {
				return mTcpConnection.Proxy;
			}
			set{
				mTcpConnection.Proxy = value;
			}
		}

		public Endpoint Destination {
			get {
				return mTcpConnection.Destination;
			}
			set{
				mTcpConnection.Destination = value;
			}
		}

		public void Connect ()
		{
			mTcpConnection.Connect();
            Connected = Connected;
		}


		public void SendMessage (string inMessage)
		{
			mTcpConnection.SendMessage(inMessage);
		}

		void ProccessMessageData(object sender, ReceivedDataArgs ea)
		{
			//Collecting all data from mMessageBuffer and ReceivedDataArgs in the single buffer - tempTotalData.
			//Collected data will be parsed for distinct commands.
			//Collecting data...
			int MessagesBufferLength = (int)mMessagesBuffer.Length;
			byte[] tempTotalData = new byte[MessagesBufferLength + ea.Data.Length];
			mMessagesBuffer.ToArray().CopyTo(tempTotalData,0);
			Array.Copy(ea.Data,0,tempTotalData,MessagesBufferLength,ea.Data.Length);

			mMessagesBuffer.SetLength(0);
			//Collecting finishes here.
			//Parsing data...
            
		    int msgStart = 0;
		    for (int i = 0; i < tempTotalData.Length - 1; i++)
		    {
		        if (tempTotalData[i] == 13 && tempTotalData[i + 1] == 10) // byte 10 = LF and byte 13 = CR
		        {
		            byte[] message = new byte[i - msgStart];
		            Array.Copy(tempTotalData, msgStart, message, 0, i - msgStart); // Copy data[msgStart:i] to message
					//Console.WriteLine("Command Received: {0}",Encoding.UTF8.GetString(message));
					string inMessage = Encoding.UTF8.GetString(message);
					if(inMessage.Length > 0){
						mMessageQ.Enqueue(inMessage);
						Console.WriteLine("COMMAND NAME: {0}",IrcCommand.Parse(inMessage).Name);
                        Console.WriteLine("PARAMETER: ");
                        foreach (var p in IrcCommand.Parse(inMessage).Parameters) {
                            Console.WriteLine(p.ToString());
                        }
                        IrcCommand incCommand = IrcCommand.Parse(inMessage);
                        IrcCommand outCommand = mIrcCommandAnalyzer.GetResponse(IrcCommand.Parse(inMessage));
                        if (outCommand != null)
                        {
                            SendMessage(outCommand.ToString());
                        }
                        if (incCommand.Name == "PRIVMSG")
                        {
                            mQE.Process(incCommand);
                        }
					}
					msgStart = i = i + 2;
		        }
		    }
 
		    // What is left from msgStart til the end of data is only a partial message.
		    // We want to save that for when the rest of the message arrives.
		    mMessagesBuffer.Write(tempTotalData, msgStart, tempTotalData.Length - msgStart);

		}

        public void StartQuiz()
        {
            string Quiz = @"C:\Users\ёрий\Documents\GitHub\TwitchChatBot\TwitchChatBotGUI\Quiz.txt";
            //string Quiz = @"C:\Quiz.txt";
            mQE.AddQuiz(Quiz);
            mQE.StartQuiz();
        }

		public void DumpMessageQ ()
		{
			foreach (var i in mMessageQ) {
				Console.WriteLine(i);
			}
		}

        public TwitchAuthorization Auth
        {
            get {
                return TA;
            }
            private set {
                TA = value;
            }
        }

        public String AuthorizedName
        {
            get {
                return TA.AuthName;
            }
            private set {
                NotifyPropertyChanged();
            }
        }

        public void JoinTwitchChannel(String inTwitchChannel)
        {
            SendMessage("JOIN #" + inTwitchChannel + "\r\n");
            TwitchChannel = inTwitchChannel;
        }

        public void SendMessageToCurrentChannel(String inMessage)
        {
            SendMessage(new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#"+TwitchChannel, false), new IrcCommandParameter(inMessage, true)).ToString() + "\r\n");
        }

        public String QuizFile
        { 
            get{
                return mQE.QuizFile;
            }
            set {
                mQE.QuizFile = value;
            }
        }

        public int TimeBetweenQuestions
        {
            get
            {
                return mQE.TimeBetweenQuestions;
            }
            set
            {
                mQE.TimeBetweenQuestions = value;
            }
        }

        public int TimeBetweenHints
        {
            get
            {
                return mQE.TimeBetweenHints;
            }
            set
            {
                mQE.TimeBetweenHints = value;
            }
        }

        public bool Connected 
        {
            get { 
                return mTcpConnection.Connected;
            }
            private set {
                NotifyPropertyChanged();
            }
        }

        public String TwitchChannel
        {
            get {
                return twitchChannel;
            }
            set {
                
                twitchChannel = value;
                NotifyPropertyChanged();
            } 
        }



        string twitchChannel;

		TcpConnection mTcpConnection;
		Queue<string> mMessageQ = new Queue<string>();
		MemoryStream mMessagesBuffer = new MemoryStream();
		IrcCommandAnalyzer mIrcCommandAnalyzer;
		QuizEngine mQE;
        HttpListener mListener;
        CustomHost host;
        TwitchAuthorization TA = new TwitchAuthorization();
	}
}

