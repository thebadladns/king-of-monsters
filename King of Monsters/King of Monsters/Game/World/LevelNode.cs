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
    public class LevelNode : GameEntity
    {
        public String linksStr, levelStr;
        public List<LevelNode> links;
        public int level;
        public int entrance;

        public Stamp graphic;

        public LevelNode(int x, int y, String linksString, String levelString)
            : base(x, y)
        {
            links = new List<LevelNode>();
            level = -1;
            entrance = -1;

            linksStr = linksString;
            levelStr = levelString;
        }

        public override void init()
        {
            base.init();

            graphic = new Stamp(game.Content.Load<Texture2D>("node"));
        }

        public override void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);

            graphic.render(sb, pos);
        }

        public void setData(String linksStr = null, String levelStr = null)
        {
            if (linksStr == null)
                linksStr = this.linksStr;
            if (levelStr == null)
                levelStr = this.levelStr;

            String[] ls = linksStr.Split(',');
            foreach (String l in ls)
            {
                int lid = int.Parse(l);
                LevelNode n = (world.find(lid) as LevelNode);
                if (n != null)
                    links.Add(n);
                else
                    Console.WriteLine("WHAT!");
            }

            string[] strs = levelStr.Split('-');
            level = int.Parse(strs[0]);
            entrance = int.Parse(strs[1]);
        }
    }

    public class PlayerLocationMarker : GameEntity
    {
        Spritemap graphic;

        public PlayerLocationMarker(int x, int y)
            : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            graphic = new Spritemap(game.Content.Load<Texture2D>("worldchars"), 8, 8);
            int[] f = {0, 1};
            graphic.add(new Anim("walk", f, 0.05f));
            graphic.play("walk");
        }

        public override void update()
        {
            base.update();

            graphic.update();
        }

        public override void  render(GameTime dt, SpriteBatch sb)
        {
 	        base.render(dt, sb);

            graphic.render(sb, pos);
        }

        public void placeAt(LevelNode node)
        {
            this.x = node.x + (node.graphic.width / 2) - graphic.spriteWidth / 2;
            this.y = node.y + (node.graphic.height / 2) - graphic.spriteHeight / 2;
        }
    }
}
