using ChessGame.Core;
using ChessGame.Models;
using ChessGame.Views;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Windows.Documents;
using static ChessGame.Models.Piece;

namespace MovesTests
{
    [TestClass]
    public class ChessGameTests
    {
        private Game game;
        private Player whitePlayer;
        private Player blackPlayer;
        private Moves moves;
        private Board board;
        private MainWindowViewModel MainWindowViewModel;
        private GameRules gameRules;
        private HashSet<Piece> activePieces;
        private ThreatMap threatMap;
        [TestInitialize]
        public void Setup()
        {
            Game.ResetInstance();
            GameRules.ResetInstance();

            game = Game.GetInstance(MainWindowViewModel);
            gameRules = GameRules.GetInstance(game, MainWindowViewModel);
            whitePlayer = new Player(true, gameRules);
            blackPlayer = new Player(false, gameRules);
            board = new Board(whitePlayer, blackPlayer, game);
            game.board = board;
            threatMap = new ThreatMap(whitePlayer, blackPlayer, game);
            moves = new Moves(game);
            activePieces = new HashSet<Piece>();
            game.SetCurrentTurn(blackPlayer);
        }
        [TestMethod]
        [STAThread]
        /*Checking to see if any moves exists that will get the king out 
         * of this spot, using black king for piece that should be in checkmate,
         * checking using white king and queen
         */
        public void TestCheckMateScenario1()
        {
            Piece blackKing = new King(false);
            Piece whiteQueen = new Queen(true);
            Piece whiteBishop = new Bishop(true);

            board.clearBoard();
            board.CreatePieces(whiteBishop, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 4, blackPlayer);
            board.CreatePieces(whiteQueen, 6, 4, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteQueen);
            gameRules.AddActivePiece(whiteBishop);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        [STAThread]
        // Checkmate scenario 2 uses a rook and a king to put the king into checkmate 
        public void TestCheckMateScenario2()
        {
            board.clearBoard(); // Clear the board first

            // Re-create and place the pieces after clearing the board
            Piece whiteKing = new King(true);
            Piece blackKing = new King(false);
            Piece whiteQueen = new Queen(true);

            board.CreatePieces(whiteKing, 0, 2, whitePlayer);
            board.CreatePieces(blackKing, 0, 0, blackPlayer);
            board.CreatePieces(whiteQueen, 0, 1, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteQueen);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false)); // Check for legal moves for black king

            Assert.IsFalse(result, "Black should be in checkmate");
        }


        [TestMethod]
        [STAThread]
        //checkmate scenario 4 uses 2 queens and a king 
        public void TestCheckMateScenario4()
        {
            Piece blackKing = new King(false);
            Piece whiteQueen1 = new Queen(true);
            Piece whiteQueen2 = new Queen(true);
            Piece whiteKing = new King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 6, 6, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen1, 5, 6, whitePlayer);
            board.CreatePieces(whiteQueen2, 6, 5, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteQueen1);
            gameRules.AddActivePiece(whiteQueen2);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        [STAThread]
        //checkmate scenario 5 uses a queen, rook, and king 
        public void TestCheckMateScenario5()
        {
            Piece blackKing = new King(false);
            Piece whiteQueen = new Queen(true);
            Piece whiteRook = new Rook(true);
            Piece whiteKing = new King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 6, 6, whitePlayer);
            board.CreatePieces(whiteRook, 7, 6, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteQueen);
            gameRules.AddActivePiece(whiteRook);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        [STAThread]
        //checkmate scenario 6 uses a bishop, pawn, and a king 
        public void TestCheckMateScenario6()
        {
            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece whiteQueen = new Queen(true);
            Piece whiteKing = new King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 6, 6, whitePlayer);
            board.CreatePieces(whiteQueen, 7, 6, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(whiteQueen);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        [STAThread]
        // Scenario where the black bishop captures the white rook to avoid checkmate
        public void TestBishopCapturesToEscapeCheckMate()
        {
            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece blackBishop = new Bishop(false);
            Piece whiteKing = new King(true);

            game.SetCurrentTurn(blackPlayer); // Set the current turn to black player
            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer);
            board.CreatePieces(blackBishop, 6, 6, blackPlayer);
            board.CreatePieces(whiteKing, 5, 6, whitePlayer); // White king to ensure checkmate scenario

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackBishop);
            gameRules.AddActivePiece(whiteKing);

