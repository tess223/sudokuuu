using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SudokuGame
{
    public class LeaderboardForm : Form
    {
        private List<LeaderboardEntry> leaderboard;
        private ListView listView;
        private ComboBox filterComboBox;

        public LeaderboardForm(List<LeaderboardEntry> leaderboard)
        {
            this.leaderboard = leaderboard;
            InitializeComponent();
            PopulateLeaderboard("全部");
        }

        private void InitializeComponent()
        {
            this.Text = "数独排行榜";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 创建筛选控件
            Label filterLabel = new Label();
            filterLabel.Text = "难度筛选:";
            filterLabel.AutoSize = true;
            filterLabel.Location = new Point(10, 15);

            filterComboBox = new ComboBox();
            filterComboBox.Location = new Point(80, 12);
            filterComboBox.Size = new Size(100, 25);
            filterComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            filterComboBox.Items.AddRange(new object[] { "全部", "简单", "中等", "困难" });
            filterComboBox.SelectedIndex = 0;
            filterComboBox.SelectedIndexChanged += FilterComboBox_SelectedIndexChanged;

            // 创建ListView控件
            listView = new ListView();
            listView.Location = new Point(10, 45);
            listView.Size = new Size(465, 300);
            listView.View = View.Details;
            listView.FullRowSelect = true;
            listView.GridLines = true;

            // 添加列
            listView.Columns.Add("排名", 50);
            listView.Columns.Add("玩家", 100);
            listView.Columns.Add("用时", 80);
            listView.Columns.Add("难度", 70);
            listView.Columns.Add("日期", 160);

            this.Controls.Add(filterLabel);
            this.Controls.Add(filterComboBox);
            this.Controls.Add(listView);
        }

        private void FilterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string filterValue = filterComboBox.SelectedItem.ToString();
            PopulateLeaderboard(filterValue);
        }

        private void PopulateLeaderboard(string filter)
        {
            listView.Items.Clear();
            List<LeaderboardEntry> filteredList;

            if (filter == "全部")
            {
                filteredList = new List<LeaderboardEntry>(leaderboard);
            }
            else
            {
                filteredList = leaderboard.FindAll(e => e.Difficulty == filter);
            }

            // 按时间排序
            filteredList.Sort((a, b) => a.CompletionTime.CompareTo(b.CompletionTime));

            // 填充列表
            for (int i = 0; i < filteredList.Count; i++)
            {
                LeaderboardEntry entry = filteredList[i];
                int minutes = entry.CompletionTime / 60;
                int seconds = entry.CompletionTime % 60;
                string time = $"{minutes:00}:{seconds:00}";

                ListViewItem item = new ListViewItem(new string[] {
                    (i + 1).ToString(),
                    entry.PlayerName,
                    time,
                    entry.Difficulty,
                    entry.Date.ToString("yyyy-MM-dd HH:mm:ss")
                });

                listView.Items.Add(item);
            }
        }
    }
}