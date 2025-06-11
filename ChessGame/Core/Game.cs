using ChessGame.Core;
using ChessGame.Models;
using ChessGame.Views;
using System.Diagnostics;

public class Game
{
    private static Game _instance;
    public Board board { get; private set; }
    public Player whitePlayer { get; private set; }
    public Player blackPlayer { get; private set; }
    public Player currentTurn { get; private set; }
    public ChessBoardSquare selectedSquare { get; set; }
    public Moves moves { get; private set; }

    public ThreatMap threatMap { get; private set; }
    public GameRules gameRules;

    // Private constructor to prevent multiple instances
    private Game(MainWindowViewModel viewModel)
    {
        gameRules = GameRules.GetInstance(this, viewModel);
        gameRules = GameRules.Create(this, viewModel);
        initializeGame();
        gameRules.InitializeActivePieces();
    }

    // Public static method to get the single instance of Game
    public static Game GetInstance(MainWindowViewModel viewModel)
    {
        if (_instance == null)
        {
            _instance = new Game(viewModel);
        }
        return _instance;
    }

    private void initializeGame()
    {
        whitePlayer = new Player(true, gameRules);
        blackPlayer = new Player(false, gameRules);
        board = new Board(whitePlayer, blackPlayer, this);
        threatMap = new ThreatMap(whitePlayer, blackPlayer, this);
        moves = new Moves(whitePlayer, blackPlayer, gameRules, threatMap);
        whitePlayer.clearCapturedPieces();
        blackPlayer.clearCapturedPieces();
        currentTurn = whitePlayer;
    }

    public (bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide) movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
    {
        return gameRules.HandleMove(fromSquare, toSquare);
    }

    public void SetCurrentTurn(Player newTurn)
    {
        currentTurn = newTurn;
    }

    public void EndGame()
    {
        if (gameRules.Draw())
        {
            Debug.WriteLine("Game ended in a draw.");
        }

        else if (gameRules.Checkmate(board))
        {
            Debug.WriteLine($"{currentTurn.IsWhite} wins by checkmate.");
        }

    }

    public static void ResetInstance()
    {

        _instance = null; // Reset the instance to allow for a new game
    }
}

