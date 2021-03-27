using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Linq;

namespace ConsoleChessApp {
    class Board {
        private static readonly Vector2Int board_size = new(64, 32); //32, 16 base size
        private static readonly Vector2Int board_buffer = new(3, 1);
        private const string start_fen = "rnbqkbnr/1pp1pppp/8/p2pP3/8/7P/PPPP1PP1/RNBQKBNR w KQkq d6 0 1";

        private static readonly Dictionary<Piece.PieceType, char> piece_type_to_char = new() {
            [Piece.PieceType.None] = ' ',
            [Piece.PieceType.King] = 'k',
            [Piece.PieceType.Pawn] = 'p',
            [Piece.PieceType.Knight] = 'n',
            [Piece.PieceType.Bishop] = 'b',
            [Piece.PieceType.Rook] = 'r',
            [Piece.PieceType.Queen] = 'q',
        };
        private static readonly Dictionary<char, Piece.PieceType> char_to_piece_type = new() {
            ['k'] = Piece.PieceType.King,
            ['p'] = Piece.PieceType.Pawn,
            ['n'] = Piece.PieceType.Knight,
            ['b'] = Piece.PieceType.Bishop,
            ['r'] = Piece.PieceType.Rook,
            ['q'] = Piece.PieceType.Queen,
        };

        public const int GridSize = 8;
        public Piece[,] Cells = new Piece[GridSize, GridSize];
        public Piece.PieceColour ColourToMove;
        public static readonly Vector2Int EnterMovePos = new(0, board_size.y + board_buffer.y * 2 + 3);
        public const Piece.PieceColour PlayerColour = Piece.PieceColour.White;
        public const Piece.PieceColour AIPlayerColour = Piece.PieceColour.Black;

        //fen shit
        public bool fen_white_kingside_castle { get; private set; } = false;
        public bool fen_white_queenside_castle { get; private set; } = false;
        public bool fen_black_kingside_castle { get; private set; } = false;
        public bool fen_black_queenside_castle { get; private set; } = false;

