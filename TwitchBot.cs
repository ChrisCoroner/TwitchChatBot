using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace TwitchChatBot
{
	public class TwitchBot
	{
		public TwitchBot () 
		{
			mTcpConnection = new TcpConnection();
			mTcpConnection.DataReceived += ProccessMessageData;
			mIrcCommandAnalyzer = new SimpleTwitchBotIrcCommandAnalyzer();
			string[] Quiz = new string[]{"{Question}hey?{Answer}hey there!"};
			mQE = new QuizEngine(Quiz);
            mQE.SendMessage = SendMessage;
            mQE.StartQuiz();
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
            mQE.StartQuiz();
        }

		public void DumpMessageQ ()
		{
			foreach (var i in mMessageQ) {
				Console.WriteLine(i);
			}
		}

		TcpConnection mTcpConnection;
		Queue<string> mMessageQ = new Queue<string>();
		MemoryStream mMessagesBuffer = new MemoryStream();
		IrcCommandAnalyzer mIrcCommandAnalyzer;
		QuizEngine mQE;
	}
}

