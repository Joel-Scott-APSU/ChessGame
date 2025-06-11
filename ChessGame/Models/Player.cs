using ChessGame.Core;
using ChessGame.Models;
using System.Diagnostics;
using static ChessGame.Models.Piece;

public class Player
{
    private bool isWhite;
    private List<Piece> pieces;
    private List<Piece> capturedPieces;
    private GameRules gameRules;

    public Player(bool isWhite, GameRules gameRules)
    {
        pieces = new List<Piece>();
        capturedPieces = new List<Piece>();
        this.isWhite = isWhite;
        this.gameRules = gameRules;
    }

    public void addPiece(Piece piece)
    {
        pieces.Add(piece);
    }

    public void removePiece(Piece piece)
    {
        bool removed = pieces.Remove(piece);
    }

    public void CapturePiece(Piece piece)
    {
        if (pieces.Remove(piece))
        {
            capturedPieces.Add(piece);  // Add to captured pieces list
            gameRules.CapturePiece(piece);
            
            Debug.WriteLine($"Piece captured: {piece}");
        }
        else
        {
            Debug.WriteLine($"Failed to capture piece: {piece}. Piece not found in list.");
        }
    }

    public void clearCapturedPieces()
    {
        capturedPieces.Clear();
    }


    public IReadOnlyList<Piece> GetPieces()
    {
        // Return a read-only view of the list to prevent external modification
        return pieces.AsReadOnly();
    }

    public void ProcessPawns(Action<Piece.Pawn> action)
    {
        foreach (var piece in pieces.OfType<Piece.Pawn>())
        {
            action(piece);
        }
    }

    public bool IsWhite => isWhite;

    public IReadOnlyList<Piece> GetCapturedPieces()
    {
        return capturedPieces.AsReadOnly();
    }

    public bool HasRemainingPieces()
    {
        return pieces.Any(p => !(p is King));
    }

    public override string ToString()
    {
        return isWhite ? "White Player" : "Black Player";
    }
}
