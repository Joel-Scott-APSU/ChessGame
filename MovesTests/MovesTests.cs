using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Windows.Documents;
using static ChessGame.Piece;

namespace ChessGame.Tests
{
    [TestClass]
    public class ChessGameTests
    {
        private Game game;
        private Player whitePlayer;
        private Player blackPlayer;
        private Moves moves;
        private Board board;

        [TestInitialize]
        public void Setup()
        {
            game = new Game();
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
            board = new Board(whitePlayer, blackPlayer);
            moves = new Moves(whitePlayer, blackPlayer);
        }
        [TestMethod]
        /*Checking to see if any moves exists that will get the king out 
         * of this spot, using black king for piece that should be in checkmate,
         * checking using white king and queen
         */
        public void TestCheckMateScenario1()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen = new Piece.Queen(true);
            Piece whiteBishop = new Piece.Bishop(true);

            board.clearBoard();
            board.CreatePieces(whiteBishop, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 4, blackPlayer);
            board.CreatePieces(whiteQueen, 6, 4, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        // Checkmate scenario 2 uses a rook and a king to put the king into checkmate 
        public void TestCheckMateScenario2()
        {
            board.clearBoard(); // Clear the board first

            // Re-create and place the pieces after clearing the board
            Piece whiteKing = new Piece.King(true);
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen = new Piece.Queen(true);

            board.CreatePieces(whiteKing, 0, 2, whitePlayer);
            board.CreatePieces(blackKing, 0, 0, blackPlayer);
            board.CreatePieces(whiteQueen, 0, 1, whitePlayer);

            board.UpdateThreatMap(whitePlayer.GetPieces()); // Update the threat map

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces()); // Check for legal moves for black king

            Assert.IsFalse(result, "Black should be in checkmate");
        }


        [TestMethod]
        //checkmate scenario 4 uses 2 queens and a king 
        public void TestCheckMateScenario4()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen1 = new Piece.Queen(true);
            Piece whiteQueen2 = new Piece.Queen(true);
            Piece whiteKing = new Piece.King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 6, 6, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen1, 5, 6, whitePlayer);
            board.CreatePieces(whiteQueen2, 6, 5, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 5 uses a queen, rook, and king 
        public void TestCheckMateScenario5()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen = new Piece.Queen(true);
            Piece whiteRook = new Piece.Rook(true);
            Piece whiteKing = new Piece.King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 6, 6, whitePlayer);
            board.CreatePieces(whiteRook, 7, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, whitePlayer.GetPieces());

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 6 uses a bishop, pawn, and a king 
        public void TestCheckMateScenario6()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece whiteQueen = new Piece.Queen(true);
            Piece whiteKing = new Piece.King(true);

