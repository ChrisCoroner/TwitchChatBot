using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace TwitchChatBot
{

	public class ReceivedDataArgs : EventArgs
	{
		private byte[] mData;

		public ReceivedDataArgs ()
		{

		}

		public ReceivedDataArgs (byte[] inData)
		{
			mData = inData;
		}

		public byte[] Data {
			get{
				return mData;
			}
			set{
				mData = value;
			}
		}
	}

	public interface ITcpConnection 
	{
		void Connect();
		void SendMessage (string inMessage);
		event EventHandler<ReceivedDataArgs> DataReceived;

	}

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
	 * V 0.TcpConnection class should perform TCP connection to the endpoint;
	 * V 1.Its should be able to connect *through* the proxy;
     * V 2.Its should provide the means of communication with endpoint.
	*/

	public class TcpConnection : ITcpConnection
	{
		public TcpConnection ()
		{

		}

		public bool Connected {
			get{
				return mTcpClient != null && mTcpClient.Connected;
			}
		}

        public void Disconnect()
        {
            if (Connected) {
                mNetworkStream.Close();
                mTcpClient.Close();
                mTcpClient = null;
                mNetworkStream = null;
            }
        }

		public void Connect ()
		{
			if (Connected) {
				throw new InvalidOperationException ("Already Connected");
			}
			if (Destination == null) {
				throw new InvalidOperationException("You should atleast specify destination endpoint");
			}
			if (mTcpClient == null) {
				mTcpClient = new TcpClient();
			}

            /*
             * As it connects it begin to read from stream http://msdn.microsoft.com/en-us/library/system.net.sockets.networkstream.beginread(v=vs.110).aspx
             * "When your application calls BeginRead, the system will wait until data is received or an error occurs, and then the system will use a separate thread to execute the specified callback method"
            */

            if (Proxy == null) {
				mTcpClient.Connect(Destination.EndpointAddress, Destination.EndpointPort);
				mNetworkStream = mTcpClient.GetStream ();
				mNetworkStream.BeginRead (Buffer, 0, Buffer.Length, new AsyncCallback (DataReceivedCallback), null);
			} else {
				mTcpClient.Connect(Proxy.EndpointAddress, Proxy.EndpointPort);
				mNetworkStream = mTcpClient.GetStream ();
				mNetworkStream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback (DataReceivedCallback), null);
				string tunnelRequest = String.Format ("CONNECT {0}  HTTP/1.1\r\nHost: {0}\r\n\r\n", Destination.ToString ());
				SendMessage (tunnelRequest);
			}

		}


		/*
		 *	SendMessage appends the message queue with an inMessage 
		 */
		public void SendMessage (string inMessage)
		{
			lock (MessageQ) {
				MessageQ.Enqueue(inMessage);
			}
			SendMessageCallback(null);
		}

		/*
		 *	SendMessageCallback transfer data to the destination through the NetworkStream 
		 */
		private void SendMessageCallback (IAsyncResult result)
		{
			//	result wont be null if current call to the SendMessageCallback is a continuation of BeginWrite execution
			//	result will be null if current call to the SendMessageCallback is a continuation of SendMessage execution 
			if (result != null && mNetworkStream!= null) {
				mNetworkStream.EndWrite(result);
				lock(MessageQ){
					sending = false;
				}
			}

			string CurrentMessage = null;

			lock (MessageQ) {
				//There is a possibility, that call to SendMessageCallback was made through SendMessage,
				//while another writing is in progress, so the call just returns, as inMessage was appended to the queue, and will be delivered in its turn
				if (sending) {
					return;
				}
				if (MessageQ.Count != 0) {
					sending = true;
					CurrentMessage = MessageQ.Dequeue();
					//Console.WriteLine(CurrentMessage);
				}
			}

			//CurrentMessage wont be null if the queue was not empty
			//CurrentMessage will be null if the queue was empty
			if (CurrentMessage != null && mNetworkStream != null) {

				mNetworkStream.BeginWrite(Encoding.UTF8.GetBytes(CurrentMessage),0,Encoding.UTF8.GetBytes(CurrentMessage).Length, new AsyncCallback(SendMessageCallback), null);
			}
		}

        /*
         *  Just passing received data to delegate and continue to read the stream
        */
		private void DataReceivedCallback( IAsyncResult result)
		{
            if (mNetworkStream != null)
            {
                int receivedDataLength = mNetworkStream.EndRead(result);
                //Console.WriteLine("Received {0} bytes:\n {1}", receivedDataLength, Encoding.UTF8.GetString(Buffer));

                byte[] ReceivedData = new byte[receivedDataLength];
                Array.Copy(Buffer, ReceivedData, receivedDataLength);

                OnDataReceived(new ReceivedDataArgs(ReceivedData));
                mNetworkStream.BeginRead(Buffer, 0, Buffer.Length, new AsyncCallback(DataReceivedCallback), null);
            }
		}

		protected virtual void OnDataReceived (ReceivedDataArgs ea)
		{
			if (DataReceived != null) {
				DataReceived(this,ea);
			}
		}

		public event EventHandler<ReceivedDataArgs> DataReceived;

		TcpClient mTcpClient;
		NetworkStream mNetworkStream;

		byte[] Buffer = new byte[2048];

		Queue<string> MessageQ = new Queue<string>();

		bool sending = false;

		public Endpoint Proxy;
		public Endpoint Destination;
	}
}

