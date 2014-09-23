using System;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace TwitchChatBot
{
    public class QuizHint
    {
        public QuizHint(string inAnswer)
        {
            mAnswer = inAnswer;
            mHint = new string('_', inAnswer.Length);
        }

        public string GiveAHint()
        {
            HintNum++;
            OpenChar();
            return mHint;
        }

        void OpenChar()
        {
            if ( (HintNum - 1) >= 0 && (HintNum - 1) < mHint.Length ) {
                //mHint[HintNum - 1] = mAnswer[HintNum - 1];
                char[] temp = new char[mHint.Length];
                mHint.CopyTo(0, temp, 0, mHint.Length);
                temp[HintNum - 1] = mAnswer[HintNum - 1];
                mHint = new string(temp);
            }
        }

        static int HintNum
        {
            get;
            set;
        }

        string mHint;

        string mAnswer;
    }

	public class QuizEngine
	{
		public QuizEngine ()
		{
			mQuizQueue = new Queue<Tuple<string, string>>();
            mIncomingMessagesQueue = new Queue<IrcCommand>();
            mTimeBetweenQuestions = 60000;
            mTimeBetweenHints = 15000;
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

				//Console.WriteLine("Strings in result: {0}",result.Length);
				//foreach(var str in result){
				//	Console.WriteLine(str);
				//}
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

        async Task<IrcCommand> GetMessageFromQ()
        { 
            while(mIncomingMessagesQueue.Count == 0)
            {
                await Task.Delay(100);
            }
            return mIncomingMessagesQueue.Dequeue();
        }

        async Task ReadIncomingMessages(CancellationToken ct)
        {
            while (true)
            {
                Task<IrcCommand> tic = Task.Run((Func<Task<IrcCommand>>)GetMessageFromQ);
                //IrcCommand ic = await GetMessageFromQ();
                IrcCommand ic = await tic;
                //here we have a privmsg and have to check for a valid answer
                //if (ic.Parameters[ic.Parameters.Length - 1].Value == mCurrentQAPair.Item2) {
                //    Console.WriteLine("{0} is right, it is \"{1}\" !", ic.Prefix, mCurrentQAPair.Item2);
                //}
                Console.WriteLine("after GetMessageFromQ:{0} ",ic.Name);
            }
        }

        async public void StartQuiz()
        {
            if (mQuizQueue.Count == 0) {
                throw new InvalidDataException("mQuizQueue is empty!");
            }

            if (cts != null) {
                cts.Cancel();
            }
            cts = new CancellationTokenSource();

            mTimeToAskAQuestion = new System.Timers.Timer(mTimeBetweenQuestions);
            mTimeToGiveAHint = new System.Timers.Timer(mTimeBetweenHints);

            mTimeToAskAQuestion.Elapsed += OnTimeToAskAQuestion;
            mTimeToAskAQuestion.Enabled = true;

            mTimeToGiveAHint.Elapsed += OnTimeToGiveAHint;
            

            await  ReadIncomingMessages(cts.Token);
        }

		//TODO: make an async read of incoming messages, read them if quiz is currently running, check for a valid answer.

        void OnTimeToAskAQuestion(object source, ElapsedEventArgs e)
        {
            //if there is no more items in Q, then the exception will be rised
            if (mQuizQueue.Count == 0) {
                return;
                //throw new InvalidDataException("mQuizQueue is empty!");
            }
            
            mCurrentQAPair = mQuizQueue.Dequeue();

            mQuizHint = new QuizHint(mCurrentQAPair.Item2);
            mTimeToGiveAHint.Enabled = true;

            SendMessage(new IrcCommand(null,"PRIVMSG", new IrcCommandParameter("#sovietmade",false), new IrcCommandParameter(mCurrentQAPair.Item1,true)).ToString() + "\r\n");
        }

        void OnTimeToGiveAHint(object source, ElapsedEventArgs e)
        {
            string currentHint = mQuizHint.GiveAHint();
            SendMessage(new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter(currentHint, true)).ToString() + "\r\n");
        }

        //delegate for communicating back to the outer world
		public Action<string> SendMessage;

        //Queue of PRIVMSGes - source of answers
		Queue<IrcCommand> mIncomingMessagesQueue;
		
        //Queue of tuples, containing questions and answers
        Queue<Tuple<string,string>> mQuizQueue;
		

        System.Timers.Timer mTimeToAskAQuestion;
        System.Timers.Timer mTimeToGiveAHint;

        Tuple<string, string> mCurrentQAPair;
        
        CancellationTokenSource cts;

        int mTimeBetweenQuestions;
        int mTimeBetweenHints;

        QuizHint mQuizHint;
	}
}

