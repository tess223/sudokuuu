using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Linq;
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
        private const string SaveFileName = "sudoku_save.dat";
        private const string LeaderboardFileName = "sudoku_leaderboard.dat";
        private List<LeaderboardEntry> leaderboard;

        // 当前选中的数字，用于高亮显示
        private int selectedNumber = 0;

        // 动画相关
        private Timer animationTimer = new Timer();

        public MainForm()
        {
            InitializeComponent();
            CreateCells();
            SetupTimer();
            SetupHintControls();
            SetupAnimationTimer();
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
            this.DoubleBuffered = true; // 启用双缓冲减少闪烁

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

            easyItem.Click += (s, e) => { difficulty = 20; difficultyText = "简单"; NewGameWithAnimation(); };
            mediumItem.Click += (s, e) => { difficulty = 35; difficultyText = "中等"; NewGameWithAnimation(); };
            hardItem.Click += (s, e) => { difficulty = 50; difficultyText = "困难"; NewGameWithAnimation(); };

            newGameItem.Click += (s, e) => NewGameWithAnimation();
            saveGameItem.Click += (s, e) => SaveGame();
            loadGameItem.Click += (s, e) => LoadGame();
            checkItem.Click += (s, e) => CheckSolution();
            solveItem.Click += (s, e) => SolvePuzzleWithAnimation();
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

        private void SetupAnimationTimer()
        {
            animationTimer.Interval = 50;
            animationTimer.Tick += AnimationTimer_Tick;
        }

        // 用于动画效果的计数器和变量
        private int animationCount = 0;
        private List<SudokuCell> highlightedCells = new List<SudokuCell>();
        private int highlightFadeCount = 0;
        private bool isFadingOut = false;

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // 处理数字选择的高亮效果
            if (highlightedCells.Count > 0)
            {
                highlightFadeCount++;

                if (!isFadingOut && highlightFadeCount >= 10) // 持续高亮一段时间
                {
                    isFadingOut = true;
                    highlightFadeCount = 0;
                }

                if (isFadingOut)
                {
                    // 淡出效果
                    int alpha = 255 - (highlightFadeCount * 25); // 降低透明度
                    if (alpha <= 0)
                    {
                        // 动画结束，恢复正常
                        foreach (var cell in highlightedCells)
                        {
                            ResetCellAppearance(cell);
                        }
                        highlightedCells.Clear();
                        animationTimer.Stop();
                        isFadingOut = false;
                    }
                    else
                    {
                        // 更新高亮颜色
                        Color highlightColor = Color.FromArgb(alpha, 255, 255, 0);
                        foreach (var cell in highlightedCells)
                        {
                            cell.BackColor = highlightColor;
                        }
                    }
                }
                else
                {
                    // 淡入效果
                    int alpha = Math.Min(255, highlightFadeCount * 50); // 增加透明度
                    Color highlightColor = Color.FromArgb(alpha, 255, 255, 0);
                    foreach (var cell in highlightedCells)
                    {
                        cell.BackColor = highlightColor;
                    }
                }
            }

            // 处理其他动画效果
            if (animationCount > 0)
            {
                animationCount--;
                if (animationCount == 0)
                {
                    animationTimer.Stop();
                }

                // 其他动画效果可以在这里实现
            }
        }

        private void ResetCellAppearance(SudokuCell cell)
        {
            int x = cell.X;
            int y = cell.Y;
            cell.BackColor = ((x / 3) + (y / 3)) % 2 == 0 ? Color.FromArgb(240, 240, 240) : Color.White;
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

            // 如果找到了空白单元格，使用动画填入正确的数字
            if (emptyCell != null)
            {
                // 播放提示动画
                AnimateHint(emptyCell, solution[emptyCell.X, emptyCell.Y]);

                hintsRemaining--;
                hintLabel.Text = $"提示剩余: {hintsRemaining}";
            }
            else
            {
                MessageBox.Show("没有找到需要提示的单元格!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void AnimateHint(SudokuCell cell, int correctValue)
        {
            // 先闪烁单元格
            for (int i = 0; i < 3; i++)
            {
                cell.BackColor = Color.LightGreen;
                await Task.Delay(100);
                ResetCellAppearance(cell);
                await Task.Delay(100);
            }

            // 设置正确的数字
            cell.Text = correctValue.ToString();
            cell.Value = correctValue;
            cell.ForeColor = Color.Green;
            cell.IsLocked = true;

            // 显示淡入效果
            cell.Font = new Font(cell.Font.FontFamily, 1, FontStyle.Bold);
            for (int size = 1; size <= 12; size++)
            {
                cell.Font = new Font(cell.Font.FontFamily, size, FontStyle.Bold);
                await Task.Delay(10);
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
                    cells[x, y].Paint += Cell_Paint; // 添加自定义绘制事件

                    this.Controls.Add(cells[x, y]);
                }
            }
        }

        private void Cell_Paint(object sender, PaintEventArgs e)
        {
            SudokuCell cell = (SudokuCell)sender;

            // 为单元格添加边框效果
            if (cell.Value > 0 && cell.Value == selectedNumber)
            {
                // 给选中相同数字的单元格添加特殊边框
                Rectangle rect = new Rectangle(0, 0, cell.Width - 1, cell.Height - 1);
                using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            SudokuCell cell = (SudokuCell)sender;

            // 如果单元格是固定的，则不能修改，但还是高亮显示相同数字
            if (cell.Value > 0)
            {
                HighlightSameNumbers(cell.Value);
                return;
            }

            // 创建数字选择窗体
            NumberSelectForm2 numberForm = new NumberSelectForm2();
            numberForm.StartPosition = FormStartPosition.Manual;

            // 计算窗体位置
            Point screenPoint = cell.PointToScreen(new Point(0, 0));
            numberForm.Location = new Point(screenPoint.X, screenPoint.Y + cell.Height);

            // 显示数字选择窗体
            if (numberForm.ShowDialog() == DialogResult.OK)
            {
                int number = numberForm.SelectedNumber;

                // 设置选定的数字
                if (number == 0)
                {
                    cell.Text = "";
                    cell.Value = 0;
                    selectedNumber = 0;
                }
                else
                {
                    // 设置数字动画效果
                    SetNumberWithAnimation(cell, number);

                    // 高亮显示相同数字
                    HighlightSameNumbers(number);
                }
            }
        }

        private async void SetNumberWithAnimation(SudokuCell cell, int number)
        {
            // 保存当前数值以便稍后恢复
            int oldValue = cell.Value;

            // 设置数字渐入效果
            cell.Text = number.ToString();
            cell.Value = number;
            cell.Font = new Font(cell.Font.FontFamily, 1, FontStyle.Bold);

            for (int size = 1; size <= 12; size++)
            {
                cell.Font = new Font(cell.Font.FontFamily, size, FontStyle.Bold);
                await Task.Delay(5);
            }
        }

        private void HighlightSameNumbers(int number)
        {
            // 停止之前的动画
            if (animationTimer.Enabled)
            {
                animationTimer.Stop();

                // 重置之前高亮的单元格
                foreach (var cell in highlightedCells)
                {
                    ResetCellAppearance(cell);
                }
                highlightedCells.Clear();
            }

            selectedNumber = number;

            // 找出所有相同数字的单元格
            highlightedCells = new List<SudokuCell>();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (cells[x, y].Value == number)
                    {
                        highlightedCells.Add(cells[x, y]);
                    }
                }
            }

            // 重绘所有单元格
            foreach (var cell in cells)
            {
                cell.Invalidate();
            }

            // 如果有相同数字单元格，开始高亮动画
            if (highlightedCells.Count > 0)
            {
                highlightFadeCount = 0;
                isFadingOut = false;
                animationTimer.Start();
            }
        }

        private async void NewGameWithAnimation()
        {
            // 禁用界面交互
            this.Enabled = false;

            // 先淡出当前数字
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    cells[x, y].Text = "";
                }
            }
            await Task.Delay(200);

            // 重置计时器
            gameTimer.Stop();
            elapsedSeconds = 0;
            UpdateTimerLabel();

            // 重置提示
            hintsRemaining = 3;
            hintLabel.Text = $"提示剩余: {hintsRemaining}";

            // 重置选择的数字
            selectedNumber = 0;

            // 生成新的数独谜题
            var puzzle = generator.GeneratePuzzle(difficulty);
            solution = puzzle.Item2;
            currentPuzzle = puzzle.Item1;

            // 使用动画效果填充单元格
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    cells[x, y].Value = puzzle.Item1[x, y];
                    cells[x, y].IsLocked = puzzle.Item1[x, y] != 0;
                    cells[x, y].ForeColor = Color.Black;
                    cells[x, y].IsCorrect = true;
                    ResetCellAppearance(cells[x, y]);

                    if (cells[x, y].IsLocked)
                    {
                        cells[x, y].ForeColor = Color.Blue;
                    }
                }
            }

            // 延迟显示数字，创造渐入效果
            Random random = new Random();
            for (int i = 0; i < 81; i++)
            {
                int x = random.Next(9);
                int y = random.Next(9);

                if (cells[x, y].Value > 0 && string.IsNullOrEmpty(cells[x, y].Text))
                {
                    cells[x, y].Text = cells[x, y].Value.ToString();
                    await Task.Delay(10);
                }
            }

            // 确保所有格子都已显示
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (cells[x, y].Value > 0)
                    {
                        cells[x, y].Text = cells[x, y].Value.ToString();
                    }
                }
            }

            // 启动计时器
            gameTimer.Start();

            // 重新启用界面交互
            this.Enabled = true;
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

            // 重置选择的数字
            selectedNumber = 0;

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
                    ResetCellAppearance(cells[x, y]);

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
            try
            {
                // 创建游戏存档数据
                GameSaveData saveData = new GameSaveData
                {
                    Difficulty = difficulty,
                    DifficultyText = difficultyText,
                    ElapsedSeconds = elapsedSeconds,
                    HintsRemaining = hintsRemaining,
                    SelectedNumber = selectedNumber,
                    Puzzle = SerializeArray(currentPuzzle),
                    Solution = SerializeArray(solution),
                    CellStates = new List<CellStateData>()
                };

                // 保存单元格状态
                for (int y = 0; y < 9; y++)
                {
                    for (int x = 0; x < 9; x++)
                    {
                        saveData.CellStates.Add(new CellStateData
                        {
                            X = x,
                            Y = y,
                            Value = cells[x, y].Value,
                            IsLocked = cells[x, y].IsLocked,
                            ForeColor = cells[x, y].ForeColor.ToArgb()
                        });
                    }
                }

                // 使用二进制格式保存
                using (FileStream stream = new FileStream(SaveFileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, saveData);
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

                // 使用二进制格式加载
                GameSaveData saveData;
                using (FileStream stream = new FileStream(SaveFileName, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (GameSaveData)formatter.Deserialize(stream);
                }

                // 停止当前计时器
                gameTimer.Stop();

                // 恢复游戏状态
                difficulty = saveData.Difficulty;
                difficultyText = saveData.DifficultyText;
                elapsedSeconds = saveData.ElapsedSeconds;
                hintsRemaining = saveData.HintsRemaining;
                selectedNumber = saveData.SelectedNumber;
                solution = DeserializeArray(saveData.Solution);
                currentPuzzle = DeserializeArray(saveData.Puzzle);

                // 更新界面
                UpdateTimerLabel();
                hintLabel.Text = $"提示剩余: {hintsRemaining}";

                // 恢复单元格状态
                foreach (var state in saveData.CellStates)
                {
                    int x = state.X;
                    int y = state.Y;
                    cells[x, y].Value = state.Value;
                    cells[x, y].Text = state.Value == 0 ? "" : state.Value.ToString();
                    cells[x, y].IsLocked = state.IsLocked;
                    cells[x, y].ForeColor = Color.FromArgb(state.ForeColor);
                    ResetCellAppearance(cells[x, y]);
                }

                // 重新启动计时器
                gameTimer.Start();

                // 刷新高亮显示
                if (selectedNumber > 0)
                {
                    HighlightSameNumbers(selectedNumber);
                }

                MessageBox.Show("游戏已成功加载!", "加载成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载游戏时出错: {ex.Message}", "加载错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 辅助方法：序列化二维数组
        private List<int> SerializeArray(int[,] array)
        {
            List<int> list = new List<int>();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    list.Add(array[x, y]);
                }
            }
            return list;
        }

        // 辅助方法：反序列化为二维数组
        private int[,] DeserializeArray(List<int> list)
        {
            int[,] array = new int[9, 9];
            for (int i = 0; i < list.Count; i++)
            {
                int x = i % 9;
                int y = i / 9;
                array[x, y] = list[i];
            }
            return array;
        }

        private void LoadLeaderboard()
        {
            try
            {
                if (File.Exists(LeaderboardFileName))
                {
                    using (FileStream stream = new FileStream(LeaderboardFileName, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        leaderboard = (List<LeaderboardEntry>)formatter.Deserialize(stream);
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
                using (FileStream stream = new FileStream(LeaderboardFileName, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, leaderboard);
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

        private async void SolvePuzzleWithAnimation()
        {
            // 停止计时器
            gameTimer.Stop();

            // 禁用界面交互
            this.Enabled = false;

            // 收集需要填写的单元格
            List<SudokuCell> cellsToFill = new List<SudokuCell>();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!cells[x, y].IsLocked || cells[x, y].Value != solution[x, y])
                    {
                        cellsToFill.Add(cells[x, y]);
                    }
                }
            }

            // 随机打乱填充顺序，使动画更自然
            Random random = new Random();
            cellsToFill = cellsToFill.OrderBy(x => random.Next()).ToList();

            // 依次填入正确数字
            foreach (var cell in cellsToFill)
            {
                int x = cell.X;
                int y = cell.Y;

                cell.Text = solution[x, y].ToString();
                cell.Value = solution[x, y];
                cell.ForeColor = Color.Green;

                await Task.Delay(10);
            }

            // 重新启用界面交互
            this.Enabled = true;
        }
    }

    // 用于存档的类（可序列化）
    [Serializable]
    public class GameSaveData
    {
        public int Difficulty { get; set; }
        public string DifficultyText { get; set; }
        public int ElapsedSeconds { get; set; }
        public int HintsRemaining { get; set; }
        public int SelectedNumber { get; set; }
        public List<int> Puzzle { get; set; }
        public List<int> Solution { get; set; }
        public List<CellStateData> CellStates { get; set; }
    }

    [Serializable]
    public class CellStateData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Value { get; set; }
        public bool IsLocked { get; set; }
        public int ForeColor { get; set; }
    }

    // 排行榜条目类（可序列化）
    [Serializable]
    public class LeaderboardEntry
    {
        public string PlayerName { get; set; }
        public int CompletionTime { get; set; }
        public string Difficulty { get; set; }
        public DateTime Date { get; set; }
    }
}