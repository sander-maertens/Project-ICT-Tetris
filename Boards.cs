using System;
using System.Linq;

namespace Tetris_WPF.Code
{
    internal class Board
    {
        public const int SIZE_X = 10;
        public const int SIZE_Y = 20;

        private readonly Tetromino.Types?[][] board;
        private Tetromino currentFigure;
        private Tetromino nextFigure;
        private readonly Random generator;

        public Tetromino.Types? this[int row, int col]
        {
            get => board[row][col];
        }
        public int Columns { get => SIZE_X; }
        public int Rows { get => SIZE_Y; }
        public int Score { get; private set; }
        public int Level { get; private set; }
        public int Lines { get; private set; }

        public Tetromino CurrentFigure { get => currentFigure; private set => currentFigure = value; }
        public Tetromino NextFigure { get => nextFigure; private set => nextFigure = value; }

        // Constructor om een nieuw bord te maken met een optioneel uniek zaad voor de Random-generator
        public Board(int UniqueSeed = -1)
        {
            Score = 0;
            Level = 1;
            Lines = 0;

            // Initialisatie van het bord
            board = new Tetromino.Types?[SIZE_Y][];
            for (int row = 0; row < SIZE_Y; row++) board[row] = new Tetromino.Types?[SIZE_X];

            // Initialisatie van de Random-generator met een uniek zaad indien opgegeven, anders willekeurig zaad
            if (UniqueSeed == -1) generator = new Random();
            else generator = new Random(UniqueSeed);

            // Genereren van de eerste Tetromino's
            CurrentFigure = GenerateNewTetromino();
            NextFigure = GenerateNewTetromino();
        }

        // Alternatieve constructor die het huidige Unix-tijdzaad gebruikt
        public Board() : this((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        { }

        // Controleer of het spel is geëindigd
        public bool GameEndCheck()
        {
            foreach (Coord c in CurrentFigure.Coords)
            {
                if (board[c.Y][c.X] != null) return true;
            }
            return false;
        }

        // Genereer een nieuwe willekeurige Tetromino
        private Tetromino GenerateNewTetromino()
        {
            Array values = Enum.GetValues(typeof(Tetromino.Types));
            Tetromino.Types type = (Tetromino.Types)values.GetValue(generator.Next(values.Length));

            Tetromino t = new Tetromino(type);
            return t;
        }

        // Sla de huidige Tetromino-coördinaten op het bord op, verschuif Tetromino's en genereer een nieuwe volgende Tetromino
        public void SaveCurrentFigureCoords()
        {
            foreach (Coord c in CurrentFigure.Coords)
            {
                board[c.Y][c.X] = CurrentFigure.Type;
            }
            ShiftTetrominoes();
            NextFigure = GenerateNewTetromino();
        }

        // Verschuif Tetromino's
        private void ShiftTetrominoes()
        {
            CurrentFigure = NextFigure;
        }

        // Controleer of de huidige Tetromino naar beneden kan bewegen
        public bool CanMoveDown()
        {
            foreach (Coord c in CurrentFigure.Coords)
            {
                if (c.Y + 1 >= SIZE_Y || board[c.Y + 1][c.X] != null) return false;
            }
            return true;
        }

        // Controleer of de huidige Tetromino naar links kan bewegen
        public bool CanMoveLeft()
        {
            foreach (Coord c in CurrentFigure.Coords)
            {
                if (c.X - 1 < 0 || board[c.Y][c.X - 1] != null) return false;
            }
            return true;
        }

        // Controleer of de huidige Tetromino naar rechts kan bewegen
        public bool CanMoveRight()
        {
            foreach (Coord c in CurrentFigure.Coords)
            {
                if (c.X + 1 >= SIZE_X || board[c.Y][c.X + 1] != null) return false;
            }
            return true;
        }

        // Beweeg de huidige Tetromino naar beneden als mogelijk, retourneer succes
        public bool MoveDown()
        {
            if (CanMoveDown() == false) return false;
            foreach (Coord c in CurrentFigure.Coords)
            {
                c.Y++;
            }
            return true;
        }

        // Beweeg de huidige Tetromino naar links als mogelijk, retourneer succes
        public bool MoveLeft()
        {
            if (CanMoveLeft() == false) return false;
            foreach (Coord c in CurrentFigure.Coords)
            {
                c.X--;
            }
            return true;
        }

        // Beweeg de huidige Tetromino naar rechts als mogelijk, retourneer succes
        public bool MoveRight()
        {
            if (CanMoveRight() == false) return false;
            foreach (Coord c in CurrentFigure.Coords)
            {
                c.X++;
            }
            return true;
        }

        // Verbrand volledige lijnen, bereken score, niveau en controleer of er een nieuw niveau is bereikt
        // Retourneer true als er een nieuw niveau is bereikt
        public bool BurnLines()
        {
            int burned_lines = 0;
            for (int row = Rows - 1; row >= 0 && burned_lines < Tetromino.SIZE; row--)
            {
                if (board[row].All(x => x != null))
                {
                    board[row] = new Tetromino.Types?[Columns];
                    burned_lines++;
                }
            }
            Lines += burned_lines;
            int current_row = Rows - 1;
            for (int row = Rows - 1; row >= 0; row--)
            {
                if (board[row].Any(x => x != null))
                {
                    (board[current_row], board[row]) = (board[row], board[current_row]);
                    current_row--;
                }
            }

            Score += burned_lines * burned_lines * 100 * Level;

            if (Lines > Level * 10)
            {
                Level++;
                return true;
            }
            return false;
        }

        // Draai de huidige Tetromino als mogelijk, retourneer succes
        public bool Rotate()
        {
            Coord mid = CurrentFigure.Coords[Tetromino.SIZE / 2];

            Coord[] coords = CurrentFigure.Coords;
            Coord[] newcoords = new Coord[coords.Length];

            Coord c;

            for (int coord_idx = 0; coord_idx < newcoords.Length; coord_idx++)
            {
                c = new Coord(coords[coord_idx].X, coords[coord_idx].Y);
                newcoords[coord_idx] = c;

                if (c.X == mid.X && c.Y == mid.Y) continue;

                if (c.X != mid.X && c.Y == mid.Y)
                {
                    c.Y = mid.Y - (mid.X - c.X);
                    c.X = mid.X;
                }
                else if (c.X == mid.X && c.Y != mid.Y)
                {
                    c.X = mid.X + (mid.Y - c.Y);
                    c.Y = mid.Y;
                }

                else if (c.X < mid.X)
                {
                    if (c.Y < mid.Y)
                    {
                        c.X = mid.X + (mid.X - c.X);
                    }
                    else
                    {
                        c.Y = mid.Y + (mid.Y - c.Y);
                    }
                }
                else
                {
                    if (c.Y < mid.Y)
                    {
                        c.Y = mid.Y - (c.Y - mid.Y);
                    }
                    else
                    {
                        c.X = mid.X - (c.X - mid.X);
                    }
                }

                if (c.X < 0 || c.X >= SIZE_X || c.Y < 0 || c.Y >= SIZE_Y || board[c.Y][c.X] != null) return false;
            }

            for (int coord_idx = 0; coord_idx < newcoords.Length; coord_idx++)
            {
                CurrentFigure.Coords[coord_idx].X = newcoords[coord_idx].X;
                CurrentFigure.Coords[coord_idx].Y = newcoords[coord_idx].Y;
            }
            return true;
        }
    }
}
