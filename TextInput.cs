using System;
using System.Windows.Forms;

namespace ImapNotify
{
    public partial class TextInput : Form
    {
        public string Value => textBox1.Text;

        public void SetEmail(string email)
        {
            label2.Text = email;
        }

        public TextInput()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = textBox1.Text.Length > 1;
        }
    }
}
