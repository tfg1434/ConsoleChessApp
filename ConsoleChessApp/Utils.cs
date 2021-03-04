using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShittyChessApp {
    static class Utils {
        public static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

        #region approach overloads
        public static int Approach(int val1, int val2, int inc) {
            return val1 + Math.Clamp(val2 - val1, -inc, inc);
        }
        public static double Approach(double val1, double val2, double inc) {
            return val1 + Math.Clamp(val2 - val1, -inc, inc);
        }
        public static decimal Approach(decimal val1, decimal val2, decimal inc) {
            return val1 + Math.Clamp(val2 - val1, -inc, inc);
        }
        public static float Approach(float val1, float val2, float inc) {
            return val1 + Math.Clamp(val2 - val1, -inc, inc);
        }
        public static long Approach(long val1, long val2, long inc) {
            return val1 + Math.Clamp(val2 - val1, -inc, inc);
        }
        #endregion

        public static void ClearCurrentConsoleLine() {
            int current_line_cursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.BufferWidth));
            Console.SetCursorPosition(0, current_line_cursor);
        }
    }
}
