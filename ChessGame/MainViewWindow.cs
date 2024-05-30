using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessGame
{
    public class MainWindowViewModel
    {
        public ObservableCollection<ChessBoardSquare> ChessBoardSquares { get; set; }

        public MainWindowViewModel()
        {
            ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
            InitializeChessBoard();
        }

        private void InitializeChessBoard()
        {
            bool isWhite = false;
            SolidColorBrush myBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A3B2C"));

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new ChessBoardSquare
                    {
                        Background = new VisualBrush
                        {
                            Visual = new Grid
                            {
                                Children =
                        {
                            new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Textures/WoodTexture.jpg")),
                                Stretch = Stretch.Fill
                            },
                            new Rectangle { Fill = isWhite ? Brushes.White : myBrush, Opacity = 0.5 }
                        }
                            }
                        },
                        PieceImage = null // or set your default image here
                    };

                    ChessBoardSquares.Add(square);

                    isWhite = !isWhite; // Toggle the color for the next square
                }

                // Flip the color for the next row
                isWhite = !isWhite;
            }
            Debug.WriteLine("ChessBoardSquares populated: " + ChessBoardSquares.Count);
        }

    }
}
