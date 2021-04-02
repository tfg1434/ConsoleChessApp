using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleChessApp {
    class Program {
        static int NPositions(int depth, Board board) {
            if (depth == 0) {
                return 1;
            }

            List<Move> moves = MoveGenerator.GenerateMoves(board);
            int n = 0;

            foreach (Move move in moves) {
                Board test_board = Board.SimulateMove(move, board);
                n += NPositions(depth - 1, test_board);
                test_board = board;
            }

            return n;
        }

        static void Test(Board board, int depth) {
            List<string> my_list = new();
            int total_n = 0;

            foreach (Move move in MoveGenerator.GenerateMoves(board)) {
                Board test_board = Board.SimulateMove(move, board);
                int n = NPositions(depth - 1, test_board);
                total_n += n;
                my_list.Add(Utils.Alphabet[move.StartSquare.x].ToString() + (8 - move.StartSquare.y).ToString() + Utils.Alphabet[move.TargetSquare.x].ToString() + (8 - move.TargetSquare.y).ToString() + ": " + n.ToString());
            }

            foreach (string notation in my_list) {
                Console.WriteLine(notation);
            }

            Console.WriteLine(total_n);
        }

        static void Main(string[] args) {
            //var board = new Board();
            //Test(board, 3);

            var board = new Board();
            board.Draw();

            while (true) {
                if (board.ColourToMove == Board.PlayerColour) {
                    Console.SetCursorPosition(Board.EnterMovePos.x, Board.EnterMovePos.y);
                    Utils.ClearCurrentConsoleLine();
                    Console.WriteLine("Enter your move below in coordinate notation: (e.g. a2 a3)");
                    Utils.ClearCurrentConsoleLine();

                    string input = Console.ReadLine();

                    if (MoveGenerator.TryParseMove(out Move move, input, board)) {
                        List<Move> moves = MoveGenerator.GenerateMoves(board);

                        if (moves.Contains(move)) {
                            board.Move(move);
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
                } else if (board.ColourToMove == Board.AIPlayerColour) {
                    board.Move(AIPlayer.ChooseMove(board));
                }
            }
        }
    }
}


