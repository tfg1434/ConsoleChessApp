using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleChessApp {
    static class AIPlayer {
        private static int search_depth = 3;

        private static Dictionary<Piece.PieceType, int> piece_values = new() {
            [Piece.PieceType.None] = 0,
            [Piece.PieceType.Pawn] = 100,
            [Piece.PieceType.Knight] = 300,
            [Piece.PieceType.Bishop] = 350,
            [Piece.PieceType.Rook] = 500,
            [Piece.PieceType.Queen] = 900,
        };

        private static int _Search(out Move best_move, int depth, Board board) {
            best_move = default;

            if (depth == 0) {
                return _Eval(board);
            }

            List<Move> moves = MoveGenerator.GenerateMoves(board);
            if (!moves.Any()) {
                Piece.PieceColour opponent_colour = board.ColourToMove == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.Black;
                List<Move> opponent_moves = MoveGenerator.GeneratePseudoLegalMoves(board, opponent_colour);
                #region get king square
                Vector2Int my_king_square = default;
                for (int y = 0; y < Board.GridSize; y++) {
                    for (int x = 0; x < Board.GridSize; x++) {
                        if (board.Cells[x, y].MyPieceColour == board.ColourToMove)
                            my_king_square = new Vector2Int(x, y);
                    }
                }
                #endregion
                //in check? checkmate
                if (opponent_moves.Any(move => move.TargetSquare == my_king_square)) {
                    return -9999;
                }

                //stalemate
                return 0;
            }

            int best_eval = -9999;

            foreach (Move move in moves) {
                Board test_board = Board.SimulateMove(move, board);
                int eval = _Search(out _, depth - 1, test_board);
                if (eval > best_eval) {
                    best_move = move;
                    best_eval = eval;
                }
                test_board = board; //is this okay? pass by ref is the root of all evil
            }

            return best_eval;
        }

        private static int _Eval(Board board) {
            int white_material = _CountMaterial(Piece.PieceColour.White, board);
            int black_material = _CountMaterial(Piece.PieceColour.Black, board);

            int eval = white_material - black_material;
            int perspective = board.ColourToMove == Piece.PieceColour.White ? 1 : -1;

            return eval * perspective;
        }

        private static int _CountMaterial(Piece.PieceColour colour, Board board) {
            int material = 0;

            foreach (Piece piece in board.Cells) {
                if (piece.MyPieceColour == colour && piece.MyPieceType != Piece.PieceType.King) {
                    material += piece_values[piece.MyPieceType];
                }
            }

            return material;
        }

        public static Move ChooseMove(Board board) {
            //var random = new Random();
            //List<Move> moves = MoveGenerator.GenerateMoves(board);
            //return moves[random.Next(moves.Count)];

            _Search(out Move best_move, search_depth, board);
            return best_move;
        }
    }
}
