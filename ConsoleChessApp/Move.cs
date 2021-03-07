using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChessApp {
    struct Move {
        public readonly Vector2Int StartSquare;
        public readonly Vector2Int TargetSquare;
        public readonly bool IsEnPassant;

        public Move(Vector2Int start_square, Vector2Int target_square, bool is_en_passant = false) {
            StartSquare = start_square;
            TargetSquare = target_square;
            IsEnPassant = is_en_passant;
        }
    }
}
