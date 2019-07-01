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
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Title = "Buscar Imagen",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "png",
                Filter = "JPG|*.jpg|JPEG|*.jpeg|PNG|*.png",
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Salsa20.EncryptFile(textBox1.Text))
                MessageBox.Show("Se guardo la imagen encriptada.");
            else
                MessageBox.Show("No se pudo encriptar la imagen.");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Salsa20.DecryptFile(textBox1.Text))
                MessageBox.Show("Se guardo la imagen desencriptada.");
            else
                MessageBox.Show("No se pudo desencriptar la imagen.");
        }
    }
}
