using Checkers.GameBoard;
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

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _board = new CheckersBoard(Player.First);
        }

        protected override void Initialize()
        {
            _board.Setup();

            _graphics.PreferredBackBufferHeight = 860;
            _graphics.PreferredBackBufferWidth = 640;

            _graphics.ApplyChanges();

            _renderTarget = new RenderTarget2D(GraphicsDevice, 430, 320);

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

            _board.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.DarkSlateBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            _board.Draw(_spriteBatch);

            _spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            GraphicsDevice.Clear(Color.DarkSlateBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            _spriteBatch.Draw(_renderTarget, new Rectangle(-64, 16, 860, 640), Color.White);

            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}