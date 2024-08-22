using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChessGame.Tests
{
    [TestClass]
    public class ChessGameTests
    {
        private Game game;
        private Player whitePlayer;
        private Player blackPlayer;
        private Board board;

        [TestInitialize]
        public void Setup()
        {
            game = new Game();
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
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
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 5, 5, whitePlayer);
            game.board.createPieces(blackKing, 7, 4, blackPlayer);
            game.board.createPieces(whiteQueen, 6, 4, whitePlayer);

            game.board.placePiece(whiteKing, "F6");
            game.board.placePiece(whiteQueen, "E7");
            game.board.placePiece(blackKing, "E8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 2 uses a rook and a king to put the king into checkmate 
        public void TestCheckMateScenario2()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteRook = new Piece.Rook(true);
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 7, 5, whitePlayer);
            game.board.createPieces(blackKing, 7, 7, blackPlayer);
            game.board.createPieces(whiteRook, 7, 6, whitePlayer);

            game.board.placePiece(whiteKing, "G8");
            game.board.placePiece(whiteRook, "F8");
            game.board.placePiece(blackKing, "H8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario uses a knight, a bishop, and a king, king should be in checkmate 
        public void TestCheckMateScenario3()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteBishop = new Piece.Bishop(true);
            Piece whiteKnight = new Piece.Knight(true);
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 6, 6, whitePlayer);
            game.board.createPieces(blackKing, 7, 7, blackPlayer);
            game.board.createPieces(whiteBishop, 5, 5, whitePlayer);
            game.board.createPieces(whiteKnight, 5, 7, whitePlayer);

            game.board.placePiece(whiteKing, "G7");
            game.board.placePiece(whiteBishop, "E8");
            game.board.placePiece(whiteKnight, "F8");
            game.board.placePiece(blackKing, "H8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 4 uses 2 queens and a king 
        public void TestCheckMateScenario4()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen1 = new Piece.Queen(true);
            Piece whiteQueen2 = new Piece.Queen(true);
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 6, 6, whitePlayer);
            game.board.createPieces(blackKing, 7, 7, blackPlayer);
            game.board.createPieces(whiteQueen1, 5, 6, whitePlayer);
            game.board.createPieces(whiteQueen2, 6, 5, whitePlayer);

            game.board.placePiece(whiteKing, "G7");
            game.board.placePiece(whiteQueen1, "E7");
            game.board.placePiece(whiteQueen2, "F6");
            game.board.placePiece(blackKing, "H8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 5 uses a queen, rook, and king 
        public void TestCheckMateScenario5()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteQueen = new Piece.Queen(true);
            Piece whiteRook = new Piece.Rook(true);
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 5, 5, whitePlayer);
            game.board.createPieces(blackKing, 7, 7, blackPlayer);
            game.board.createPieces(whiteQueen, 6, 6, whitePlayer);
            game.board.createPieces(whiteRook, 7, 6, whitePlayer);

            game.board.placePiece(whiteKing, "F6");
            game.board.placePiece(whiteQueen, "G7");
            game.board.placePiece(whiteRook, "F7");
            game.board.placePiece(blackKing, "H8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

        [TestMethod]
        //checkmate scenario 6 uses a bishop, pawn, and a king 
        public void TestCheckMateScenario6()
        {
            Piece blackKing = new Piece.King(false);
            Piece whiteBishop = new Piece.Bishop(true);
            Piece whitePawn = new Piece.Pawn(true);
            Piece whiteKing = new Piece.King(true);

            game.board.clearBoard();
            game.board.createPieces(whiteKing, 5, 5, whitePlayer);
            game.board.createPieces(blackKing, 7, 7, blackPlayer);
            game.board.createPieces(whiteBishop, 6, 6, whitePlayer);
            game.board.createPieces(whitePawn, 7, 6, whitePlayer);

            game.board.placePiece(whiteKing, "F6");
            game.board.placePiece(whiteBishop, "G7");
            game.board.placePiece(whitePawn, "G8");
            game.board.placePiece(blackKing, "H8");

            bool result = game.moves.checkForLegalMoves(false, game.board);

            Assert.IsTrue(result, "Black should be in checkmate");
        }

    }
}