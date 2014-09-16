using System;
using System.Net.Sockets;
using System.Text;

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

		public void ReadMessage ()
		{
			byte[] buffer = new byte[2048];
			int bytes = mNetworkStream.Read(buffer, 0, buffer.Length);
           	Console.Write(Encoding.UTF8.GetString(buffer, 0, bytes));
		}

		public void Connect(){
			if (Proxy == null) {
				mTcpClient = new TcpClient(Destination.EndpointAddress,Destination.EndpointPort);
				mNetworkStream = mTcpClient.GetStream();
			} 
			else {
				mTcpClient = new TcpClient(Proxy.EndpointAddress,Proxy.EndpointPort);
				mNetworkStream = mTcpClient.GetStream();
				string tunnelRequest = String.Format("CONNECT {0}:{1}  HTTP/1.1\r\nHost: {0}\r\n\r\n", Destination.EndpointAddress,Destination.EndpointPort);
				SendMessage(tunnelRequest);
				ReadMessage();
				SendMessage("PASS oauth:4286uj803vttts1k6reigd8snxngmjl");
				SendMessage("NICK sovietmade");
				ReadMessage();
			}
		}

		TcpClient mTcpClient;
		NetworkStream mNetworkStream;
		public Endpoint Proxy;
		public Endpoint Destination;
	}
}