            // The black bishop should be able to capture the white rook on F8
            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black should be able to capture the rook with the bishop and escape checkmate.");
        }


        [TestMethod]
        [STAThread]
        // Scenario where the black king can capture the attacking piece and avoid checkmate
        public void TestCaptureToEscapeCheckMate()
        {
            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece whiteKing = new King(true);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 6, whitePlayer);
            board.CreatePieces(whiteKing, 5, 6, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black should be able to capture the rook and escape checkmate.");
        }

        [TestMethod]
        [STAThread]
        // Scenario where the black king can move out of checkmate
        public void TestMoveKingToEscapeCheckMate()
        {
            Piece blackKing = new King(false);
            Piece whiteQueen = new Queen(true);
            Piece whiteRook = new Rook(true);

            board.clearBoard();

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 7, 5, whitePlayer);
            board.CreatePieces(whiteRook, 5, 5, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteQueen);
            gameRules.AddActivePiece(whiteRook);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black should be able to move to H7 and escape checkmate.");
        }

        [TestMethod]
        [STAThread]
        public void TestPawnBlockCheckMate()
        {
            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece blackRook = new Rook(false);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer);
            board.CreatePieces(blackRook, 5, 6, blackPlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackRook);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black should be able to block the checkmate with the Rook.");
        }

        [TestMethod]
        [STAThread]
        public void Test_KingHasLegalMove_NotCheckmate()
        {
            board.clearBoard();

            // Create white king on E1 (7,4)
            Piece whiteKing = new King(true);
            Piece blackRook = new Rook(false);
            Piece blackKing = new King(false);

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(blackRook, 6, 4, blackPlayer);
            board.CreatePieces(blackKing, 0, 7, blackPlayer); // Place black king somewhere safe

            gameRules.InitializeActivePiecesForTest();
            //Create white king on E1 (7,4)
            gameRules.AddActivePiece(whiteKing);

            // Create black rook on E2 (6,4), threatening the king
            gameRules.AddActivePiece(blackRook);

            gameRules.AddActivePiece(blackKing);

            // Call checkForLegalMoves to determine if the king has an escape
            bool hasLegalMoves = moves.checkForLegalMoves(whitePlayer, board, gameRules.GetActivePieces(true));

            // Assert that the king is not in checkmate
            Assert.IsTrue(hasLegalMoves, "King should be able to move out of check.");
        }

        [TestMethod]
        public void Test_KingHasNoLegalMove_IsCheckmate()
        {
            board.clearBoard();

            game.SetCurrentTurn(blackPlayer);
            // Black King on H8 (0, 7)
            Piece blackKing = new King(false);
            board.CreatePieces(blackKing, 0, 7, blackPlayer);

            // White Rook on H6 (2, 7) - covers h-file
            Piece whiteQueen = new Queen(true);
            board.CreatePieces(whiteQueen, 2, 7, whitePlayer);

            // White Rook on G7 (1, 6) - covers 7th rank
            Piece whiteRook1 = new Rook(true);
            board.CreatePieces(whiteRook1, 1, 6, whitePlayer);

            Piece whiteKing = new King(true);
            board.CreatePieces(whiteKing, 7, 0, whitePlayer); // Place white king somewhere safe

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook1);
            gameRules.AddActivePiece(whiteQueen);
            gameRules.AddActivePiece(whiteKing);

            bool hasLegalMoves = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(hasLegalMoves, "Black King is in checkmate and should have no legal moves.");
        }

        [TestMethod]
        public void Test_KingHasNoLegalMove_Stalemate()
        {
            board.clearBoard();

            game.SetCurrentTurn(blackPlayer);
            gameRules.InitializeActivePiecesForTest();

            // Black King on H8 (0,7)
            Piece blackKing = new King(false);
            board.CreatePieces(blackKing, 0, 7, blackPlayer);
            gameRules.AddActivePiece(blackKing);

            // White King somewhere far to ensure the game is valid (e.g., A1 at 7,0)
            Piece whiteKing = new King(true);
            board.CreatePieces(whiteKing, 7, 0, whitePlayer);
            gameRules.AddActivePiece(whiteKing);

            // White Queen on G6 (2,6), controlling squares around black king
            Piece whiteQueen = new Queen(true);
            board.CreatePieces(whiteQueen, 2, 6, whitePlayer);
            gameRules.AddActivePiece(whiteQueen);

            Debug.WriteLine($"King Location: {whiteKing.getCurrentPosition()} Queen Location: {whiteQueen.getCurrentPosition()}");
        
            bool hasLegalMoves = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            // Not in check, but no legal moves => stalemate
            Assert.IsFalse(hasLegalMoves, "Black King has no legal moves, but is not in check (stalemate).");
        }

        [TestMethod]
        public void TestDraw_KingBishopVKing_ReturnsTrue()
        {
            // Clear and setup pieces
            board.clearBoard();

            // Create pieces
            var whiteKing = new King(true);
            var blackKing = new King(false);
            var whiteBishop = new Bishop(true);

            // Place pieces on board
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteBishop, 4, 4, whitePlayer);

            // Initialize active pieces in gameRules for the test
            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteBishop);

            bool result = gameRules.Draw();

            Assert.IsTrue(result, "King + Bishop vs King should be detected as draw");
        }

        [TestMethod]
        public void TestDraw_KingBishopVKingBishop_DifferentColorBishops_ReturnsFalse()
        {
            board.clearBoard();

            var whiteKing = new King(true);
            var blackKing = new King(false);
            var whiteBishop = new Bishop(true);
            var blackBishop = new Bishop(false);

            // Place bishops on different color squares
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteBishop, 2, 0, whitePlayer); // black square
            board.CreatePieces(blackBishop, 3, 2, blackPlayer); // white square

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteBishop);
            gameRules.AddActivePiece(blackBishop);

            bool result = gameRules.Draw();

            Assert.IsFalse(result, "King + Bishop vs King + Bishop on different color should NOT be draw");
        }

        [TestMethod]
        public void TestDraw_KingKnightVKing_ReturnsTrue()
        {
            board.clearBoard();

            var whiteKing = new King(true);
            var blackKing = new King(false);
            var whiteKnight = new Knight(true);

            board.CreatePieces(whiteKing, 0, 0, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteKnight, 4, 4, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteKnight);

            bool result = gameRules.Draw();

            Assert.IsTrue(result, "King + Knight vs King should be draw");
        }

        [TestMethod]
        public void TestDraw_TwoKingsOnly_ReturnsTrue()
        {
            board.clearBoard();

            var whiteKing = new King(true);
            var blackKing = new King(false);

            board.CreatePieces(whiteKing, 0, 0, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);

            bool result = gameRules.Draw();

            Assert.IsTrue(result, "Two kings only should be draw");
        }

        [TestMethod]
        public void TestDraw_OtherPieces_NoDraw()
        {
            board.clearBoard();

            var whiteKing = new King(true);
            var blackKing = new King(false);
            var whiteQueen = new Queen(true);

            board.CreatePieces(whiteKing, 0, 0, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 4, 4, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteQueen);

            bool result = gameRules.Draw();

            Assert.IsFalse(result, "Presence of queen should NOT result in draw");
        }

        [TestMethod]
        [STAThread]
        public void TestEnPassantRemovesCheck()
        {
            board.clearBoard();

            // Black King at top row, e8 (0,4)
            King blackKing = new King(false);
            board.CreatePieces(blackKing, 0, 4, blackPlayer);

            // White Rook delivering check at e5 (3,4)
            Rook whiteRook = new Rook(true);
            board.CreatePieces(whiteRook, 3, 4, whitePlayer);

            // Black Pawn at d7 (1,3)
            Pawn blackPawn = new Pawn(false);
            board.CreatePieces(blackPawn, 1, 3, blackPlayer);

            // White Pawn at e7 (1,4) — set enPassant = true
            Pawn whitePawn = new Pawn(true) { isEnPassant = true };
            board.CreatePieces(whitePawn, 1, 4, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(blackPawn);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(whitePawn);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black pawn should be able to perform en passant capture and escape check.");
        }

        [TestMethod]
        [STAThread]
        public void TestPawnBlocksCheck()
        {
            board.clearBoard();

            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece blackPawn = new Pawn(false);
            Piece whiteKing = new King(true);

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 5, 7, whitePlayer);
            board.CreatePieces(blackPawn, 6, 6, blackPlayer); // Can block at (6,7)
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackPawn);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black pawn should be able to block the check.");
        }

        [TestMethod]
        [STAThread]
        public void TestKnightCapturesToPreventCheckmate()
        {
            board.clearBoard();

            Piece blackKing = new King(false);
            Piece whiteQueen = new Queen(true);
            Piece blackKnight = new Knight(false);
            Piece whiteKing = new King(true);

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 6, 5, whitePlayer); // Delivering check
            board.CreatePieces(blackKnight, 5, 6, blackPlayer); // Can jump to (6,5) and capture
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteQueen);
            gameRules.AddActivePiece(blackKnight);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black knight should be able to capture the queen and stop checkmate.");
        }

        [TestMethod]
        [STAThread]
        public void TestBishopBlocksDiagonalCheck()
        {
            board.clearBoard();

            Piece blackKing = new King(false);
            Piece whiteBishop = new Bishop(true);
            Piece blackBishop = new Bishop(false);
            Piece whiteKing = new King(true);

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteBishop, 5, 5, whitePlayer); // Delivering diagonal check
            board.CreatePieces(blackBishop, 6, 6, blackPlayer); // Can block at (6,6)
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteBishop);
            gameRules.AddActivePiece(blackBishop);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black bishop should be able to block the diagonal check.");
        }

        [TestMethod]
        [STAThread]
        public void TestRookBlocksHorizontalCheck()
        {
            board.clearBoard();

            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece blackRook = new Rook(false);
            Piece whiteKing = new King(true);

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer); // Delivering check
            board.CreatePieces(blackRook, 7, 6, blackPlayer); // Can block the check
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackRook);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black rook should be able to block the horizontal check.");
        }

        [TestMethod]
        [STAThread]
        public void TestQueenCapturesToPreventCheckmate()
        {
            board.clearBoard();

            Piece blackKing = new King(false);
            Piece whiteRook = new Rook(true);
            Piece blackQueen = new Queen(false);
            Piece whiteKing = new King(true);

            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer); // Check on same rank
            board.CreatePieces(blackQueen, 6, 6, blackPlayer); // Can capture rook at (7,5)
            board.CreatePieces(whiteKing, 0, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackQueen);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black queen should be able to capture the rook and escape checkmate.");
        }

        [TestMethod]
        [STAThread]
        public void TestPinnedRookBlocksKingAndNoLegalMoves()
        {
            board.clearBoard();

            // Black King on E8 (0,4)
            Piece blackKing = new King(false);
            board.CreatePieces(blackKing, 0, 4, blackPlayer);

            // Black Rook on E7 (1,4) — pinned piece
            Piece blackRook = new Rook(false);
            board.CreatePieces(blackRook, 1, 4, blackPlayer);

            // White Rook on E1 (7,4) — pinning rook along file
            Piece whiteRook = new Rook(true);
            board.CreatePieces(whiteRook, 7, 4, whitePlayer);

            // White Rook on D8 (0,3) — controlling king escape square
            Piece whiteRook2 = new Rook(true);
            board.CreatePieces(whiteRook2, 0, 3, whitePlayer);

            // White Rook on F8 (0,5) — controlling king escape square
            Piece whiteRook3 = new Rook(true);
            board.CreatePieces(whiteRook3, 0, 5, whitePlayer);

            // White King somewhere safe
            Piece whiteKing = new King(true);
            board.CreatePieces(whiteKing, 7, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(blackRook);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(whiteRook2);
            gameRules.AddActivePiece(whiteRook3);
            gameRules.AddActivePiece(whiteKing);

            bool result = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsFalse(result, "Black rook is pinned, king cannot move, no legal moves — checkmate.");
        }

        [TestMethod]
        public async Task TestCastlingFails_KingHasMoved()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = true };
            var whiteRookKingside = new Rook(true) { hasMoved = false };

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);  // E1
            board.CreatePieces(whiteRookKingside, 7, 7, whitePlayer);  // H1

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookKingside);

            var kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, true, board); // Kingside

            Assert.IsFalse(result, "Castling should fail if the king has already moved.");
        }

        [TestMethod]
        public async Task TestCastlingFails_RookHasMoved()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = false };
            var whiteRookKingside = new Rook(true) { hasMoved = true };

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(whiteRookKingside, 7, 7, whitePlayer);

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookKingside);

            ChessBoardSquare kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, true, board); // Kingside

            Assert.IsFalse(result, "Castling should fail if the rook has already moved.");
        }

        [TestMethod]
        public async Task TestCastlingFails_PiecesBetweenKingAndRook()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = false };
            var whiteRookKingside = new Rook(true) { hasMoved = false };
            var blockingPiece = new Knight(true);

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(whiteRookKingside, 7, 7, whitePlayer);
            board.CreatePieces(blockingPiece, 7, 5, whitePlayer); // Piece between king and rook

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookKingside);
            gameRules.AddActivePiece(blockingPiece);

            ChessBoardSquare kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, true, board); // Kingside

            Assert.IsFalse(result, "Castling should fail if pieces are between the king and rook.");
        }

        [TestMethod]
        public async Task TestCastlingFails_KingInCheckOrPassesThroughCheck()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = false };
            var whiteRookKingside = new Rook(true) { hasMoved = false };
            var blackRook = new Rook(false);

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(whiteRookKingside, 7, 7, whitePlayer);

            // Black rook attacking square king must cross (F1 - 7,5)
            board.CreatePieces(blackRook, 5, 5, blackPlayer);

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookKingside);
            gameRules.AddActivePiece(blackRook);

            ChessBoardSquare kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, true, board); // Kingside

            Assert.IsFalse(result, "Castling should fail if the king is in check or passes through attacked squares.");
        }

        [TestMethod]
        public async Task TestCastlingSucceeds_Kingside()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = false };
            var whiteRookKingside = new Rook(true) { hasMoved = false };

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(whiteRookKingside, 7, 7, whitePlayer);

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookKingside);

            ChessBoardSquare kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, true, board); // Kingside

            Assert.IsTrue(result, "Castling should succeed kingside when all conditions are met.");
        }

        [TestMethod]
        public async Task TestCastlingSucceeds_Queenside()
        {
            board.clearBoard();

            var whiteKing = new King(true) { hasMoved = false };
            var whiteRookQueenside = new Rook(true) { hasMoved = false };

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(whiteRookQueenside, 7, 0, whitePlayer);

            gameRules.InitializeActivePiecesForTest();
            gameRules.AddActivePiece(whiteKing);
            gameRules.AddActivePiece(whiteRookQueenside);

            ChessBoardSquare kingSquare = new ChessBoardSquare(7, 4) { piece = whiteKing };

            bool result = await gameRules.PerformCastling(whiteKing, kingSquare, false, board); // Queenside

            Assert.IsTrue(result, "Castling should succeed queenside when all conditions are met.");
        }

    }
}