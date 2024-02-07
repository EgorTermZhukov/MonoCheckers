using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameBoard
{
    internal class BoardPositionEqualityComparer : IEqualityComparer<BoardPosition>
    {
        public bool Equals(BoardPosition x, BoardPosition y)
        {
            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] BoardPosition obj)
        {
            return obj.X.GetHashCode() ^ obj.Y.GetHashCode();
        }
    }

    internal class BoardPosition : IEquatable<BoardPosition>
    {
        private int _x, _y;

        public BoardPosition(int x, int y) 
        {
            X = x;
            Y = y;
        }
        public int X 
        {
            get 
            {
                return _x;
            } 
            set 
            {
                if(value < 0)
                    throw new ArgumentOutOfRangeException();
                if(value >= 8)
                    throw new ArgumentOutOfRangeException();
                _x = value;
            }
        }
        public int Y 
        {
            get
            {
                return _y;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException();
                if (value >= 8)
                    throw new ArgumentOutOfRangeException();
                _y = value;
            }
        }

        public bool Equals(BoardPosition other)
        {
            if (X == other.X && Y == other.Y)
                return true;
            return false;
        }

        public string ToConventional() 
        {
            char letter = 'A';
            char digit = '1';
            letter += (char)_x;
            digit += (char)_y;

            string result = char.ToString(letter) + char.ToString(digit);

            return result;
        }
        public BoardPosition TopLeftDiagonal(int range = 1) 
        {
            int x = (X - 1) * range;
            int y = (Y + 1) * range;
            if (!IsInBoardRange(x, y))
                return null;
            return new BoardPosition(x, y);
        }
        public BoardPosition TopRightDiagonal(int range = 1) 
        {
            int x = (X + 1) * range;
            int y = (Y + 1) * range;
            if (!IsInBoardRange(x, y))
                return null;
            return new BoardPosition(x, y);
        }
        public BoardPosition BottomLeftDiagonal(int range = 1)
        {
            int x = (X - 1) * range;
            int y = (Y - 1) * range;
            if (!IsInBoardRange(x, y))
                return null;
            return new BoardPosition(x, y);
        }
        public BoardPosition BottomRightDiagonal(int range = 1)
        {
            int x = (X + 1) * range;
            int y = (Y - 1) * range;
            if (!IsInBoardRange(x, y))
                return null;
            return new BoardPosition(x, y);
        }
        public static bool IsInBoardRange(int x, int y) 
        {
            if (x < 0) return false;
            else if (x > 7) return false;
            if (y < 0) return false;
            else if (y > 7) return false;
            return true;
        }
    }
}
