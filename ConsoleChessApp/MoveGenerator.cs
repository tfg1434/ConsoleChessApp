using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Numerics;

namespace ShittyChessApp {
    static class MoveGenerator {
        //n, e, s, w, ne, se, nw, sw
        private static readonly int[,] sliding_offsets = { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, -1 }, { -1, 1 } };
        //north-left, north-right, east-up, east-down, south-right, south-left, west-down, west-up
        private static readonly int[,] knight_offsets = { { -1, -2 }, { 1, -2 }, { 2, -1 }, { 2, 1 }, { 1, 2 }, { -1, 2 }, { -2, 1 }, { -2, -1 } };
        private static List<Move> moves;

        public static List<Move> GenerateMoves(Board board) {
            moves = new List<Move>();

            for (int start_x = 0; start_x < Board.GridSize; start_x++) {
                for (int start_y = 0; start_y < Board.GridSize; start_y++) {
                    Piece piece = board.Cells[start_x, start_y];
                    var start_cell = new Vector2Int(start_x, start_y);

                    if (piece.MyPieceColour == board.ColourToMove) {
                        if (piece.IsSlidingPiece) {
                            _GenerateSlidingMoves(start_cell, board);
                        } else if (piece.MyPieceType == Piece.PieceType.Pawn) {
                            _GeneratePawnMoves(start_cell, board);
                        } else if (piece.MyPieceType == Piece.PieceType.Knight) {
                            _GenerateKnightMoves(start_cell, board);
                        } else if (piece.MyPieceType == Piece.PieceType.King) {
                            _GenerateKingMoves(start_cell, board);
                        }
                    }
                }
            }

            return moves;
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

        private static void _GenerateSlidingMoves(Vector2Int start_cell, Board board) {
            Piece piece = board.Cells[start_cell.x, start_cell.y];

            var enemy_colour = piece.MyPieceColour == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;

            //n, e, s, w, ne, se, nw, sw
            int start_dir_index = piece.MyPieceType == Piece.PieceType.Bishop ? 4 : 0;
            int end_dir_index = piece.MyPieceType == Piece.PieceType.Rook ? 4 : 8;

            for (int dir_index = start_dir_index; dir_index < end_dir_index; dir_index++) {
                for (int n = 0; n < _GetSquaresToEdge(start_cell, dir_index); n++) {
                    var target_cell = new Vector2Int(start_cell.x + sliding_offsets[dir_index, 0] * (n + 1),
                        start_cell.y + sliding_offsets[dir_index, 1] * (n + 1));
                    Piece target_piece = board.Cells[target_cell.x, target_cell.y];

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

        private static void _GeneratePawnMoves(Vector2Int start_cell, Board board) {
            Piece piece = board.Cells[start_cell.x, start_cell.y];

            int forward = piece.MyPieceColour == Piece.PieceColour.White ? -1 : 1;

            //one forward
            var target_cell = new Vector2Int(start_cell.x, start_cell.y + forward);
            if (Board.InRange(target_cell) && board.Cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None) {
                moves.Add(new Move(start_cell, target_cell));
            }
            //two forward
            if (piece.CanDoubleMove) {
                target_cell = new Vector2Int(start_cell.x, start_cell.y + forward * 2);
                if (Board.InRange(target_cell) && board.Cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None) {
                    moves.Add(new Move(start_cell, target_cell));
                }
            }

            //capture diagonally
            int side = 1;
            for (int i = 0; i < 2; i++) {
                side = -side;

                target_cell = new Vector2Int(start_cell.x + side, start_cell.y + forward);
                if (Board.InRange(target_cell) && board.Cells[target_cell.x, target_cell.y].MyPieceType != Piece.PieceType.None && board.Cells[target_cell.x, target_cell.y].MyPieceColour != piece.MyPieceColour){
                    moves.Add(new Move(start_cell, target_cell));
                }
            }
        }

        private static void _GenerateKnightMoves(Vector2Int start_cell, Board board) {
            Piece piece = board.Cells[start_cell.x, start_cell.y];

            for (int i = 0; i < knight_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + knight_offsets[i, 0], start_cell.y + knight_offsets[i, 1]);
                if (Board.InRange(target_cell)) {
                    Piece target_piece = board.Cells[target_cell.x, target_cell.y];

                    if (target_piece.MyPieceColour != piece.MyPieceColour) {
                        moves.Add(new Move(start_cell, target_cell));
                    }
                }
            }
        }

        private static void _GenerateKingMoves(Vector2Int start_cell, Board board) {
            Piece piece = board.Cells[start_cell.x, start_cell.y];

            for (int i = 0; i < sliding_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + sliding_offsets[i, 0], start_cell.y + sliding_offsets[i, 1]);

                if (Board.InRange(target_cell)) {
                    Piece target_piece = board.Cells[target_cell.x, target_cell.y];

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


