using Checkers.GameBoard;
using Checkers.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Checkers
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private CheckersBoard _board;
        private RenderTarget2D _renderTarget;
        private MouseInput _mouseInput;


        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _mouseInput = new MouseInput();
        }

        protected override void Initialize()
        {

            _graphics.PreferredBackBufferHeight = 860;
            _graphics.PreferredBackBufferWidth = 640;

            _graphics.ApplyChanges();
            _mouseInput.OffsetMarginX = 860 / 430;
            _mouseInput.OffsetMarginY = 640 / 320;

            _renderTarget = new RenderTarget2D(GraphicsDevice, 430, 320);

            _board = new CheckersBoard(CheckerColor.White, _renderTarget);
            _board.Init(_mouseInput);
            _board.Setup();
            
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _board.LoadContent(GraphicsDevice, Content);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _mouseInput.Update();
            _board.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.DarkSlateBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, sortMode:SpriteSortMode.FrontToBack);

            _board.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.DarkSlateBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            _spriteBatch.Draw(_renderTarget, new Rectangle(0, 0, 860, 640), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}