        public void Move(Move move, bool change_colour=true, bool draw=true, bool promote=true) {
            //is_real_move determines whether to ask about pawn promotion and draw board

            if (!move.IsCastleMove) {
                Piece to = Cells[move.TargetSquare.x, move.TargetSquare.y];
                Piece from = Cells[move.StartSquare.x, move.StartSquare.y];

                //is this move a double move?
                if (from.MyPieceType == Piece.PieceType.Pawn && Math.Abs(move.StartSquare.y - move.TargetSquare.y) == 2) {
                    to.JustDoubleMoved = true;
                }

                //this move is en passant
                int backward = ColourToMove == Piece.PieceColour.White ? 1 : -1;
                var behind_vec = new Vector2Int(move.TargetSquare.x, move.TargetSquare.y + backward);

                //Console.WriteLine($"JustDoubleMoved: {Cells[behind_vec.x, behind_vec.y].JustDoubleMoved}");
                if (from.MyPieceType == Piece.PieceType.Pawn && InRange(behind_vec) && Cells[behind_vec.x, behind_vec.y].MyPieceType == Piece.PieceType.Pawn && move.StartSquare.x != move.TargetSquare.x && Cells[behind_vec.x, behind_vec.y].JustDoubleMoved && draw) {
                    Cells[behind_vec.x, behind_vec.y].MyPieceType = Piece.PieceType.None;
                    Cells[behind_vec.x, behind_vec.y].MyPieceColour = Piece.PieceColour.None;

                    Console.SetCursorPosition(move.TargetSquare.x * (board_size.x / GridSize) + board_buffer.x + (board_size.x / GridSize) / 2,
                            (move.TargetSquare.y + backward) * (board_size.y) / GridSize + board_buffer.y + (board_size.y / GridSize) / 2);
                    Console.Write(" ");
                }

                //if it's not a double move
                if (!(from.MyPieceType == Piece.PieceType.Pawn && Math.Abs(move.StartSquare.y - move.TargetSquare.y) == 2)) {
                    foreach (Piece piece in Cells) {
                        piece.JustDoubleMoved = false;
                    }
                }

                to.MyPieceType = from.MyPieceType;
                to.MyPieceColour = from.MyPieceColour;
                to.HasMovedBefore = true;
                from.MyPieceType = Piece.PieceType.None;
                from.MyPieceColour = Piece.PieceColour.None;
                from.HasMovedBefore = false;

                //it's a fuckin pawn promotion
                if ((move.TargetSquare.y == GridSize - 1 || move.TargetSquare.y == 0) && to.MyPieceType == Piece.PieceType.Pawn && promote) {
                    Console.SetCursorPosition(EnterMovePos.x, EnterMovePos.y);
                    Console.WriteLine("What would you like to promote to? Write your answer as a char (e.g. q, r, b, n).");
                    Utils.ClearCurrentConsoleLine();

                    Piece.PieceType promote_to;
                    while (true) {
                        try {
                            promote_to = char_to_piece_type[char.Parse(Console.ReadLine())];

                            if (MoveGenerator.CanPromoteTo.Contains(promote_to)) {
                                break;
                            }
                        } catch {
                            Utils.ClearCurrentConsoleLine();
                            continue;
                        }
                    }

                    to.MyPieceType = promote_to;
                }

                to.CanDoubleMove = false;

                if (draw) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.SetCursorPosition(move.StartSquare.x * (board_size.x / GridSize) + board_buffer.x + (board_size.x / GridSize) / 2,
                                move.StartSquare.y * (board_size.y) / GridSize + board_buffer.y + (board_size.y / GridSize) / 2);
                    Console.Write(" ");
                    Console.SetCursorPosition(move.TargetSquare.x * (board_size.x / GridSize) + board_buffer.x + (board_size.x / GridSize) / 2,
                                move.TargetSquare.y * (board_size.y) / GridSize + board_buffer.y + (board_size.y / GridSize) / 2);
                    char print = piece_type_to_char[to.MyPieceType];
                    if (to.MyPieceColour == Piece.PieceColour.White) {
                        print = char.ToUpper(print);
                    }
                    Console.Write(print);

                    Console.ResetColor();
                }

            } else if (move.IsCastleMove) {
                if (move.TargetSquare.x > move.StartSquare.x) {
                    //kingside castle
                    Vector2Int rook_start_square = new Vector2Int(move.StartSquare.x + 3, move.StartSquare.y);
                    Vector2Int rook_target_square = new Vector2Int(rook_start_square.x - 2, rook_start_square.y);

                    Move(new Move(rook_start_square, rook_target_square, false), false, true, false);
                    Move(new Move(move.StartSquare, move.TargetSquare, false), false, true, false);

                } else if (move.TargetSquare.x < move.StartSquare.x) {
                    //queenside castle
                    Vector2Int rook_start_square = new Vector2Int(move.StartSquare.x - 4, move.StartSquare.y);
                    Vector2Int rook_target_square = new Vector2Int(rook_start_square.x + 3, rook_start_square.y);

                    Move(new Move(rook_start_square, rook_target_square, false), false, true, false);
                    Move(new Move(move.StartSquare, move.TargetSquare, false), false, true, false);
                }
            }

            if (change_colour) {
                ColourToMove = ColourToMove == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;
            }
        }

        public static Board SimulateMove(Move move, Board board) {
            //simulates a move and returns a new board
            //only works with normal moves cuz thats all i need it for
            var new_board = new Board(board.ColourToMove);
            for (int y = 0; y < GridSize; y++) {
                for (int x = 0; x < GridSize; x++) {
                    new_board.Cells[x, y] = new Piece(board.Cells[x, y].MyPieceType, board.Cells[x, y].MyPieceColour, board.Cells[x, y].CanDoubleMove, board.Cells[x, y].HasMovedBefore, board.Cells[x, y].JustDoubleMoved);
                }
            }

            new_board.Move(move, true, false, false);

            return new_board;
        }

