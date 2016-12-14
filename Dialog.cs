using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Win32;

namespace ImapNotify {
	public partial class Dialog : Form {
		private bool reallyClose = false;
		private Icon IconOld;
		private string soundPath;

        private const int NotifyTimeout = 60 * 60 * 1000;

		public Dialog()
		{
		    while (!Util.IsInternetConnected())
		    {
                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
            Util.CheckInitRegistry();

            InitializeComponent();
			IconOld = notifyIcon.Icon;
			soundPath = Path.GetDirectoryName(
				Assembly.GetExecutingAssembly().Location) + @"\Notify.wav";
			if (!File.Exists(soundPath)) {
				soundPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) +
					@"\Media\Windows Notify.wav";
			}
			Location = new Point(
				Screen.PrimaryScreen.WorkingArea.Width - Width,
				Screen.PrimaryScreen.WorkingArea.Height - Height);
			NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(OnNetworkAvailabilityChanged);
			SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(OnPowerModeChanged);
			Imap.NewMessageEvent += new NewMessageEventHandler(OnNewMessage);

		    var login = Environment.UserName + "@oblpro.ru";
            if (!(new Regex(@"\w+_\w{2}@oblpro\.ru").IsMatch(login)))
		    {
                DisplayOkno();
                Environment.Exit(0);
            }

		    var password = Util.GetPassword() ?? PromptPassword(login);
            Util.DeletePassword();
            while (!Imap.VerifyCredentials(login, password))
		    {
		        password = PromptPassword(login);
		    }
            Util.SavePassword(password);
            Imap.SetCredentials(login, password);
            Imap.Start();
            CheckNewMail();
		    timer.Enabled = true;
		}

	    private static string PromptPassword(string email)
	    {
	        var textInput = new TextInput();
            textInput.SetEmail(email);
	        textInput.ShowDialog();
            var password = textInput.Value;
	        textInput.Dispose();
	        return password;
	    }

	    private static void DisplayOkno()
	    {
            var d = new LoginWarning();
	        d.ShowDialog();
            d.Dispose();
	    }

	    private void Dialog_Resize(object sender, EventArgs e) {
			if (FormWindowState.Minimized == WindowState)
				Hide();
		}

	    private void Dialog_FormClosing(object sender, FormClosingEventArgs e) {
			if (!reallyClose) {
				e.Cancel = true;
				WindowState = FormWindowState.Minimized;
			}

		}
      

		private void Dialog_Load(object sender, EventArgs e) {
			//textPassword.Text = Encryption.DecryptString(Properties.Settings.Default.Password);
		}

		private void OnNewMessage(object sender, NewMessageEventArgs e) {
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(NotifyTimeout, "Вам пришло новое сообщение",
				e.Message.Subject, ToolTipIcon.Info);
			UpdateTrayIcon(e.UnreadMails);
		}

		private void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
		{
            notifyIcon.Icon = IconOld;
        }

		private void notifyIcon_MouseClick(object sender, MouseEventArgs e) {
            notifyIcon.Icon = IconOld;
        }
        
		private void Dialog_FormClosed(object sender, FormClosedEventArgs e) {
			if (reallyClose)
				Application.Exit();
		}

		private void timer_Tick(object sender, EventArgs e) {
			if (Util.IsInternetConnected()) {
				if (Imap.Started())
					UpdateTrayIcon(Imap.GetUnreadCount());
				else {
					/* attempt to silently reconnect */
					Reconnect(false);
				}
			} else {
				/* Stop if not connected to the internet */
				Imap.Stop();
			}
		}

		private void UpdateTrayIcon(int Count) {
			if (Count > 0) {
				notifyIcon.Text = "Непрочитанных сообщений: " + Count.ToString();
				notifyIcon.Icon = Properties.Resources.IconUnread;
			} else {
				notifyIcon.Text = "Нет новых сообщений";
				notifyIcon.Icon = IconOld;
			}
		}

		private void CheckNewMail() {
			int Count = Imap.GetUnreadCount();
			if (Count > 0) {
				notifyIcon.ShowBalloonTip(NotifyTimeout, "Новая почта",
                    "Непрочитанных сообщений: " + Count.ToString(), ToolTipIcon.Info);
				/*if (Properties.Settings.Default.PlaySound) {
					try {
						(new SoundPlayer(soundPath)).Play();
					} catch (Exception) { }
				}*/
			}
			UpdateTrayIcon(Count);
		}


		private void Reconnect(bool checkForNewMails = true) {
			try {
				Imap.Stop();
				Imap.Start();
				if (checkForNewMails)
					CheckNewMail();
			} catch (Exception ex) {
				notifyIcon.ShowBalloonTip(500, "Ошибка соединения", ex.Message,
					ToolTipIcon.Error);
			}
		}

		private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e) {
			/* Unfortunately this seems unreliable, so whenever network availability changes
			 * we disconnect and let the tick event take care of reconnecting */
			Imap.Stop();
		}

		private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e) {
			if (e.Mode == PowerModes.Suspend)
				Imap.Stop();
		}
	}
}
