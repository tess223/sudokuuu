using System;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuGame
{
    public class NumberSelectForm : Form
    {
        public int SelectedNumber { get; private set; }

        public NumberSelectForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "选择数字";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.Size = new Size(180, 70);
            this.StartPosition = FormStartPosition.CenterParent;

            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.Padding = new Padding(5);

            // 添加数字按钮1-9和清除按钮
            for (int i = 1; i <= 9; i++)
            {
                Button btn = new Button();
                btn.Text = i.ToString();
                btn.Size = new Size(30, 30);
                btn.Tag = i;
                btn.Click += Number_Click;
                panel.Controls.Add(btn);
            }

            // 添加清除按钮
            Button clearBtn = new Button();
            clearBtn.Text = "C";
            clearBtn.Size = new Size(30, 30);
            clearBtn.Tag = 0;
            clearBtn.Click += Number_Click;
            panel.Controls.Add(clearBtn);

            this.Controls.Add(panel);
        }

        private void Number_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            SelectedNumber = (int)btn.Tag;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}