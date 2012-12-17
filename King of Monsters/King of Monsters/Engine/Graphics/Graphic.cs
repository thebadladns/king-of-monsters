using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine.Graphics
{
    public class Graphic
    {
        public int width = 0, height = 0;
        public Color color = Color.White;

        virtual public void render(SpriteBatch sb, int x, int y)
        {
            render(sb, new Vector2(x, y));
        }

        virtual public void render(SpriteBatch sb, Vector2 position)
        {
        }
    }
}
