using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace TwitchChatBot
{

	public class Endpoint{
		public string EndpointAddress
		{
			get;
			set;
		}
		public int EndpointPort
		{
			get;
			set;
		}

		public override string ToString ()
		{
			return string.Format ("{0}:{1}", EndpointAddress, EndpointPort);
		}
	}

	/*
	 * TcpConnection class should perform TCP connection to the endpoint;
	 * Its should be able to connect *through* the proxy;
	*/
	public class TcpConnection
	{
		public TcpConnection ()
		{

		}

		public void SendMessage (string inMessage)
		{
			byte[] byteMessageAsBufferOfBytes = Encoding.UTF8.GetBytes(inMessage);
			mNetworkStream.Write(byteMessageAsBufferOfBytes,0,byteMessageAsBufferOfBytes.Length);
			mNetworkStream.Flush();
		}


		public void Send (string inMessage)
		{
			lock (MessageQ) {
				MessageQ.Enqueue(inMessage);
			}
			SendData(null);
		}

		public void Connect(){
			if (Proxy == null) {
				mTcpClient = new TcpClient(Destination.EndpointAddress,Destination.EndpointPort);
				mNetworkStream = mTcpClient.GetStream();
				mNetworkStream.BeginRead(Buffer,0,Buffer.Length,new AsyncCallback(DataReceived),null);
			} 
			else {
				mTcpClient = new TcpClient(Proxy.EndpointAddress,Proxy.EndpointPort);
				mNetworkStream = mTcpClient.GetStream();
				mNetworkStream.BeginRead(Buffer,0,Buffer.Length,new AsyncCallback(DataReceived),null);
				string tunnelRequest = String.Format("CONNECT {0}  HTTP/1.1\r\nHost: {0}\r\n\r\n", Destination.ToString());
				Send(tunnelRequest);
			}


            Send("PASS oauth:lxubjjlsavkv1o3ih44d3csztfpw7vu\r\n");
            Send("NICK sovietmade\r\n");
            //ReadMessage();

            Send("JOIN #sovietmade\r\n");
            //ReadMessage();
            Send("PRIVMSG #sovietmade :test\r\n");
            //SendMessage("JOIN sovietmade\r\n");
            //ReadMessage();
		}


		private void SendData (IAsyncResult result)
		{

			if (result != null) {
				mNetworkStream.EndWrite(result);
				lock(MessageQ){
					sending = false;
				}
			}

			string CurrentMessage = null;

			lock (MessageQ) {
				if (sending) {
					return;
				}
				if (MessageQ.Count != 0) {
					sending = true;
					CurrentMessage = MessageQ.Dequeue();

					Console.WriteLine(CurrentMessage);
				}
			}

			if (CurrentMessage != null) {

				mNetworkStream.BeginWrite(Encoding.UTF8.GetBytes(CurrentMessage),0,Encoding.UTF8.GetBytes(CurrentMessage).Length, new AsyncCallback(SendData), null);
			}
		}
		private void DataReceived( IAsyncResult result)
		{
			int receivedDataLength = mNetworkStream.EndRead(result);
			Console.WriteLine("Received {0} bytes:\n {1}", receivedDataLength, Encoding.UTF8.GetString(Buffer));
			mNetworkStream.BeginRead(Buffer,0,Buffer.Length,new AsyncCallback(DataReceived),null);

		}

		TcpClient mTcpClient;
		NetworkStream mNetworkStream;
		public Endpoint Proxy;
		public Endpoint Destination;
		byte[] Buffer = new byte[2048];
		Queue<string> MessageQ = new Queue<string>();
		bool sending = false;
	}
}

