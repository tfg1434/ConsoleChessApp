using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleChessApp {
    class Piece {
        public PieceType MyPieceType { get; set; }
        public PieceColour MyPieceColour { get; set; }
        public bool HasMovedBefore { get; set; }
        public bool CanDoubleMove { get; set; }
        public bool JustDoubleMoved { get; set; }

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

        public Piece(PieceType type, PieceColour colour, bool can_double_move=true, bool has_moved_before=false, bool just_double_moved=false) {
            MyPieceType = type;
            MyPieceColour = colour;
            CanDoubleMove = can_double_move;
            HasMovedBefore = has_moved_before;
            JustDoubleMoved = just_double_moved;
        }
    }
}
