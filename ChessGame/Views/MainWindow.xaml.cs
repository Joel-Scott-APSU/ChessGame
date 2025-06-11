using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChessGame.Views;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel viewModel;

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
                viewModel.OnSquareSelected(square);
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e)
        {
            Game.ResetInstance();
            viewModel = new MainWindowViewModel();
            DataContext = viewModel;
        }
    }
}
