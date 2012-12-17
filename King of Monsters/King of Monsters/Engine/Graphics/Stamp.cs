using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine.Graphics
{
    public class Stamp : Graphic
    {
        protected Texture2D image;

        public Stamp(Texture2D image)
        {
            this.image = image;
            width = image.Width;
            height = image.Height;
        }

        override public void render(SpriteBatch sb, Vector2 position)
        {
            sb.Draw(image, position, color);
        }
    }
}
