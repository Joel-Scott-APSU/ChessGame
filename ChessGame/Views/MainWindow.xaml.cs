using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChessGame.Views;
using System.Timers;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

        private TimeSpan WhiteTimerDuration;
        private TimeSpan BlackTimerDuration;
        private bool isTimerIndefinite = false;
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        private TimeSpan _whiteTimeRemaining;
        private TimeSpan _blackTimeRemaining;
        public MainWindow()
        {
            InitializeComponent();

            double newWidth = this.Width * 0.75;
            double newHeight = this.Height * 0.75;

            this.Width = newWidth;
            this.Height = newHeight;

            viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            // Removed ShowPromotionSelection delegate hookup
        }

        private void PieceImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is ChessBoardSquare square)
            {
                viewModel.OnSquareSelectedAsync(square);
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            Game.ResetInstance();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }


        public void SetTimer_2Min_Click(object sender, EventArgs e)
        {
            isTimerIndefinite = false;
            WhiteTimerDuration = TimeSpan.FromMinutes(2);
            BlackTimerDuration = TimeSpan.FromMinutes(2);
            InitializeTimers();
            viewModel.HasGameBegun = true; // Ensure the game begins when a timer is set
        }

        private void SetTimer_5Min_Click(object sender, EventArgs e)
        {
            isTimerIndefinite = false;
            WhiteTimerDuration = TimeSpan.FromMinutes(5);
            BlackTimerDuration = TimeSpan.FromMinutes(5);
            InitializeTimers();
            viewModel.HasGameBegun = true; // Ensure the game begins when a timer is set
        }

        private void SetTimer_10Min_Click(object sender, EventArgs e)
        {
            isTimerIndefinite = false;
            WhiteTimerDuration = TimeSpan.FromMinutes(10);
            BlackTimerDuration = TimeSpan.FromMinutes(10);
            InitializeTimers();
            viewModel.HasGameBegun = true; // Ensure the game begins when a timer is set
        }

        private void SetTimer_20Min_Click(object sender, EventArgs e)
        {
            isTimerIndefinite = false;
            WhiteTimerDuration = TimeSpan.FromMinutes(20);
            BlackTimerDuration = TimeSpan.FromMinutes(20);
            InitializeTimers();
            viewModel.HasGameBegun = true; // Ensure the game begins when a timer is set
        }

        private void SetTimer_Indefinite_Click(object sender, EventArgs e)
        {
            isTimerIndefinite = true;
            WhiteTimerDuration = TimeSpan.MaxValue;
            BlackTimerDuration = TimeSpan.MaxValue;
            InitializeTimers();
            viewModel.HasGameBegun = true; // Ensure the game begins when a timer is set
        }

        private void InitializeTimers()
        {
            if (isTimerIndefinite)
            {
                viewModel.WhiteClock = "∞";
                viewModel.BlackClock = "∞";
                return;
            }

            _whiteTimeRemaining = WhiteTimerDuration;
            _blackTimeRemaining = BlackTimerDuration;

            viewModel.WhiteClock = WhiteTimerDuration.ToString(@"mm\:ss");
            viewModel.BlackClock = BlackTimerDuration.ToString(@"mm\:ss");

            if(_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
            }

            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimer.Tick += Timer_Tick;
            _dispatcherTimer.Start();

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!viewModel.HasGameBegun || isTimerIndefinite) return;

            var game = Game.GetInstance(viewModel);

            if (game.currentTurn.IsWhite)
            {
                _whiteTimeRemaining = _whiteTimeRemaining.Subtract(TimeSpan.FromSeconds(1));
                viewModel.WhiteClock = _whiteTimeRemaining.ToString(@"mm\:ss");


                if (_whiteTimeRemaining <= TimeSpan.Zero)
                {
                    _whiteTimeRemaining = TimeSpan.Zero;
                    viewModel.WhiteClock = "00:00";
                    _dispatcherTimer.Stop();
                    viewModel.IsGameOver = true;
                    viewModel.InvalidMoveMessage = "White's time is up! Black wins!";
                    return;
                }
            }
            else
            {
                _blackTimeRemaining = _blackTimeRemaining.Subtract(TimeSpan.FromSeconds(1));
                viewModel.BlackClock = _blackTimeRemaining.ToString(@"mm\:ss");

                if (_blackTimeRemaining <= TimeSpan.Zero)
                {
                    _blackTimeRemaining = TimeSpan.Zero;
                    viewModel.BlackClock = "00:00";
                    _dispatcherTimer.Stop();
                    viewModel.IsGameOver = true;
                    viewModel.InvalidMoveMessage = "Black's time is up! White wins!";
                    return;
                }
            }
        }
    }
}
