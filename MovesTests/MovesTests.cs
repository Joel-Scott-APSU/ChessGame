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
            moves = new Moves(whitePlayer, blackPlayer, gameRules, threatMap);
            activePieces = new HashSet<Piece>();
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

            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

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

            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

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

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer);
            board.CreatePieces(blackBishop, 6, 6, blackPlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook);
            gameRules.AddActivePiece(blackBishop);

            // The black bishop should be able to capture the white rook on F8
            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

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

            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

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

            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black should be able to block the checkmate with the Rook.");
        }

        [TestMethod]
        [STAThread]
        public void TestEnPassantSetup()
        {
            // Set up the board
            board.clearBoard();

            // Create pawns
            Pawn whitePawn = new Pawn(true);
            Pawn blackPawn = new Pawn(false);

            // Place kings back on the board after clearing
            board.CreatePieces(new King(true), 0, 4, whitePlayer);
            board.CreatePieces(new King(false), 7, 4, blackPlayer);

            // Place white pawn at its starting position
            board.CreatePieces(whitePawn, 6, 4, whitePlayer);
            // Place black pawn at its starting position
            board.CreatePieces(blackPawn, 1, 5, blackPlayer);

            // Move white pawn two squares forward (this should set it up for en passant)
            Spot startSpot = board.GetSpot(6, 4);
            Spot endSpot = board.GetSpot(4, 4); // Moving to row 4 from row 6
            whitePawn.legalMove(threatMap, startSpot, endSpot, board);

            // Check that the white pawn is set for en passant
            Assert.IsTrue(whitePawn.isEnPassant, "White pawn should be marked for en passant after moving two squares forward.");

            // Move black pawn next to white pawn
            Spot blackStartSpot = board.GetSpot(1, 5);
            Spot blackEndSpot = board.GetSpot(2, 5); // Moving to row 2 from row 1
            blackPawn.legalMove(threatMap, blackStartSpot, blackEndSpot, board);

            // Now try en passant move
            Spot enPassantSpot = board.GetSpot(4, 5); // The spot where the white pawn will be captured en passant
            Spot captureSpot = board.GetSpot(5, 4); // The destination spot for the black pawn capturing en passant
            bool moveResult = blackPawn.legalMove(threatMap, blackEndSpot, captureSpot, board);

            // Check that the black pawn can capture the white pawn en passant
            Assert.IsTrue(moveResult, "Black pawn should be able to capture white pawn en passant.");
            Assert.IsNull(board.GetSpot(4, 4).Piece, "The white pawn should be removed from the board after en passant.");
        }

        [TestMethod]
        [STAThread]
        public async Task TestKingSideCastle()
        {
            // Set up the board
            board.clearBoard();
            gameRules.InitializeActivePiecesForTest();
            // Create the necessary pieces for kingside castling (White King and White Rook)
            Piece whiteKing = new King(true);
            Piece whiteRook = new Rook(true);

            // Place the King and Rook on their initial positions
            board.CreatePieces(whiteKing, 7, 4, whitePlayer); // King at E1 (7,4)
            board.CreatePieces(whiteRook, 7, 7, whitePlayer); // Rook at H1 (7,7)

            // Ensure the spaces between the King and Rook are clear and no threat is posed
            board.GetSpot(7, 5).Piece = null; // F1
            board.GetSpot(7, 6).Piece = null; // G1

            // Attempt to castle kingside
            ChessBoardSquare kingFrom = new ChessBoardSquare(7, 4);
            ChessBoardSquare rookTo = new ChessBoardSquare(7, 7); // This is where we check castling

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = await game.movePiece(kingFrom, rookTo);

            // Assert that kingside castling was successful
            Assert.IsTrue(castledKingSide, "White should be able to castle kingside.");

        }

        [TestMethod]
        [STAThread]
        public async Task TestQueenSideCastle()
        {
            // Set up the board
            board.clearBoard();

            // Create the necessary pieces for queenside castling (White King and White Rook)
            Piece whiteKing = new King(true);
            Piece whiteRook = new Rook(true);

            // Place the King and Rook on their initial positions
            board.CreatePieces(whiteKing, 7, 4, whitePlayer); // King at E1 (7,4)
            board.CreatePieces(whiteRook, 7, 0, whitePlayer); // Rook at A1 (7,0)

            // Ensure the spaces between the King and Rook are clear and no threat is posed
            board.GetSpot(7, 1).Piece = null; // B1
            board.GetSpot(7, 2).Piece = null; // C1
            board.GetSpot(7, 3).Piece = null; // D1

            // Attempt to castle queenside
            ChessBoardSquare kingFrom = new ChessBoardSquare(7, 4);
            ChessBoardSquare rookTo = new ChessBoardSquare(7, 0); // This is where we check castling

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = await game.movePiece(kingFrom, rookTo);

            // Assert that queenside castling was successful
            Assert.IsTrue(castledQueenSide, "White should be able to castle queenside.");
            Assert.IsNull(board.GetSpot(7, 4).Piece, "The King's original spot (E1) should be empty.");
            Assert.IsNotNull(board.GetSpot(7, 2).Piece, "The King should be moved to C1.");
            Assert.IsNotNull(board.GetSpot(7, 3).Piece, "The Rook should be moved to D1.");
        }

        [TestMethod]
        [STAThread]
        public void Test_KingHasLegalMove_NotCheckmate()
        {


            board.clearBoard();

            // Create white king on E1 (7,4)
            Piece whiteKing = new King(true);
            Piece blackRook = new Rook(false);

            board.CreatePieces(whiteKing, 7, 4, whitePlayer);
            board.CreatePieces(blackRook, 6, 4, blackPlayer);

            gameRules.InitializeActivePiecesForTest();
            //Create white king on E1 (7,4)
            gameRules.AddActivePiece(whiteKing);

            // Create black rook on E2 (6,4), threatening the king
            gameRules.AddActivePiece(blackRook);

            // Call checkForLegalMoves to determine if the king has an escape
            bool hasLegalMoves = !moves.checkForLegalMoves(whitePlayer, board, gameRules.GetActivePieces(true));

            foreach (var piece in gameRules.GetActivePieces(true))
            {
                Debug.WriteLine($"White Active Piece: {piece}");
            }

            foreach (var piece in gameRules.GetActivePieces(false))
            {
                Debug.WriteLine($"Black Active Piece: {piece}");
            }
            // Assert that the king is not in checkmate
            Assert.IsTrue(hasLegalMoves, "King should be able to move out of check.");
        }

        [TestMethod]
        public void Test_KingHasNoLegalMove_IsCheckmate()
        {
            board.clearBoard();

            // Black King on H8 (0, 7)
            Piece blackKing = new King(false);
            board.CreatePieces(blackKing, 0, 7, blackPlayer);

            // White Rook on H6 (2, 7) - covers h-file
            Piece whiteQueen = new Queen(true);
            board.CreatePieces(whiteQueen, 2, 7, whitePlayer);

            // White Rook on G7 (1, 6) - covers 7th rank
            Piece whiteRook1 = new Rook(true);
            board.CreatePieces(whiteRook1, 1, 6, whitePlayer);

            gameRules.InitializeActivePiecesForTest();

            gameRules.AddActivePiece(blackKing);
            gameRules.AddActivePiece(whiteRook1);
            gameRules.AddActivePiece(whiteQueen);

            bool hasLegalMoves = moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Debug.WriteLine(hasLegalMoves);
            Assert.IsFalse(hasLegalMoves, "Black King is in checkmate and should have no legal moves.");
        }

        [TestMethod]
        public void Test_KingHasNoLegalMove_Stalemate()
        {
            board.clearBoard();

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

            bool result = !moves.checkForLegalMoves(blackPlayer, board, gameRules.GetActivePieces(false));

            Assert.IsTrue(result, "Black pawn should be able to perform en passant capture and escape check.");
        }



    }
}