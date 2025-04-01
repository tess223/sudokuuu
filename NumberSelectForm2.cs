using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sudokuuu
{
    public partial class NumberSelectForm2 : Form
    {
        public int SelectedNumber { get; private set; }
        public NumberSelectForm2()
        {
            InitializeComponent();
            //this.Show();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            SelectedNumber = 1;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SelectedNumber = 2;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SelectedNumber = 3;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SelectedNumber = 4;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SelectedNumber = 5;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SelectedNumber = 6;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SelectedNumber = 7;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SelectedNumber = 8;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SelectedNumber = 9;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void NumberSelectForm2_Load(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            SelectedNumber = 0;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
