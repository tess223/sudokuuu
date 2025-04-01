using System;
using System.Collections.Generic;

namespace SudokuGame
{
    public class SudokuGenerator
    {
        private Random random = new Random();

        // 生成完整的数独谜题和解决方案
        public Tuple<int[,], int[,]> GeneratePuzzle(int emptyCount)
        {
            int[,] solution = GenerateSolution();
            int[,] puzzle = CreatePuzzle(solution, emptyCount);
            return new Tuple<int[,], int[,]>(puzzle, solution);
        }

        // 生成完整的解决方案
        private int[,] GenerateSolution()
        {
            int[,] grid = new int[9, 9];
            SolveSudoku(grid);
            return grid;
        }

        // 使用回溯法解数独
        private bool SolveSudoku(int[,] grid)
        {
            int row = 0, col = 0;
            bool isEmpty = true;

            // 寻找空单元格
            for (row = 0; row < 9; row++)
            {
                for (col = 0; col < 9; col++)
                {
                    if (grid[row, col] == 0)
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if (!isEmpty)
                    break;
            }

            // 如果没有空单元格，返回true
            if (isEmpty)
                return true;

            // 生成随机顺序的数字
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            Shuffle(numbers);

            // 尝试填充数字
            foreach (int num in numbers)
            {
                if (IsSafe(grid, row, col, num))
                {
                    grid[row, col] = num;

                    if (SolveSudoku(grid))
                        return true;

                    grid[row, col] = 0;
                }
            }

            return false;
        }

        // 创建带有空格的谜题
        private int[,] CreatePuzzle(int[,] solution, int emptyCount)
        {
            int[,] puzzle = new int[9, 9];
            // 复制解决方案
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    puzzle[i, j] = solution[i, j];

            // 随机选择单元格设置为空
            List<Tuple<int, int>> cells = new List<Tuple<int, int>>();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    cells.Add(new Tuple<int, int>(i, j));

            Shuffle(cells);

            for (int i = 0; i < emptyCount && i < cells.Count; i++)
            {
                int row = cells[i].Item1;
                int col = cells[i].Item2;
                puzzle[row, col] = 0;
            }

            return puzzle;
        }

        // 检查在指定位置放置数字是否安全
        private bool IsSafe(int[,] grid, int row, int col, int num)
        {
            // 检查行
            for (int x = 0; x < 9; x++)
                if (grid[row, x] == num)
                    return false;

            // 检查列
            for (int x = 0; x < 9; x++)
                if (grid[x, col] == num)
                    return false;

            // 检查3x3方格
            int startRow = row - row % 3;
            int startCol = col - col % 3;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (grid[i + startRow, j + startCol] == num)
                        return false;

            return true;
        }

        // 打乱列表
        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}