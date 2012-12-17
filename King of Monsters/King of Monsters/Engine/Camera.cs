using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine
{
    public class Camera2d
    {
        protected float _zoom; // Camera Zoom
        public Matrix _transform; // Matrix Transform
        public Vector2 _pos; // Camera Position
        protected float _rotation; // Camera Rotation

        public Rectangle viewRectangle
        {
            get
            {
                return new Rectangle((int)(Pos.X - (graphicsDevice.Viewport.Width * 0.5f / _zoom)),
                                       (int)(Pos.Y - (graphicsDevice.Viewport.Height * 0.5f / _zoom)),
                                       (int) (graphicsDevice.Viewport.Width / _zoom), 
                                       (int) (graphicsDevice.Viewport.Height / _zoom));
            }
        }

        public Rectangle bounds;
        public GraphicsDevice graphicsDevice;

        public Camera2d(GraphicsDevice gd)
        {
            _zoom = 3.0f;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
            bounds = new Rectangle(0, 0, -1, -1);
            graphicsDevice = gd;
        }

        // Sets and gets zoom
        public float Zoom
        {
            get { return _zoom; }
            set { _zoom = value; if (_zoom < 0.1f) _zoom = 0.1f; } // Negative zoom will flip image
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            _pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return _pos; }
            set 
            {
                // Check inbounds
                var p = value;
                if (bounds.Width > 0 && bounds.Height > 0)
                {
                    p.X = Math.Min(Math.Max(bounds.Left + (graphicsDevice.Viewport.Width * 0.5f / _zoom),
                               p.X),
                               bounds.Right - (graphicsDevice.Viewport.Width * 0.5f / _zoom));
                    p.Y = Math.Min(Math.Max(bounds.Top + (graphicsDevice.Viewport.Height * 0.5f / _zoom),
                               p.Y),
                               bounds.Bottom - (graphicsDevice.Viewport.Height * 0.5f / _zoom));
                    _pos = p;
                }
                else
                {
                    _pos = value;
                }
            }
        }

        public Matrix get_transformation()
        {
            _transform =
                Matrix.CreateTranslation(
                new Vector3((graphicsDevice.Viewport.Width * 0.5f) - (_pos.X * _zoom),
                                 (graphicsDevice.Viewport.Height * 0.5f) - (_pos.Y * _zoom),
                                  0));
            return _transform;
        }
    }
}
