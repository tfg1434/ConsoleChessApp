using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Numerics;
using System.Linq;

namespace ConsoleChessApp {
    static class MoveGenerator {
        //n, e, s, w, ne, se, nw, sw
        private static readonly int[,] sliding_offsets = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, -1 }, { -1, 1 } };
        //north-left, north-right, east-up, east-down, south-right, south-left, west-down, west-up
        private static readonly int[,] knight_offsets = { { -1, -2 }, { 1, -2 }, { 2, -1 }, { 2, 1 }, { 1, 2 }, { -1, 2 }, { -2, 1 }, { -2, -1 } };
        private static List<Move> moves;

        public static List<Move> GenerateMoves(Piece[,] cells, Piece.PieceColour colour_to_move) {
            moves = new List<Move>();

            for (int start_x = 0; start_x < Board.GridSize; start_x++) {
                for (int start_y = 0; start_y < Board.GridSize; start_y++) {
                    Piece piece = cells[start_x, start_y];
                    var start_cell = new Vector2Int(start_x, start_y);

                    if (piece.MyPieceColour == colour_to_move) {
                        if (piece.IsSlidingPiece) {
                            _GenerateSlidingMoves(start_cell, cells);
                        } else if (piece.MyPieceType == Piece.PieceType.Pawn) {
                            _GeneratePawnMoves(start_cell, cells);
                        } else if (piece.MyPieceType == Piece.PieceType.Knight) {
                            _GenerateKnightMoves(start_cell, cells);
                        } else if (piece.MyPieceType == Piece.PieceType.King) {
                            _GenerateKingMoves(start_cell, cells);
                        }
                    }
                }
            }

            return moves;
        }

        public static List<Move> GenerateMoves(Board board) {
            return GenerateMoves(board.Cells, board.ColourToMove);
        }

        public static List<Move> PruneIllegalMoves(List<Move> moves, Board board) {
            Piece.PieceColour enemy_colour = board.ColourToMove == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;
            List<Move> pruned_moves = new();
            Vector2Int my_king_square = default;

            foreach (Move move in moves) {
                Piece[,] test_cells = board.SimulateMove(move);
                List<Move> opponent_responses = GenerateMoves(test_cells, enemy_colour);

                for (var y = 0; y < Board.GridSize; y++) {
                    for (var x = 0; x < Board.GridSize; x++) {
                        if (test_cells[x, y].MyPieceType == Piece.PieceType.King && test_cells[x, y].MyPieceColour == board.ColourToMove) {
                            my_king_square = new Vector2Int(x, y);
                        }
                    }
                }

                if (opponent_responses.Any(response => response.TargetSquare == my_king_square)) {
                    //opponent can capture king - so last move was illegal
                } else {
                    pruned_moves.Add(move);
                }
            }

            return pruned_moves;
        }

        private static int _GetSquaresToEdge(Vector2Int cell, int dir_offset_index) {
            int n_north = cell.y;
            int n_south = Board.GridSize - 1 - cell.y;
            int n_east = Board.GridSize - 1 - cell.x;
            int n_west = cell.x;

            int[] squares_to_edge = new int[] {
                n_north,
                n_east,
                n_south,
                n_west,
                Math.Min(n_north, n_east), //ne
                Math.Min(n_south, n_east), //se
                Math.Min(n_north, n_west), //nw
                Math.Min(n_south, n_west), //sw
            };

            return squares_to_edge[dir_offset_index];
        }

        private static void _GenerateSlidingMoves(Vector2Int start_cell, Piece[,] cells) {
            Piece piece = cells[start_cell.x, start_cell.y];

            var enemy_colour = piece.MyPieceColour == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;

            //n, e, s, w, ne, se, nw, sw
            int start_dir_index = piece.MyPieceType == Piece.PieceType.Bishop ? 4 : 0;
            int end_dir_index = piece.MyPieceType == Piece.PieceType.Rook ? 4 : 8;

            for (int dir_index = start_dir_index; dir_index < end_dir_index; dir_index++) {
                for (int n = 0; n < _GetSquaresToEdge(start_cell, dir_index); n++) {
                    var target_cell = new Vector2Int(start_cell.x + sliding_offsets[dir_index, 0] * (n + 1),
                        start_cell.y + sliding_offsets[dir_index, 1] * (n + 1));
                    Piece target_piece = cells[target_cell.x, target_cell.y];

                    //blocked by friendly piece? break to next dir index
                    if (target_piece.MyPieceColour == piece.MyPieceColour) {
                        break;
                    }

                    moves.Add(new Move(start_cell, target_cell));

                    //if capturing piece, break to next dir index
                    if (target_piece.MyPieceColour == enemy_colour) {
                        break;
                    }
                }
            }
        }

        private static void _GeneratePawnMoves(Vector2Int start_cell, Piece[,] cells) {
            Piece piece = cells[start_cell.x, start_cell.y];

            int forward = piece.MyPieceColour == Piece.PieceColour.White ? -1 : 1;

            //one forward
            var target_cell = new Vector2Int(start_cell.x, start_cell.y + forward);
            if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None) {
                moves.Add(new Move(start_cell, target_cell));
            }
            //two forward
            if (piece.CanDoubleMove) {
                target_cell = new Vector2Int(start_cell.x, start_cell.y + forward * 2);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None) {
                    moves.Add(new Move(start_cell, target_cell));
                }
            }

            //capture diagonally
            int side = 1;
            for (int i = 0; i < 2; i++) {
                side = -side;

                target_cell = new Vector2Int(start_cell.x + side, start_cell.y + forward);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType != Piece.PieceType.None && cells[target_cell.x, target_cell.y].MyPieceColour != piece.MyPieceColour){
                    moves.Add(new Move(start_cell, target_cell));
                }

                //en passant capture
                //.
                target_cell = new Vector2Int(start_cell.x + side, start_cell.y);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].JustDoubleMoved && cells[target_cell.x, target_cell.y].MyPieceColour != piece.MyPieceColour) {
                    moves.Add(new Move(start_cell, new Vector2Int(target_cell.x, target_cell.y + forward)));
                }
            }
        }

        private static void _GenerateKnightMoves(Vector2Int start_cell, Piece[,] cells) {
            Piece piece = cells[start_cell.x, start_cell.y];

            for (int i = 0; i < knight_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + knight_offsets[i, 0], start_cell.y + knight_offsets[i, 1]);
                if (Board.InRange(target_cell)) {
                    Piece target_piece = cells[target_cell.x, target_cell.y];

                    if (target_piece.MyPieceColour != piece.MyPieceColour) {
                        moves.Add(new Move(start_cell, target_cell));
                    }
                }
            }
        }

        private static void _GenerateKingMoves(Vector2Int start_cell, Piece[,] cells) {
            Piece piece = cells[start_cell.x, start_cell.y];

            for (int i = 0; i < sliding_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + sliding_offsets[i, 0], start_cell.y + sliding_offsets[i, 1]);

                if (Board.InRange(target_cell)) {
                    Piece target_piece = cells[target_cell.x, target_cell.y];

                    if (target_piece.MyPieceColour != piece.MyPieceColour) {
                        moves.Add(new Move(start_cell, target_cell));
                    }
                }
            }
        }

        public static bool TryParse(out Move move, string notation) { //see discord chat for out
            Match match =  Regex.Match(notation, "^([a-h])([1-8]) ([a-h])([1-8])$", RegexOptions.IgnoreCase);


            if (match.Success) {
                move = new Move(
                    new Vector2Int(Array.IndexOf(Utils.Alphabet, char.Parse(match.Groups[1].Value)), Board.GridSize - int.Parse(match.Groups[2].ToString())),
                    new Vector2Int(Array.IndexOf(Utils.Alphabet, char.Parse(match.Groups[3].Value)), Board.GridSize - int.Parse(match.Groups[4].ToString()))
                );

                return true;
            }

            move = default;
            return false;
        }
    }
}


