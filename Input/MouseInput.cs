using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers.Input
{
    public class MouseInputEventArgs : EventArgs
    {
        public Vector2 Position;
        public Rectangle Hitbox;

        public MouseInputEventArgs(Vector2 position, Rectangle hitbox)
        {
            Position = position;
            Hitbox = hitbox;
        }
    }
    internal class MouseInput
    {
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        public float OffsetMarginX { get; set; } = 1f;
        public float OffsetMarginY { get; set; } = 1f;
        public Vector2 MousePosition { get => new Vector2(_currentMouseState.X / OffsetMarginX, _currentMouseState.Y / OffsetMarginY); }
        public event EventHandler<MouseInputEventArgs> OnLeftClick;
        public event EventHandler<MouseInputEventArgs> OnRightClick;
        public event EventHandler<MouseInputEventArgs> OnLeftButtonHeld;
        public event EventHandler<MouseInputEventArgs> OnLeftButtonReleased;


        private int _hitboxWidth = 4;
        private int _hitboxHeight = 4;


        public Rectangle Hitbox { get { return new Rectangle((int)MousePosition.X - _hitboxWidth / 2, (int)MousePosition.Y - _hitboxHeight / 2, _hitboxWidth, _hitboxHeight); } }

        public void Update()
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            if (LeftButtonReleased())
                return;
            LeftClicked();
            RightClicked();
            LeftButtonHeld();
        }
        public bool LeftButtonReleased()
        {
            if (_currentMouseState.LeftButton == ButtonState.Released)
                OnLeftButtonReleased?.Invoke(this, new MouseInputEventArgs(MousePosition, Hitbox));
            return false;
        }
        public bool LeftClicked()
        {
            if (_previousMouseState.LeftButton == ButtonState.Released && _currentMouseState.LeftButton == ButtonState.Pressed)
            {
                OnLeftClick?.Invoke(this, new MouseInputEventArgs(MousePosition, Hitbox));
                return true;
            }
            return false;
        }
        public bool LeftButtonHeld()
        {
            if (_previousMouseState.LeftButton != ButtonState.Pressed)
            {
                return false;

            }
            if (_currentMouseState.LeftButton != ButtonState.Pressed)
            {
                return false;
            }
            OnLeftButtonHeld?.Invoke(this, new MouseInputEventArgs(MousePosition, Hitbox));
            return true;
        }
        public bool RightClicked()
        {
            if (_previousMouseState.RightButton == ButtonState.Released && _currentMouseState.RightButton == ButtonState.Pressed)
            {
                OnRightClick?.Invoke(this, new MouseInputEventArgs(MousePosition, Hitbox));
                return true;
            }
            return false;
        }
    }
}
