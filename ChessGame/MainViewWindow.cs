﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using ChessGame;
using System.Windows.Controls;
using System.Windows.Shapes;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    public ObservableCollection<ChessBoardSquare> ChessBoardSquares { get; set; }
    public Board GameBoard { get; private set; }
    private ChessBoardSquare selectedSquare;
    private Game game;

    public MainWindowViewModel()
    {
        ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
        InitializeChessBoard();
        game = new Game();  // Initialize the game
    }

    private void InitializeChessBoard()
    {
        bool isWhiteSquare = true;
        SolidColorBrush darkSquareBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A3B2C"));

        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                var square = new ChessBoardSquare
                {
                    X = col,
                    Y = row,
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
                                new Rectangle
                                {
                                    Fill = (isWhiteSquare ? Brushes.White : darkSquareBrush),
                                    Opacity = 0.5
                                }
                            }
                        }
                    },
                    PieceImage = getInitialPieceImage(row, col)
                };

                ChessBoardSquares.Add(square);

                isWhiteSquare = !isWhiteSquare; // Toggle the color for the next square
            }

            // Toggle the color for the start of the next row
            isWhiteSquare = !isWhiteSquare;
        }
    }

    private Piece getInitialPiece(int row, int col)
    {
        // Correctly setting black pawns on Row 1 and white pawns on Row 6
        if (row == 1) return new Piece.Pawn(false);  // Black pawns on the second row from top
        if (row == 6) return new Piece.Pawn(true);   // White pawns on the second row from bottom

        if (row == 0 || row == 7)
        {
            bool isWhite = row == 7; // Bottom row (7) is white, top row (0) is black

            switch (col)
            {
                case 0: case 7: return new Piece.Rook(isWhite);
                case 1: case 6: return new Piece.Knight(isWhite);
                case 2: case 5: return new Piece.Bishop(isWhite);
                case 3: return new Piece.Queen(isWhite);
                case 4: return new Piece.King(isWhite);
            }
        }

        return null;
    }

    private ImageSource getInitialPieceImage(int row, int col)
    {
        Piece piece = getInitialPiece(row, col);
        return getPieceImage(piece);
    }

    private ImageSource getPieceImage(Piece piece)
    {
        if (piece == null)
        {
            return null;
        }

        string uri = $"pack://application:,,,/ChessPieces/{piece.type.ToString()}_{(piece.isWhite() ? "White" : "Black")}.png";

        try
        {
            return new BitmapImage(new Uri(uri));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString() + uri);
            return null;
        }
    }

    public void OnSquareSelected(ChessBoardSquare square)
    {
        if (selectedSquare == square)
        {
            // If the same square is clicked again, remove highlight
            square.IsHighlighted = false;
            selectedSquare = null;
        }
        else
        {
            // Remove highlight from the previously selected square
            if (selectedSquare != null)
            {
                selectedSquare.IsHighlighted = false;
            }

            // Highlight the newly selected square
            square.IsHighlighted = true;
            selectedSquare = square;
        }
    }

    private void updateUIForMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
    {
        toSquare.PieceImage = fromSquare.PieceImage;
        fromSquare.PieceImage = null;

        // Remove highlight from the fromSquare after move
        fromSquare.IsHighlighted = false;

        // Notify that the properties have changed (UI binding)
        OnPropertyChanged(nameof(ChessBoardSquares));
    }

    protected void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
