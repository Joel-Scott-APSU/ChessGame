using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ChessGame.Models;
using ChessGame.Commands;
using System.Windows.Controls;

namespace ChessGame.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ChessBoardSquare> ChessBoardSquares { get; set; }

        public ICommand PromoteCommand { get; }
        private ChessBoardSquare selectedSquare;
        private Board board => game.board;
        private Game game;

        public Action<Piece> PromotionCallback { get; set; } // Callback for promotion selection

        private bool _isPromotionVisible = false;
        public bool IsPromotionVisible
        {
            get => _isPromotionVisible;
            set
            {
                if (_isPromotionVisible != value)
                {
                    _isPromotionVisible = value;
                    OnPropertyChanged(nameof(IsPromotionVisible));
                }
            }
        }

        private bool _promotionIsWhite; // Store promotion color

        public MainWindowViewModel()
        {
            ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
            InitializeChessBoard();
            game = Game.GetInstance(this);

            PromoteCommand = new RelayCommand(param =>
            {
                if (param is string promotionType && PromotionCallback != null)
                {
                    Piece promotedPiece = promotionType switch
                    {
                        "Queen" => new Piece.Queen(_promotionIsWhite),
                        "Rook" => new Piece.Rook(_promotionIsWhite),
                        "Bishop" => new Piece.Bishop(_promotionIsWhite),
                        "Knight" => new Piece.Knight(_promotionIsWhite),
                        _ => null
                    };

                    if (promotedPiece != null)
                    {
                        PromotionCallback(promotedPiece);
                        IsPromotionVisible = false;
                        PromotionCallback = null;
                    }
                }
            });
        }

        private void InitializeChessBoard()
        {
            bool isWhiteSquare = true;
            SolidColorBrush darkSquareBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A3B2C"));

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var square = new ChessBoardSquare(i, j)
                    {
                        isWhiteSquare = isWhiteSquare,
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
                                        Fill = isWhiteSquare ? Brushes.White : darkSquareBrush,
                                        Opacity = 0.5,
                                    }
                                }
                            }
                        },
                        PieceImage = GetInitialPieceImage(i, j)
                    };

                    ChessBoardSquares.Add(square);
                    isWhiteSquare = !isWhiteSquare;
                }
                isWhiteSquare = !isWhiteSquare;
            }
        }

        public bool? GetSquareColor(int row, int col)
        {
            var square = ChessBoardSquares.FirstOrDefault(s => s.row == row && s.column == col);
            return square?.isWhiteSquare;
        }

        private ImageSource GetInitialPieceImage(int row, int col)
        {
            Piece piece = GetInitialPiece(row, col);
            return GetPieceImage(piece);
        }

        private Piece GetInitialPiece(int row, int col)
        {
            if (row == 1) return new Piece.Pawn(false);
            if (row == 6) return new Piece.Pawn(true);

            if (row == 0 || row == 7)
            {
                bool isWhite = row == 7;

                return col switch
                {
                    0 or 7 => new Piece.Rook(isWhite),
                    1 or 6 => new Piece.Knight(isWhite),
                    2 or 5 => new Piece.Bishop(isWhite),
                    3 => new Piece.Queen(isWhite),
                    4 => new Piece.King(isWhite),
                    _ => null
                };
            }

            return null;
        }

        private ImageSource GetPieceImage(Piece piece)
        {
            if (piece == null)
            {
                return null;
            }

            string uri = $"pack://application:,,,/ChessPieces/{piece.type}_{(piece.isWhite() ? "White" : "Black")}.png";

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
            if (selectedSquare == null)
            {
                if (square.PieceImage != null)
                {
                    square.IsHighlighted = true;
                    selectedSquare = square;
                }
            }
            else
            {
                if (!square.IsHighlighted)
                {
                    (bool moveSuccessful, bool enPassantCapture, bool CastleKingSide, bool CastleQueenSide) = game.movePiece(selectedSquare, square);

                    if (moveSuccessful)
                    {
                        if (square.piece is Piece.Pawn pawn)
                        {
                            bool promotionRow = (pawn.isWhite() && square.row == 0) || (!pawn.isWhite() && square.row == 7);
                            if (promotionRow)
                            {
                                Spot promotionSpot = board.GetSpot(square.row, square.column);
                                _promotionIsWhite = pawn.isWhite();  // Store color for promotion
                                IsPromotionVisible = true;

                                // Set the PromotionCallback for command to call later
                                PromotionCallback = (promotedPiece) =>
                                {
                                    game.gameRules.PromotePawn(promotedPiece.type.ToString(), pawn, promotionSpot);
                                    square.PieceImage = GetPieceImage(promotionSpot.Piece);

                                    OnPropertyChanged(nameof(ChessBoardSquares));
                                    IsPromotionVisible = false;
                                };
                            }
                        }
                        else if (enPassantCapture)
                        {
                            Debug.WriteLine("En Passant Capture is occurring in the UI");
                            MovePieceEnPassant(selectedSquare, square);
                        }
                        else if (CastleKingSide)
                        {
                            MovePiecesCastleKingside(selectedSquare, square);
                        }
                        else if (CastleQueenSide)
                        {
                            MovePiecesCastleQueenside(selectedSquare, square);
                        }
                        else
                        {
                            MovePiece(selectedSquare, square);
                        }
                    }
                }

                selectedSquare.IsHighlighted = false;
                selectedSquare = null;
            }
        }

        private void MovePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            toSquare.PieceImage = fromSquare.PieceImage;
            fromSquare.PieceImage = null;
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private void MovePieceEnPassant(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Player player = game.currentTurn;
            int direction = player.IsWhite ? -1 : 1;
            int enPassantRow = toSquare.row + direction;

            ChessBoardSquare enPassantSquare = FindSquare(enPassantRow, toSquare.column);
            if (enPassantSquare != null)
            {
                enPassantSquare.PieceImage = null;
            }

            toSquare.PieceImage = fromSquare.PieceImage;
            fromSquare.PieceImage = null;
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private void MovePiecesCastleKingside(ChessBoardSquare kingSquare, ChessBoardSquare rookSquare)
        {
            ChessBoardSquare kingTarget = FindSquare(kingSquare.row, kingSquare.column + 2);
            ChessBoardSquare rookTarget = FindSquare(rookSquare.row, kingSquare.column + 1);

            kingTarget.PieceImage = kingSquare.PieceImage;
            rookTarget.PieceImage = rookSquare.PieceImage;

            kingSquare.PieceImage = null;
            rookSquare.PieceImage = null;
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private void MovePiecesCastleQueenside(ChessBoardSquare kingSquare, ChessBoardSquare rookSquare)
        {
            ChessBoardSquare kingTarget = FindSquare(kingSquare.row, kingSquare.column - 2);
            ChessBoardSquare rookTarget = FindSquare(rookSquare.row, kingSquare.column - 1);

            kingTarget.PieceImage = kingSquare.PieceImage;
            rookTarget.PieceImage = rookSquare.PieceImage;

            kingSquare.PieceImage = null;
            rookSquare.PieceImage = null;
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private ChessBoardSquare FindSquare(int row, int column)
        {
            return ChessBoardSquares.FirstOrDefault(square => square.row == row && square.column == column);
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
