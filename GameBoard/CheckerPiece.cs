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
        private Vector2 _squarePosition;

        public bool IsKing = false;

        private bool _isMoving = false;
        private static readonly float _startingMovingSpeed = 0.001f;
        private float _currentMovingSpeed = _startingMovingSpeed;

        private bool _isDestroying = false;

        private Random _random;
        private float _startupTime;

        public event EventHandler OnPieceDestroyed;

        private Texture2D _texture;
        private float _textureOpacity = 1f;

        private SpriteFont _debugFont;
        private string _debugText = string.Empty;

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
            _isMoving = true;
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

            spriteBatch.Draw(_texture, Position, null, Color.White * _textureOpacity, 0f, Vector2.Zero, 1f, SpriteEffects.None, depth);

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
