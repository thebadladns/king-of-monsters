using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using kom.Engine;

namespace kom.Game.Puzzle
{
    public interface IActivable
    {
        bool isActive();
        void activate(GameEntity by);
        void deactivate(GameEntity by);
        void change(GameEntity by);
        void onActivate(GameEntity by);
        void onDeactivate(GameEntity by);
    }

    public class AreaTrigger : GameEntity
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
                    GameEntity e = world.find(targetId);
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

    public class ActivableDisplay : GameEntity, IActivable
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

        public void activate(GameEntity e)
        {
            active = true;
            onActivate(e);
        }

        public void deactivate(GameEntity e)
        {
            active = false;
            onDeactivate(e);
        }

        public void change(GameEntity e)
        {
            active = !active;
            if (active)
                onActivate(e);
            else
                onDeactivate(e);
        }

        public void onActivate(GameEntity e)
        {
        }

        public void onDeactivate(GameEntity e)
        {
        }
    }
}
