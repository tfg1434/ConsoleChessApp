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

        public static Piece.PieceType[] CanPromoteTo { get; } = { Piece.PieceType.Bishop, Piece.PieceType.Knight, Piece.PieceType.Rook, Piece.PieceType.Queen };

        public static List<Move> GeneratePseudoLegalMoves(Board board, Piece.PieceColour colour_to_move) {
            var moves = new List<Move>();

            for (int start_x = 0; start_x < Board.GridSize; start_x++) {
                for (int start_y = 0; start_y < Board.GridSize; start_y++) {
                    Piece piece = board.Cells[start_x, start_y];
                    var start_cell = new Vector2Int(start_x, start_y);

                    var this_piece_moves = new List<Move>();
                    if (piece.MyPieceColour == colour_to_move) {
                        if (piece.IsSlidingPiece) {
                            this_piece_moves = _GenerateSlidingMoves(start_cell, board.Cells);
                        } else if (piece.MyPieceType == Piece.PieceType.Pawn) {
                            this_piece_moves = _GeneratePawnMoves(start_cell, board.Cells);
                        } else if (piece.MyPieceType == Piece.PieceType.Knight) {
                            this_piece_moves = _GenerateKnightMoves(start_cell, board.Cells);
                        } else if (piece.MyPieceType == Piece.PieceType.King) {
                            this_piece_moves = _GenerateKingMoves(start_cell, board.Cells);
                        }
                    }

                    foreach (Move move in this_piece_moves) {
                        moves.Add(move);
                    }
                }
            }

            return moves;
        }

        public static List<Move> GenerateMoves(Board board, Piece.PieceColour colour) {
            List<Move> moves = GeneratePseudoLegalMoves(board, colour);
            Piece.PieceColour opponent_colour = colour == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;
            List<Move> pruned_moves = new();

            #region normal moves
            foreach (Move move in moves) {
                Board test_board = Board.SimulateMove(move, board);
                Vector2Int king_square = default;
                for (var y = 0; y < Board.GridSize; y++) {
                    for (var x = 0; x < Board.GridSize; x++) {
                        if (test_board.Cells[x, y].MyPieceType == Piece.PieceType.King && test_board.Cells[x, y].MyPieceColour == board.ColourToMove) {
                            king_square = new Vector2Int(x, y);
                        }
                    }
                }

                //order is important - make move, simulate based on board after that move so pins work
                //we can use pseudolegal moves because even if the piece is pinned, it still counts as being able to capture the king
                List<Move> opponent_responses = GeneratePseudoLegalMoves(test_board, opponent_colour);

                if (opponent_responses.Any(response => response.TargetSquare == king_square)) {
                    //opponent can capture king - so last move was illegal
                } else {
                    pruned_moves.Add(move);
                }
            }
            #endregion

            #region castling
            Vector2Int king_start_square = default;
            for (var y = 0; y < Board.GridSize; y++) {
                for (var x = 0; x < Board.GridSize; x++) {
                    if (board.Cells[x, y].MyPieceType == Piece.PieceType.King && board.Cells[x, y].MyPieceColour == board.ColourToMove) {
                        king_start_square = new Vector2Int(x, y);
                    }
                }
            }

            for (int y = 0; y < Board.GridSize; y++) {
                for (int x = 0; x < Board.GridSize; x++) {
                    Piece piece = board.Cells[x, y];
                    Piece king = board.Cells[king_start_square.x, king_start_square.y];

                    List<Move> opponent_moves = GeneratePseudoLegalMoves(board, opponent_colour);

                    #region kingside
                    if (piece.MyPieceType == Piece.PieceType.Rook && x > king_start_square.x && piece.MyPieceColour == board.ColourToMove && 
                        king_start_square.y == y && Math.Abs(king_start_square.x - x) == 3 && !piece.HasMovedBefore && !king.HasMovedBefore) {

                        var rook_start_square = new Vector2Int(x, y);
                        bool passing_through_check = false;
                        bool piece_in_way = false;

                        //check if KING is passing through check or starting in check
                        for (var i = 0; i < 3; i++) {
                            if (opponent_moves.Any(response => response.TargetSquare == new Vector2Int(king_start_square.x + i, king_start_square.y))) {
                                passing_through_check = true;
                            }
                        }

                        //check if there are any pieces in the way
                        for (var i = 1; i < 3; i++) {
                            if (board.Cells[king_start_square.x + i, king_start_square.y].MyPieceType != Piece.PieceType.None) {
                                piece_in_way = true;
                            }
                        }

                        //are we allowed to castle?
                        if (!passing_through_check && !piece_in_way) {
                            var king_target_square = new Vector2Int(king_start_square.x + 2, king_start_square.y);
                            pruned_moves.Add(new Move(king_start_square, king_target_square, true));
                        }
                    }
                    #endregion
                    #region queenside
                    else if (piece.MyPieceType == Piece.PieceType.Rook && x < king_start_square.x && piece.MyPieceColour == board.ColourToMove && 
                        king_start_square.y == y && Math.Abs(king_start_square.x - x) == 4 && !piece.HasMovedBefore && !king.HasMovedBefore) {

                        var rook_start_square = new Vector2Int(x, y);
                        bool passing_through_check = false;
                        bool piece_in_way = false;

                        for (var i = 0; i > -3; i--) {
                            if (opponent_moves.Any(response => response.TargetSquare == new Vector2Int(king_start_square.x + i, king_start_square.y))) {
                                passing_through_check = true;
                            }
                        }

                        for (var i = 1; i < 4; i++) {
                            if (board.Cells[rook_start_square.x + i, rook_start_square.y].MyPieceType != Piece.PieceType.None) {
                                piece_in_way = true;
                            }
                        }

                        if (!piece.HasMovedBefore && !king.HasMovedBefore && !passing_through_check && !piece_in_way) {
                            var king_target_square = new Vector2Int(king_start_square.x - 2, king_start_square.y);

                            pruned_moves.Add(new Move(king_start_square, king_target_square, true));
                        }
                    }
                    #endregion
                }
            }
            #endregion

            return pruned_moves;
        }

        public static List<Move> GenerateMoves(Board board) {
            Piece.PieceColour colour = board.ColourToMove;
            return GenerateMoves(board, colour);
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

        private static List<Move> _GenerateSlidingMoves(Vector2Int start_cell, Piece[,] cells) {
            var moves = new List<Move>();

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

                    moves.Add(new Move(start_cell, target_cell, false));

                    //if capturing piece, break to next dir index
                    if (target_piece.MyPieceColour == enemy_colour) {
                        break;
                    }
                }
            }

            return moves;
        }

        private static List<Move> _GeneratePawnMoves(Vector2Int start_cell, Piece[,] cells) {
            var moves = new List<Move>();

            Piece piece = cells[start_cell.x, start_cell.y];

            int forward = piece.MyPieceColour == Piece.PieceColour.White ? -1 : 1;

            //one forward
            var target_cell = new Vector2Int(start_cell.x, start_cell.y + forward);
            if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None) {
                moves.Add(new Move(start_cell, target_cell, false));
            }
            //two forward
            if (piece.CanDoubleMove) {
                target_cell = new Vector2Int(start_cell.x, start_cell.y + forward * 2);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType == Piece.PieceType.None && cells[target_cell.x, target_cell.y - forward].MyPieceType == Piece.PieceType.None) {
                    moves.Add(new Move(start_cell, target_cell, false));
                }
            }

            //capture diagonally
            int side = 1;
            for (int i = 0; i < 2; i++) {
                side = -side;

                target_cell = new Vector2Int(start_cell.x + side, start_cell.y + forward);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].MyPieceType != Piece.PieceType.None && cells[target_cell.x, target_cell.y].MyPieceColour != piece.MyPieceColour){
                    moves.Add(new Move(start_cell, target_cell, false));
                }

                //en passant capture
                //.
                target_cell = new Vector2Int(start_cell.x + side, start_cell.y);
                if (Board.InRange(target_cell) && cells[target_cell.x, target_cell.y].JustDoubleMoved && cells[target_cell.x, target_cell.y].MyPieceColour != piece.MyPieceColour) {
                    moves.Add(new Move(start_cell, new Vector2Int(target_cell.x, target_cell.y + forward), false));
                }
            }

            return moves;
        }

        private static List<Move> _GenerateKnightMoves(Vector2Int start_cell, Piece[,] cells) {
            var moves = new List<Move>();

            Piece piece = cells[start_cell.x, start_cell.y];

            for (int i = 0; i < knight_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + knight_offsets[i, 0], start_cell.y + knight_offsets[i, 1]);
                if (Board.InRange(target_cell)) {
                    Piece target_piece = cells[target_cell.x, target_cell.y];

                    if (target_piece.MyPieceColour != piece.MyPieceColour) {
                        moves.Add(new Move(start_cell, target_cell, false));
                    }
                }
            }

            return moves;
        }

        private static List<Move> _GenerateKingMoves(Vector2Int start_cell, Piece[,] cells) {
            var moves = new List<Move>();

            Piece piece = cells[start_cell.x, start_cell.y];

            for (int i = 0; i < sliding_offsets.GetLength(0); i++) {
                var target_cell = new Vector2Int(start_cell.x + sliding_offsets[i, 0], start_cell.y + sliding_offsets[i, 1]);

                if (Board.InRange(target_cell)) {
                    Piece target_piece = cells[target_cell.x, target_cell.y];

                    if (target_piece.MyPieceColour != piece.MyPieceColour) {
                        moves.Add(new Move(start_cell, target_cell, false));
                    }
                }
            }

            return moves;
        }

        public static bool TryParseMove(out Move move, string notation, Board board) {
            #region normal move
            Match match =  Regex.Match(notation, "^([a-h])([1-8]) ([a-h])([1-8])$", RegexOptions.IgnoreCase);

            if (match.Success) {
                move = new Move(
                    new Vector2Int(Array.IndexOf(Utils.Alphabet, char.Parse(match.Groups[1].Value)), Board.GridSize - int.Parse(match.Groups[2].ToString())),
                    new Vector2Int(Array.IndexOf(Utils.Alphabet, char.Parse(match.Groups[3].Value)), Board.GridSize - int.Parse(match.Groups[4].ToString())),
                    false
                );

                return true;
            }
            #endregion

            #region castle move
            if (notation == "0-0") {
                Vector2Int king_start_square = default;
                for (int y = 0; y < Board.GridSize; y++) {
                    for (int x = 0; x < Board.GridSize; x++) {
                        Piece piece = board.Cells[x, y];
                        if (piece.MyPieceType == Piece.PieceType.King && piece.MyPieceColour == board.ColourToMove) {
                            king_start_square = new Vector2Int(x, y);
                        }
                    }
                }

                var king_target_square = new Vector2Int(king_start_square.x + 2, king_start_square.y);

                move = new Move(king_start_square, king_target_square, true);
                return true;

            } else if (notation == "0-0-0") {
                Vector2Int king_start_square = default;
                for (int y = 0; y < Board.GridSize; y++) {
                    for (int x = 0; x < Board.GridSize; x++) {
                        Piece piece = board.Cells[x, y];
                        if (piece.MyPieceType == Piece.PieceType.King && piece.MyPieceColour == board.ColourToMove) {
                            king_start_square = new Vector2Int(x, y);
                        }
                    }
                }

                var king_target_square = new Vector2Int(king_start_square.x - 2, king_start_square.y);

                move = new Move(king_start_square, king_target_square, true);
                return true;
            }
            #endregion

            move = default;
            return false;
        }
    }
}


