using System.Windows.Forms;

namespace SudokuGame
{
    public class SudokuCell : Button
    {
        public int Value { get; set; }
        public bool IsLocked { get; set; }
        public bool IsCorrect { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public SudokuCell()
        {
            this.Value = 0;
            this.IsLocked = false;
            this.IsCorrect = true;
        }
    }
}