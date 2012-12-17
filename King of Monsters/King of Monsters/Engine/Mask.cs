using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine
{
    public class Mask
    {
        public Rectangle rect;
        public int offsetx, offsety;
        public Texture2D gfx;

        public int x
        {
            get { return rect.X; }
            set { rect.X = value + offsetx; }
        }
        
        public int y
        {
            get { return rect.Y; }
            set { rect.Y = value + offsety; }
        }

        public int w
        {
            get { return rect.Width; }
            set { rect.Width = value; }
        }

        public int h
        {
            get { return rect.Height; }
            set { rect.Height = value; }
        }

        protected KoM _game;
        public KoM game 
        { 
            get { return _game; } 
            set { _game = value; gfx = _game.Content.Load<Texture2D>("rect"); } 
        }

        public Mask(int x, int y, int w, int h, int offsetx = 0, int offsety = 0)
        {
            this.offsetx = offsetx;
            this.offsety = offsety;
            this.rect = new Rectangle(x + offsetx, y + offsety, w, h);
        }

        virtual public void update(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        virtual public bool collides(Mask other)
        {
            if (other is SolidGrid)
                return other.collides(this);
            else
                return rect.Intersects(other.rect);
        }

        virtual public void render(SpriteBatch sb)
        {
            sb.Draw(gfx, rect, rect, Color.Purple);
        }
    }
}
