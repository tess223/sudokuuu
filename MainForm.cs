using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using sudokuuu;

namespace SudokuGame
{
    public partial class MainForm : Form
    {
        private SudokuCell[,] cells = new SudokuCell[9, 9];
        private SudokuGenerator generator = new SudokuGenerator();
        private int[,] solution;
        private int[,] currentPuzzle;
        private int difficulty = 30; // 默认有30个空白格
        private string difficultyText = "中等"; // 当前难度文本

        // 计时器相关
        private Timer gameTimer = new Timer();
        private int elapsedSeconds = 0;
        private Label timerLabel = new Label();

        // 提示相关
        private int hintsRemaining = 3;
        private Label hintLabel = new Label();
        private Button hintButton = new Button();

        // 存档和排行榜相关
        private const string SaveFileName = "sudoku_save.xml";
        private const string LeaderboardFileName = "sudoku_leaderboard.xml";
        private GameSave currentSave;
        private List<LeaderboardEntry> leaderboard;

        public MainForm()
        {
            InitializeComponent();
            CreateCells();
            SetupTimer();
            SetupHintControls();
            LoadLeaderboard();

            // 检查是否有存档
            if (File.Exists(SaveFileName))
            {
                DialogResult result = MessageBox.Show(
                    "检测到游戏存档，是否继续上次游戏？",
                    "加载存档",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    LoadGame();
                }
                else
                {
                    NewGame();
                }
            }
            else
            {
                NewGame();
            }
        }

