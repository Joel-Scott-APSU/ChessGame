using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Chess.Core;
using ChessGame.Commands;
using ChessGame.Core;
using System.Timers;
using ChessGame.Models;
using static ChessGame.Models.Piece;

namespace ChessGame.Views
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        // === Events ===
        public event PropertyChangedEventHandler PropertyChanged;

        // === Public Properties ===

        public ObservableCollection<ChessBoardSquare> ChessBoardSquares { get; set; }
        public ObservableCollection<string> WhiteMoves { get; set; } = new();
        public ObservableCollection<string> BlackMoves { get; set; } = new();
        public ICommand SquareClickedCommand { get; }
        public ICommand PromoteCommand { get; }
        public ICommand ClaimDrawCommand { get; }

        public string _currentTurnText;
        public string CurrentTurnText
        {
            get => _currentTurnText;
            set
            {
                _currentTurnText = value;
                OnPropertyChanged(nameof(CurrentTurnText));
            }
        }

        public bool _hasGameBegun;
        public bool HasGameBegun
        {
            get => _hasGameBegun;
            set
            {
                _hasGameBegun = value;
                OnPropertyChanged(nameof(HasGameBegun));
            }
        }

        public bool _isGameOver;
        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                _isGameOver = value;
                OnPropertyChanged(nameof(IsGameOver));
            }
        }

        private string _invalidMoveMessage;
        public string InvalidMoveMessage
        {
            get => _invalidMoveMessage;
            set
            {
                _invalidMoveMessage = value;
                OnPropertyChanged(nameof(InvalidMoveMessage));
            }
        }
        private string _whiteClock;
        public string WhiteClock
        {
            get => _whiteClock;
            set
            {
                _whiteClock = value;
                OnPropertyChanged(nameof(WhiteClock));
            }
        }

        private string _blackClock;
        public string BlackClock
        {
            get => _blackClock;
            set
            {
                _blackClock = value;
                OnPropertyChanged(nameof(BlackClock));
            }
        }
        public Brush CurrentTurnColor
        {
            get => _currentTurnColor;
            set
            {
                _currentTurnColor = value;
                OnPropertyChanged(nameof(CurrentTurnColor));
            }
        }

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

        private bool isWhiteTurn;
        public bool IsWhiteTurn
        {
            get => isWhiteTurn;
            set
            {
                if (isWhiteTurn != value)
                {
                    isWhiteTurn = value;
                    OnPropertyChanged(nameof(IsWhiteTurn));
                    OnPropertyChanged(nameof(IsBlackTurn)); // Raise both to keep the radio buttons in sync
                }
            }
        }

        public bool IsBlackTurn => !IsWhiteTurn;

        private bool _isDrawClaimVisible;
        public bool IsDrawClaimVisible
        {
            get => _isDrawClaimVisible;
            set
            {
                _isDrawClaimVisible = value;
                OnPropertyChanged(nameof(IsDrawClaimVisible));
            }
        }



        // === Private Fields ===
        private Game game;
        private bool _IsAwaitingPromotion = false;
        private ChessBoardSquare selectedSquare;
        private Brush _currentTurnColor;
        private bool _isPromotionVisible;
        private bool _promotionIsWhite;
        private TaskCompletionSource<Piece> _promotionTcs;
        private Board board => game.board;
        private ThreatMap threatMap => game.threatMap;

        // === Constructor ===
        public MainWindowViewModel()
        {
            ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
            InitializeChessBoard();
            game = Game.GetInstance(this);

            UpdateTurnDisplay(game.currentTurn);

            PromoteCommand = new RelayCommand(param => HandlePromotion(param));
            ClaimDrawCommand = new RelayCommand(_ => HandleClaimDraw());
            HasGameBegun = false;
            IsGameOver = false;
        }

        // === Core UI Logic ===
        public async Task OnSquareSelectedAsync(ChessBoardSquare square)
        {
            if (!HasGameBegun)
            {
                InvalidMoveMessage = "Select a time to begin the game";
                return;
            }
            else if (_IsAwaitingPromotion)
            {
                InvalidMoveMessage = "Promote Your Pawn.";
                return;
            }
            else if (IsGameOver)
            {

                return;
            }


            if (selectedSquare == null)
            {
                if (square.PieceImage != null)
                {
                    square.IsHighlighted = true;
                    selectedSquare = square;
                }
                return;
            }

            if (!square.IsHighlighted)
            {
                MoveResult result =
                    await game.movePiece(selectedSquare, square);

                if (result.MoveSuccessful)
                {
                    InvalidMoveMessage = string.Empty;

                    Spot promotionSpot = board.GetSpot(square.row, square.column);

                    if (promotionSpot.Piece is Piece.Pawn pawn &&
                        ((pawn.isWhite() && square.row == 0) || (!pawn.isWhite() && square.row == 7)))
                    {
                        MovePiece(selectedSquare, square);
                        selectedSquare.IsHighlighted = false;
                        selectedSquare = null;
                        _IsAwaitingPromotion = true;
                        Piece promotedPiece = await UpdateUIForPromotion(promotionSpot, pawn, square);
                        pawn.setCurrentPosition(promotionSpot);
                        _IsAwaitingPromotion = false;
                        Debug.WriteLine("Promotion Piece: " + pawn);
                        game.gameRules.WriteMoveOutputPromotion(pawn, game.currentTurn, result.fromSquare, result.toSquare, result.capturedPiece, promotedPiece);
                        game.gameRules.swapTurn();
                        OnPropertyChanged(nameof(CurrentTurnText));
                        
                        return;
                    }

                    if (result.EnPassantCaptureOccurred)
                    {
                        Debug.WriteLine("En passant Capture");
                        MovePieceEnPassant(selectedSquare, square);
                    }
                    else if (result.CastledKingSide)
                    {
                        MovePiecesCastleKingside(selectedSquare, square);
                        if(!game.currentTurn.IsWhite)
                            WhiteMoves.Insert(0, "O-O");
                        else
                            BlackMoves.Insert(0, "O-O");
                    }
                    else if (result.CastledQueenSide)
                    {
                        MovePiecesCastleQueenside(selectedSquare, square);
                        if (!game.currentTurn.IsWhite)
                            WhiteMoves.Insert(0, "O-O-O");
                        else
                            BlackMoves.Insert(0, "O-O-O");
                    }
                    else
                    {
                        MovePiece(selectedSquare, square);
                    }


                    OnPropertyChanged(nameof(CurrentTurnText));
                    if(game.gameRules.counter >= 100)
                    {
                        IsDrawClaimVisible = true;
                    }
                    else
                    {
                        IsDrawClaimVisible = false;
                    }

                    game.gameRules.swapTurn();
                    game.gameRules.WriteMoveOutput(result.movingPiece, game.currentTurn, result.fromSquare, result.toSquare, result.capturedPiece);

                    if (game.EndGame())
                    {
                        IsGameOver = true;
                    }

                    }
                else
                {
                    if (threatMap.IsKingInCheck(game.currentTurn.IsWhite))
                    {
                        string currentPlayer = game.currentTurn.IsWhite ? "White king in check" : "Black king in check";
                        InvalidMoveMessage = $"Invalid move. {currentPlayer}";
                    }
                    else
                    {
                        InvalidMoveMessage = "Invalid move.";
                    }
                }
            }

            if (selectedSquare != null)
                selectedSquare.IsHighlighted = false;

            selectedSquare = null;
        }

        public void UpdateTurnDisplay(Player currentTurn)
        {
            CurrentTurnText = currentTurn.IsWhite ? "White's Turn" : "Black's Turn";
            CurrentTurnColor = currentTurn.IsWhite ? Brushes.Turquoise : Brushes.Yellow;

            IsWhiteTurn = currentTurn.IsWhite;
        }

        // === Promotion Handling ===
        private async Task<Piece> UpdateUIForPromotion(Spot promotionSpot, Piece.Pawn pawn, ChessBoardSquare square)
        {
            _promotionIsWhite = pawn.isWhite();
            IsPromotionVisible = true;
            _promotionTcs = new TaskCompletionSource<Piece>();

            Piece promotedPiece = await _promotionTcs.Task;
            game.gameRules.PromotePawn(promotedPiece.type.ToString(), pawn, promotionSpot);

            Spot updatedSpot = board.GetSpot(square.row, square.column);
            square.PieceImage = GetPieceImage(updatedSpot.Piece);

            OnPropertyChanged(nameof(ChessBoardSquares));
            return updatedSpot.Piece;
        }

        private void HandlePromotion(object param)
        {
            if (param is string promotionType && _promotionTcs != null)
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
                    _promotionTcs.SetResult(promotedPiece);
                    IsPromotionVisible = false;
                }
            }
        }

        // === Movement Helpers ===
        private void MovePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            toSquare.PieceImage = fromSquare.PieceImage;
            fromSquare.PieceImage = null;
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private void MovePieceEnPassant(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Player player = game.currentTurn;
            int direction = player.IsWhite ? 1 : -1;
            ChessBoardSquare enPassantSquare = FindSquare(toSquare.row + direction, toSquare.column);

            if (enPassantSquare != null)
                enPassantSquare.PieceImage = null;

            MovePiece(fromSquare, toSquare);
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
            return ChessBoardSquares.FirstOrDefault(s => s.row == row && s.column == column);
        }

        // === Board Initialization ===
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
                                        Opacity = 0.5
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

        private ImageSource GetInitialPieceImage(int row, int col)
        {
            Piece piece = GetInitialPiece(row, col);
            return GetPieceImage(piece);
        }

        private ImageSource GetPieceImage(Piece piece)
        {
            if (piece == null) return null;

            string uri = $"pack://application:,,,/ChessPieces/{piece.type}_{(piece.isWhite() ? "White" : "Black")}.png";

            try
            {
                return new BitmapImage(new Uri(uri));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error loading piece image: {e.Message} for URI: {uri}");
                MessageBox.Show(e.ToString() + uri);
                return null;
            }
        }

        // === Utility ===
        public bool? GetSquareColor(int row, int col)
        {
            var square = ChessBoardSquares.FirstOrDefault(s => s.row == row && s.column == col);
            return square?.isWhiteSquare;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void HandleClaimDraw()
        {
            IsGameOver = true;
            InvalidMoveMessage = "Draw claimed by player.";

        }
    }
}
