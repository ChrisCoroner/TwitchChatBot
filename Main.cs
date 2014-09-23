using System;
using System.Threading;
//oauth:4286uj803vttts1k6reigd8snxngmjl
namespace TwitchChatBot {
	class Flow{
		static void Main(){

			TwitchBot bot = new TwitchBot();
			//bot.Proxy = new Endpoint();
			bot.Destination = new Endpoint();

		
			//bot.Proxy.EndpointAddress = "eproxy.volga";
			//bot.Proxy.EndpointPort = 8080;

			bot.Destination.EndpointAddress = "irc.twitch.tv";
			bot.Destination.EndpointPort = 6667;

			bot.Connect();
            
			bot.SendMessage("PASS oauth:lxubjjlsavkv1o3ih44d3csztfpw7vu\r\n");
            bot.SendMessage("NICK sovietmade\r\n");
            bot.SendMessage("JOIN #sovietmade\r\n");
            bot.SendMessage("PRIVMSG #sovietmade :test\r\n");
			Thread.Sleep(10000);
			bot.DumpMessageQ();
			Thread.Sleep(1000000);
            Console.WriteLine("");
		}
	}
}


