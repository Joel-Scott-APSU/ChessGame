using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChessGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            double newWidth = this.Width * 0.75;
            double newHeight = this.Height * 0.75;

            // Set the new width and height for the window
            this.Width = newWidth;
            this.Height = newHeight;

            DataContext = new MainWindowViewModel();


        }

        private void PieceImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid && grid.DataContext is ChessBoardSquare square)
            {
                var viewModel = this.DataContext as MainWindowViewModel;
                viewModel.OnSquareSelected(square);  // Pass the clicked square to the view model
            }
        }

        private void ResetGame_Click(object sender, RoutedEventArgs e) 
        {
            DataContext = new MainWindowViewModel();
        }

    }
}