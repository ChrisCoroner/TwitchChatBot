using System;

namespace TwitchChatBot
{
	public class TwitchBot
	{
		public TwitchBot () 
		{
			mTcpConnection = new TcpConnection();
			mTcpConnection.DataReceived += ProccessMessageData;
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

			Console.WriteLine("DataReceived");
		}

		TcpConnection mTcpConnection;

	}
}

