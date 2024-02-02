using Checkers.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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

    internal class CheckerPiece : GameObject, IPiece
    {
        public CheckerColor PieceColor { get; private set; }
        private Square _square;
        private bool _isMoving = false;
        private bool _isKing = false;
        private Texture2D _texture;

        private Vector2 _squarePosition;

        private static readonly float _startingMovingSpeed = 0.001f;
        private float _currentMovingSpeed = _startingMovingSpeed;
        private Random _random;
        private float _startupTime;

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
            throw new NotImplementedException();
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
            _texture = content.Load<Texture2D>(textureName);

        }
        public override void Update(GameTime gameTime) 
        {
            if (gameTime.TotalGameTime.TotalSeconds < _startupTime) return;
            if(IsVisible == false)
                IsVisible = true;

            if (Math.Abs(_squarePosition.X - Position.X) < 0.5 && Math.Abs(_squarePosition.Y - Position.Y) < 0.5)
            {
                _isMoving = false;
                _currentMovingSpeed = _startingMovingSpeed;
                Position = _squarePosition;
            }
            else
            {
                _isMoving = true;
            }

            if (_isMoving) 
            {
                Position = Vector2.Lerp(Position, _squarePosition, 0.08f);
            }
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
            Color color = Color.White;
            if (_isMoving)
                color = Color.White;

            spriteBatch.Draw(_texture, Position, color);
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
    }
}
