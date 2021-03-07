using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleChessApp {
    class Piece {
        public PieceType MyPieceType { get; set; } = PieceType.None;
        public PieceColour MyPieceColour { get; set; } = PieceColour.None;

        //backing field for CanDoubleMove (to avoid infinite recursion)
        private bool _can_double_move = true;

        public bool CanDoubleMove {
            get { 
                return _can_double_move;
            }
            set {
                if (_can_double_move != value) {
                    JustDoubleMoved = true;
                }

                _can_double_move = value;
            } 
        }
        public bool JustDoubleMoved { get; set; } = false;

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
            _can_double_move = can_double_move;
        }
    }
}
