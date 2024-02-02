using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Engine
{
    internal abstract class GameObject
    {
        private float _x, _y;
        private int _width, _height;

        public Vector2 Position { get { return new Vector2(_x, _y); } set { _x = value.X; _y = value.Y; } }
        public Rectangle Bounds
        {
            get { return new Rectangle((int)_x, (int)_y, _width, _height); }
            set { _x = value.X; _y = value.Y; _width = value.Width; _height = value.Height; }
        }
        public Texture2D DebugTexture { get; private set; }
        public Color DebugDrawColor { get; set; } = Color.White;
        public bool IsVisible { get; set; } = true;
        //public GameObject(float x, float y, int width, int height)
        //{
        //    _x = x;
        //    _y = y;
        //    _width = width;
        //    _height = height;
        //}
        public virtual void Update(GameTime gameTime)
        {

        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible) return;
            spriteBatch.Draw(DebugTexture, Bounds, DebugDrawColor);
        }
        public virtual void LoadContent(GraphicsDevice graphicsDevice, ContentManager content)
        {
            DebugTexture = new Texture2D(graphicsDevice, 1, 1);
            DebugTexture.SetData(new Color[] { Color.White });
        }
        public virtual void Destroy()
        {

        }
    }
}
