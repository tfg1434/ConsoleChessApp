using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleChessApp {
    class Program {
        static void Main(string[] args) {
            var board = new Board();
            board.Draw();

            while (true) {
                Console.SetCursorPosition(Board.EnterMovePos.x, Board.EnterMovePos.y);
                Utils.ClearCurrentConsoleLine();
                Console.WriteLine("Enter your move below in coordinate notation: (e.g. a2 a3)");
                Utils.ClearCurrentConsoleLine();

                if (MoveGenerator.TryParse(out Move move, Console.ReadLine())) {
                    List<Move> moves = MoveGenerator.GenerateMoves(board);
                    moves = MoveGenerator.PruneIllegalMoves(moves, board);

                    if (moves.Contains(move)) {
                        board.Move(move);
                    } else {
                        Console.Write("illegal move!!!!");
                        Thread.Sleep(500);
                        Utils.ClearCurrentConsoleLine();

                    }
                } else {
                    Console.Write("this is not a real move!!!");
                    Thread.Sleep(500);
                    Utils.ClearCurrentConsoleLine();
                }
            }
        }
    }
}


