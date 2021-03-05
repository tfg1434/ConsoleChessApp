using System;
using System.Collections.Generic;
using System.Text;

namespace ShittyChessApp {
    class Piece {
        public PieceType MyPieceType { get; set; } = PieceType.None;
        public PieceColour MyPieceColour { get; set; } = PieceColour.None;
        public bool CanDoubleMove { get; set; } = true;

        public bool IsSlidingPiece => MyPieceType == PieceType.Bishop || MyPieceType == PieceType.Rook || MyPieceType == PieceType.Queen;

        public enum PieceType : int {
            None = 0,
            King = 1,
            Pawn = 2,
            Knight = 3,
            Bishop = 4,
            Rook = 5,
            Queen = 6,
        }

        public enum PieceColour : int {
            None = 0,
            White = 1,
            Black = 2,
        }

        public Piece(PieceType type, PieceColour colour, bool can_double_move = true) {
            MyPieceType = type;
            MyPieceColour = colour;
            CanDoubleMove = can_double_move;
        }
    }
}
