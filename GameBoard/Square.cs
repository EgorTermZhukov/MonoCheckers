using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Checkers.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Checkers.GameBoard
{
    public enum SquareColor 
    {
        White,
        Red
    }
    internal class Square
    {
        public BoardPosition BoardPosition { get; private set; }
        private CheckersBoard _board;
        private IPiece _piece;
        private int _size;

        public Square(CheckersBoard board, BoardPosition boardPosition)
        {
            _board = board;
            _size = board.SquareSize;
            BoardPosition = boardPosition;
        }
        public bool IsEmpty() 
        {
            return _piece == null;
        }
        public void AssignPiece(IPiece piece) 
        {
            if (!IsEmpty())
                throw new InvalidOperationException();
            _piece = piece;
        }
        public void RemovePiece() 
        {
            if(IsEmpty()) 
                throw new InvalidOperationException();
            _piece = null;
        }
        public CheckerColor GetPieceColor() 
        {
            if (IsEmpty())
                throw new InvalidOperationException();
            return _piece.GetColor();
        }
        public IPiece GetPiece() 
        {
            if(IsEmpty())
                throw new InvalidOperationException();
            return _piece;
        }
    }
}
