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
        private Dictionary<CheckerPiece, Dictionary<BoardPosition, MoveType>> _possibleMoves;
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
            _possibleMoves = new Dictionary<CheckerPiece, Dictionary<BoardPosition, MoveType>>();
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
            Dictionary<BoardPosition, MoveType> possiblePieceMoves = _possibleMoves[piece];
            BoardPosition movePosition = _selectedSquare.BoardPosition;

            if (possiblePieceMoves.Count == 0)
                return;

            if (!possiblePieceMoves.ContainsKey(movePosition))
                return;

            MoveType moveType = possiblePieceMoves[movePosition];

            if (moveType == MoveType.Jump)
                DestroyPiecesOnJump(piece.GetSquarePosition(), movePosition);

            piece.MoveTo(this, _selectedSquare);

            // if piece's move was jump recalculate for more possible jumps 

            if(moveType == MoveType.Jump) 
            { 
                Dictionary<BoardPosition, MoveType> newMovesForTheJumpedPiece = piece.CalculatePossibleMoves(this);

                var onlyJumps = from move in newMovesForTheJumpedPiece.Keys where newMovesForTheJumpedPiece[move] == MoveType.Jump select move; 

                if(onlyJumps.Count() > 0) 
                {
                    //clean all moves for pieces
                    //assign only jumps to piece things
                    foreach(var cPiece in _possibleMoves.Keys) 
                    {
                        _possibleMoves[cPiece] = new Dictionary<BoardPosition, MoveType>(PositionEqualityComparer);
                    }
                    foreach(var move in onlyJumps) 
                    {
                        _possibleMoves[piece].Add(move, MoveType.Jump);
                    }
                    return;
                }
            }
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
        private void RecalculateMovesForPieces() 
        {
            _possibleMoves.Clear();
            foreach(CheckerPiece piece in _checkers) 
            {
                Dictionary<BoardPosition, MoveType> moves = piece.CalculatePossibleMoves(this);
                _possibleMoves.Add(piece, moves);
            }
        }
        private void DestroyPiecesOnJump(BoardPosition piecePosition, BoardPosition landingPosition) 
        {
            int movingDirectionX = landingPosition.X - piecePosition.X;
            int distance = Math.Abs(movingDirectionX);
            movingDirectionX = movingDirectionX / Math.Abs(movingDirectionX);
            int movingDirectionY = landingPosition.Y - piecePosition.Y;
            movingDirectionY = movingDirectionY / Math.Abs(movingDirectionY);

            Vector2 movingDirection = new Vector2(movingDirectionX, movingDirectionY);

            List<CheckerPiece> piecesToDestroy = new List<CheckerPiece>();
            for(int range = 1; range < distance; range++) 
            {
                Vector2 currentMovingDirection = movingDirection * range;
                int x = piecePosition.X + (int)currentMovingDirection.X;
                int y = piecePosition.Y + (int)currentMovingDirection.Y;

                BoardPosition currentBoardPosition = new BoardPosition(x, y);

                Square currentSquare = Squares[currentBoardPosition];
                if (!currentSquare.IsEmpty())
                    piecesToDestroy.Add(currentSquare.GetPiece() as CheckerPiece);
            }
            foreach(var piece in piecesToDestroy) 
            {
                piece.Destroy();
            }
        }
        private bool DestroyPieceOnJumpoverOld(CheckerPiece piece, Square pieceSquare, Square targetSquare) 
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
        public bool IsCurrentPlayerPiece(CheckerPiece checkerPiece) 
        {
            return CurrentPlayer == checkerPiece.PieceColor;
        }
        public override void Draw(SpriteBatch spriteBatch) 
        {
            //base.Draw(spriteBatch);

            spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(_boardTexture, Position, null, Color.White, 0f, new Vector2(0, 0), 1f, SpriteEffects.None, 0.1f);

            spriteBatch.DrawString(_debugFont, _debugText, _origin, Color.White);
            //spriteBatch.Draw(_mountTexture, Position - new Vector2(3, 9), Color.White);
            //spriteBatch.Draw(_boardTexture, Position, Color.White);
            foreach (CheckerPiece checker in _checkers)
                checker.Draw(spriteBatch);
            if (_selectedSquare != null && !_selectedSquare.IsEmpty()) 
            {
                CheckerPiece piece = _selectedSquare.GetPiece() as CheckerPiece;
                if (piece.PieceColor != CurrentPlayer)
                    return;

                if (!_possibleMoves.ContainsKey(piece))
                    return;

                Dictionary<BoardPosition, MoveType> moves = _possibleMoves[piece];
                if(moves.Count > 0) 
                {
                    foreach (var move in moves) 
                    {
                        spriteBatch.Draw(_moveSelectionTexture, BoardPositionToWorld(move.Key), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.3f);
                    }
                }
            }     
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
