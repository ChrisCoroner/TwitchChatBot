using System;
using System.Threading;
//oauth:4286uj803vttts1k6reigd8snxngmjl
namespace TwitchChatBot {
	class Flow{
		static void Main(){
			TcpConnection tt = new TcpConnection();
			tt.Proxy = new Endpoint();
			tt.Proxy.EndpointAddress = "eproxy.volga";
			tt.Proxy.EndpointPort = 8080;
			tt.Destination = new Endpoint();
			tt.Destination.EndpointAddress = "irc.twitch.tv";
			tt.Destination.EndpointPort = 6667;
			tt.Connect();
			Thread.Sleep(1000000);
            Console.WriteLine("");
		}
	}
}


