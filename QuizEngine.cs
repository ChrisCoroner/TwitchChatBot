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
using System.Net;
using System.Web.Script.Serialization;
using System.Runtime.Serialization;
using System.Drawing;

namespace TwitchChatBot
{

    static class LevenshteinDistance
    {
        public static int Compute(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }


    public class QuizObject : IEquatable<QuizObject>
    {
        public QuizObject(String inQuestion, String inAnswer)
        {
            Question = inQuestion;
            Answer = inAnswer;
            if (!String.IsNullOrEmpty(inQuestion))
            {
                if (Uri.IsWellFormedUriString(inQuestion, UriKind.RelativeOrAbsolute))
                {
                    isImageQuestion = true;
                    HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(inQuestion);

                    using (HttpWebResponse httpWebReponse = (HttpWebResponse)httpWebRequest.GetResponse())
                    {
                        using (Stream stream = httpWebReponse.GetResponseStream())
                        {
                            mImageQuestion = Image.FromStream(stream);
                        }
                    }
                }
            }

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

        public override string ToString()
        {
            return "{Question}" + Question + "{Answer}" + Answer;
        }

        public bool IsAnswered()
        {
            return isAnswered;
        }
        public void SetAnswered(bool inAnswered)
        {
            isAnswered = inAnswered;
        }
        public bool IsImageQuestion()
        {
            return isImageQuestion;
        }

        public Image GetImageQuestion()
        {
            return mImageQuestion;
        }

        bool isAnswered = false;
        bool isImageQuestion = false;

        Image mImageQuestion = null;

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
            mDispatchTable["!Top5"] = Top5;
            mDispatchTable["!TopX"] = Top5;
            mDispatchTable["!ChuckNorris"] = ChuckNorris;
            //mDispatchTable["!GetRandomQuote"] = GetRandomQuote;

            mIncomingMessagesQueue = new Queue<IrcCommand>();
            mScore = new Dictionary<string, int>();
            mTimeBetweenQuestions = 120000;
            mTimeBetweenHints = 15000;
            DelayBetweenQuestions = 10000;
            IsRandom = false;
            ForgiveSmallMisspelling = false;
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

        public void AppendQuizObjectToTheQuizFile(QuizObject qo)
        {
            string[] quizObjectLine = new string[] { qo.ToString() };
            if (File.Exists(QuizFile))
            {
                File.AppendAllLines(QuizFile, quizObjectLine);
            }
        }

		void ProcessStringsArrayAsQuiz(string[] inStringsArray)
		{
			string[] separators = new string[]{"{Question}","{Answer}"};

			for(int i = 0; i < inStringsArray.Length; i++ ){
				string[] result = inStringsArray[i].Split(separators,StringSplitOptions.RemoveEmptyEntries);
				if(result.Length % 2 == 0)
				{
                    for (int j = 0; j < result.Length; j = j + 2)
                    {
                        mQuizList.Add(new QuizObject(result[j], result[j+1]));
                    }
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

                if(ic != null){
                    Console.WriteLine("InCommand: " + ic.ToString());
                }

                //check privmsg for being a command
                if (ic != null && ic.Prefix != null)
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
                if (quizIsRunning && mCurrentObject != null && !(mCurrentObject.IsAnswered()) && ic != null &&  ic.Prefix != null)
                {

                    Console.WriteLine("{0} is guessed it is \"{1}\" ({2})!", ic.Prefix, ic.Parameters[ic.Parameters.Length - 1].Value, mCurrentObject.Answer);

                    bool IsRightAnswer = false;

                    if (ForgiveSmallMisspelling)
                    {
                        int distance = LevenshteinDistance.Compute(mCurrentObject.Answer.ToLower(), ic.Parameters[ic.Parameters.Length - 1].Value.ToLower());
                        double percentage = ((double)distance / (double)mCurrentObject.Answer.Length) * 100;
                        if (percentage < 25)
                        {
                            IsRightAnswer = true;
                        }
                    }
                    else
                    {
                        if (ic.Parameters[ic.Parameters.Length - 1].Value.ToLower() == mCurrentObject.Answer.ToLower())
                        {
                            IsRightAnswer = true;
                        }
                    }


                    if (IsRightAnswer)
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
                        string commandL = ProcessLoyalityCommand(name);
                        if (commandL != null)
                        {
                            SendMessage(commandL);
                        }
                        mCurrentObject.SetAnswered(true);
                        OnTimeToAskAQuestion(null, null);
                    }
                    Console.WriteLine("after GetMessageFromQ:{0} ", ic.Name);
                }
            }
        }

        public string ProcessLoyalityCommand(string inName)
        {
            string command = LoyalityCommand;
            if (!(String.IsNullOrEmpty(LoyalityCommand)) && inName != null)
            {
                if (!(LoyalityCommand.Contains("*UserName*")))
                {
                    return null;
                }
                command = command.Replace("*UserName*", inName);
                
            }
            return command;
        }

        public void StopQuiz()
        {
            quizIsRunning = false;

            if (ctsDelay != null) {
                ctsDelay.Cancel();
            }

            mTimeToAskAQuestion.Enabled = false;
            mTimeToGiveAHint.Enabled = false;
            mTimeTillNextQuestion.Enabled = false;

            string message = "Quiz is about to stop.";

            if ( mCurrentObject != null && !(mCurrentObject.IsAnswered()))
            {
                mCurrentObject.SetAnswered(true);
                string messageAppend = String.Format("The answer was: {0}!", mCurrentObject.Answer);
                message = message + messageAppend;
            }
            SendMessage(message);
        }

        async public void BeginAcceptMessages()
        {
            if (cts != null)
            {
                cts.Cancel();
            }

            cts = new CancellationTokenSource();

            if (cts != null) // will be set to null in StopQuiz,so have to check again after OnTimeToAskAQuestion 
            {
                await ReadIncomingMessages(cts.Token);
            }
        }

        public void EndAcceptMessages()
        {
            //stop all running entertainment things
            if (quizIsRunning)
            {
                InternalInitiationQuizStop();
               
            }

            if (cts != null)
            {
                cts.Cancel();

                cts = null;
            }
        }

        async public void StartQuiz()
        {
            if (mQuizList.Count == 0) {
                throw new InvalidDataException("mQuizList is empty!");
            }

            quizIsRunning = true;
            
            mTimeToAskAQuestion = new System.Timers.Timer(mTimeBetweenQuestions);
            mTimeToGiveAHint = new System.Timers.Timer(mTimeBetweenHints);
            mTimeTillNextQuestion = new System.Timers.Timer(1000);

            TimeTillNextQuestion = TimeBetweenQuestions;

            mTimeToAskAQuestion.Elapsed += OnTimeToAskAQuestion;
            mTimeToGiveAHint.Elapsed += OnTimeToGiveAHint;

            mTimeTillNextQuestion.Elapsed += ReduceTimeTillQuestion;
            
            OnTimeToAskAQuestion(null, null);

        }

        void ReduceTimeTillQuestion(object source, ElapsedEventArgs e)
        {
            TimeTillNextQuestion--;
        }

        async Task DelayExecution(CancellationToken ct)
        {
            while (!(ct.IsCancellationRequested))
            {
                await Task.Delay(DelayBetweenQuestions);
                ctsDelay.Cancel();
            }
        }

        async void OnTimeToAskAQuestion(object source, ElapsedEventArgs e)
        {
            mTimeToGiveAHint.Stop();
            mTimeToAskAQuestion.Stop();
            mTimeTillNextQuestion.Stop();

            //if there is no more items in Q, then the exception will be rised
            if (mQuizList.Count == 0) {
                return;
                
            }

            if (ctsDelay != null)
            {
                ctsDelay.Cancel();
            }
            ctsDelay = new CancellationTokenSource();


            if ( mCurrentObject != null && mCurrentObject.IsAnswered() == false) {
                string message = String.Format("The answer was: {0}!", mCurrentObject.Answer);
                mCurrentObject.SetAnswered(true);
                SendMessage(message);
            }

            CurrentQuizObject = GetQuizObject();
            
            if (CurrentQuizObject == null)
            {
                InternalInitiationQuizStop();
                return;
            }

            mQuizHint = new QuizHint(CurrentQuizObject.Answer);

            TimeTillNextQuestion = TimeBetweenQuestions / 1000;
            
            await DelayExecution(ctsDelay.Token);
            if (mCurrentObject == null || cts == null || !quizIsRunning) {
                return;
            }
            mTimeToGiveAHint.Start();
            mTimeToAskAQuestion.Start();

            
            mTimeTillNextQuestion.Start();
            //SendMessage(new IrcCommand(null,"PRIVMSG", new IrcCommandParameter("#sovietmade",false), new IrcCommandParameter(mCurrentQAPair.Item1,true)).ToString() + "\r\n");
            SendMessage(CurrentQuizObject.Question);
            if (CurrentQuizObject.IsImageQuestion())
            {
                ShowStaff(CurrentQuizObject.GetImageQuestion());
            }
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

        public int DelayBetweenQuestions
        {
            get;
            set;
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

        public string LoyalityCommand
        {
            get;
            set;
        }

        bool quizIsRunning = false;
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

        public bool ForgiveSmallMisspelling
        {
            get;
            set;
        }

        public int GetRandomIndex()
        {
            return rnd.Next(mQuizList.Count);
        }

        public int GetRandomIndex(int Limit)
        {
            return rnd.Next(Limit);
        }

        public void PreviousQuestion()
        {
            
            if (mCurrentObject != null && mCurrentObject.IsAnswered() == false)
            {
                string message = String.Format("The answer was: {0}!", mCurrentObject.Answer);
                mCurrentObject.SetAnswered(true);
                SendMessage(message);
            }

            if (mCurrentObject != null && (indexOfCurrentQuizObjext - 1) >= 0)
            {
                indexOfCurrentQuizObjext = indexOfCurrentQuizObjext - 1;
                mQuizList[indexOfCurrentQuizObjext].SetAnswered(false);
                OnTimeToAskAQuestion(null, null);
            }
            
            
        }

        public void NextQuestion()
        {
            if (mCurrentObject != null && mCurrentObject.IsAnswered() == false)
            {
                string message = String.Format("The answer was: {0}!", mCurrentObject.Answer);
                mCurrentObject.SetAnswered(true);
                SendMessage(message);
            }
            OnTimeToAskAQuestion(null, null);
        }

        QuizObject GetQuizObject()
        {
            QuizObject[] isNotAnsweredQuestions = mQuizList.Where(p=> p.IsAnswered() != true).ToArray();
            if (isNotAnsweredQuestions.Count() == 0)
            {
                return null;
            }
            if (IsRandom)
            {
                int temp = GetRandomIndex(isNotAnsweredQuestions.Count());
                indexOfCurrentQuizObjext = mQuizList.IndexOf(isNotAnsweredQuestions[temp]);
                return isNotAnsweredQuestions[temp];
            }
            else {
                indexOfCurrentQuizObjext = mQuizList.IndexOf(isNotAnsweredQuestions.ElementAt(0));
                return isNotAnsweredQuestions.ElementAt(0);
            }
            //if (IsRandom)
            //{
            //    indexOfCurrentQuizObjext = GetRandomIndex();
            //}
            //else
            //{
            //    indexOfCurrentQuizObjext++;
            //}
            //return indexOfCurrentQuizObjext < 0 ? mQuizList[(indexOfCurrentQuizObjext + mQuizList.Count) % (mQuizList.Count)] : mQuizList[indexOfCurrentQuizObjext % (mQuizList.Count)];
        }

        public string QuizFile { get; set; }

        //delegate for communicating back to the outer world (assigned to SendMessageToCurrentChannel in TwitchBot contructor)
		public Action<string> SendMessage;

        public Action InternalInitiationQuizStop;

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

        public Action<Image> ShowStaff
        {
            get;
            set;
        }



        QuizObject mCurrentObject;

        System.Timers.Timer mTimeToAskAQuestion;
        System.Timers.Timer mTimeToGiveAHint;
        System.Timers.Timer mTimeTillNextQuestion;
 
        Dictionary<string, int> mScore;

        CancellationTokenSource cts;
        CancellationTokenSource ctsDelay;

        int mTimeBetweenQuestions;
        int mTimeBetweenHints;

        QuizHint mQuizHint;

        #region Bot Commands Dispatching

        // { "type": "success", "value": { "id": 120, "joke": "Chuck Norris played Russian Roulette with a fully loaded gun and won.", "categories": [] } }

        class ChuckNorrisJoke
        {
            public int id { get; set; }
            public string joke { get; set; }
            public string[] categories { get; set; } 
        }

        class ChuckNorrisObject
        {
            public string type { get; set; }
            public ChuckNorrisJoke value { get; set; }
        }

        public void ChuckNorris(string inSender)
        {
            WebRequest request = WebRequest.Create("http://api.icndb.com/jokes/random");
            HttpWebResponse httpWebResponse = (HttpWebResponse)request.GetResponse();
            Stream dataStream = httpWebResponse.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader dataReader = new StreamReader(dataStream);
            // Read the content. 
            string responseFromServer = dataReader.ReadToEnd();
            ChuckNorrisObject CNO = new JavaScriptSerializer().Deserialize<ChuckNorrisObject>(responseFromServer);
            Console.WriteLine(CNO.value.joke);
            SendMessage(CNO.value.joke);
            
        }

        void TopX(int inX)
        {
            string score;
            var result = mScoreList.OrderBy(p => p.Score).Where((p, i) => i < inX).Select(p => p.ToString());
            score = String.Join(" ", result);

            SendMessage(score);
        }

        void ProcessIncomingBotCommand(string inMessage, string inSender)
        {
            if (inMessage.StartsWith("!Top"))
            {
                if (inMessage.Length > 4)
                {
                    string sReminder = inMessage.Substring(4);
                    int X = 0;
                    if (Int32.TryParse(sReminder, out X))
                    {
                        TopX(X);
                        return;
                    }
                    else {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

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

        void Top5(string inSender)
        {
            string score;
            var result = mScoreList.OrderBy(p=>p.Score).Where((p,i)=>i<5).Select(p=>p.ToString());
            score = String.Join(" ",result);

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

