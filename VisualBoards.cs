using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;
using System.Threading;

namespace Tetris_WPF.Code
{
    internal class VisualBoard
    {
        private const double InitialDelay = 1000;

        private Board board;
        private readonly Grid MainGrid;
        private readonly Grid NextFigureGrid;
        private readonly TextBlock ScoreValueTextBlock;
        private readonly TextBlock LinesLVLValueTextBlock;
        private List<Label> currentFigure_labels;
        private List<Label> nextFigure_labels;
        private double delay;

        public DispatcherTimer DTimer { get; set; }
        public bool Stopped { get; set; }
        public double Delay { get => delay; private set => delay = value; }

        // Constructor om een VisualBoard te maken met een bestaand bord en UI-elementen
        public VisualBoard(Board board, ref Grid MainGrid, ref Grid NextFigureGrid, ref TextBlock ScoreValueTextBlock, ref TextBlock LinesLVLValueTextBlock)
        {
            this.board = board;
            this.MainGrid = MainGrid;
            this.NextFigureGrid = NextFigureGrid;
            this.ScoreValueTextBlock = ScoreValueTextBlock;
            this.LinesLVLValueTextBlock = LinesLVLValueTextBlock;

            currentFigure_labels = new List<Label>();
            nextFigure_labels = new List<Label>();
            Stopped = true;
            delay = InitialDelay;
        }

        // Constructor om een VisualBoard te maken met nieuwe elementen en een nieuw bord
        public VisualBoard(ref Grid MainGrid, ref Grid NextFigureGrid, ref TextBlock ScoreValueTextBlock, ref TextBlock LinesLVLValueTextBlock)
            : this(new Board(), ref MainGrid, ref NextFigureGrid, ref ScoreValueTextBlock, ref LinesLVLValueTextBlock)
        { }

        // Toon de huidige Tetromino op het hoofdgrid
        public void ShowCurrentFigure()
        {
            currentFigure_labels.Clear();
            Tetromino t = board.CurrentFigure;
            Label l;
            foreach (Coord c in t.Coords)
            {
                l = new Label();
                l.Style = (Style)Application.Current.Resources["TetrominoLabel"];
                l.Background = Tetromino.GetColor(t.Type);
                Grid.SetColumn(l, c.X);
                Grid.SetRow(l, c.Y);
                currentFigure_labels.Add(l);
                MainGrid.Children.Add(l);
            }
        }

        // Verberg de volgende Tetromino van het volgende grid
        public void HideNextFigure()
        {
            foreach (Label l in nextFigure_labels)
            {
                NextFigureGrid.Children.Remove(l);
            }
        }

        // Toon de volgende Tetromino op het volgende grid met een optionele offset
        public void ShowNextFigure(int offset_x = 1, int offset_y = 1)
        {
            HideNextFigure();
            nextFigure_labels.Clear();
            Tetromino t = board.NextFigure;
            Label l;
            foreach (Coord c in t.Coords)
            {
                l = new Label();
                l.Style = (Style)Application.Current.Resources["TetrominoLabel"];
                l.Background = Tetromino.GetColor(t.Type);
                Grid.SetColumn(l, c.X + offset_x - Tetromino.InitialX);
                Grid.SetRow(l, c.Y + offset_y);
                nextFigure_labels.Add(l);
                NextFigureGrid.Children.Add(l);
            }
        }

        // Toon de huidige score op het score-UI-element
        public void ShowScore()
        {
            ScoreValueTextBlock.Text = board.Score.ToString();
        }

        // Toon het aantal verbrande lijnen en het huidige niveau op het bijbehorende UI-element
        public void ShowLinesLVL()
        {
            LinesLVLValueTextBlock.Text = board.Lines.ToString() + " / " + board.Level.ToString();
        }

        // Update de posities van labels die de huidige Tetromino weergeven
        private void UpdateCurrentFigureLabels()
        {
            Coord c;
            Label l;
            for (int label_id = 0; label_id < currentFigure_labels.Count; label_id++)
            {
                c = board.CurrentFigure.Coords[label_id];
                l = currentFigure_labels[label_id];
                Grid.SetColumn(l, c.X);
                Grid.SetRow(l, c.Y);
            }
        }

        // Wis alle labels op het hoofdgrid en teken de Tetromino's en het bord opnieuw
        public void UpdateLabels()
        {
            MainGrid.Children.Clear();
            Label l;
            for (int row = board.Rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < board.Columns; col++)
                {
                    if (board[row, col] == null) continue;
                    l = new Label();
                    l.Style = (Style)Application.Current.Resources["TetrominoLabel"];
                    l.Background = Tetromino.GetColor(board[row, col]);
                    Grid.SetRow(l, row);
                    Grid.SetColumn(l, col);
                    MainGrid.Children.Add(l);
                }
            }
        }

        // Handel het einde van het spel af, toon een bericht en wis de grids
        public void GameEnd()
        {
            Stopped = true;
            MessageBox.Show("Game Over!", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            MainGrid.Children.Clear();
            NextFigureGrid.Children.Clear();
        }

        // Wordt aangeroepen bij elke tick van de timer, beweeg de huidige Tetromino naar beneden en handel het spelende spel af
        public void NextTick(object sender, EventArgs e)
        {
            if (Stopped == false)
            {
                MoveDown();
            }
            if (board.GameEndCheck())
            {
                GameEnd();
            }
        }

        // Beweeg de huidige Tetromino naar links
        public void MoveLeft()
        {
            if (board.MoveLeft())
            {
                UpdateCurrentFigureLabels();
            }
        }

        // Beweeg de huidige Tetromino naar rechts
        public void MoveRight()
        {
            if (board.MoveRight())
            {
                UpdateCurrentFigureLabels();
            }
        }

        // Bereken de vertraging op basis van het niveau en stel de timerinterval in
        void CalcDelay()
        {
            Delay = InitialDelay - (board.Level - 1) * 60;
            if (Delay < 100) Delay = 100;
            DTimer.Interval = TimeSpan.FromMilliseconds(Delay);
        }

        // Beweeg de huidige Tetromino naar beneden, handel lijnverbranding en updates af
        public void MoveDown()
        {
            if (board.MoveDown())
            {
                UpdateCurrentFigureLabels();
            }
            else
            {
                board.SaveCurrentFigureCoords();
                if (board.BurnLines()) CalcDelay();
                UpdateLabels();
                ShowLinesLVL();
                ShowScore();
                ShowNextFigure();
                ShowCurrentFigure();
            }
        }

        // Draai de huidige Tetromino
        public void Rotate()
        {
            if (board.Rotate())
            {
                UpdateCurrentFigureLabels();
            }
        }
    }
}
