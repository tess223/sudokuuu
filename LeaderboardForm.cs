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
        private Label filterLabel;
        private ComboBox filterComboBox;

        public LeaderboardForm(List<LeaderboardEntry> leaderboard)
        {
            this.leaderboard = leaderboard;
            InitializeComponent();
            PopulateLeaderboard("全部");
        }

        private void InitializeComponent()
        {
            this.filterLabel = new System.Windows.Forms.Label();
            this.filterComboBox = new System.Windows.Forms.ComboBox();
            this.listView = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // filterLabel
            // 
            this.filterLabel.AutoSize = true;
            this.filterLabel.Location = new System.Drawing.Point(10, 15);
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(118, 24);
            this.filterLabel.TabIndex = 0;
            this.filterLabel.Text = "难度筛选:";
            // 
            // filterComboBox
            // 
            this.filterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filterComboBox.Items.AddRange(new object[] {
            "全部",
            "简单",
            "中等",
            "困难"});
            this.filterComboBox.Location = new System.Drawing.Point(80, 12);
            this.filterComboBox.Name = "filterComboBox";
            this.filterComboBox.Size = new System.Drawing.Size(100, 32);
            this.filterComboBox.TabIndex = 1;
            // 
            // listView
            // 
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(10, 45);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(465, 300);
            this.listView.TabIndex = 2;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            // 
            // LeaderboardForm
            // 
            this.ClientSize = new System.Drawing.Size(474, 329);
            this.Controls.Add(this.filterLabel);
            this.Controls.Add(this.filterComboBox);
            this.Controls.Add(this.listView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LeaderboardForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "数独排行榜";
            this.Load += new System.EventHandler(this.LeaderboardForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

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

        private void LeaderboardForm_Load(object sender, EventArgs e)
        {

        }
    }
}