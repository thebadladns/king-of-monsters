using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BananaEngine;
using BananaEngine.Graphics;

using kom.Game.Puzzle;

namespace kom.Game
{
    class LevelMap : GameEntity
    {
        public String mapName;
        public Tilemap tilemap;
        public List<GameEntity> entities;

        public LevelMap(String fname) : base(0, 0)
        {
            entities = new List<GameEntity>();

            mapName = fname;
        }

        override public void init()
        {
            base.init();

            String filename = "Assets/" + mapName + ".oel";

            Stack<String> parseStack = new Stack<String>();

            int w = 0, h = 0;
            string tileset;
            string exportMode;
            string[] tiles = {""}, solids = {""};

            using (var stream = System.IO.File.OpenText(filename))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (parseStack.Count > 0 && parseStack.Peek() == "Entities")
                        {
                            GameEntity e = parseEntity(reader);
                            if (e != null)
                                entities.Add(e);
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
                                case "Solids":
                                    exportMode = reader.GetAttribute("exportMode");
                                    break;
                                case "Entities":
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
                            case "Solids":
                                v = reader.Value;
                                solids = v.Split('\n');
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

            tilemap = new Tilemap(w, h, 16, 16, game.Content.Load<Texture2D>("tilemap0"));
            tilemap.parseTiles(tiles);

            mask = new SolidGrid(w / 16, h / 16, 16, 16);
            mask.game = game;
            (mask as SolidGrid).parseSolids(solids);
        }

        public GameEntity parseEntity(XmlReader element)
        {
            GameEntity ge = null;

            // Fetch common attributes
            int id, x, y;
            id = int.Parse(element.GetAttribute("id"));
            x = int.Parse(element.GetAttribute("x"));
            y = int.Parse(element.GetAttribute("y"));

            // Create entitiy by element name
            switch (element.Name)
            {
                case "PlayerStart":
                    //ge = new Player(x, y);
                    break;
                case "TestEnemy":
                    ge = new Enemy(x, y);
                    break;
                case "Ladder":
                    // Fetch height
                    int height = int.Parse(element.GetAttribute("height"));
                    ge = new Stairs(x, y, 16, height);
                    break;
                case "Beast":
                    ge = new Beast(x, y);
                    break;
                case "PickableBlock":
                    ge = new PickableBlock(x, y);
                    break;
                case "OneWayPlatform":
                    ge = new OneWayPlatform(x, y);
                    break;
                case "ActivableDisplay":
                    string text = element.GetAttribute("text");
                    ge = new ActivableDisplay(x, y);
                    (ge as ActivableDisplay).text = text;
                    break;
                case "AreaTrigger":
                    int target = int.Parse(element.GetAttribute("targetId"));
                    int w = int.Parse(element.GetAttribute("width"));
                    int h = int.Parse(element.GetAttribute("height"));
                    ge = new AreaTrigger(x, y, w, h);
                    (ge as AreaTrigger).targetId = target;
                    break;
                case "BackDoor":
                    w = int.Parse(element.GetAttribute("width"));
                    h = int.Parse(element.GetAttribute("height"));
                    ge = new Door(x, y, w, h, Door.Location.Back);
                    break;
                case "FrontDoor":
                    w = int.Parse(element.GetAttribute("width"));
                    h = int.Parse(element.GetAttribute("height"));
                    ge = new Door(x, y, w, h, Door.Location.Front);
                    break;
                case "LevelEntrance":
                    int eid = int.Parse(element.GetAttribute("entranceId"));
                    ge = new LevelEntrance(x, y);
                    (ge as LevelEntrance).entranceId = eid;
                    break;
            }

            if (ge != null)
                ge.id = id;

            return ge;
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            tilemap.render(sb, pos);
            base.render(dt, sb);
        }
    }
}