        private void InitializeComponent()
        {
            this.Text = "数独游戏";
            this.Size = new Size(380, 490);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormClosing += MainForm_FormClosing;

            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem gameMenu = new ToolStripMenuItem("游戏");
            ToolStripMenuItem newGameItem = new ToolStripMenuItem("新游戏");
            ToolStripMenuItem saveGameItem = new ToolStripMenuItem("保存游戏");
            ToolStripMenuItem loadGameItem = new ToolStripMenuItem("加载游戏");
            ToolStripMenuItem checkItem = new ToolStripMenuItem("检查");
            ToolStripMenuItem solveItem = new ToolStripMenuItem("解决");
            ToolStripMenuItem leaderboardItem = new ToolStripMenuItem("排行榜");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("退出");

            ToolStripMenuItem levelMenu = new ToolStripMenuItem("难度");
            ToolStripMenuItem easyItem = new ToolStripMenuItem("简单");
            ToolStripMenuItem mediumItem = new ToolStripMenuItem("中等");
            ToolStripMenuItem hardItem = new ToolStripMenuItem("困难");

            easyItem.Click += (s, e) => { difficulty = 20; difficultyText = "简单"; NewGame(); };
            mediumItem.Click += (s, e) => { difficulty = 35; difficultyText = "中等"; NewGame(); };
            hardItem.Click += (s, e) => { difficulty = 50; difficultyText = "困难"; NewGame(); };

            newGameItem.Click += (s, e) => NewGame();
            saveGameItem.Click += (s, e) => SaveGame();
            loadGameItem.Click += (s, e) => LoadGame();
            checkItem.Click += (s, e) => CheckSolution();
            solveItem.Click += (s, e) => SolvePuzzle();
            leaderboardItem.Click += (s, e) => ShowLeaderboard();
            exitItem.Click += (s, e) => Application.Exit();

            gameMenu.DropDownItems.Add(newGameItem);
            gameMenu.DropDownItems.Add(saveGameItem);
            gameMenu.DropDownItems.Add(loadGameItem);
            gameMenu.DropDownItems.Add(checkItem);
            gameMenu.DropDownItems.Add(solveItem);
            gameMenu.DropDownItems.Add(leaderboardItem);
            gameMenu.DropDownItems.Add(exitItem);

            levelMenu.DropDownItems.Add(easyItem);
            levelMenu.DropDownItems.Add(mediumItem);
            levelMenu.DropDownItems.Add(hardItem);

            menuStrip.Items.Add(gameMenu);
            menuStrip.Items.Add(levelMenu);

            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
            
            //Button a = new Button();
            //a.Text = "1";
            //a.Location = new Point(10, 10);
            //a.Size = new Size(30, 30);
            //this.Controls.Add(a);

            //a.Click += (s, e) =>
            //{
            //    NumberSelectForm2 numberForm = new NumberSelectForm2();
            //    numberForm.AutoSize = true;
            //};
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 在关闭窗体时询问是否保存游戏
            DialogResult result = MessageBox.Show(
                "是否保存当前游戏进度？",
                "保存游戏",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                SaveGame();
            }
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        private void SetupTimer()
        {
            // 设置计时器标签
            timerLabel.AutoSize = true;
            timerLabel.Location = new Point(15, 380);
            timerLabel.Font = new Font(FontFamily.GenericSansSerif, 10);
            timerLabel.Text = "时间: 00:00";
            Controls.Add(timerLabel);

            // 设置游戏计时器
            gameTimer.Interval = 1000; // 1秒
            gameTimer.Tick += (s, e) =>
            {
                elapsedSeconds++;
                UpdateTimerLabel();
            };
        }

        private void SetupHintControls()
        {
            // 设置提示标签
            hintLabel.AutoSize = true;
            hintLabel.Location = new Point(200, 380);
            hintLabel.Font = new Font(FontFamily.GenericSansSerif, 10);
            hintLabel.Text = $"提示剩余: {hintsRemaining}";
            Controls.Add(hintLabel);

            // 设置提示按钮
            hintButton.Text = "提示";
            hintButton.Location = new Point(280, 375);
            hintButton.Size = new Size(80, 30);
            hintButton.Click += HintButton_Click;
            Controls.Add(hintButton);
        }

        private void UpdateTimerLabel()
        {
            int minutes = elapsedSeconds / 60;
            int seconds = elapsedSeconds % 60;
            timerLabel.Text = $"时间: {minutes:00}:{seconds:00}";
        }

        private void HintButton_Click(object sender, EventArgs e)
        {
            if (hintsRemaining <= 0)
            {
                MessageBox.Show("您已用完所有提示!", "提示用尽", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 寻找一个空白或错误的单元格
            SudokuCell emptyCell = null;
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!cells[x, y].IsLocked &&
                        (cells[x, y].Value == 0 || cells[x, y].Value != solution[x, y]))
                    {
                        emptyCell = cells[x, y];
                        break;
                    }
                }
                if (emptyCell != null) break;
            }

            // 如果找到了空白单元格，填入正确的数字
            if (emptyCell != null)
            {
                emptyCell.Text = solution[emptyCell.X, emptyCell.Y].ToString();
                emptyCell.Value = solution[emptyCell.X, emptyCell.Y];
                emptyCell.ForeColor = Color.Green;
                emptyCell.IsLocked = true;

                hintsRemaining--;
                hintLabel.Text = $"提示剩余: {hintsRemaining}";
            }
            else
            {
                MessageBox.Show("没有找到需要提示的单元格!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CreateCells()
        {
            int cellSize = 35;
            int offset = 30;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    // 计算单元格位置
                    int xPos = x * cellSize + offset;
                    int yPos = y * cellSize + offset;

                    // 添加边距以分隔3x3区块
                    if (x > 2) xPos += 3;
                    if (x > 5) xPos += 3;
                    if (y > 2) yPos += 3;
                    if (y > 5) yPos += 3;

                    // 创建单元格
                    cells[x, y] = new SudokuCell();
                    cells[x, y].Location = new Point(xPos, yPos);
                    cells[x, y].Size = new Size(cellSize, cellSize);
                    cells[x, y].FlatStyle = FlatStyle.Flat;
                    cells[x, y].X = x;
                    cells[x, y].Y = y;
                    cells[x, y].Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
                    cells[x, y].ForeColor = Color.Black;
                    cells[x, y].BackColor = ((x / 3) + (y / 3)) % 2 == 0 ? Color.FromArgb(240, 240, 240) : Color.White;
                    cells[x, y].FlatAppearance.BorderColor = Color.Gray;
                    cells[x, y].Click += Cell_Click;

                    this.Controls.Add(cells[x, y]);
                }
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            SudokuCell cell = (SudokuCell)sender;

            // 如果单元格是固定的，则不能修改
            if (cell.IsLocked)
                return;

            // 创建数字选择窗体
            NumberSelectForm2 numberForm = new NumberSelectForm2();
            numberForm.StartPosition = FormStartPosition.Manual;

            // 计算窗体位置
            Point screenPoint = cell.PointToScreen(new Point(0, 0));
            numberForm.Location = new Point(screenPoint.X, screenPoint.Y + cell.Height);

            // 显示数字选择窗体
            if (numberForm.ShowDialog() == DialogResult.OK)
            {
                // 设置选定的数字
                if (numberForm.SelectedNumber == 0)
                {
                    cell.Text = "";
                    cell.Value = 0;
                }
                else
                {
                    cell.Text = numberForm.SelectedNumber.ToString();
                    cell.Value = numberForm.SelectedNumber;
                }
            }
        }

        private void NewGame()
        {
            // 重置计时器
            gameTimer.Stop();
            elapsedSeconds = 0;
            UpdateTimerLabel();

            // 重置提示
            hintsRemaining = 3;
            hintLabel.Text = $"提示剩余: {hintsRemaining}";

            // 生成新的数独谜题
            var puzzle = generator.GeneratePuzzle(difficulty);
            solution = puzzle.Item2;
            currentPuzzle = puzzle.Item1;

            // 填充单元格
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    cells[x, y].Text = puzzle.Item1[x, y] == 0 ? "" : puzzle.Item1[x, y].ToString();
                    cells[x, y].Value = puzzle.Item1[x, y];
                    cells[x, y].IsLocked = puzzle.Item1[x, y] != 0;
                    cells[x, y].ForeColor = Color.Black;
                    cells[x, y].IsCorrect = true;

                    if (cells[x, y].IsLocked)
                    {
                        cells[x, y].ForeColor = Color.Blue;
                    }
                }
            }

            // 启动计时器
            gameTimer.Start();
        }

