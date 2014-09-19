using System;
using System.Linq;

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
			if (inCommandString [0] == ':') {
				prefix = inCommandString.Substring(1,inCommandString.IndexOf(' ') - 1);
				inCommandString = inCommandString.Substring(inCommandString.IndexOf(' ') + 1);
			}
			name = inCommandString.Substring(0,inCommandString.IndexOf(' '));
			inCommandString = inCommandString.Substring(inCommandString.IndexOf(' ') + 1);

			return new IrcCommand(prefix,name); 
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
	}
}

