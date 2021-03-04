using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShittyChessApp {
    struct Move {
        public readonly Vector2Int StartSquare;
        public readonly Vector2Int TargetSquare;

        public Move(Vector2Int start_square, Vector2Int target_square) {
            StartSquare = start_square;
            TargetSquare = target_square;
        }
    }
}
