using System;
using System.Linq;
using System.Collections.Generic;

namespace TwitchChatBot
{
	public class IrcCommand
	{
		public IrcCommand(string inPrefix,string inName, params IrcCommandParameter[] inParams)
		{
			mPrefix = inPrefix;
			mName = inName;
			mParams = inParams;
		}

		/*	IRC-command format
			<message>  ::= [':' <prefix> <SPACE> ] <command> <params> <crlf>
			<prefix>   ::= <servername> | <nick> [ '!' <user> ] [ '@' <host> ]
			<command>  ::= <letter> { <letter> } | <number> <number> <number>
			<SPACE>    ::= ' ' { ' ' }
			<params>   ::= <SPACE> [ ':' <trailing> | <middle> <params> ]

			<middle>   ::= <Any *non-empty* sequence of octets not including SPACE
			               or NUL or CR or LF, the first of which may not be ':'>
			<trailing> ::= <Any, possibly *empty*, sequence of octets not including
			                 NUL or CR or LF>

			<crlf>     ::= CR LF
		*/

		public static IrcCommand Parse (string inCommandString)
		{
			string prefix = null, name = null;
            List<IrcCommandParameter> parameters = new List<IrcCommandParameter>(); 
			if (inCommandString [0] == ':') {
				prefix = inCommandString.Substring(1,inCommandString.IndexOf(' ') - 1);
				inCommandString = inCommandString.Substring(inCommandString.IndexOf(' ') + 1);
			}
			name = inCommandString.Substring(0,inCommandString.IndexOf(' '));
			inCommandString = inCommandString.Substring(inCommandString.IndexOf(' ') + 1);

            while (inCommandString.Length > 0)
            {
                if (inCommandString[0] == ':')
                {
                    parameters.Add(inCommandString.Substring(1).AsIrcCommandTraillingParameter());
                    inCommandString = "";
                }
                else 
                {
                    int nextParamEnd = inCommandString.IndexOf(' ');
                    if (nextParamEnd == -1) 
                    {
                        nextParamEnd = inCommandString.Length;
                    }
                    string nextParam = inCommandString.Substring(0, nextParamEnd);
                    parameters.Add(new IrcCommandParameter(nextParam,false)); //TODO: create an extension for not trailling - parameter case
                    if (nextParamEnd != inCommandString.Length)
                    {
                        inCommandString = inCommandString.Substring(nextParamEnd + 1);
                    }
                    else
                    {
                        inCommandString = "";
                    }
                }
            }

			return new IrcCommand(prefix,name,parameters.ToArray()); 
		}

		public override string ToString ()
		{
			return (mPrefix == null ? "" : ":" + mPrefix + " ") + mName + " " + String.Join(" ",mParams.Select(p=>p.ToString()));
		}

		public string Name {
			get{
				return mName;
			}
		}

        public string Prefix {
            get {
                return mPrefix;
            }
        }

        public IrcCommandParameter[] Parameters{
            get {
                return mParams;
            }
        }

		string mPrefix;
		string mName;
		IrcCommandParameter[] mParams;

	}

	public class IrcCommandParameter
	{
		bool mTrailling;
		string mValue;

		public IrcCommandParameter ( string inValue, bool inTrailling = false )
		{
			mValue = inValue;
			mTrailling = inTrailling;
		}

		public override string ToString ()
		{
			return (mTrailling ? ":" : "") + mValue;
		}

        public string Value
        {
            get {
                return mValue;
            }
        }


	}

    public static class IrcCommandParameterExtensions
    {
        public static IrcCommandParameter AsIrcCommandTraillingParameter(this string inParameter) 
        {
            return new IrcCommandParameter(inParameter, true);
        }
    }

    public abstract class IrcCommandAnalyzer
    {
        public abstract IrcCommand GetResponse(IrcCommand inRequestCommand);

    }

    public class SimpleTwitchBotIrcCommandAnalyzer : IrcCommandAnalyzer
    {
        public SimpleTwitchBotIrcCommandAnalyzer()
        {
            mDispatchTable = new Dictionary<string, Func<IrcCommand, IrcCommand>>();
            mDispatchTable["PING"] = Ping;
        }

        public override IrcCommand GetResponse(IrcCommand inRequestCommand)
        {
            return mDispatchTable.ContainsKey(inRequestCommand.Name) ? mDispatchTable[inRequestCommand.Name].Invoke(inRequestCommand) : null;
            
        }

        IrcCommand Ping(IrcCommand inCommand) 
        {
            Console.WriteLine("Ping-Pong");
            return new IrcCommand(null, "PONG", inCommand.Parameters);
        }

		IrcCommand Privmsg(IrcCommand inCommand)
		{
			return null;
		}

        Dictionary<string, Func<IrcCommand, IrcCommand>> mDispatchTable;
    }



}

