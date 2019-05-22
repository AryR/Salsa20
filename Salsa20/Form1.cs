using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Salsa20
{
    public partial class Form1 : Form
    {
        Salsa20 salsa;

        public Form1()
        {
            InitializeComponent();
            salsa = new Salsa20("aaaabbbbccccdddd", "iiiijjjj");
            salsa.BytesEncripted += BytesEncripted;
            salsa.BytesDecripted += BytesDecripted;
            CheckForIllegalCrossThreadCalls = false;
        }

        private void originalTextBox_TextChanged(object sender, EventArgs e)
        {
            char lastchar = originalTextBox.Text.ToCharArray()[originalTextBox.Text.Length - 1];
            salsa.Encrypt((byte)lastchar);
        }

        private void BytesEncripted(object sender, byte[] e)
        {
            try
            {
                encryptedTextBox.Text += Encoding.ASCII.GetString(e);
                salsa.Decrypt(e);
            }
            catch(Exception ex)
            {
                int a = 0;
            }
        }

        private void BytesDecripted(object sender, byte[] e)
        {
            decryptedTextBox.Text += Encoding.ASCII.GetString(e);
        }
    }
}
