using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine.Graphics
{
    public class Tilemap : Graphic
    {
        public int tileWidth { get; set; }
        public int tileHeight { get; set; }
        public int columns { get; set; }
        public int rows { get; set; }

        public Tileset tileset;
        int[,] tiles;

        public Tilemap(int width, int height, int tileWidth, int tileHeight, Texture2D source)
        {
            this.width = width;
            this.height = height;

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;

            columns = width / tileWidth;
            rows = height / tileHeight;

            tileset = new Tileset(tileWidth, tileHeight, source);

            tiles = new int[columns, rows];
        }

        public void setTiles(int[,] src)
        {
            this.tiles = src;
        }

        public bool parseTiles(string[] src)
        {
            string[] val;
            int x = 0, y = 0;

            if (src.Length != rows)
                return false;

            foreach (string s in src)
            {
                // Val is a row of tiles
                val = s.Split(',');
                if (val.Length != columns)
                    return false;

                x = 0;

                foreach (string ss in val)
                {
                    tiles[x++, y] = int.Parse(ss);
                }
                y++;
            }

            return true;
        }

        override public void render(SpriteBatch sb, Vector2 position)
        {
            int x = (int) position.X;
            int y = (int) position.Y;

            Texture2D tex = tileset.texture;
            Color color = Color.White;
            for (int c = 0; c < columns; c++)
                for (int r = 0; r < rows; r++)
                    sb.Draw(tex, new Vector2(x + c * tileWidth, y + r * tileHeight), tileset.getTile(tiles[c, r]), color);
        }
    }

    public class Tileset
    {
        public Texture2D texture;
        public int width { get; set; }
        public int height { get; set; }
        public int tileWidth { get; set; }
        public int tileHeight { get; set; }
        public int columns { get; set; }
        public int rows { get; set; }
        
        public Tileset(int tileWidth, int tileHeight, Texture2D source)
        {
            texture = source;
            width = source.Width;
            height = source.Height;

            this.tileWidth = tileWidth;
            this.tileHeight = tileHeight;

            columns = width / tileWidth;
            rows = height / tileHeight;
        }

        public Rectangle getTile(int id)
        {
            Rectangle rect = new Rectangle((id % columns) * tileWidth,
                                           (id / columns) * tileHeight, 
                                           tileWidth, tileHeight);
            return rect;
        }
    }
}
