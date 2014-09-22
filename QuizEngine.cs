using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace TwitchChatBot
{
	public class QuizEngine
	{
		public QuizEngine ()
		{
			mQuizQueue = new Queue<Tuple<string, string>>();
		}

		public QuizEngine (string inFileWithQuiz) : this()
		{
			if (File.Exists (inFileWithQuiz)) {
				using(FileStream fs = File.OpenRead(inFileWithQuiz)){
					string[] linesOfFile = null;
					File.ReadAllLines(inFileWithQuiz);
					ProcessStringsArrayAsQuiz(linesOfFile);
				}
			} else {
				throw new FileNotFoundException();
			}

		}

		public QuizEngine (string[] inQuiz) : this()
		{
			ProcessStringsArrayAsQuiz(inQuiz);

		}

		void ProcessStringsArrayAsQuiz (string[] inStringsArray)
		{
			string[] separators = new string[]{"{Question}","{Answer}"};

			for(int i = 0; i < inStringsArray.Length; i++ ){
				string[] result = inStringsArray[i].Split(separators,StringSplitOptions.RemoveEmptyEntries);
				if(result.Length == 2)
				{
					mQuizQueue.Enqueue(new Tuple<string, string>(result[0],result[1]));
				}

				Console.WriteLine("Strings in result: {0}",result.Length);
				foreach(var str in result){
					Console.WriteLine(str);
				}
			}
		}



		public void Process (IrcCommand inCommand)
		{
			if (inCommand.Name == "PRIVMSG") {
				//int intIndexOfLastParameter = inCommand.Parameters.Length - 1;
				//mQueue.Enqueue(inCommand.Parameters[intIndexOfLastParameter]);
				mIncomingMessagesQueue.Enqueue(inCommand);
			}

		}

		//TODO: make an async read of incoming messages, read them if quiz is currently running, check for a valid answer.
		//TODO: make a timered method, which is extracting quiz pairs from Q over time, assigning to the temp pair

		bool QuizRuns = false;

		public Action<string> SendMessage;
		Queue<IrcCommand> mIncomingMessagesQueue;
		Queue<Tuple<string,string>> mQuizQueue;
		System.Timers.Timer mTimer;
	}
}

