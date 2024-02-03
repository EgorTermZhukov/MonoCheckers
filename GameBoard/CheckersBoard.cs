using Checkers.Engine;
using Checkers.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
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
        private BoardPositionEqualityComparer _positionEqualityComparer;
        private Dictionary<BoardPosition, Square> _squares;
        private List<CheckerPiece> _checkers;
        public int SquareSize { get; private set; }
        public CheckerColor CurrentPlayer { get; private set; }
        public int MovesCount { get; private set; }

        private Texture2D _boardTexture;
        private Texture2D _mountTexture;

        private Texture2D _selectionTexture;
        private SpriteFont _debugFont;

        private Square _selectedSquare;
        private Square _previouslySelectedSquare;

        private Vector2 _origin { get { return Position + new Vector2(0, 8) * SquareSize; } }

        private bool hasMovedPiece = false;

        public CheckersBoard(CheckerColor startingPlayer)
        {
            CurrentPlayer = startingPlayer;
            MovesCount = 0;
            SquareSize = 16;
            Position = new Vector2 (128, 128);
            Bounds = new Rectangle(128, 128, 8 * 16, 8 * 16);
        }
        public void Init(MouseInput mouseInput) 
        {
            mouseInput.OnLeftClick += AssignSelectedSquare;
        }
        public void Setup() 
        {
            SetupSquares();
            SetupCheckers(_squares);
        }

        private void SetupSquares() 
        {
            _positionEqualityComparer = new BoardPositionEqualityComparer();
            _squares = new Dictionary<BoardPosition, Square>(_positionEqualityComparer);

            for(int y = 0; y < 8; y++) 
            {
                for(int x = 0; x < 8; x++) 
                {
                    BoardPosition boardPosition = new(x, y);

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
                    BoardPosition boardPosition = new(x, y);

                    Square square = squares[boardPosition];

                    Vector2 startingPosition = new Vector2(Position.X + Bounds.Width / 2 - SquareSize / 2,
                                                    Position.Y + Bounds.Height / 2 - SquareSize / 2);

                    CheckerPiece checker = CheckerPiece.CreateChecker(checkerColor, square, this, startingPosition);
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
            _selectionTexture = content.Load<Texture2D>("selection");
            _debugFont = content.Load<SpriteFont>("DebugFont");

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

            if (gameTime.TotalGameTime.TotalSeconds < 3)
                return;

            if (_previouslySelectedSquare == null)
                return;
            if (_selectedSquare == null) 
                return;
            if (_previouslySelectedSquare.IsEmpty())
                return;
            if (!_selectedSquare.IsEmpty())
                return;

            CheckerPiece piece = _previouslySelectedSquare.GetPiece() as CheckerPiece;
            if (!IsCurrentPlayerPiece(piece))
                return;

            int differenceY = _previouslySelectedSquare.BoardPosition.Y - _selectedSquare.BoardPosition.Y;
            int differenceX = _previouslySelectedSquare.BoardPosition.X - _selectedSquare.BoardPosition.X;
            differenceX = Math.Abs(differenceX);

            if(piece.PieceColor == CheckerColor.White && !piece.IsKing) 
            {
                if (differenceY >= 0)
                    return;
            }
            else 
            {
                if (differenceY <= 0)
                    return;
            }
            if (differenceX != 1)
                return;

            piece.MoveTo(this, _selectedSquare);

            ChangeCurrentPlayer();

            _previouslySelectedSquare = null;
            _selectedSquare = null;

            //if (!hasMovedPiece && gameTime.TotalGameTime.TotalSeconds > 4) 
            //{
            //    BoardPosition boardPosition = new(1, 4);

            //    _checkers[12].MoveTo(this, _squares[boardPosition]);
            //    hasMovedPiece = true;
            //}
        }
        private void ChangeCurrentPlayer() 
        {
            switch (CurrentPlayer) 
            {
                case CheckerColor.White:
                    CurrentPlayer = CheckerColor.Red;
                    break;
                case CheckerColor.Red:
                    CurrentPlayer = CheckerColor.White;
                    break;
            }
        }
        private bool IsCurrentPlayerPiece(CheckerPiece checkerPiece) 
        {
            return CurrentPlayer == checkerPiece.PieceColor;
        }
        public override void Draw(SpriteBatch spriteBatch) 
        {
            //base.Draw(spriteBatch);
            spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), Color.White);
            spriteBatch.Draw(_boardTexture, Position, Color.White);
            foreach (CheckerPiece checker in _checkers)
                checker.Draw(spriteBatch);
            if(_selectedSquare != null) 
            {
                spriteBatch.Draw(_selectionTexture, BoardPositionToWorld(_selectedSquare.BoardPosition), Color.White);
            }
        }
        public Vector2 ConventionalBoardPositionToWorld(string boardPosition) 
        {
            if (boardPosition.Length != 2)
                throw new InvalidOperationException();

            int x = BoardPositionXToUnit(boardPosition.First());
            int y = BoardPositionYToUnit(boardPosition.Last());

            Vector2 origin = Position + new Vector2(0, 7) * SquareSize;

            Vector2 result = origin + new Vector2(x, -y) * SquareSize;
            return result;
        }
        public Vector2 BoardPositionToWorld(BoardPosition boardPosition) 
        {
            int x = boardPosition.X;
            int y = boardPosition.Y;

            Vector2 result = _origin + new Vector2(x, -y) * SquareSize - new Vector2(0, 16);
            return result;
        }
        public void AssignSelectedSquare(object mouse, MouseInputEventArgs args) 
        {
            BoardPosition position = WorldPositionToBoard(args.Position);
            if (position == null) 
            {
                _selectedSquare = null;
                return;
            }
            _previouslySelectedSquare = _selectedSquare;
            _selectedSquare = _squares[position];
        }
        private bool IsSquareSelected(int tiledX, int tiledY) 
        {
            if (tiledX < 0) return false;
            else if (tiledX > 7) return false;
            if (tiledY < 0) return false;
            else if (tiledY > 7) return false;
            return true;
        }
        public BoardPosition WorldPositionToBoard(Vector2 worldPosition) 
        {
            Vector2 boardSpacePosition = worldPosition - _origin;

            int tiledX = (int) boardSpacePosition.X / SquareSize;
            int tiledY = (int) -boardSpacePosition.Y / SquareSize;

            int resultX = tiledX;
            int resultY = tiledY;

            if (!IsSquareSelected(tiledX, tiledY))
                return null;

            return new BoardPosition(resultX, resultY);
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
