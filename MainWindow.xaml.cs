using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tetris_WPF.Code;
using System.Windows.Threading;
using Project_ICT_Tetris;
using System.IO.Ports;

namespace Tetris_WPF
{
    public partial class MainWindow : Window
    {
        private const string StartText = "Start";
        private const string StopText = "Stop";
        private const string ContinueText = "Continue";

        private VisualBoard visualBoard;
        DispatcherTimer dt;

        // Functie om de status van SW1 te lezen (placeholder, moet worden geïmplementeerd)
        private bool ReadSW1Status()
        {
            return false;
        }

        // Functie om de status van SW2 te lezen (placeholder, moet worden geïmplementeerd)
        private bool ReadSW2Status()
        {
            return false;
        }

        // Functie om de status van SW3 te lezen (placeholder, moet worden geïmplementeerd)
        private bool ReadSW3Status()
        {
            return false;
        }

        // Functie om de status van SW4 te lezen (placeholder, moet worden geïmplementeerd)
        private bool ReadSW4Status()
        {
            return false;
        }

        // Constructor van het MainWindow
        public MainWindow()
        {
            InitializeComponent();
        }

        // Simuleert bewegingen op basis van statussen van schakelaars
        private void SimulateMovements(bool sw1, bool sw2, bool sw3, bool sw4)
        {
            // Controleert of het spelbord niet null is en niet is gestopt
            if (visualBoard == null || visualBoard.Stopped) return;

            // Simuleert bewegingen op basis van de statussen van de schakelaars
            if (sw1)
            {
                visualBoard.MoveLeft();
            }
            else if (sw2)
            {
                visualBoard.MoveRight();
            }
            else if (sw3)
            {
                visualBoard.MoveDown();
            }
            else if (sw4)
            {
                visualBoard.Rotate();
            }
        }

        // Leest de status van de knoppen van de microcontroller
        private void ReadButtonStatusFromMicrocontroller()
        {
            // Leest de status van de knoppen van de microcontroller via de seriële poort
            string buttonStatusString = SerialPort.ReadLine();
            bool sw1 = buttonStatusString.Contains("SW1");
            bool sw2 = buttonStatusString.Contains("SW2");
            bool sw3 = buttonStatusString.Contains("SW3");
            bool sw4 = buttonStatusString.Contains("SW4");

            // Simuleert bewegingen op basis van de knopstatussen
            SimulateMovements(sw1, sw2, sw3, sw4);
        }

        // Het startpunt van het programma
        static void Main()
        {
            // Maakt een instantie van het MainWindow
            MainWindow mainWindow = new MainWindow();
            mainWindow.InitializeComponent();

            // Opent een seriële poort op COM3 met een baudrate van 9600
            SerialPort serialPort = new SerialPort("COM3", 9600);
            serialPort.Open();

            while (true)
            {
                // Leest de statussen van de knoppen van de microcontroller via de seriële poort
                bool sw1 = mainWindow.ReadSW1Status();
                bool sw2 = mainWindow.ReadSW2Status();
                bool sw3 = mainWindow.ReadSW3Status();
                bool sw4 = mainWindow.ReadSW4Status();

                // Simuleert bewegingen op basis van de knopstatussen
                mainWindow.ReadButtonStatusFromMicrocontroller();

                // Wacht kort om onnodige CPU-belasting te voorkomen
                System.Threading.Thread.Sleep(100);
            }
        }

        // Event handler voor het klikken op de start/stop/continue knop
        private void Start_ButtonClicked(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;

            // Controleert welke tekst op de knop staat en voert de juiste actie uit
            if (b.Content.ToString() == StartText)
            {
                StartGame();
            }
            else if (b.Content.ToString() == StopText)
            {
                StopGame();
            }
            else if (b.Content.ToString() == ContinueText)
            {
                ContinueGame();
            }
        }

        // Start het spel
        private void StartGame()
        {
            // Maakt een nieuw VisualBoard en geeft de nodige elementen door
            visualBoard = new VisualBoard(ref PlayGrid, ref NextFigureGrid, ref ScoreValueTextBlock, ref LinesLVLValueTextBlock);
            StartButton.Content = StopText;

            // Maakt score en level zichtbaar
            ScoreTextBlock.Visibility = Visibility.Visible;
            ScoreValueTextBlock.Visibility = Visibility.Visible;
            LinesLVLTextBlock.Visibility = Visibility.Visible;
            LinesLVLValueTextBlock.Visibility = Visibility.Visible;

            // Toont het huidige en volgende figuur, score en level
            visualBoard.ShowCurrentFigure();
            visualBoard.ShowNextFigure();
            visualBoard.ShowScore();
            visualBoard.ShowLinesLVL();

            // Initialiseert een DispatcherTimer voor het regelmatig uitvoeren van acties
            dt = new DispatcherTimer(DispatcherPriority.Send);
            dt.Tick += new EventHandler(visualBoard.NextTick);
            dt.Tick += new EventHandler(CheckGameEnd);
            dt.Interval = TimeSpan.FromMilliseconds(visualBoard.Delay);

            visualBoard.DTimer = dt;

            // Start de timer
            dt.Start();

            visualBoard.Stopped = false;
        }

        // Controleert of het spel is beëindigd
        private void CheckGameEnd(object sender, EventArgs e)
        {
            if (visualBoard.Stopped)
            {
                EndGame();
            }
        }

        // Eindigt het spel
        private void EndGame()
        {
            // Stopt de timer en maakt visualBoard null
            dt.Stop();
            dt = null;
            visualBoard = null;

            // Verbergt de score en level elementen
            ScoreValueTextBlock.Visibility = Visibility.Hidden;
            ScoreTextBlock.Visibility = Visibility.Hidden;
            LinesLVLValueTextBlock.Visibility = Visibility.Hidden;
            LinesLVLTextBlock.Visibility = Visibility.Hidden;

            // Zet de knop tekst terug naar "Start"
            StartButton.Content = StartText;
        }

        // Stopt het spel
        private void StopGame()
        {
            dt.Stop();
            visualBoard.Stopped = true;
            StartButton.Content = ContinueText;
        }

        // Gaat verder met het spel
        private void ContinueGame()
        {
            visualBoard.Stopped = false;
            StartButton.Content = StopText;

            // Start de timer
            dt.Start();
        }

        // Event handler voor het indrukken van een toets
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Controleert of het spelbord niet null is en niet is gestopt
            if (visualBoard == null || visualBoard.Stopped) return;

            // Voert de juiste actie uit op basis van de ingedrukte toets
            if (e.Key == Key.A || e.Key == Key.Left)
            {
                visualBoard.MoveLeft();
            }
            if (e.Key == Key.D || e.Key == Key.Right)
            {
                visualBoard.MoveRight();
            }
            if (e.Key == Key.S || e.Key == Key.Down)
            {
                visualBoard.MoveDown();
            }
            if (e.Key == Key.R || e.Key == Key.Up)
            {
                visualBoard.Rotate();
            }
            if (e.Key == Key.Escape && visualBoard.Stopped == false)
            {
                StopGame();
            }
        }
    }
}