        public void Draw() {
            #region Pieces
            Console.ForegroundColor = ConsoleColor.Green;

            for (int y = 0; y < Cells.GetLength(0); y++) {
                for (int x = 0; x < Cells.GetLength(1); x++) {
                    Console.SetCursorPosition(x * (board_size.x / GridSize) + board_buffer.x + (board_size.x / GridSize) / 2,
                        y * (board_size.y) / GridSize + board_buffer.y + (board_size.y / GridSize) / 2);

                    char print = piece_type_to_char[Cells[x, y].MyPieceType];

                    if (Cells[x, y].MyPieceColour == Piece.PieceColour.White) {
                        print = char.ToUpper(print);
                    }
                    Console.Write(print);
                }
            }
            #endregion

            #region Board
            Console.ForegroundColor = ConsoleColor.White;

            //Letters on bottom, numbers on right
            //Chess board letters
            for (int i = 0; i < GridSize; i++) {
                Console.SetCursorPosition(board_buffer.x + i * (board_size.x / GridSize) + (board_size.x / GridSize) / 2,
                board_size.y + board_buffer.y * 2 + 1); //board_size.y + board_size.y / GridSize);

                Console.Write(Utils.Alphabet[i]);
            }
            
            //Numbers
            for (int i = 1; i <= GridSize; i++) {
                Console.SetCursorPosition(board_size.x + board_buffer.x * 2 + 1,//board_size.x + board_size.x / GridSize + 1,
                    board_buffer.y + board_size.y + (board_size.y / GridSize) / 2 - (i * board_size.y / GridSize));

                Console.Write(i);
            }

            //Horizontal
            for (int i = 0; i <= GridSize; i++) {
                for (int j = 0; j <= board_size.x; j++) {
                    Console.SetCursorPosition(j + board_buffer.x, i * (board_size.y / GridSize) + board_buffer.y);
                    if (j % (board_size.x / GridSize) == 0) {
                        Console.Write("+");
                    } else {
                        Console.Write("-");
                    }
                }
            }

            //Vertical
            for (int i = 0; i <= GridSize; i++) {
                for (int j = 0; j <= board_size.y; j++) {
                    Console.SetCursorPosition(board_buffer.x + i * (board_size.x / GridSize), j + board_buffer.y);
                    if (j % (board_size.y / GridSize) == 0) {
                        Console.Write("+");
                    } else {
                        Console.Write("|");
                    }
                }
            }

            #endregion

            Console.ResetColor();
        }

        public void LoadFromFen(string fen) {
            #region piece placement
            string fen_board = fen.Split(' ')[0]; //skip the can castle part and shit
            int board_x = 0;
            int board_y = 0;

            foreach (char symbol in fen_board) {
                if (symbol == '/') {
                    board_x = 0;
                    board_y++;
                } else if (char.IsDigit(symbol)) {
                    board_x += (int)char.GetNumericValue(symbol);
                } else {
                    //it's a letter

                    //upper fen is white, lower is black
                    Piece.PieceColour colour = char.IsUpper(symbol) ? Piece.PieceColour.White : Piece.PieceColour.Black;
                    Piece.PieceType type = char_to_piece_type[char.ToLower(symbol)];

                    Cells[board_x, board_y].MyPieceColour = colour;
                    Cells[board_x, board_y].MyPieceType = type;
                    board_x++;
                }
            }
            #endregion
            #region active colour
            char fen_colour = char.Parse(fen.Split(' ')[1]);
            ColourToMove = fen_colour == 'w' ? Piece.PieceColour.White : Piece.PieceColour.Black;
            #endregion
            #region castling availability
            //- for nothing, KQkq for each castle option otherwise
            string fen_castle_availability = fen.Split(' ')[2];
            if (fen_castle_availability != "-") {
                fen_white_kingside_castle = fen_castle_availability.Contains('K');
                fen_white_queenside_castle = fen_castle_availability.Contains('Q');
                fen_black_kingside_castle = fen_castle_availability.Contains('k');
                fen_black_queenside_castle = fen_castle_availability.Contains('q');
            } else {
                //do nothing, they're initialized to false already
            }
            #endregion
            #region en passant square
            string fen_en_passant = fen.Split(' ')[3];
            if (fen_en_passant != "-") {
                var fen_en_passant_square = new Vector2Int(Array.IndexOf(Utils.Alphabet, fen_en_passant[0]), GridSize - int.Parse(fen_en_passant[1].ToString()));
                int backward = ColourToMove == Piece.PieceColour.White ? 1 : -1;
                Cells[fen_en_passant_square.x, fen_en_passant_square.y + backward].JustDoubleMoved = true;
            }
            #endregion
            #region double move availability
            for (int y = 0; y < GridSize; y++) {
                for (int x = 0; x < GridSize; x++) {
                    if (y == GridSize - 2 || y == 1) {
                        break;
                    }

                    if (Cells[x, y].MyPieceType == Piece.PieceType.Pawn) {
                        Cells[x, y].CanDoubleMove = false;
                    }
                }
            }
            #endregion
        }

        public static bool InRange(Vector2Int pos) {
            return pos.x >= 0 && pos.x < GridSize && pos.y >= 0 && pos.y < GridSize;
        }

        public Board(Piece.PieceColour colour_to_move=Piece.PieceColour.White) {
            for (int y = 0; y < Cells.GetLength(0); y++) {
                for (int x = 0; x < Cells.GetLength(1); x++) {
                    Cells[x, y] = new Piece(Piece.PieceType.None, Piece.PieceColour.None);
                }
            }
            LoadFromFen(start_fen);
        }
    }
}
