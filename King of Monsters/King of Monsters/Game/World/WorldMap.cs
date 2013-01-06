using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using bEngine;
using bEngine.Graphics;

namespace kom.Game
{
    class WorldMap : bEntity
    {
        public String mapName;
        public bTilemap tilemap;
        public List<LevelNode> nodes;

        public WorldMap(String fname)
            : base(0, 0)
        {
            nodes = new List<LevelNode>();
            mapName = fname;
        }

        public override void init()
        {
            base.init();
            String fname = "Assets/" + mapName + ".oel";

            Stack<String> parseStack = new Stack<String>();

            int w = 0, h = 0;
            string tileset = "";
            string[] tiles = { "" };

            using (var stream = System.IO.File.OpenText(fname))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (parseStack.Count > 0 && parseStack.Peek() == "Nodes")
                        {
                            LevelNode e = parseNode(reader);
                            if (e != null)
                                nodes.Add(e);
                        }
                        else
                        {
                            parseStack.Push(reader.Name);

                            switch (reader.Name)
                            {
                                case "level":
                                    w = int.Parse(reader.GetAttribute("width"));
                                    h = int.Parse(reader.GetAttribute("height"));
                                    break;
                                case "Tiles":
                                    tileset = reader.GetAttribute("tileset");
                                    break;
                                case "Nodes":
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        String current = parseStack.Pop();
                        switch (current)
                        {
                            case "level":
                                break;
                            case "Tiles":
                                string v = reader.Value;
                                tiles = v.Split('\n');
                                break;
                        }
                        parseStack.Push(current);
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        parseStack.Pop();
                    }
                }
            }

            tilemap = new bTilemap(w, h, 8, 8, game.Content.Load<Texture2D>(tileset));
            tilemap.parseTiles(tiles);
        }

        protected LevelNode parseNode(XmlReader element)
        {
            LevelNode node = null;

            // Fetch common attributes
            int id, x, y;
            id = int.Parse(element.GetAttribute("id"));
            x = int.Parse(element.GetAttribute("x"));
            y = int.Parse(element.GetAttribute("y"));

            // Create entitiy by element name
            switch (element.Name)
            {
                case "LevelNode":
                    node = new LevelNode(x, y, element.GetAttribute("links"), element.GetAttribute("level"));
                    node.id = id;
                    break;
            }

            return node;
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            tilemap.render(sb, pos);
            base.render(dt, sb);
        }
    }
}
