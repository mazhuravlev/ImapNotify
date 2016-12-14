using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Win32;
using S22.Imap;
using System.Net.Mail;

namespace ImapNotify {
	public delegate void NewMessageEventHandler(object sender, NewMessageEventArgs e);

	class Imap {
		private static ImapClient IC;
		private static bool IsRunning = false;
		public static event NewMessageEventHandler NewMessageEvent;

	    private static string Username;
	    private static string Password;

        private const string Host = "imap.yandex.com";
		private const int Port = 993;
		private const bool SSL = true;

	    public static void SetCredentials(string login, string password)
	    {
            Username = login;
            Password = password;
        }

		public static void Start()
		{
			IC = new ImapClient(Host, Port, Username, Password, AuthMethod.Login, SSL);
			IsRunning = true;

			/* Does NOT run in the context of the "UI thread" but in its _own_ thread */
			IC.NewMessage += (sender, e) => {
				MailMessage m = null;
				int messageCount;
				lock (IC) {
				    try
				    {
				        m = IC.GetMessage(e.MessageUID, FetchOptions.TextOnly, false);
				        messageCount = IC.Search(SearchCondition.Unseen()).Count();
                    }
                    catch (IOException)
                    {
                        return;
                    }
                };
				NewMessageEventArgs args = new NewMessageEventArgs(m, messageCount);
				NewMessageEvent(sender, args);
			};
		}

		public static void Stop() {
			if (IsRunning)
			    lock (IC)
			    {
			        IC.Dispose();
			    }
		    IsRunning = false;
		}

		public static bool Started() {
			return IsRunning;
		}

		public static int GetUnreadCount() {
			if (!IsRunning)
				return 0;
		        lock (IC)
		        {
		            try
		            {
		                return IC.Search(SearchCondition.Unseen()).Count();
		            }
		            catch (IOException)
		            {
		                return 0;
		            }
		        }
		}

	    public static bool VerifyCredentials(string login, string password)
	    {
	        try
	        {
	            var client = new ImapClient(Host, Port, login, password, AuthMethod.Login, SSL);
	        }
	        catch (InvalidCredentialsException)
	        {
	            return false;
	        }
            return true;
        }
	}

	public class NewMessageEventArgs : EventArgs {
		public NewMessageEventArgs(MailMessage m, int u) {
			this.Message = m;
			this.UnreadMails = u;
		}

		public MailMessage Message;
		public int UnreadMails;
	}
}
