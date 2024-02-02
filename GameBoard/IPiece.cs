using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameBoard
{
    internal interface IPiece
    {
        public void MoveTo(CheckersBoard board, Square square);
        public bool CanMoveTo(CheckersBoard board, Square square);
    }
}
