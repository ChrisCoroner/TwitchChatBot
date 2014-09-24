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
            HintNum = 0;
            //for (int i = 0; i < inAnswer.Length; i++) {
            //    if (inAnswer[i] == ' ') {
                    
            //    }
            //}
        }

        public string GiveAHint()
        {
            HintNum++;
            OpenChar();
            return mHint;
        }

        void OpenChar()
        {
            if ( (HintNum - 2) >= 0 && (HintNum - 1) < mHint.Length ) {
                //mHint[HintNum - 1] = mAnswer[HintNum - 1];
                char[] temp = new char[mHint.Length];
                mHint.CopyTo(0, temp, 0, mHint.Length);
                temp[HintNum - 2] = mAnswer[HintNum - 2];
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
            mScore = new Dictionary<string, int>();
            mTimeBetweenQuestions = 60000;
            mTimeBetweenHints = 15000;
		}

		public QuizEngine (string inFileWithQuiz) : this()
		{
            AddQuiz(inFileWithQuiz);

		}

		public QuizEngine (string[] inQuiz) : this()
		{
			ProcessStringsArrayAsQuiz(inQuiz);
		}

        public void AddQuiz(string inFileWithQuiz)
        {
        	if (File.Exists (inFileWithQuiz)) {
				using(FileStream fs = File.OpenRead(inFileWithQuiz)){
					
					string[] linesOfFile = File.ReadAllLines(inFileWithQuiz);
					ProcessStringsArrayAsQuiz(linesOfFile);
				}
			} else {
				throw new FileNotFoundException();
			}
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
                Task<IrcCommand> tic = Task.Run((Func<Task<IrcCommand>>)GetMessageFromQ,ct);
                //IrcCommand ic = await GetMessageFromQ();
                IrcCommand ic = await tic;
                //here we have a privmsg and have to check for a valid answer
                if (mCurrentQAPair != null && ic.Prefix != null)
                {
                   
                    Console.WriteLine("{0} is guessed it is \"{1}\" ({2})!", ic.Prefix, ic.Parameters[ic.Parameters.Length - 1].Value, mCurrentQAPair.Item2);

                    if (ic.Parameters[ic.Parameters.Length - 1].Value == mCurrentQAPair.Item2)
                    {

                        int indexOfExclamationSign = ic.Prefix.IndexOf('!');
                        string name = ic.Prefix.Substring(0, indexOfExclamationSign);

                        if (mScore.ContainsKey(name))
                        {
                            mScore[name]++;
                        }
                        else {
                            mScore[name] = 1;
                        }

                        
                        //Console.WriteLine("{0} is right, it is \"{1}\" !", name, mCurrentQAPair.Item2);
                        string message = String.Format("{0} is right, it is \"{1}\", {0}'s score is {2}!", name, mCurrentQAPair.Item2, mScore[name]);
                        SendMessage(new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter(message, true)).ToString() + "\r\n");

                        OnTimeToAskAQuestion(null, null);
                    }
                    Console.WriteLine("after GetMessageFromQ:{0} ", ic.Name);
                }
            }
        }

        async public void StartQuiz()
        {
            if (mQuizQueue.Count == 0) {
                throw new InvalidDataException("mQuizQueue is empty!");
            }

            if (cts != null) {
                mTimeToAskAQuestion.Enabled = false;
                mTimeToGiveAHint.Enabled = false;
                cts.Cancel();
            }
            cts = new CancellationTokenSource();

            mTimeToAskAQuestion = new System.Timers.Timer(mTimeBetweenQuestions);
            mTimeToGiveAHint = new System.Timers.Timer(mTimeBetweenHints);

            mTimeToAskAQuestion.Elapsed += OnTimeToAskAQuestion;
            mTimeToAskAQuestion.Enabled = true;

            mTimeToGiveAHint.Elapsed += OnTimeToGiveAHint;

            //OnTimeToAskAQuestion(null,null);
            OnTimeToAskAQuestion(null, null);
            await ReadIncomingMessages(cts.Token);
            
  
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
        Dictionary<string, int> mScore;

        CancellationTokenSource cts;
        
        int mTimeBetweenQuestions;
        int mTimeBetweenHints;

        QuizHint mQuizHint;
	}
}