            board.clearBoard();
            board.CreatePieces(whiteKing, 5, 5, whitePlayer);
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 6, 6, whitePlayer);
            board.CreatePieces(whiteQueen, 7, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsFalse(result, "Black should be in checkmate");
        }

        [TestMethod]
        // Scenario where the black bishop captures the white rook to avoid checkmate
        public void TestBishopCapturesToEscapeCheckMate()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece blackBishop = new Piece.Bishop(false);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer);
            board.CreatePieces(blackBishop, 6, 6, blackPlayer);

            // The black bishop should be able to capture the white rook on F8
            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsTrue(result, "Black should be able to capture the rook with the bishop and escape checkmate.");
        }


        [TestMethod]
        // Scenario where the black king can capture the attacking piece and avoid checkmate
        public void TestCaptureToEscapeCheckMate()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece whiteKing = new Piece.King(true);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 6, whitePlayer);
            board.CreatePieces(whiteKing, 5, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsTrue(result, "Black should be able to capture the rook and escape checkmate.");
        }

        [TestMethod]
        // Scenario where the black king can move out of checkmate
        public void TestMoveKingToEscapeCheckMate()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen = new Piece.Queen(true);
            Piece whiteRook = new Piece.Rook(true);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteQueen, 7, 5, whitePlayer);
            board.CreatePieces(whiteRook, 5, 5, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsTrue(result, "Black should be able to move to H7 and escape checkmate.");
        }

        [TestMethod]
        public void TestPawnBlockCheckMate()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece blackRook = new Piece.Rook(false);

            board.clearBoard();
            board.CreatePieces(blackKing, 7, 7, blackPlayer);
            board.CreatePieces(whiteRook, 7, 5, whitePlayer);
            board.CreatePieces(blackRook, 5, 6, blackPlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.GetPieces());

            Assert.IsTrue(result, "Black should be able to block the checkmate with the Rook.");
        }

        [TestMethod]
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
            whitePawn.legalMove(board, startSpot, endSpot);

            // Check that the white pawn is set for en passant
            Assert.IsTrue(whitePawn.isEnPassant, "White pawn should be marked for en passant after moving two squares forward.");

            // Move black pawn next to white pawn
            Spot blackStartSpot = board.GetSpot(1, 5);
            Spot blackEndSpot = board.GetSpot(2, 5); // Moving to row 2 from row 1
            blackPawn.legalMove(board, blackStartSpot, blackEndSpot);

            // Now try en passant move
            Spot enPassantSpot = board.GetSpot(4, 5); // The spot where the white pawn will be captured en passant
            Spot captureSpot = board.GetSpot(5, 4); // The destination spot for the black pawn capturing en passant
            bool moveResult = blackPawn.legalMove(board, blackEndSpot, captureSpot);

            // Check that the black pawn can capture the white pawn en passant
            Assert.IsTrue(moveResult, "Black pawn should be able to capture white pawn en passant.");
            Assert.IsNull(board.GetSpot(4, 4).Piece, "The white pawn should be removed from the board after en passant.");
        }

        [TestMethod]
        public void TestKingSideCastle()
        {
            // Set up the board
            board.clearBoard();

            // Create the necessary pieces for kingside castling (White King and White Rook)
            Piece whiteKing = new Piece.King(true);
            Piece whiteRook = new Piece.Rook(true);

            // Place the King and Rook on their initial positions
            board.CreatePieces(whiteKing, 7, 4, whitePlayer); // King at E1 (7,4)
            board.CreatePieces(whiteRook, 7, 7, whitePlayer); // Rook at H1 (7,7)

            // Ensure the spaces between the King and Rook are clear and no threat is posed
            board.GetSpot(7, 5).Piece = null; // F1
            board.GetSpot(7, 6).Piece = null; // G1

            // Attempt to castle kingside
            ChessBoardSquare kingFrom = new ChessBoardSquare(7, 4);
            ChessBoardSquare rookTo = new ChessBoardSquare(7, 7); // This is where we check castling

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = game.movePiece(kingFrom, rookTo);

            // Assert that kingside castling was successful
            Assert.IsTrue(castledKingSide, "White should be able to castle kingside.");
            Assert.IsNull(board.GetSpot(7, 4).Piece, "The King's original spot (E1) should be empty.");
            Assert.IsNotNull(board.GetSpot(7, 6).Piece, "The King should be moved to G1.");
            Assert.IsNotNull(board.GetSpot(7, 5).Piece, "The Rook should be moved to F1.");
        }

        [TestMethod]
        public void TestQueenSideCastle()
        {
            // Set up the board
            board.clearBoard();

            // Create the necessary pieces for queenside castling (White King and White Rook)
            Piece whiteKing = new Piece.King(true);
            Piece whiteRook = new Piece.Rook(true);

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

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = game.movePiece(kingFrom, rookTo);

            // Assert that queenside castling was successful
            Assert.IsTrue(castledQueenSide, "White should be able to castle queenside.");
            Assert.IsNull(board.GetSpot(7, 4).Piece, "The King's original spot (E1) should be empty.");
            Assert.IsNotNull(board.GetSpot(7, 2).Piece, "The King should be moved to C1.");
            Assert.IsNotNull(board.GetSpot(7, 3).Piece, "The Rook should be moved to D1.");
        }

    }
}