﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameBoard
{
    internal struct BoardPosition : IEquatable<BoardPosition>
    {
        private int _x, _y;

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
            if (this.ToConventional() == other.ToConventional())
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
    }
}
