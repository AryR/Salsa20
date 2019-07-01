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
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
		}

		private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
		{
			string text = textBox1.Text;
			string encrypted = Salsa20.EncryptText(text);
			textBox2.Text = encrypted;
			textBox3.Text = Salsa20.EncryptText(encrypted);
		}
	}
}
