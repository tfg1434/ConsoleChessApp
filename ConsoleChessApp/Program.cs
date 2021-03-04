using System;
using System.Threading;

namespace ShittyChessApp {
    class Program {
        static void Main(string[] args) {
            var board = new Board();
            board.Draw();

            while (true) {
                Console.SetCursorPosition(Board.EnterMovePos.x, Board.EnterMovePos.y);
                Console.WriteLine("Enter your move below in coordinate notation: (e.g. a2 a3)");
                Utils.ClearCurrentConsoleLine();

                Move move;
                if (MoveGenerator.TryParse(out move, Console.ReadLine())) {
                    if (MoveGenerator.GenerateMoves(board).Contains(move)) {
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


