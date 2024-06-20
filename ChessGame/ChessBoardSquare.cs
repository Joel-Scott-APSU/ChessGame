using System.ComponentModel;
using System.Windows.Media;

namespace ChessGame
{
    public class ChessBoardSquare : INotifyPropertyChanged
    {
        private bool isHighlighted;
        private ImageSource pieceImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public int X { get; set; }
        public int Y { get; set; }
        public VisualBrush Background { get; set; }

        public ImageSource PieceImage
        {
            get => pieceImage;
            set
            {
                pieceImage = value;
                OnPropertyChanged(nameof(PieceImage));
            }
        }

        public bool IsHighlighted
        {
            get => isHighlighted;
            set
            {
                if (isHighlighted != value)
                {
                    isHighlighted = value;
                    OnPropertyChanged(nameof(IsHighlighted));
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
