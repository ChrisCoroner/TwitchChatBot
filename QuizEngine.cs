using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TwitchChatBot
{
	public class QuizEngine
	{
		public QuizEngine ()
		{

		}



		public void Process (IrcCommand inCommand)
		{
			if (inCommand.Name == "PRIVMSG") {
				//int intIndexOfLastParameter = inCommand.Parameters.Length - 1;
				//mQueue.Enqueue(inCommand.Parameters[intIndexOfLastParameter]);
				mIncomingMessagesQueue.Enqueue(inCommand);
			}

		}

		 

		bool QuizRuns = false;

		public Action<string> SendMessage;
		Queue<IrcCommand> mIncomingMessagesQueue;
		Queue<Tuple<string,string>> mQuizQueue;
		System.Timers.Timer mTimer;
	}
}

