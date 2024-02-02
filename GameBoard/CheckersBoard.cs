using Checkers.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.GameBoard
{
    public enum Player
    {
        First,
        Second
    }
    internal class CheckersBoard : GameObject
    {
        private Dictionary<BoardPosition, Square> _squares;
        private List<CheckerPiece> _checkers;
        public int SquareSize { get; private set; }
        public Player CurrentPlayer { get; private set; }
        public int MovesCount { get; private set; }

        private Texture2D _boardTexture;
        private Texture2D _mountTexture;

        public CheckersBoard(Player startingPlayer)
        {
            CurrentPlayer = startingPlayer;
            MovesCount = 0;
            SquareSize = 16;
            Position = new Vector2 (128, 128);
            Bounds = new Rectangle(128, 128, 8 * 16, 8 * 16);
        }
        public void Setup() 
        {
            SetupSquares();
            SetupCheckers(_squares);
        }

        private void SetupSquares() 
        {
            _squares = new Dictionary<BoardPosition, Square>();

            for(int y = 0; y < 8; y++) 
            {
                for(int x = 0; x < 8; x++) 
                {
                    BoardPosition boardPosition = new()
                    {
                        X = x,
                        Y = y,
                    };
                    var square = new Square(this, boardPosition);
                    _squares.Add(boardPosition, square);
                }
            }
        }
        private void SetupCheckers(Dictionary<BoardPosition, Square> squares) 
        {
            _checkers = new List<CheckerPiece>();

            for(int y = 0; y < 8; y++) 
            {
                for(int x = 0; x < 8; x++) 
                {
                    // some magic numbers haha...

                    if ((y + 1) % 2 == 0 && (x + 1) % 2 == 0)
                        continue;
                    else if ((y + 1) % 2 != 0 && (x + 1) % 2 != 0)
                        continue;

                    if (y == 3 || y == 4)
                        continue;

                    CheckerColor checkerColor = y > 4 ? CheckerColor.Red : CheckerColor.White;
                    BoardPosition boardPosition = new()
                    {
                        X = x,
                        Y = y,
                    };
                    Square square = squares[boardPosition];

                    //CheckerPiece checker = CheckerPiece.CreateChecker(checkerColor, square, BoardPositionToWorld(boardPosition));
                    CheckerPiece checker = CheckerPiece.CreateChecker(checkerColor, square, this, new Vector2(Position.X + Bounds.Width / 2 - 8, Position.Y + Bounds.Height / 2 - 8));
                    _checkers.Add(checker);
                    square.AssignPiece(checker);
                }
            }
        }
        public override void LoadContent(GraphicsDevice graphicsDevice, ContentManager content) 
        {
            base.LoadContent(graphicsDevice, content);
            _boardTexture = content.Load<Texture2D>("boardSquares");
            _mountTexture = content.Load<Texture2D>("mount");
            foreach(CheckerPiece checker in _checkers) 
            {
                checker.LoadContent(graphicsDevice, content);
            }
            DebugDrawColor = Color.DarkSlateBlue;
        }
        public override void Update(GameTime gameTime) 
        {
            foreach (CheckerPiece checker in _checkers)
                checker.Update(gameTime);
        }
        public override void Draw(SpriteBatch spriteBatch) 
        {
            //base.Draw(spriteBatch);
            spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), Color.White);
            spriteBatch.Draw(_boardTexture, Position, Color.White);
            foreach (CheckerPiece checker in _checkers)
                checker.Draw(spriteBatch);
        }
        public Vector2 ConventionalBoardPositionToWorld(string boardPosition) 
        {
            if (boardPosition.Length != 2)
                throw new InvalidOperationException();

            int x = BoardPositionXToUnit(boardPosition.First());
            int y = BoardPositionYToUnit(boardPosition.Last());

            Vector2 result = Position + new Vector2(x, y) * SquareSize;
            return result;
        }
        public Vector2 BoardPositionToWorld(BoardPosition boardPosition) 
        {
            int x = boardPosition.X;
            int y = boardPosition.Y;

            Vector2 origin = Position + new Vector2(0, 7) * SquareSize;

            Vector2 result = origin + new Vector2(x, -y) * SquareSize;
            return result;
        }

        public int BoardPositionXToUnit(char letter) 
        {
            char letterLowercase = char.ToLower(letter);
            char startLetter = 'a';
            if (letterLowercase > 'h')
                throw new ArgumentOutOfRangeException();
            if (letterLowercase == startLetter)
                return 1;
            return letterLowercase - startLetter;
        }
        public int BoardPositionYToUnit(char digit)
        {
            char startDigit = '1';
            if (digit > '8')
                throw new ArgumentOutOfRangeException();
            if (digit == startDigit)
                return 1;
            return digit - startDigit;
        }
    }
}
