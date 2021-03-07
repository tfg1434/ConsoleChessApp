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
                Console.WriteLine("Enter your move below in coordinate notation: (e.g. a2 a3)");
                Utils.ClearCurrentConsoleLine();

                if (MoveGenerator.TryParse(out Move move, Console.ReadLine())) {
                    List<Move> moves = MoveGenerator.GenerateMoves(board.Cells, board.ColourToMove);
                    moves = MoveGenerator.PruneIllegalMoves(moves, board);
                    bool found_move = false;

                    foreach (Move curr_move in moves) {
                        if (curr_move.StartSquare == move.StartSquare && curr_move.TargetSquare == move.TargetSquare) {
                            found_move = true;
                            board.Move(curr_move);
                            break;
                        }
                    }

                    if (!found_move) {
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