        private void SaveGame()
        {
            // 创建存档对象
            currentSave = new GameSave
            {
                Difficulty = difficulty,
                DifficultyText = difficultyText,
                ElapsedSeconds = elapsedSeconds,
                HintsRemaining = hintsRemaining,
                Puzzle = new int[9, 9],
                Solution = new int[9, 9],
                CurrentState = new CellState[9, 9]
            };

            // 保存当前游戏状态
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    currentSave.Puzzle[x, y] = currentPuzzle[x, y];
                    currentSave.Solution[x, y] = solution[x, y];

                    currentSave.CurrentState[x, y] = new CellState
                    {
                        Value = cells[x, y].Value,
                        IsLocked = cells[x, y].IsLocked,
                        ForeColor = cells[x, y].ForeColor.ToArgb()
                    };
                }
            }

            try
            {
                // 序列化并保存游戏
                XmlSerializer serializer = new XmlSerializer(typeof(GameSave));
                using (FileStream stream = new FileStream(SaveFileName, FileMode.Create))
                {
                    serializer.Serialize(stream, currentSave);
                }

                MessageBox.Show("游戏已成功保存!", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存游戏时出错: {ex.Message}", "保存错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGame()
        {
            try
            {
                // 加载游戏存档
                if (!File.Exists(SaveFileName))
                {
                    MessageBox.Show("没有找到存档文件!", "加载失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                XmlSerializer serializer = new XmlSerializer(typeof(GameSave));
                using (FileStream stream = new FileStream(SaveFileName, FileMode.Open))
                {
                    currentSave = (GameSave)serializer.Deserialize(stream);
                }

                // 停止当前计时器
                gameTimer.Stop();

                // 恢复游戏状态
                difficulty = currentSave.Difficulty;
                difficultyText = currentSave.DifficultyText;
                elapsedSeconds = currentSave.ElapsedSeconds;
                hintsRemaining = currentSave.HintsRemaining;
                solution = currentSave.Solution;
                currentPuzzle = currentSave.Puzzle;

                // 更新界面
                UpdateTimerLabel();
                hintLabel.Text = $"提示剩余: {hintsRemaining}";

                // 恢复单元格状态
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        CellState state = currentSave.CurrentState[x, y];
                        cells[x, y].Value = state.Value;
                        cells[x, y].Text = state.Value == 0 ? "" : state.Value.ToString();
                        cells[x, y].IsLocked = state.IsLocked;
                        cells[x, y].ForeColor = Color.FromArgb(state.ForeColor);
                    }
                }

                // 重新启动计时器
                gameTimer.Start();

                MessageBox.Show("游戏已成功加载!", "加载成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载游戏时出错: {ex.Message}", "加载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLeaderboard()
        {
            try
            {
                if (File.Exists(LeaderboardFileName))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<LeaderboardEntry>));
                    using (FileStream stream = new FileStream(LeaderboardFileName, FileMode.Open))
                    {
                        leaderboard = (List<LeaderboardEntry>)serializer.Deserialize(stream);
                    }
                }
                else
                {
                    leaderboard = new List<LeaderboardEntry>();
                }
            }
            catch
            {
                leaderboard = new List<LeaderboardEntry>();
            }
        }

        private void SaveLeaderboard()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<LeaderboardEntry>));
                using (FileStream stream = new FileStream(LeaderboardFileName, FileMode.Create))
                {
                    serializer.Serialize(stream, leaderboard);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存排行榜时出错: {ex.Message}", "保存错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddToLeaderboard(string playerName, int time, string difficulty)
        {
            LeaderboardEntry entry = new LeaderboardEntry
            {
                PlayerName = playerName,
                CompletionTime = time,
                Difficulty = difficulty,
                Date = DateTime.Now
            };

            leaderboard.Add(entry);

            // 按时间排序
            leaderboard.Sort((a, b) => a.CompletionTime.CompareTo(b.CompletionTime));

            // 只保留前50名
            if (leaderboard.Count > 50)
            {
                leaderboard.RemoveRange(50, leaderboard.Count - 50);
            }

            SaveLeaderboard();
        }

        private void ShowLeaderboard()
        {
            // 创建并显示排行榜窗体
            LeaderboardForm leaderboardForm = new LeaderboardForm(leaderboard);
            leaderboardForm.ShowDialog();
        }

        private void CheckSolution()
        {
            bool isComplete = true;
            bool isCorrect = true;

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (cells[x, y].Value == 0)
                    {
                        isComplete = false;
                    }
                    else if (cells[x, y].Value != solution[x, y])
                    {
                        cells[x, y].ForeColor = Color.Red;
                        cells[x, y].IsCorrect = false;
                        isCorrect = false;
                    }
                    else
                    {
                        cells[x, y].ForeColor = cells[x, y].IsLocked ? Color.Blue : Color.Black;
                        cells[x, y].IsCorrect = true;
                    }
                }
            }

