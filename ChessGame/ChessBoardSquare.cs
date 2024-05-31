using System.Windows.Media;

namespace ChessGame
{
    public class ChessBoardSquare
    {

        public int X {  get; set; }
        public int Y { get; set; }
        public Brush Background { get; set; }
        public ImageSource PieceImage { get; set; }
    }
}
