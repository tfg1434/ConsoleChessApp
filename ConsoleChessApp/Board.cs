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
        private const string start_fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KBkq - 0 1";

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

        public void Move(Move move, bool change_colour_to_move=true) {
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
            if (InRange(behind_vec) && Cells[behind_vec.x, behind_vec.y].MyPieceType == Piece.PieceType.Pawn && move.StartSquare.x != move.TargetSquare.x && Cells[behind_vec.x, behind_vec.y].JustDoubleMoved) {
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
            if ((move.TargetSquare.y == GridSize - 1 || move.TargetSquare.y == 0) && to.MyPieceType == Piece.PieceType.Pawn) {
                Console.SetCursorPosition(EnterMovePos.x, EnterMovePos.y);
                Console.WriteLine("What would you like to promote to? Write your answer as a char (e.g. q, r, b, n).");
                Utils.ClearCurrentConsoleLine();

                Piece.PieceType promote_to;
                while (true) {
                    try {
                        promote_to = char_to_piece_type[char.Parse(Console.ReadLine())];

                        if (promote_to != Piece.PieceType.King && promote_to != Piece.PieceType.Pawn) {
                            break;
                        }
                    } catch {
                        Utils.ClearCurrentConsoleLine();
                        continue;
                    }
                }

                to.MyPieceType = promote_to;
            }

            if (change_colour_to_move) {
                ColourToMove = ColourToMove == Piece.PieceColour.White ? Piece.PieceColour.Black : Piece.PieceColour.White;
            }
            to.CanDoubleMove = false;

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

        //simulates a move and returns Cells, but does not actually change the board.
        public static Board SimulateMove(Move move, Board board) {
            var new_board = new Board(board.ColourToMove);
            for (int y = 0; y < GridSize; y++) {
                for (int x = 0; x < GridSize; x++) {
                    new_board.Cells[x, y] = new Piece(board.Cells[x, y].MyPieceType, board.Cells[x, y].MyPieceColour, board.Cells[x, y].CanDoubleMove);
                }
            }

            Piece to = new_board.Cells[move.TargetSquare.x, move.TargetSquare.y];
            Piece from = new_board.Cells[move.StartSquare.x, move.StartSquare.y];

            to.MyPieceType = from.MyPieceType;
            to.MyPieceColour = from.MyPieceColour;

            from.MyPieceType = Piece.PieceType.None;
            from.MyPieceColour = Piece.PieceColour.None;
            to.CanDoubleMove = false;

            return new_board;
        }

        public bool IsSquareAttacked(Vector2Int square, Board board, Piece.PieceColour attacking_colour) {
            List<Move> moves = MoveGenerator.GenerateMoves(board, attacking_colour);

            if (moves.Any(move => move.TargetSquare == square)) {
                return true;
            }

            return false;
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
            string fen_board = fen.Split(' ')[0]; //skip the can castle part and shit
            int x = 0;
            int y = 0;

            foreach (char symbol in fen_board) {
                if (symbol == '/') {
                    x = 0;
                    y++;
                } else if (char.IsDigit(symbol)) {
                    x += (int)char.GetNumericValue(symbol);
                } else {
                    //it's a letter

                    //upper fen is white, lower is black
                    Piece.PieceColour colour = char.IsUpper(symbol) ? Piece.PieceColour.White : Piece.PieceColour.Black;
                    Piece.PieceType type = char_to_piece_type[char.ToLower(symbol)];

                    Cells[x, y].MyPieceColour = colour;
                    Cells[x, y].MyPieceType = type;
                    x++;
                }
            }
        }

        public static Move ParseNotation(string notation) {
            string[] split = notation.Split(" ");

            var from = new Vector2Int(Array.IndexOf(Utils.Alphabet, split[0][0]), GridSize - int.Parse(split[0][1].ToString()));
            var to = new Vector2Int(Array.IndexOf(Utils.Alphabet, split[1][0]), GridSize - int.Parse(split[1][1].ToString()));

            return new Move(from, to);
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

            ColourToMove = colour_to_move;
        }
    }
}
