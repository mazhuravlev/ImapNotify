using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImapNotify
{
    public partial class LoginWarning : Form
    {
        private int timeout = 19;

        public LoginWarning()
        {
            InitializeComponent();
            label3.Text = Environment.UserName;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (timeout > 0)
            {
                timeout -= 1;
                button1.Text = $"OK ({timeout})";
            }
            else
            {
                button1.Text = "OK";
                button1.Enabled = true;
                timer1.Enabled = false;
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://192.168.3.2/login.html");
        }
    }
}
