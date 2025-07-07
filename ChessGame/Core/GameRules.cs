using Chess.Core;
using ChessGame.Models;
using ChessGame.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static ChessGame.Models.Piece;

namespace ChessGame.Core
{
    public class GameRules
    {
        // === Singleton Management ===
        private static GameRules? instance;
        public static GameRules GetInstance(Game game, MainWindowViewModel viewModel)
        {
            if (instance == null)
                instance = new GameRules(game, viewModel);
            return instance;
        }

        public static void ResetInstance() => instance = null;

        internal static GameRules Create(Game game, MainWindowViewModel viewModel)
        {
            instance = new GameRules(game, viewModel);
            return instance;
        }

        // === Fields ===
        private Game game;
        private HashSet<Piece> activePieces;
        private MainWindowViewModel viewModel;
        private ThreatMap threatMap => game.threatMap;
        private Board board => game.board;
        public int counter { get; private set; }

        // === Constructor ===
        public GameRules(Game game, MainWindowViewModel viewModel)
        {
            this.game = game;
            this.viewModel = viewModel;
            activePieces = new HashSet<Piece>();
            counter = 0;
        }

        // === Initialization ===
        public void InitializeActivePieces()
        {
            activePieces = new HashSet<Piece>(game.whitePlayer.GetPieces().Concat(game.blackPlayer.GetPieces()));
        }

        public void InitializeActivePiecesForTest()
        {
            activePieces = new HashSet<Piece>();
        }

        // === Move Handling ===
        public async Task<MoveResult> HandleMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Board board = game.board;
            Player currentTurn = game.currentTurn;

            Spot start = board.GetSpot(fromSquare.row, fromSquare.column);
            Spot end = board.GetSpot(toSquare.row, toSquare.column);
            Piece? movingPiece = start?.Piece;
            Piece? capturedPiece = null;

            if (movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return new MoveResult
                {
                    MoveSuccessful = false,
                    EnPassantCaptureOccurred = false,
                    CastledKingSide = false,
                    CastledQueenSide = false,
                    movingPiece = null,
                    capturedPiece = null,
                    fromSquare = fromSquare,
                    toSquare = toSquare
                };
            }

            if (threatMap.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
                return new MoveResult
                {
                    MoveSuccessful = false,
                    EnPassantCaptureOccurred = false,
                    CastledKingSide = false,
                    CastledQueenSide = false,
                    movingPiece = null,
                    capturedPiece = null,
                    fromSquare = fromSquare,
                    toSquare = toSquare
                };
            }

            bool moveSuccessful = false;
            bool enPassantCaptureOccurred = false;
            bool castledKingSide = false;
            bool castledQueenSide = false;

            if (movingPiece.legalMove(threatMap, start, end, board))
            {
                capturedPiece = end.Piece;
                if (movingPiece is Piece.Pawn)
                    counter = 0;
                else if(capturedPiece != null)
                    counter = 0;
                else
                    counter++;

                if (movingPiece is Piece.Pawn && enPassantCapture(movingPiece, toSquare, board, out Piece? enPassantCapturedPiece))
                {
                    capturedPiece = enPassantCapturedPiece;
                    enPassantCaptureOccurred = true;
                }

                if (capturedPiece != null && capturedPiece.isWhite() != movingPiece.isWhite())
                    CapturePiece(capturedPiece);

                end.Piece = movingPiece;
                start.Piece = null;
                movingPiece.setCurrentPosition(end);
                moveSuccessful = true;
            }
            else if (movingPiece is Piece.King king && end.Piece is Piece.Rook)
            {
                castledKingSide = await PerformCastling(king, toSquare, true, board);
                castledQueenSide = await PerformCastling(king, toSquare, false, board);

                if ((toSquare.column == 7 && castledKingSide) || (toSquare.column == 0 && castledQueenSide))
                    moveSuccessful = true;
            }

            return new MoveResult
            {
                MoveSuccessful = moveSuccessful,
                EnPassantCaptureOccurred = enPassantCaptureOccurred,
                CastledKingSide = castledKingSide,
                CastledQueenSide = castledQueenSide,
                movingPiece = movingPiece,
                capturedPiece = capturedPiece,
                fromSquare = fromSquare,
                toSquare = toSquare
            };
        }

