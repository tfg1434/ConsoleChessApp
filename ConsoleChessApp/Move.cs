using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChessApp {
    struct Move {
        public readonly Vector2Int StartSquare;
        public readonly Vector2Int TargetSquare;

        public Move(Vector2Int start_square, Vector2Int target_square) {
            StartSquare = start_square;
            TargetSquare = target_square;
        }
    }

    struct CastleMove {
        public readonly Vector2Int RookStartSquare;
        public readonly Vector2Int RookTargetSquare;
        public readonly Vector2Int KingStartSquare;
        public readonly Vector2Int KingTargetSquare;

        public CastleMove(Vector2Int rook_start_square, Vector2Int rook_target_square, Vector2Int king_start_square, Vector2Int king_target_square) {
            RookStartSquare = rook_start_square;
            RookTargetSquare = rook_target_square;
            KingStartSquare = king_start_square;
            KingTargetSquare = king_target_square;
        }
    }
}
