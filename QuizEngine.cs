using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TwitchChatBot
{
    public class QuizObject : IEquatable<QuizObject>
    {
        public QuizObject(String inQuestion, String inAnswer)
        {
            Question = inQuestion;
            Answer = inAnswer;
        }

        public bool Equals(QuizObject other)
        {
            if (other == null)
            {
                return false;
            }
            else if (this.Question == other.Question)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            QuizObject quizObj = obj as QuizObject;
            if (quizObj == null)
            {
                return false;
            }
            else
            {
                return Equals(quizObj);
            }
        }

        public override int GetHashCode()
        {
            return Question.GetHashCode();
        }

        public static bool operator ==(QuizObject quest1, QuizObject quest2)
        {
            if ((object)quest1 == null || (object)quest2 == null)
            {
                return Object.Equals(quest1, quest2);
            }
            else
            {
                return quest1.Equals(quest2);
            }
        }

        public static bool operator !=(QuizObject quest1, QuizObject quest2)
        {
            if ((object)quest1 == null || (object)quest2 == null)
            {
                return !Object.Equals(quest1, quest2);
            }
            else
            {
                return !(quest1.Equals(quest2));
            }
        }


        public String Question { get; set; }
        public String Answer { get; set; }
    }

    public class ScoreObject : IEquatable<ScoreObject>
    {
        public ScoreObject(String inName, int inScore)
        {
            Name = inName;
            Score = inScore;
        }

        public ScoreObject(String inName) : this(inName, 0) { }

        public bool Equals(ScoreObject other)
        { 
            if( other == null)
            {
                return false;
            }
            else if (this.Name == other.Name)
            {
                return true;
            }
            else 
            {
                return false;    
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ScoreObject scoreObj = obj as ScoreObject;
            if (scoreObj == null)
            {
                return false;
            }
            else {
                return Equals(scoreObj);
            }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static bool operator == (ScoreObject score1, ScoreObject score2)
        {
            if ((object)score1 == null || (object)score2 == null)
            {
                return Object.Equals(score1,score2);
            }
            else {
                return score1.Equals(score2);
            }
        }

        public static bool operator != (ScoreObject score1, ScoreObject score2)
        {
            if ((object)score1 == null || (object)score2 == null)
            {
                return ! Object.Equals(score1, score2);
            }
            else
            {
                return ! (score1.Equals(score2));
            }
        }

        public override string ToString()
        {
            return String.Format("Name: {0} Score: {1}",Name,Score);
        }

        public String Name { get; set; }
        public int Score { get; set; }
    }

    public class QuizObjectsList : ObservableCollection<QuizObject>
    { 
        
    }

    public static class ScoreObjectExtensions
    {
        public static ScoreObject AsScoreObject(this String inName)
        {
            return new ScoreObject(inName);
        }
    }

    public static partial class StringExtensions
    {
        public static string ReplaceCharAtIndex(this string inString, int inIndex, char inNewChar )
        {
            if (inString == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = inString.ToCharArray();
            chars[inIndex] = inNewChar;
            return new string(chars);
        }

        public static string ReplaceCharAtIndex(this string inString, int[] inIndexes, char inNewChar )
        {
            if (inString == null)
            {
                throw new ArgumentNullException("input");
            }
            char[] chars = inString.ToCharArray();
            for(int i = 0; i < inIndexes.Length; i++)
            {
                chars[inIndexes[i]] = inNewChar;
            }
            
            return new string(chars);
        }

    }

    public class QuizHint
    {
        public QuizHint(string inAnswer)
        {
            mAnswer = inAnswer;
            mHint = new string('_', inAnswer.Length);
            
            if(inAnswer.Contains(' '))
            {
                List<int> indexesOfSpaces = new List<int>();
                for(int indexOfSpace = 0; indexOfSpace < inAnswer.Length; indexOfSpace++)
                {
                   if(inAnswer[indexOfSpace] == ' ')
                   {
                        indexesOfSpaces.Add(indexOfSpace);
                   }
                }
                mHint = mHint.ReplaceCharAtIndex(indexesOfSpaces.ToArray(),' ');
            }

            //for(int i = 0; i < mAnswer.Length; i++)
            //{
            //    if(mAnswer[i] == ' ')
            //    {
            //        mHint = mHint.ReplaceCharAtIndex(i,' ');    
            //    }
            //}

            HintNum = 0;
        }

        public string GiveAHint()
        {
            HintNum++;
            OpenChar();
            return mHint;
        }

        void OpenChar()
        {
            if ( (HintNum - 2) >= 0 && (HintNum - 1) < mHint.Length && (mHint.Where(p=>p=='_').Count() > 1) ) {
                //mHint[HintNum - 1] = mAnswer[HintNum - 1];
                char[] temp = new char[mHint.Length];
                mHint.CopyTo(0, temp, 0, mHint.Length);

                int rndIndex = rnd.Next(mHint.Length);
                temp[rndIndex] = mAnswer[rndIndex];
                //temp[HintNum - 2] = mAnswer[HintNum - 2];
                mHint = new string(temp);
            }
        }

        static Random rnd = new Random();

        static int HintNum
        {
            get;
            set;
        }

        string mHint;

        string mAnswer;
    }



	public class QuizEngine : INotifyPropertyChanged
	{
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

		public QuizEngine ()
		{
			
            mQuizList = new QuizObjectsList();
            mScoreList = new List<ScoreObject>();
            mDispatchTable = new Dictionary<string, Action<string>>();
            mDispatchTable["!ShowScore"] = ShowScore;
            mDispatchTable["!RepeatQuestion"] = RepeatQuestion;
            mDispatchTable["!Commands"] = Commands;

            mIncomingMessagesQueue = new Queue<IrcCommand>();
            mScore = new Dictionary<string, int>();
            mTimeBetweenQuestions = 120000;
            mTimeBetweenHints = 15000;
            IsRandom = false;
		}

		public QuizEngine (string inFileWithQuiz) : this()
		{
            AddQuiz(inFileWithQuiz);
		}

		public QuizEngine (string[] inQuiz) : this()
		{
			ProcessStringsArrayAsQuiz(inQuiz);
		}

        public async Task AddQuiz() {
            await AddQuiz(QuizFile);
        }

        void Processing()
        {
            using (FileStream fs = File.OpenRead(QuizFile))
            {

                string[] linesOfFile = File.ReadAllLines(QuizFile);
                ProcessStringsArrayAsQuiz(linesOfFile);
            }
          

           
        }

        async public Task  AddQuiz(string inFileWithQuiz)
        {
            QuizFile = inFileWithQuiz;
        	if (File.Exists (inFileWithQuiz)) {
                await Task.Factory.StartNew((System.Action)Processing, CancellationToken.None,TaskCreationOptions.None,TaskScheduler.FromCurrentSynchronizationContext());
                //await Task.Run((System.Action)Processing);
			} else {
				throw new FileNotFoundException();
			}
        }

		void ProcessStringsArrayAsQuiz(string[] inStringsArray)
		{
			string[] separators = new string[]{"{Question}","{Answer}"};

			for(int i = 0; i < inStringsArray.Length; i++ ){
				string[] result = inStringsArray[i].Split(separators,StringSplitOptions.RemoveEmptyEntries);
				if(result.Length == 2)
				{
                    mQuizList.Add(new QuizObject(result[0], result[1]));
					
				}
			}

            QuizList = QuizList;
            QuizListOrig = QuizListOrig;
		}

        public void AddNewQuizObject(string inQuestion, string inAnswer)
        {
            mQuizList.Add(new QuizObject(inQuestion, inAnswer));
            //QuizList = QuizList;
        }

        public void DropTheQuizList()
        {
            mQuizList.Clear();
        }

        public void DropTheItemFromList(QuizObject inObjectToDrop)
        {
            mQuizList.Remove(inObjectToDrop);
        }

		public void Process (IrcCommand inCommand)
		{
			if (inCommand.Name == "PRIVMSG") {
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
            while (!ct.IsCancellationRequested)
            {
                Task<IrcCommand> tic = Task.Run((Func<Task<IrcCommand>>)GetMessageFromQ,ct);
                IrcCommand ic = await tic;

                //check privmsg for being a command
                if (ic.Prefix != null)
                {
                    string message = ic.Parameters[ic.Parameters.Length - 1].Value;
                    if (message.Length > 0 && message[0] == '!')
                    {
                        int indexOfExclamationSign = ic.Prefix.IndexOf('!');
                        string name = ic.Prefix.Substring(0, indexOfExclamationSign);
                        //then its a bot-command
                        ProcessIncomingBotCommand(message,name);
                    }
                }


                //here we have a privmsg and have to check for a valid answer
                if (mCurrentObject != null && ic.Prefix != null)
                {

                    Console.WriteLine("{0} is guessed it is \"{1}\" ({2})!", ic.Prefix, ic.Parameters[ic.Parameters.Length - 1].Value, mCurrentObject.Answer);

                    if (ic.Parameters[ic.Parameters.Length - 1].Value.ToLower() == mCurrentObject.Answer.ToLower())
                    {

                        int indexOfExclamationSign = ic.Prefix.IndexOf('!');
                        string name = ic.Prefix.Substring(0, indexOfExclamationSign);

                        if (mScore.ContainsKey(name))
                        {
                            mScore[name]++;
                        }
                        else 
                        {
                            mScore[name] = 1;
                        }

                        ScoreObject scoreObj = name.AsScoreObject();
                        if (mScoreList.Contains(scoreObj))
                        {
                            mScoreList[mScoreList.IndexOf(scoreObj)].Score++;
                        }
                        else 
                        {
                            scoreObj.Score++;
                            mScoreList.Add(scoreObj);
                        }
                        Score = Score; //Notifying


                        //Console.WriteLine("{0} is right, it is \"{1}\" !", name, mCurrentQAPair.Item2);
                        string message = String.Format("{0} is right, it is \"{1}\", {0}'s score is {2}!", name, mCurrentObject.Answer, mScore[name]);
                        //SendMessage(new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter(message, true)).ToString() + "\r\n");
                        SendMessage(message);
                        OnTimeToAskAQuestion(null, null);
                    }
                    Console.WriteLine("after GetMessageFromQ:{0} ", ic.Name);
                }
            }
        }



        public void StopQuiz()
        {
            if (cts != null)
            {
                mTimeToAskAQuestion.Enabled = false;
                mTimeToGiveAHint.Enabled = false;
                mTimeTillNextQuestion.Enabled = false;
                cts.Cancel();
                cts = null;
            }
        }

        async public void StartQuiz()
        {
            if (mQuizList.Count == 0) {
                throw new InvalidDataException("mQuizList is empty!");
            }

            if (cts != null) {
                mTimeToAskAQuestion.Enabled = false;
                mTimeToGiveAHint.Enabled = false;
                cts.Cancel();
            }
            cts = new CancellationTokenSource();

            mTimeToAskAQuestion = new System.Timers.Timer(mTimeBetweenQuestions);
            mTimeToGiveAHint = new System.Timers.Timer(mTimeBetweenHints);
            mTimeTillNextQuestion = new System.Timers.Timer(1000);

            TimeTillNextQuestion = TimeBetweenQuestions;

            mTimeToAskAQuestion.Elapsed += OnTimeToAskAQuestion;
            mTimeToAskAQuestion.Enabled = true;
            

            mTimeToGiveAHint.Elapsed += OnTimeToGiveAHint;
            mTimeTillNextQuestion.Elapsed += ReduceTimeTillQuestion;
            //OnTimeToAskAQuestion(null,null);
            OnTimeToAskAQuestion(null, null);
            await ReadIncomingMessages(cts.Token);
        }

        void ReduceTimeTillQuestion(object source, ElapsedEventArgs e)
        {
            TimeTillNextQuestion--;
        }

        void OnTimeToAskAQuestion(object source, ElapsedEventArgs e)
        {
            //if there is no more items in Q, then the exception will be rised
            if (mQuizList.Count == 0) {
                return;
                
            }

            CurrentQuizObject = GetQuizObject();
            mQuizHint = new QuizHint(CurrentQuizObject.Answer);
            mTimeToGiveAHint.Enabled = true;

            TimeTillNextQuestion = TimeBetweenQuestions / 1000;
            mTimeTillNextQuestion.Enabled = true;
            //SendMessage(new IrcCommand(null,"PRIVMSG", new IrcCommandParameter("#sovietmade",false), new IrcCommandParameter(mCurrentQAPair.Item1,true)).ToString() + "\r\n");
            SendMessage(CurrentQuizObject.Question);
        }

        public void AskScpecifiedQuestion(QuizObject inQuizObject)
        {
            CurrentQuizObject = inQuizObject;
            mQuizHint = new QuizHint(CurrentQuizObject.Answer);
            mTimeToGiveAHint.Enabled = true;

            TimeTillNextQuestion = TimeBetweenQuestions / 1000;
            mTimeTillNextQuestion.Enabled = true;
            //SendMessage(new IrcCommand(null,"PRIVMSG", new IrcCommandParameter("#sovietmade",false), new IrcCommandParameter(mCurrentQAPair.Item1,true)).ToString() + "\r\n");
            SendMessage(CurrentQuizObject.Question);
        }

        void OnTimeToGiveAHint(object source, ElapsedEventArgs e)
        {
            string currentHint = mQuizHint.GiveAHint();
            //SendMessage(new IrcCommand(null, "PRIVMSG", new IrcCommandParameter("#sovietmade", false), new IrcCommandParameter(currentHint, true)).ToString() + "\r\n");
            SendMessage(currentHint);
        }

        public int TimeBetweenQuestions 
        {
            get {
                return mTimeBetweenQuestions;
            }
            set {
                mTimeBetweenQuestions = value;
                if(mTimeToAskAQuestion != null)
                {
                    mTimeToAskAQuestion.Interval = value;
                }
            }
        }

        public int TimeBetweenHints
        {
            get
            {
                return mTimeBetweenHints;
            }
            set
            {
                mTimeBetweenHints = value;
                if (mTimeToGiveAHint != null)
                {
                    mTimeToGiveAHint.Interval = value;
                }
            }
        }

        public int TimeTillNextQuestion
        {
            get 
            {
                return TillNextQuestion;
            }
            set 
            {
                TillNextQuestion = value;
                NotifyPropertyChanged();
            } 
        }
        int TillNextQuestion;

        public int TimeTillNextHint { get; set; }


        bool quizIsRunning;
        public QuizObjectsList QuizListOrig
        {
            get
            {
                return mQuizList;
            }
            set
            {
                //mQuizList = value;
                NotifyPropertyChanged();
            }
        }

        public QuizObject[] QuizList
        {
            get {
                return mQuizList.ToArray();
            }
            set
            {
                NotifyPropertyChanged();
            }
        }

        public ScoreObject[] Score
        {
            get {
                return mScoreList.ToArray();
            }
            set {
                NotifyPropertyChanged();
            }
        }

        public bool IsRandom
        {
            get;
            set;
        }

        public int GetRandomIndex()
        {
            return rnd.Next(mQuizList.Count);
        }

        public void PreviousQuestion()
        {
            indexOfCurrentQuizObjext -= 2;
            OnTimeToAskAQuestion(null, null);
        }

        public void NextQuestion()
        {
            OnTimeToAskAQuestion(null, null);
        }

        QuizObject GetQuizObject()
        {
            if (IsRandom)
            {
                indexOfCurrentQuizObjext = GetRandomIndex();
            }
            else
            {
                indexOfCurrentQuizObjext++;
            }
            return indexOfCurrentQuizObjext < 0 ? mQuizList[(indexOfCurrentQuizObjext + mQuizList.Count) % (mQuizList.Count)] : mQuizList[indexOfCurrentQuizObjext % (mQuizList.Count)];
        }

        public string QuizFile { get; set; }

        //delegate for communicating back to the outer world (assigned to SendMessageToCurrentChannel in TwitchBot contructor)
		public Action<string> SendMessage;

        //Queue of PRIVMSGes - source of answers
		Queue<IrcCommand> mIncomingMessagesQueue;
		

        QuizObjectsList mQuizList;
        int indexOfCurrentQuizObjext = -1;
        Random rnd = new Random();
        List<ScoreObject> mScoreList;

        public QuizObject CurrentQuizObject
        {
            get {
                return mCurrentObject;
            }
            set {
                mCurrentObject = value;
                NotifyPropertyChanged();
            }
        }

        QuizObject mCurrentObject;

        System.Timers.Timer mTimeToAskAQuestion;
        System.Timers.Timer mTimeToGiveAHint;
        System.Timers.Timer mTimeTillNextQuestion;
 
        Dictionary<string, int> mScore;

        CancellationTokenSource cts;
        
        int mTimeBetweenQuestions;
        int mTimeBetweenHints;

        QuizHint mQuizHint;

        #region Bot Commands Dispatching



        void ProcessIncomingBotCommand(string inMessage, string inSender)
        {
            if (mDispatchTable.ContainsKey(inMessage))
            {
                mDispatchTable[inMessage].Invoke(inSender);
            }
            
        }

        void ShowScore(string inSender)
        {
            string score;
            if (mScoreList.Contains(new ScoreObject(inSender)))
            {
                int indexOfSenderScore = mScoreList.IndexOf(new ScoreObject(inSender));
                score = inSender + ", your score is " + mScoreList[indexOfSenderScore].Score;
                //String.Join(" ", mScoreList.Where(p => (p.Name == inSender)).Select(p => p.Score));
            }
            else 
            {
                score = inSender + ", your score is 0";
            }
            SendMessage(score);
        }

        void RepeatQuestion(string inSender)
        {
            SendMessage(mCurrentObject.Question);
        }

        void Commands(string inSender)
        {
            string availableCommands = "Available commands: " + String.Join(" ",mDispatchTable.Select(p=>p.Key));
            SendMessage(availableCommands);
        }

        Dictionary<string, Action<string> > mDispatchTable;

        #endregion
    }
}

