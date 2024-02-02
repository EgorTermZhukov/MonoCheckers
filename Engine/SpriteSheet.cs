using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Engine
{
    internal class SpriteSheet
    {
        private readonly Texture2D _texture;
        private Vector2 _scaleOne = Vector2.One;

        private int _rows, _columns;
        private int _frameWidth, _frameHeight;

        public SpriteSheet(Texture2D texture, int frameWidth, int frameHeight)
        {
            _texture = texture;

            _frameWidth = frameWidth;
            _frameHeight = frameHeight;

            _columns = _texture.Width / frameWidth;
            _rows = _texture.Height / frameHeight;
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float depth, int frame, Color color)
        {
            if (frame < 0 || frame >= _columns * _rows)
                throw new ArgumentOutOfRangeException($"{frame} is out of range!");

            var column = frame % _columns;
            var row = frame / _columns;
            var x = column * _frameWidth;
            var y = row * _frameHeight;

            spriteBatch.Draw(_texture,
                             position,
                             new Rectangle(x, y, _frameWidth, _frameHeight),
                             color,
                             0f,
                             Vector2.Zero,
                             new Vector2(1, 1),
                             SpriteEffects.None,
                             depth);
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 position, float depth, int frame, Color color, Vector2 scale, Vector2 origin)
        {
            if (frame < 0 || frame >= _columns * _rows)
                throw new ArgumentOutOfRangeException($"{frame} is out of range!");

            var column = frame % _columns;
            var row = frame / _columns;
            var x = column * _frameWidth;
            var y = row * _frameHeight;

            spriteBatch.Draw(_texture,
                             position,
                             new Rectangle(x, y, _frameWidth, _frameHeight),
                             color,
                             0f,
                             origin,
                             scale,
                             SpriteEffects.None,
                             depth);
        }
    }
}