            if (isComplete && isCorrect)
            {
                gameTimer.Stop();

                string message = $"恭喜！您已成功完成数独！\n用时: {elapsedSeconds / 60}分{elapsedSeconds % 60}秒";
                MessageBox.Show(message, "游戏完成", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 询问玩家姓名并添加到排行榜
                string playerName = ShowInputDialog("恭喜完成游戏！请输入您的名字以记录到排行榜：");
                if (!string.IsNullOrEmpty(playerName))
                {
                    AddToLeaderboard(playerName, elapsedSeconds, difficultyText);
                    ShowLeaderboard();
                }

                // 删除存档文件
                try
                {
                    if (File.Exists(SaveFileName))
                    {
                        File.Delete(SaveFileName);
                    }
                }
                catch { }
            }
            else if (!isComplete)
            {
                MessageBox.Show("您还没有填完所有空格！", "未完成", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private string ShowInputDialog(string prompt)
        {
            Form inputForm = new Form();
            inputForm.Width = 300;
            inputForm.Height = 150;
            inputForm.Text = "输入姓名";
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;

            Label label = new Label();
            label.Text = prompt;
            label.Left = 10;
            label.Top = 20;
            label.Width = 280;

            TextBox textBox = new TextBox();
            textBox.Left = 10;
            textBox.Top = 50;
            textBox.Width = 280;

            Button okButton = new Button();
            okButton.Text = "确定";
            okButton.Left = 120;
            okButton.Top = 80;
            okButton.DialogResult = DialogResult.OK;

            inputForm.Controls.Add(label);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.AcceptButton = okButton;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                return textBox.Text;
            }
            return null;
        }

        private void SolvePuzzle()
        {
            // 停止计时器
            gameTimer.Stop();

            // 显示完整解决方案
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    cells[x, y].Text = solution[x, y].ToString();
                    cells[x, y].Value = solution[x, y];
                    if (!cells[x, y].IsLocked)
                    {
                        cells[x, y].ForeColor = Color.Green;
                    }
                }
            }
        }
    }

    // 用于存档的类
    public class GameSave
    {
        public int Difficulty { get; set; }
        public string DifficultyText { get; set; }
        public int ElapsedSeconds { get; set; }
        public int HintsRemaining { get; set; }
        public int[,] Puzzle { get; set; }
        public int[,] Solution { get; set; }
        public CellState[,] CurrentState { get; set; }
    }

    public class CellState
    {
        public int Value { get; set; }
        public bool IsLocked { get; set; }
        public int ForeColor { get; set; }
    }

    // 排行榜条目类
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; }
        public int CompletionTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime Date { get; set; }
    }
}