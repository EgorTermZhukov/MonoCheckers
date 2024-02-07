using Checkers.Engine;
using Checkers.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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
        public static BoardPositionEqualityComparer PositionEqualityComparer { get { return new BoardPositionEqualityComparer(); } }
        public Dictionary<BoardPosition, Square> Squares { get; private set; }
        private List<CheckerPiece> _checkers;
        private Dictionary<CheckerPiece, List<BoardPosition>> _possibleMoves;
        public int SquareSize { get; private set; }
        public CheckerColor CurrentPlayer { get; private set; }
        public int MovesCount { get; private set; }

        private Texture2D _boardTexture;
        private Texture2D _mountTexture;
        private Texture2D _moveSelectionTexture;
        private Texture2D _selectionTexture;
        private SpriteFont _debugFont;

        private Square _selectedSquare;
        private Square _previouslySelectedSquare;

        private string _debugText = String.Empty;

        private List<GameObject> _gameObjectsToDestroy = new List<GameObject>();

        private Vector2 _origin { get { return Position + new Vector2(0, 8) * SquareSize; } }

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
            SetupCheckers(Squares);
            _possibleMoves = new Dictionary<CheckerPiece, List<BoardPosition>>();
            RecalculateMovesForPieces();
        }

        private void SetupSquares() 
        {
            Squares = new Dictionary<BoardPosition, Square>(PositionEqualityComparer);

            for(int y = 0; y < 8; y++) 
            {
                for(int x = 0; x < 8; x++) 
                {
                    BoardPosition boardPosition = new(x, y);

                    var square = new Square(this, boardPosition);
                    Squares.Add(boardPosition, square);
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

                    if (!IsInTheDiagonals(x, y))
                        continue;

                    if (y == 3 || y == 4)
                        continue;

                    CheckerColor checkerColor = y > 4 ? CheckerColor.Red : CheckerColor.White;
                    BoardPosition boardPosition = new(x, y);

                    Square square = squares[boardPosition];

                    Vector2 startingPosition = new Vector2(Position.X + Bounds.Width / 2 - SquareSize / 2,
                                                    Position.Y + Bounds.Height / 2 - SquareSize / 2);

                    CheckerPiece checker = CheckerPiece.CreateChecker(checkerColor, square, this, startingPosition);
                    checker.OnPieceDestroyed += AddPieceToRemoved;
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
            _moveSelectionTexture = content.Load<Texture2D>("moveSelection");
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

            // gameObjectDestruction manager needed
            foreach (GameObject obj in _gameObjectsToDestroy) 
            {
                if(obj == null) continue;
                if (!obj.IsDestroyed)
                    continue;
                CheckerPiece destroyingPiece = obj as CheckerPiece;
                if (!_checkers.Contains(destroyingPiece))
                    continue;
                _checkers.Remove(destroyingPiece);
            }
            _gameObjectsToDestroy.Clear();
            //

            if (gameTime.TotalGameTime.TotalSeconds < 3)
                return;

            if (GetWhitePiecesCount() == 0) 
            {
                _debugText = "REDS WIN";
                return;
            }
            else if (GetRedPiecesCount() == 0) 
            {
                _debugText = "WHITES WIN";
                return;
            }

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
            List<BoardPosition> possibleMoves = _possibleMoves[piece];
            BoardPosition movePosition = _selectedSquare.BoardPosition;

            if (possibleMoves.Count == 0)
                return;

            bool isMoveLegit = false;
            foreach(var move in possibleMoves) 
            {
                if (move.Equals(movePosition))
                    isMoveLegit = true;
            }
            if (!isMoveLegit)
                return;

            piece.MoveTo(this, _selectedSquare);

            IsAJumpOver(piece, _previouslySelectedSquare, _selectedSquare);

            ChangeTurn();

            _previouslySelectedSquare = null;
            _selectedSquare = null;
        }
        private void ChangeTurn() 
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
            RecalculateMovesForPieces();
            MovesCount++;
        }
        private bool IsPieceAllowedToMove(CheckerPiece piece, Square pieceSquare, Square targetSquare) 
        {
            if (!IsMovingDiagonally(piece, pieceSquare, targetSquare))
                return false;
            if (!targetSquare.IsEmpty())
                return false;
            if (!IsPieceAllowedToMoveY(piece, pieceSquare, targetSquare))
                return false;
            if (!IsPieceAllowedToMoveX(piece, pieceSquare, targetSquare))
                return false;
            return true;
        }
        private bool IsMovingDiagonally(CheckerPiece piece, Square pieceSquare, Square targetSquare) 
        {
            int differenceX = targetSquare.BoardPosition.X - pieceSquare.BoardPosition.X;
            int differenceY = targetSquare.BoardPosition.Y - pieceSquare.BoardPosition.Y;

            if (Math.Abs(differenceX) != Math.Abs(differenceY))
                return false;
            return true;
        }
        private bool IsPieceAllowedToMoveX(CheckerPiece piece, Square pieceSquare, Square targetSquare)
        {
            int differenceX = pieceSquare.BoardPosition.X - targetSquare.BoardPosition.X;

            if (Math.Abs(differenceX) != 1)
                return false;

            return true;
        }
        private bool IsPieceAllowedToMoveY(CheckerPiece piece, Square pieceSquare, Square targetSquare)
        {
            int differenceY = pieceSquare.BoardPosition.Y - targetSquare.BoardPosition.Y;

            if (MathF.Abs(differenceY) != 1)
                return false;

            if (piece.PieceColor == CheckerColor.White)
            {
                if (differenceY >= 0)
                    return false;
            }
            else
            {
                if (differenceY <= 0)
                    return false;
            }
            return true;
        }
        private void RecalculateMovesForPieces() 
        {
            _possibleMoves.Clear();
            foreach(CheckerPiece piece in _checkers) 
            {
                List<BoardPosition> moves = CalculateMoves(piece);
                _possibleMoves.Add(piece, moves);
            }
        }
        private List<BoardPosition> CalculateMoves(CheckerPiece piece) 
        {
            //check for 3 by 3 area around piece (temporary)
            BoardPosition piecePosition = piece.GetSquarePosition();
            List<BoardPosition> moves = new();

            if (!IsCurrentPlayerPiece(piece))
                return moves;

            List<BoardPosition> visited = new();

            int area = 1;

            for(int y = piecePosition.Y - area; y <= piecePosition.Y + area * 2; y++) 
            {
                for(int x = piecePosition.X - area; x <= piecePosition.X + area * 2; x++) 
                {
                    if (!BoardPosition.IsInBoardRange(x, y))
                        continue;
                    if (x == piecePosition.X && y == piecePosition.Y)
                        continue;
                    
                    BoardPosition possibleMovePosition = new(x, y);

                    visited.Add(possibleMovePosition);

                    if(!Squares[possibleMovePosition].IsEmpty() && IsPieceAbleToJumpOver(piece.PieceColor, piecePosition, possibleMovePosition)) 
                    {
                        int distanceX = possibleMovePosition.X - piecePosition.X;
                        int distanceY = possibleMovePosition.Y - piecePosition.Y;

                        int newPositionX = possibleMovePosition.X + distanceX;
                        int newPositionY = possibleMovePosition.Y + distanceY;

                        moves.Add(new(newPositionX, newPositionY));

                        continue;
                    }
                    if (!IsPieceAllowedToMove(piece, Squares[piecePosition], Squares[possibleMovePosition]))
                        continue;
                    moves.Add(possibleMovePosition);
                }
            }
            return moves;
        }
        private bool IsAJumpOver(CheckerPiece piece, Square pieceSquare, Square targetSquare) 
        {
            BoardPosition pieceBoardPosition = pieceSquare.BoardPosition;

            int differenceX = targetSquare.BoardPosition.X - pieceSquare.BoardPosition.X;
            int differenceY = targetSquare.BoardPosition.Y - pieceSquare.BoardPosition.Y;

            //_debugText = "diffX: " + differenceX + "\n" + "diffY " + differenceY;

            //if (Math.Abs(differenceY) != 2)
            //    return false;

            int potentionalPositionX = pieceBoardPosition.X;
            int potentionalPositionY = pieceBoardPosition.Y;

            if (differenceX > 0)
                potentionalPositionX++;
            else
                potentionalPositionX--;
            if (differenceY > 0)
                potentionalPositionY++;
            else
                potentionalPositionY--;

            if (potentionalPositionX < 0 || potentionalPositionX > 7)
                return false;
            if (potentionalPositionY < 0 || potentionalPositionY > 7)
                return false;

            BoardPosition potentionalPosition = new(potentionalPositionX, potentionalPositionY);
            Square potentionalSquare = Squares[potentionalPosition];

            if (potentionalSquare.IsEmpty())
                return false;
            if (piece.PieceColor == potentionalSquare.GetPieceColor())
                return false;

            // this part is in need of change
            CheckerPiece pieceToDestroy = potentionalSquare.GetPiece() as CheckerPiece;
            pieceToDestroy.Destroy();
            return true;
        }
        private bool IsPieceAbleToJumpOver(CheckerColor checkerColor, BoardPosition piecePosition, BoardPosition otherPiecePosition) 
        {
            if (checkerColor == Squares[otherPiecePosition].GetPieceColor())
                return false;

            int distanceX = otherPiecePosition.X - piecePosition.X;
            int distanceY = otherPiecePosition.Y - piecePosition.Y;

            int newPositionX = otherPiecePosition.X + distanceX;
            int newPositionY = otherPiecePosition.Y + distanceY;

            if (!BoardPosition.IsInBoardRange(newPositionX, newPositionY))
                return false;

            BoardPosition newPiecePosition = new(newPositionX, newPositionY);

            if (!Squares[newPiecePosition].IsEmpty())
                return false;
            return true;
        }
        public bool IsCurrentPlayerPiece(CheckerPiece checkerPiece) 
        {
            return CurrentPlayer == checkerPiece.PieceColor;
        }
        public override void Draw(SpriteBatch spriteBatch) 
        {
            //base.Draw(spriteBatch);

            spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_boardTexture, Position, null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.1f);

            //spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), Color.White);
            //spriteBatch.Draw(_boardTexture, Position, Color.White);
            foreach (CheckerPiece checker in _checkers)
                checker.Draw(spriteBatch);
            if (_selectedSquare != null && !_selectedSquare.IsEmpty()) 
            {
                CheckerPiece piece = _selectedSquare.GetPiece() as CheckerPiece;

                List<BoardPosition> moves = _possibleMoves[piece];
                if(moves.Count > 0) 
                {
                    foreach (BoardPosition move in moves) 
                    {
                        spriteBatch.Draw(_moveSelectionTexture, BoardPositionToWorld(move), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.3f);
                    }
                }

            }
                
            spriteBatch.DrawString(_debugFont, _debugText, _origin, Color.White);
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
            _selectedSquare = Squares[position];
        }
        public int GetWhitePiecesCount() 
        {
            var whiteQuery = from piece in _checkers where piece.PieceColor == CheckerColor.White select piece;
            return whiteQuery.Count();
        }
        public int GetRedPiecesCount() 
        {
            var redQuery = from piece in _checkers where piece.PieceColor == CheckerColor.Red select piece;
            return redQuery.Count();
        }
        public void AddPieceToRemoved(object sender, EventArgs e) 
        {
            CheckerPiece piece = sender as CheckerPiece;

            if (piece == null)
                throw new ArgumentNullException();
            if (!_checkers.Contains(piece))
                throw new ArgumentException();
            _gameObjectsToDestroy.Add(piece);
        }
        public static bool IsInTheDiagonals(int x, int y)
        {
            if ((y + 1) % 2 == 0 && (x + 1) % 2 == 0)
                return false;
            else if ((y + 1) % 2 != 0 && (x + 1) % 2 != 0)
                return false;
            return true;
        }
        public BoardPosition WorldPositionToBoard(Vector2 worldPosition) 
        {
            Vector2 boardSpacePosition = worldPosition - _origin;

            int tiledX = (int) boardSpacePosition.X / SquareSize;
            int tiledY = (int) -boardSpacePosition.Y / SquareSize;

            int resultX = tiledX;
            int resultY = tiledY;

            if (!BoardPosition.IsInBoardRange(tiledX, tiledY))
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
        public Vector2 ConventionalBoardPositionToWorld(string boardPosition) 
        {
            if (boardPosition.Length != 2)
                throw new ArgumentOutOfRangeException();

            int x = BoardPositionXToUnit(boardPosition.First());
            int y = BoardPositionYToUnit(boardPosition.Last());

            Vector2 origin = Position + new Vector2(0, 7) * SquareSize;

            Vector2 result = origin + new Vector2(x, -y) * SquareSize;
            return result;
        }
    }
}