        private bool enPassantCapture(Piece movingPiece, ChessBoardSquare toSquare, Board board, out Piece enPassantCapturedPiece)
        {
            enPassantCapturedPiece = null;
            if (movingPiece is Piece.Pawn pawn)
            {
                int direction = movingPiece.isWhite() ? 1 : -1;
                Spot enPassantSpot = board.GetSpot(toSquare.row + direction, toSquare.column);
                Piece? enPassantPiece = enPassantSpot?.Piece;

                if (enPassantPiece is Piece.Pawn enPassantPawn && enPassantPawn.isEnPassant && enPassantPawn.isWhite() != pawn.isWhite())
                {
                    enPassantCapturedPiece = enPassantPawn;
                    CapturePiece(enPassantPawn);
                    enPassantSpot.Piece = null;
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> PerformCastling(Piece.King king, ChessBoardSquare fromSquare, bool isKingside, Board board)
        {
            if ((isKingside && king.canCastleKingside(king.isWhite(), threatMap, board) && !king.hasMoved) ||
                (!isKingside && king.canCastleQueenside(king.isWhite(), threatMap, board) && !king.hasMoved))
            {
                try
                {
                    int rookColumn = isKingside ? 7 : 0;
                    int kingTargetColumn = isKingside ? 6 : 2;
                    int rookTargetColumn = isKingside ? 5 : 3;

                    Spot kingOriginalSpot = board.GetSpot(fromSquare.row, fromSquare.column);
                    Spot rookSpot = board.GetSpot(fromSquare.row, rookColumn);
                    Spot kingTargetSpot = board.GetSpot(fromSquare.row, kingTargetColumn);
                    Spot rookTargetSpot = board.GetSpot(fromSquare.row, rookTargetColumn);

                    if (rookSpot.Piece is Piece.Rook rook && !rook.hasMoved)
                    {
                        king.setCurrentPosition(kingTargetSpot);
                        rook.setCurrentPosition(rookTargetSpot);
                        rook.hasMoved = true;
                        king.hasMoved = true;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during castling: {ex.Message}");
                }
            }
            return false;
        }

        public void swapTurn()
        {
            Player currentTurn = game.currentTurn == game.whitePlayer ? game.blackPlayer : game.whitePlayer;
            currentTurn.ProcessPawns(pawn => pawn.isEnPassant = false);
            game.SetCurrentTurn(currentTurn);
            game.threatMap.UpdateThreatMap(GetActivePieces(!game.currentTurn.IsWhite));
            viewModel.UpdateTurnDisplay(game.currentTurn);
        }

        // === Piece State Management ===
        public void AddActivePiece(Piece piece)
        {
            if (piece != null && !activePieces.Contains(piece))
                activePieces.Add(piece);
        }

        public void RemoveActivePiece(Piece piece) => activePieces.Remove(piece);

        public void CapturePiece(Piece piece) => RemoveActivePiece(piece);

        public IEnumerable<Piece> GetActivePieces(bool isWhite) => activePieces?.Where(piece => piece.isWhite() == isWhite) ?? [];

        public bool? GetSquareColor(int row, int col) => viewModel?.GetSquareColor(row, col);

        private void PiecePositions(IEnumerable<Piece> activePieces) => Debug.WriteLine(activePieces.Count());

        private Player getOpponent(Player player)
        {
            return player.IsWhite ? game.blackPlayer : game.whitePlayer;
        }

        // === Promotion Logic ===
        public void PromotePawn(string PromotionType, Piece.Pawn pawn, Spot promotionSpot)
        {
            Player currentPlayer = pawn.isWhite() ? game.whitePlayer : game.blackPlayer;
            Piece promotedPiece = PromotionType switch
            {
                "Queen" => new Piece.Queen(pawn.isWhite()),
                "Rook" => new Piece.Rook(pawn.isWhite()),
                "Bishop" => new Piece.Bishop(pawn.isWhite()),
                "Knight" => new Piece.Knight(pawn.isWhite()),
                _ => throw new ArgumentException("Invalid Promotion Type")
            };

            CapturePiece(pawn);
            board.CreatePieces(promotedPiece, promotionSpot.Row, promotionSpot.Column, currentPlayer);
            AddActivePiece(promotedPiece);
            if(promotedPiece is Piece.Rook rook)
            {
                rook.hasMoved = true;
            }
        }

        // === Draw Detection ===
        private bool DrawKingBishopVKing() => activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Bishop);
        private bool DrawKingKnightVKing() => activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Knight);
        private bool DrawKingBishopVKingBishop()
        {
            if (activePieces.Count != 4 || activePieces.Count(p => p.type == Piece.PieceType.Bishop) != 2)
                return false;
            var bishops = activePieces.Where(p => p.type == Piece.PieceType.Bishop).ToList();
            Spot position1 = bishops[0].getCurrentPosition();
            Spot position2 = bishops[1].getCurrentPosition();
            bool? color1 = GetSquareColor(position1.Row, position1.Column);
            bool? color2 = GetSquareColor(position2.Row, position2.Column);
            return color1.HasValue && color2.HasValue && color1.Value == color2.Value;
        }

        public bool Draw()
        {
            if (activePieces.Count == 4 && DrawKingBishopVKingBishop()) return true;
            if (activePieces.Count == 3 && (DrawKingBishopVKing() || DrawKingKnightVKing())) return true;
            return activePieces.Count == 2;
        }

        // === Checkmate Detection ===
        public bool Checkmate(Player player)
        {
            var opponentPieces = GetActivePieces(player.IsWhite);
            bool canMove = game.moves.checkForLegalMoves(player, game.board, opponentPieces);

            if (!canMove && threatMap.IsKingInCheck(player.IsWhite))
            {
                Debug.WriteLine("Checkmate! Game Over.");
                return true;
            }
            else if (!canMove)
            {
                Debug.WriteLine("Stalemate! No legal moves available.");
                return true;
            }
            return false;
        }


        // === Notation & Logging ===
        public void WriteMoveOutput(Piece movingPiece, Player player, ChessBoardSquare fromSquare, ChessBoardSquare toSquare, Piece capturedPiece)
        {
            bool kingInCheck = threatMap.IsKingInCheck(!movingPiece.isWhite());
            bool checkmate = Checkmate(player);
            string annotation = checkmate ? "#" : (kingInCheck ? "+" : "");
            string moveNotation = $"{GetMoveNotation(movingPiece, fromSquare, toSquare, capturedPiece != null)}{annotation}";

            if(game.currentTurn.IsWhite)
                viewModel.WhiteMoves.Insert(0, moveNotation);
            else
                viewModel.BlackMoves.Insert(0, moveNotation);

            if (checkmate)
            {
                viewModel.InvalidMoveMessage = "Checkmate";
            }
            else if (kingInCheck)
            {
                string currentPlayer = game.currentTurn.IsWhite ? "White" : "Black";
                viewModel.InvalidMoveMessage = currentPlayer + " King is in check";
            }
            else
            {
                viewModel.InvalidMoveMessage = string.Empty;
            }
        }

        public void WriteMoveOutputPromotion(Piece MovingPiece, Player player, ChessBoardSquare fromSquare, ChessBoardSquare toSquare, Piece capturedPiece, Piece promotedPieceType)
        {
            bool kingInCheck = threatMap.IsKingInCheck(!MovingPiece.isWhite());
            bool checkmate = Checkmate(player);
            string annotation = checkmate ? "#" : (kingInCheck ? "+" : "");
            string promotedPiece = "=" + GetPieceNotationSymbol(promotedPieceType);
            string moveNotation = $"{GetMoveNotation(MovingPiece, fromSquare, toSquare, capturedPiece != null)}{promotedPiece}{annotation}";

            if (game.currentTurn.IsWhite)
                viewModel.WhiteMoves.Insert(0, moveNotation);
            else
                viewModel.BlackMoves.Insert(0, moveNotation);
        }

        public string GetPieceNotationSymbol(Piece piece) => piece?.type switch
        {
            Piece.PieceType.King => "K",
            Piece.PieceType.Queen => "Q",
            Piece.PieceType.Rook => "R",
            Piece.PieceType.Bishop => "B",
            Piece.PieceType.Knight => "N",
            Piece.PieceType.Pawn => "",
            _ => ""
        };

        private string GetMoveNotation(Piece movingPiece, ChessBoardSquare fromSquare, ChessBoardSquare toSquare, bool isCapture, bool isEnPassantCapture = false)
        {
            string toAlgebraic = ToAlgebraic(toSquare.row, toSquare.column);
            string pieceLetter = movingPiece is Piece.Pawn ? "" : GetPieceNotationSymbol(movingPiece);
            string disambiguation = "";

            if (!(movingPiece is Piece.Pawn || movingPiece is Piece.King))
            {
                string otherFrom = GetDisambiguationSquare(movingPiece, toSquare);
                if (otherFrom != null)
                {
                    if (otherFrom[0] != ToAlgebraic(fromSquare.row, fromSquare.column)[0])
                        disambiguation = $"{(char)('a' + fromSquare.column)}";
                    else
                        disambiguation = $"{8 - fromSquare.row}";
                }
            }

            string notation;
            if (movingPiece is Piece.Pawn && isCapture)
            {
                notation = $"{(char)('a' + fromSquare.column)}x{toAlgebraic}";
            }
            else
            {
                notation = isCapture ? $"{pieceLetter}{disambiguation}x{toAlgebraic}" : $"{pieceLetter}{disambiguation}{toAlgebraic}";
            }

            if (isEnPassantCapture)
            {
                notation += " e.p.";
            }

            return notation;
        }


        private string ToAlgebraic(int row, int col) => $"{(char)('a' + col)}{8 - row}";

        private string GetDisambiguationSquare(Piece piece, ChessBoardSquare toSquare)
        {
            foreach (var otherPiece in GetActivePieces(piece.isWhite()))
            {
                if (otherPiece == piece || otherPiece.type != piece.type) continue;
                switch (piece.type)
                {
                    case Piece.PieceType.Knight:
                        if (KnightCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;
                    case Piece.PieceType.Bishop:
                        if (BishopCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;
                    case Piece.PieceType.Rook:
                        if (RookCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;
                    case Piece.PieceType.Queen:
                        if (QueenCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;
                }
            }
            return null;
        }

        // === Piece-Specific Movement Checks ===
        private bool KnightCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            Spot spot = piece.getCurrentPosition();
            int dr = Math.Abs(spot.Row - toSquare.row);
            int dc = Math.Abs(spot.Column - toSquare.column);
            return (dr == 2 && dc == 1) || (dr == 1 && dc == 2);
        }

        private bool BishopCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            Spot spot = piece.getCurrentPosition();
            int sr = spot.Row, sc = spot.Column, tr = toSquare.row, tc = toSquare.column;
            if (Math.Abs(sr - tr) != Math.Abs(sc - tc)) return false;
            int rStep = Math.Sign(tr - sr), cStep = Math.Sign(tc - sc);
            for (int i = 1; i < Math.Abs(sr - tr); i++)
                if (board.GetSpot(sr + i * rStep, sc + i * cStep).Piece != null)
                    return false;
            return true;
        }

        private bool RookCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            Spot spot = piece.getCurrentPosition();
            int sr = spot.Row, sc = spot.Column, tr = toSquare.row, tc = toSquare.column;
            if (sr != tr && sc != tc) return false;
            if (sr == tr)
            {
                int step = Math.Sign(tc - sc);
                for (int c = sc + step; c != tc; c += step)
                    if (board.GetSpot(sr, c).Piece != null) return false;
                return true;
            }
            if (sc == tc)
            {
                int step = Math.Sign(tr - sr);
                for (int r = sr + step; r != tr; r += step)
                    if (board.GetSpot(r, sc).Piece != null) return false;
                return true;
            }
            return false;
        }

        private bool QueenCanReach(Piece piece, ChessBoardSquare toSquare) =>
            BishopCanReach(piece, toSquare) || RookCanReach(piece, toSquare);
    }
}
