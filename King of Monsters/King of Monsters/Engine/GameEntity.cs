using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine
{
    public class GameEntity
    {
        public int id;

        public Vector2 pos;
        public int x { get { return (int)pos.X; } set { pos.X = value; } }
        public int y { get { return (int)pos.Y; } set { pos.Y = value; } }

        public List<String> attributes;
        public Mask mask;
        public bool collidable;

        public Color color;
        public int layer;

        public KoM game;
        public GameState world;

        public GameInput input = KoM.input;

        public int[] timer;
        public static int NTIMERS = 5;

        public GameEntity(int x, int y)
        {
            pos = new Vector2(x, y);
            id = -1;
            layer = 0;
            mask = new Mask(0, 0, 0, 0);
            color = Color.White;
            collidable = true;
            attributes = new List<String>();
            timer = new int[5];
            for (int i = 0; i < NTIMERS; i++)
                timer[i] = -1;
        }

        virtual public void init()
        {
            mask.game = this.game;
        }

        virtual public void update()
        {
            mask.update(x, y);
            tick();
        }

        virtual public void render(GameTime dt, SpriteBatch sb)
        {
            if (KoM.DEBUG)
            {
                mask.render(sb);
                sb.DrawString(game.gameFont, ""+id, new Vector2(pos.X, pos.Y - 8), color);
            }
        }

        virtual public bool collides(GameEntity other)
        {
            return this.mask.collides(other.mask);
        }

        virtual public void onCollision(String type, GameEntity other)
        {
        }

        virtual public bool collides(String category)
        {
            String[] c = {category};
            return world.collides(this, c);
        }

        virtual public bool collides(String[] categories)
        {
            return world.collides(this, categories);
        }

        virtual public bool placeMeeting(int x, int y, String category, Func<GameEntity, GameEntity, bool> condition = null)
        {
            String[] c = { category };
            Vector2 old = this.pos;

            this.pos = new Vector2(x, y);
            mask.update((int)pos.X, (int)pos.Y);

            bool collision = world.collides(this, c, condition);

            this.pos = old;
            mask.update((int)pos.X, (int)pos.Y);

            return collision;
        }

        virtual public bool placeMeeting(Vector2 position, String category, Func<GameEntity, GameEntity, bool> condition = null)
        {
            String[] c = { category };
            Vector2 old = this.pos;

            this.pos = position;
            mask.update((int) pos.X, (int) pos.Y);

            bool collision = world.collides(this, c, condition);

            this.pos = old;
            mask.update((int)pos.X, (int)pos.Y);

            return collision;
        }

        public Vector2 moveToContact(Vector2 to, String category, Func<GameEntity, GameEntity, bool> condition = null)
        {
            Vector2 remnant = Vector2.Zero;

            to.X = (int) Math.Round(to.X);
            to.Y = (int) Math.Round(to.Y);

            // Move to contact in the X
            int s = Math.Sign(to.X - pos.X);
            bool found = false;
            Vector2 tp = pos;
            for (float i = to.X; i != pos.X; i -= s)
            {
                tp.X = i;
                if (!placeMeeting(tp, category, condition))
                {
                    found = true;
                    break;
                }
            }

            if (found)
                pos.X = tp.X;

            // Move to contact in the Y
            s = Math.Sign(to.Y - pos.Y);
            found = false;
            tp = pos;
            for (float i = to.Y; i != pos.Y; i -= s)
            {
                tp.Y = i;
                if (!placeMeeting(tp, category, condition))
                {
                    found = true;
                    break;
                }
            }

            if (found)
                pos.Y = tp.Y;

            remnant = to - pos;
            return remnant;
        }

        virtual public GameEntity instancePlace(int x, int y, String category, String attr = null, Func<GameEntity, GameEntity, bool> condition = null)
        {
            return instancePlace(new Vector2(x, y), category, attr, condition);
        }

        virtual public GameEntity instancePlace(Vector2 position, String category, String attr = null, Func<GameEntity, GameEntity, bool> condition = null)
        {
            Vector2 old = this.pos;

            this.pos = position;
            mask.update(x, y);

            GameEntity e = world.instanceCollision(this, category, attr, condition);

            this.pos = old;
            mask.update(x, y);

            return e;
        }

        virtual public bool hasAttribute(String attr)
        {
            return attributes.Contains(attr);
        }

        virtual public void tick()
        {
            for (int i = 0; i < NTIMERS; i++)
                if (timer[i] >= 0)
                {
                    timer[i]--;
                    if (timer[i] < 0)
                        onTimer(i);
                }
        }

        virtual public void onTimer(int n)
        {
        }

        virtual public bool isInView()
        {
            return world.isInstanceInView(this);
        }
    }

    public interface IPausable
    {
        bool isPaused();
    }
}
