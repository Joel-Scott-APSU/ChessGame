using System.ComponentModel;
using System.Windows.Media;

namespace ChessGame
{
    public class ChessBoardSquare : INotifyPropertyChanged
    {
        private bool isHighlighted;
        private ImageSource pieceImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public int row { get; set; }
        public int column { get; set; }
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

        public string Position => $"{row},{column}"; // Combine X and Y into a string

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"ChessBoardSquare [{row}, {column}]";
        }
    }
}
