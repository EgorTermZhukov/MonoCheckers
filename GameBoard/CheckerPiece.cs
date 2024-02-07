using Checkers.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameBoard
{
    public enum CheckerColor 
    {
        White,
        Red
    }
    public enum MoveType 
    {
        Normal,
        Jump,
    }

    internal class CheckerPiece : GameObject, IPiece
    {
        public CheckerColor PieceColor { get; private set; }
        
        private Square _square;
        private BoardPosition _boardPosition { get { return GetSquarePosition(); } }
        private Vector2 _squarePosition;

        public bool IsKing = false;

        private bool _isMoving = false;
        private static readonly float _startingMovingSpeed = 0.001f;
        private float _currentMovingSpeed = _startingMovingSpeed;

        private bool _isDestroying = false;
        private bool _isBecomingKing = false;

        private Random _random;
        private float _startupTime;

        public event EventHandler OnPieceDestroyed;

        private Texture2D _texture;
        private float _textureOpacity = 1f;

        private SpriteFont _debugFont;
        private string _debugText = string.Empty;

        private Color _currentDrawColor = Color.White;
        private float t = 0f;
        private CheckerPiece(CheckerColor color, Square square)
        {
            PieceColor = color;
            _square = square;
            _random = new Random();
            _startupTime = RandomFloat(1f, 1.1f, _random);
        }
        public float RandomFloat(float min, float max, Random random) 
        {
            return (float)random.NextDouble() * min * max + min;
        }
        public bool CanMoveTo(CheckersBoard board, Square square)
        {
            if (!square.IsEmpty())
                return false;
            return true;
        }
        public void MoveTo(CheckersBoard board, Square square)
        {
            _square.RemovePiece();
            _square = square;

            _square.AssignPiece(this);

            _squarePosition = board.BoardPositionToWorld(square.BoardPosition);

            if (((_boardPosition.Y == 7 && PieceColor == CheckerColor.White) || (_boardPosition.Y == 0 && PieceColor == CheckerColor.Red)) && !IsKing) 
            {
                IsKing = true;
                _isBecomingKing = true;
            }

            _isMoving = true;
        }
        public Dictionary<BoardPosition, MoveType> CalculatePossibleMoves(CheckersBoard board) 
        {
            int maxRange = 2;
            if (IsKing)
                maxRange = 7;

            Dictionary<BoardPosition, MoveType> possibleMoves = new();
            List<BoardPosition> currentMoves = new();
            List<BoardPosition> visitedDirections = new();
            List<Vector2> blockedDirections = new();

            for (int r = 1; r < maxRange; r++) 
            {
                currentMoves.Add(_boardPosition.TopLeftDiagonal(r));
                currentMoves.Add(_boardPosition.TopRightDiagonal(r));
                currentMoves.Add(_boardPosition.BottomLeftDiagonal(r));
                currentMoves.Add(_boardPosition.BottomRightDiagonal(r));
                foreach(BoardPosition move in currentMoves) 
                {
                    if(move == null) 
                        continue;

                    int directionX = (move.X - _boardPosition.X);
                    int directionY = (move.Y - _boardPosition.Y);

                    Vector2 blockedDirection = new Vector2(directionX / Math.Abs(directionX), directionY / Math.Abs(directionY));

                    if (ContainsDirection(blockedDirections, blockedDirection))
                        continue;

                    Square moveSquare = board.Squares[move];

                    // if there were two piece in a row in that direction then block it

                    if (!moveSquare.IsEmpty() && !IsAbleToJumpOver(move, board, out BoardPosition blockage)) 
                    {
                        visitedDirections.Add(move);
                        blockedDirections.Add(blockedDirection);
                        visitedDirections.Add(blockage);
                        continue;
                    }
                    if (!moveSquare.IsEmpty() && IsAbleToJumpOver(move, board, out BoardPosition jump))
                    {
                        possibleMoves.Add(jump, MoveType.Jump);
                        visitedDirections.Add(move);
                        visitedDirections.Add(jump);
                        continue;
                    }
                    if (!moveSquare.IsEmpty()) 
                    {
                        visitedDirections.Add(move);
                        continue;
                    }
                    if (!IsAllowedToMoveY(move))
                        continue;

                    possibleMoves.Add(move, MoveType.Normal);
                    visitedDirections.Add(move);
                }
                currentMoves.Clear();
            }
            return possibleMoves;
        }
        private bool ContainsDirection(List<Vector2> directions, Vector2 dir) 
        {
            foreach(var direction in directions) 
            {
                if(direction == dir) return true;
            }
            return false;
        }
        private bool ContainsMove(List<BoardPosition> moves, BoardPosition boardPosition) 
        {
            foreach(BoardPosition move in moves) 
            {
                if (move.Equals(boardPosition))
                    return true;
            }
            return false;
        }
        private bool IsAllowedToMoveY(BoardPosition move)
        {
            if (IsKing)
                return true;

            int differenceY = move.Y - _boardPosition.Y;

            if (PieceColor == CheckerColor.White)
            {
                if (differenceY <= 0)
                    return false;
            }
            else
            {
                if (differenceY >= 0)
                    return false;
            }
            return true;
        }
        private bool IsAbleToJumpOver(BoardPosition otherPiecePosition, CheckersBoard board, out BoardPosition jumpPosition)
        {
            int distanceX = otherPiecePosition.X - _boardPosition.X;
            int distanceY = otherPiecePosition.Y - _boardPosition.Y;
            jumpPosition = null;
            int sqBeforeOtherPieceX = otherPiecePosition.X - distanceX / Math.Abs(distanceX);
            int sqBeforeOtherPieceY = otherPiecePosition.Y - distanceY / Math.Abs(distanceY);

            if(BoardPosition.IsInBoardRange(sqBeforeOtherPieceX, sqBeforeOtherPieceY) && !board.Squares[new(sqBeforeOtherPieceX, sqBeforeOtherPieceY)].IsEmpty())
            {
                if (board.Squares[new(sqBeforeOtherPieceX, sqBeforeOtherPieceY)].GetPiece() != this)
                {
                    jumpPosition = new(sqBeforeOtherPieceX, sqBeforeOtherPieceY);
                    return false;
                }
            }
            int newPositionX = otherPiecePosition.X + distanceX;
            int newPositionY = otherPiecePosition.Y + distanceY;

            if (!BoardPosition.IsInBoardRange(newPositionX, newPositionY))
                return false;

            BoardPosition newPiecePosition = new(newPositionX, newPositionY);
            jumpPosition = newPiecePosition;
            
            if (!board.Squares[newPiecePosition].IsEmpty())
                return false;
            if (PieceColor == board.Squares[otherPiecePosition].GetPieceColor()) 
            {
                return false;
            }



            return true;
        }
        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content) 
        {
            base.LoadContent(graphicsDevice, content);
            string textureName = string.Empty;
            switch (PieceColor)
            {
                case CheckerColor.White:
                    textureName = "checkerWhite";
                    break;
                case CheckerColor.Red:
                    textureName = "checkerRed";
                    break;
            }
#if DEBUG
            _debugFont = content.Load<SpriteFont>("DebugFont");
#endif
            _texture = content.Load<Texture2D>(textureName);

        }
        public override void Update(GameTime gameTime) 
        {
            if(IsDestroyed) return;

            // i will implement this properly after i make a gameObjectManager of some sort
            if (_isDestroying)
            {
                //_debugText = "opacity: " + _textureOpacity + "\n" + "state: " + IsDestroyed;
                _textureOpacity = MathHelper.Lerp(_textureOpacity, 0, 0.1f);
                if (_textureOpacity <= 0.03f)
                    DestroyCompletely();
            }
            if (t > 1f)
                _isBecomingKing = false;
            if (_isBecomingKing) 
            {
                t += 0.1f;
                _currentDrawColor = Color.Lerp(Color.White, Color.Goldenrod, t);
            }
            if (gameTime.TotalGameTime.TotalSeconds < _startupTime) return;

            if(IsVisible == false)
                IsVisible = true;

            if (_isMoving) 
            {
                Position = Vector2.Lerp(Position, _squarePosition, 0.1f);
            }
            if (_isMoving && _squarePosition == Position)
                _isMoving = false;

        }
        private float EaseOut(float t) 
        {
            return 1 - (float) Math.Pow(1 - t, 4);
        }
        private float EaseIn(float t) 
        {
            return MathF.Sqrt(t);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;
            //base.Draw(spriteBatch);

            float depth = 0.2f;

            if (_isMoving)
                depth = 0.3f;

            spriteBatch.Draw(_texture, Position, null, _currentDrawColor * _textureOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth);

            //spriteBatch.Draw(_texture, Position, Color.White);
#if DEBUG
            spriteBatch.DrawString(_debugFont, _debugText, Position + new Vector2(0, 16), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.4f);
#endif
        }
        public static CheckerPiece CreateChecker(CheckerColor color, Square square, CheckersBoard board, Vector2 startingPosition) 
        {
            CheckerPiece checker = new CheckerPiece(color, square);
            int checkerSize = 16;
            switch (color) 
            {
                case CheckerColor.White:
                    checker.DebugDrawColor = Color.GhostWhite;
                    break;
                case CheckerColor.Red:
                    checker.DebugDrawColor = Color.Red;
                    break;
            }
            checker.Position = startingPosition;
            checker._squarePosition = board.BoardPositionToWorld(square.BoardPosition);
            checker.Bounds = new Rectangle((int)startingPosition.X, (int)startingPosition.Y, checkerSize, checkerSize);
            checker._isMoving = true;
            checker.IsVisible = false;
            return checker;
        }
        public BoardPosition GetSquarePosition() 
        {
            if (_square == null)
                return null;
            return _square.BoardPosition;
        }
        public CheckerColor GetColor() 
        {
            return this.PieceColor;
        }
        public override void Destroy()
        {
            _square.RemovePiece();
            _isDestroying = true;
        }
        public void DestroyCompletely() 
        {
            OnPieceDestroyed?.Invoke(this, new EventArgs());
            base.Destroy();
            _texture = null;
        }
    }
}
