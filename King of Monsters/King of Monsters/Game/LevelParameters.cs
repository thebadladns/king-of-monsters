using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using bEngine;

namespace kom.Game
{
    public class LevelParameters
    {
        public int world;
        public int level;
        public string fname;

        public string name;
        public int layers;
        public Dictionary<int, string> layerFiles;
        public Dictionary<string, Pair<int, int>> entries;

        public int numEntries { get { return entries.Count; } }

        public LevelParameters(int world, int level)
        {
            this.world = world;
            this.level = level;
            fname = "Assets/w" + world + "l" + level + ".level";

            name = "";
            layers = 0;
            layerFiles = new Dictionary<int, string>();
            entries = new Dictionary<string, Pair<int, int>>();

            parseLevelFile();
        }

        protected void parseLevelFile()
        {
            Stack<String> parseStack = new Stack<String>();
            Pair<int, int> pair;

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
                            case "level":
                                name = reader.GetAttribute("name");
                                layers = int.Parse(reader.GetAttribute("layers"));
                                break;
                            case "layers":
                            case "entries":
                                break;
                            case "layer":
                                layerFiles.Add(int.Parse(reader.GetAttribute("id")), reader.GetAttribute("file"));
                                break;
                            case "entry":
                                pair = new Pair<int, int>(int.Parse(reader.GetAttribute("layer")), int.Parse(reader.GetAttribute("eid")));
                                entries.Add(reader.GetAttribute("id"), pair);
                                break;
                        }
                    }
                }
            }
        }
    }
}