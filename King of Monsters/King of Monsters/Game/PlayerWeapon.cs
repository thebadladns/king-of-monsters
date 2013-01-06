using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace kom.Game
{
    public class PlayerWeapon : bEntity, IPausable, IListener
    {
        public float hspeed;
        public float vspeed;
        public float gravity;

        public bSpritemap graphic;

        public PlayerWeapon(int x, int y) : base(x, y)
        {
            hspeed = 0;
            vspeed = 0;
        }

        public override void init()
        {
            base.init();
            gravity = 0.2f;
            graphic = new bSpritemap(game.Content.Load<Texture2D>("weapons"), 8, 8);
            int[] f = {0, 1};
            graphic.add(new bAnim("bone", f, 0.1f));
            graphic.play("bone");
            mask.w = 8;
            mask.h = 8;
        }

        public override void update()
        {
            if (isPaused())
                return;

            Vector2 moveTo = pos;

            vspeed += gravity;

            moveTo.X += hspeed;
            moveTo.Y += vspeed;

            Vector2 r = moveToContact(moveTo, "solid");
            if (r.Length() != 0)
                world.remove(this);

            if (!isInView())
                world.remove(this);

            graphic.update();
            base.update();
        }

        public override void onCollision(string type, bEntity other)
        {
            if (type == "solid" || type == "enemy")
                world.remove(this);
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.render(sb, pos);
        }

        public bool isPaused()
        {
            return (world as GameLevel).isPaused();
        }

        public void onEvent(String ev)
        {
            if (ev == "layerChange")
                world.remove(this);
        }
    }
}
