using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleChessApp {
    class Program {
        static void Main(string[] args) {
            var board = new Board();
            board.Draw();

            //throw new Exception("need to somehow represent the castle move in the list of moves. probably add an if/else in the parse method and a CastleMove struct. TryCastleMove method can generate the castle moves!");

            while (true) {
                Console.SetCursorPosition(Board.EnterMovePos.x, Board.EnterMovePos.y);
                Utils.ClearCurrentConsoleLine();
                Console.WriteLine("Enter your move below in coordinate notation: (e.g. a2 a3)");
                Utils.ClearCurrentConsoleLine();

                string input = Console.ReadLine();

                if (MoveGenerator.TryParseMove(out Move move, input)) {
                    List<Move> moves = MoveGenerator.GenerateMoves(board, board.ColourToMove);
                    moves = MoveGenerator.PruneIllegalMoves(moves, board);

                    if (moves.Contains(move)) {
                        board.Move(move);
                    } else {
                        Console.Write("illegal move!!!!");
                        Thread.Sleep(500);
                        Utils.ClearCurrentConsoleLine();
                        continue;
                    }
                } else if (MoveGenerator.TryParseCastleMove(out CastleMove castle_move, input, board)) {
                    List<CastleMove> castle_moves = MoveGenerator.GenerateCastleMoves(input, board);

                    if (castle_moves.Contains(castle_move)) {
                        board.Move(new Move(castle_move.RookStartSquare, castle_move.RookTargetSquare), false);
                        board.Move(new Move(castle_move.KingStartSquare, castle_move.KingTargetSquare), true);
                    } else {
                        Console.Write("illegal move!!!!");
                        Thread.Sleep(500);
                        Utils.ClearCurrentConsoleLine();
                        continue;
                    }

                } else {
                    Console.Write("this is not a real move!!!");
                    Thread.Sleep(500);
                    Utils.ClearCurrentConsoleLine();
                    continue;
                }
            }
        }
    }
}


