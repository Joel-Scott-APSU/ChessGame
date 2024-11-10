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
            board.createPieces(whiteBishop, 5, 5, whitePlayer);
            board.createPieces(blackKing, 7, 4, blackPlayer);
            board.createPieces(whiteQueen, 6, 4, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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

            board.createPieces(whiteKing, 0, 2, whitePlayer);
            board.createPieces(blackKing, 0, 0, blackPlayer);
            board.createPieces(whiteQueen, 0, 1, whitePlayer);

            board.updateThreatMap(whitePlayer.getPieces()); // Update the threat map

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces()); // Check for legal moves for black king

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
            board.createPieces(whiteKing, 6, 6, whitePlayer);
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteQueen1, 5, 6, whitePlayer);
            board.createPieces(whiteQueen2, 6, 5, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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
            board.createPieces(whiteKing, 5, 5, whitePlayer);
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteQueen, 6, 6, whitePlayer);
            board.createPieces(whiteRook, 7, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, whitePlayer.getPieces());

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
            board.createPieces(whiteKing, 5, 5, whitePlayer);
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteRook, 6, 6, whitePlayer);
            board.createPieces(whiteQueen, 7, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteRook, 7, 5, whitePlayer);
            board.createPieces(blackBishop, 6, 6, blackPlayer);

            // The black bishop should be able to capture the white rook on F8
            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteRook, 7, 6, whitePlayer);
            board.createPieces(whiteKing, 5, 6, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteQueen, 7, 5, whitePlayer);
            board.createPieces(whiteRook, 5, 5, whitePlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

            Assert.IsTrue(result, "Black should be able to move to H7 and escape checkmate.");
        }

        [TestMethod]
        public void TestPawnBlockCheckMate()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece blackRook = new Piece.Rook(false);

            board.clearBoard();
            board.createPieces(blackKing, 7, 7, blackPlayer);
            board.createPieces(whiteRook, 7, 5, whitePlayer);
            board.createPieces(blackRook, 5, 6, blackPlayer);

            (bool result, Spot spot) = moves.checkForLegalMoves(blackPlayer, board, blackPlayer.getPieces());

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
            board.createPieces(new King(true), 0, 4, whitePlayer);
            board.createPieces(new King(false), 7, 4, blackPlayer);

            // Place white pawn at its starting position
            board.createPieces(whitePawn, 6, 4, whitePlayer);
            // Place black pawn at its starting position
            board.createPieces(blackPawn, 1, 5, blackPlayer);

            // Move white pawn two squares forward (this should set it up for en passant)
            Spot startSpot = board.getSpot(6, 4);
            Spot endSpot = board.getSpot(4, 4); // Moving to row 4 from row 6
            whitePawn.legalMove(board, startSpot, endSpot);

            // Check that the white pawn is set for en passant
            Assert.IsTrue(whitePawn.isEnPassant, "White pawn should be marked for en passant after moving two squares forward.");

            // Move black pawn next to white pawn
            Spot blackStartSpot = board.getSpot(1, 5);
            Spot blackEndSpot = board.getSpot(2, 5); // Moving to row 2 from row 1
            blackPawn.legalMove(board, blackStartSpot, blackEndSpot);

            // Now try en passant move
            Spot enPassantSpot = board.getSpot(4, 5); // The spot where the white pawn will be captured en passant
            Spot captureSpot = board.getSpot(5, 4); // The destination spot for the black pawn capturing en passant
            bool moveResult = blackPawn.legalMove(board, blackEndSpot, captureSpot);

            // Check that the black pawn can capture the white pawn en passant
            Assert.IsTrue(moveResult, "Black pawn should be able to capture white pawn en passant.");
            Assert.IsNull(board.getSpot(4, 4).GetPiece(), "The white pawn should be removed from the board after en passant.");
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
            board.createPieces(whiteKing, 7, 4, whitePlayer); // King at E1 (7,4)
            board.createPieces(whiteRook, 7, 7, whitePlayer); // Rook at H1 (7,7)

            // Ensure the spaces between the King and Rook are clear and no threat is posed
            board.getSpot(7, 5).SetPiece(null); // F1
            board.getSpot(7, 6).SetPiece(null); // G1

            // Attempt to castle kingside
            ChessBoardSquare kingFrom = new ChessBoardSquare(7, 4);
            ChessBoardSquare rookTo = new ChessBoardSquare(7, 7); // This is where we check castling

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = game.movePiece(kingFrom, rookTo);

            // Assert that kingside castling was successful
            Assert.IsTrue(castledKingSide, "White should be able to castle kingside.");
            Assert.IsNull(board.getSpot(7, 4).GetPiece(), "The King's original spot (E1) should be empty.");
            Assert.IsNotNull(board.getSpot(7, 6).GetPiece(), "The King should be moved to G1.");
            Assert.IsNotNull(board.getSpot(7, 5).GetPiece(), "The Rook should be moved to F1.");
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
            board.createPieces(whiteKing, 7, 4, whitePlayer); // King at E1 (7,4)
            board.createPieces(whiteRook, 7, 0, whitePlayer); // Rook at A1 (7,0)

            // Ensure the spaces between the King and Rook are clear and no threat is posed
            board.getSpot(7, 1).SetPiece(null); // B1
            board.getSpot(7, 2).SetPiece(null); // C1
            board.getSpot(7, 3).SetPiece(null); // D1

            // Attempt to castle queenside
            ChessBoardSquare kingFrom = new ChessBoardSquare(7, 4);
            ChessBoardSquare rookTo = new ChessBoardSquare(7, 0); // This is where we check castling

            var (moveSuccessful, enPassant, castledKingSide, castledQueenSide) = game.movePiece(kingFrom, rookTo);

            // Assert that queenside castling was successful
            Assert.IsTrue(castledQueenSide, "White should be able to castle queenside.");
            Assert.IsNull(board.getSpot(7, 4).GetPiece(), "The King's original spot (E1) should be empty.");
            Assert.IsNotNull(board.getSpot(7, 2).GetPiece(), "The King should be moved to C1.");
            Assert.IsNotNull(board.getSpot(7, 3).GetPiece(), "The Rook should be moved to D1.");
        }

    }
}