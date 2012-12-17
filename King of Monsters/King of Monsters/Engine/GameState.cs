using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine
{
    public class GameState
    {
        public KoM game;

        protected Dictionary<string, List<GameEntity>> entities;
        protected Dictionary<GameEntity, string> categories;
        protected List<GameEntity> deathRow;
        protected List<Pair<GameEntity, String>> birthRow;

        public GameState()
        {
            entities = new Dictionary<string, List<GameEntity>>();
            categories = new Dictionary<GameEntity, string>();
            deathRow = new List<GameEntity>();
            birthRow = new List<Pair<GameEntity, String>>();
        }

        virtual public void init()
        {
            Console.WriteLine("GameLevel init!");
        }

        virtual public void end()
        {
            Console.WriteLine("GameLevel end!");
        }

        virtual public void update(GameTime dt)
        {
        }

        virtual public void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
        }

        virtual public bool add(GameEntity ge, String category)
        {
            birthRow.Add(new Pair<GameEntity, String>(ge, category));
            return true;
        }

        virtual protected bool _add(GameEntity e, String category)
        {
            // Store container list
            categories[e] = category;

            e.world = this;
            e.game = this.game;
            e.init();
            return true;
        }

        virtual public void remove(GameEntity e)
        {
            deathRow.Add(e);
        }

        virtual public void actuallyRemove(GameEntity e)
        {
            String c = categories[e];
            if (c != null)
                entities[c].Remove(e);
        }

        virtual public bool collides(GameEntity e, string[] categories, Func<GameEntity, GameEntity, bool> condition = null)
        {
            foreach (string category in categories)
                if (entities[category] != null)
                    foreach (GameEntity ge in entities[category])
                        if (ge != e && ge.collidable && e.collides(ge) && (condition == null || condition(e, ge)))
                            return true;
            return false;
        }

        virtual public GameEntity instanceCollision(GameEntity e, string category, string attr = null, Func<GameEntity, GameEntity, bool> condition = null)
        {
            if (entities[category] != null)
                foreach (GameEntity ge in entities[category])
                    if (ge != e && ge.collidable && e.collides(ge) && (condition == null || condition(e, ge)))
                        if (attr == null || ge.hasAttribute(attr))
                            return ge;
            return null;
        }

        virtual public List<GameEntity> instancesCollision(GameEntity e, string category, string attr = null)
        {
            List<GameEntity> result = new List<GameEntity>();

            if (entities[category] != null)
                foreach (GameEntity ge in entities[category])
                    if (ge != e && ge.collidable && e.collides(ge))
                        if (attr == null || ge.hasAttribute(attr))
                            result.Add(ge);

            return result;
        }

        virtual public GameEntity find(int id)
        {
            foreach (String key in entities.Keys)
            {
                foreach (GameEntity e in entities[key])
                {
                    if (e.id == id)
                        return e;
                }
            }

            return null;
        }

        virtual public int instanceNumber(Type target)
        {
            int count = 0;
            foreach (String key in entities.Keys)
            {
                foreach (GameEntity e in entities[key])
                    if (target.IsInstanceOfType(e))
                        count += 1;
            }
            return count;
        }

        virtual public bool isInstanceInView(GameEntity e)
        {
            // TODO: Handle invisible and not managed by world entities
            return true;
        }
    }

    public class Pair<T1, T2>
    {
        public T1 first;
        public T2 second;

        public Pair(T1 a, T2 b)
        {
            first = a; second = b;
        }
    }
}
