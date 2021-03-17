using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleChessApp {
    static class AIPlayer {
        public static Move ChooseMove(Board board) {
            var random = new Random();
            List<Move> moves = MoveGenerator.GenerateMoves(board);
            return moves[random.Next(moves.Count)];
        }
    }
}
