using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;

namespace kom.Game.Puzzle
{
    public interface IActivable
    {
        bool isActive();
        void activate(bEntity by);
        void deactivate(bEntity by);
        void change(bEntity by);
        void onActivate(bEntity by);
        void onDeactivate(bEntity by);
    }

    public class AreaTrigger : bEntity
    {
        public int width, height;
        public int targetId = -1;
        public IActivable target;

        public AreaTrigger(int x, int y, int w, int h, IActivable target = null) : base(x, y)
        {
            this.width = w;
            this.height = h;
            this.target = target;
        }

        public override void init()
        {
            base.init();

            mask.w = width;
            mask.h = height;
        }

        public override void update()
        {
            base.update();

            if (target == null)
            {
                if (targetId > 0)
                {
                    bEntity e = world.find(targetId);
                    if (e is IActivable)
                        target = e as IActivable;
                    else return;
                }
            }

            if (placeMeeting(x, y, "player"))
                target.activate(null);
            else
                target.deactivate(null);
        }
    }

    public class ActivableDisplay : bEntity, IActivable
    {
        public bool active;
        public string text;

        public ActivableDisplay(int x, int y) : base(x, y)
        {
        }

        public override void init()
        {
            base.init();
            active = false;
        }

        public override void render(GameTime dt, Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.render(dt, sb);
            if (active)
                sb.DrawString(game.gameFont, text, pos, color);
        }

        public bool isActive()
        {
            return active;
        }

        public void activate(bEntity e)
        {
            active = true;
            onActivate(e);
        }

        public void deactivate(bEntity e)
        {
            active = false;
            onDeactivate(e);
        }

        public void change(bEntity e)
        {
            active = !active;
            if (active)
                onActivate(e);
            else
                onDeactivate(e);
        }

        public void onActivate(bEntity e)
        {
        }

        public void onDeactivate(bEntity e)
        {
        }
    }
}
