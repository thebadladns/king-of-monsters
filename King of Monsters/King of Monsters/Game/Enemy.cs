using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using kom.Engine;
using kom.Engine.Graphics;

namespace kom.Game
{
    class Enemy : GameEntity
    {
        Stamp graphic;

        public Enemy(int x, int y) : base(x, y)
        {
        }

        override public void init()
        {
            base.init();
            graphic = new Stamp(game.Content.Load<Texture2D>("monster"));

            attributes.Add("enemy");
            mask = new Mask(x, y, 16, 24, 0, 2);
            mask.game = game;
        }

        public override void update()
        {
            color = Color.White;
            base.update();
        }

        override public void onCollision(string type, GameEntity other)
        {
            if (type == "weapon")
            {
                world.remove(this);
            }
            else if (type == "enemy")
                color = Color.Green;
            else if (type == "solid")
                color = Color.Violet;
            else if (type == "player")
            {
                color = Color.Turquoise;
                color.A = 100;
            }
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.color = color;
            graphic.render(sb, pos);
        }
    }
}
