using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using kom.Engine;

namespace kom.Game
{
    public class WorldParameters
    {
        public int world;
        public string fname;

        public string name;
        public string mapfile;

        public WorldParameters(int world)
        {
            this.world = world;
            fname = "Assets/world" + world + ".world";

            name = "";
            mapfile = "";

            parseWorldFile();
        }

        protected void parseWorldFile()
        {
            Stack<String> parseStack = new Stack<String>();
            
            using (var stream = System.IO.File.OpenText(fname))
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        parseStack.Push(reader.Name);

                        switch (reader.Name)
                        {
                            case "leveworld":
                                name = reader.GetAttribute("name");
                                break;
                            case "map":
                                mapfile = reader.GetAttribute("file");
                                break;
                        }
                    }
                }
            }
        }
    }
}