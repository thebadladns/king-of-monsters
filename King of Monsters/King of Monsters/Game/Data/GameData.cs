using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

using Microsoft.Xna.Framework.Storage;

namespace kom.Game.Data
{
    public class GameDataManager
    {
        public GameData state;

        public GameDataManager()
        {
            state.playerName = null;
            state.currentWorld = -1;
            state.currentNode = -1;
            state.enemiesDefeated = -1;
        }

        public void startNewGame()
        {
            string[] a = {"Dr. Sparkles", "Maximum Sugar", "Fish Ramirez"};
            state.playerName = a[(new Random()).Next(a.Length)];
            state.currentWorld = 0;
            state.currentNode = 0;
            state.enemiesDefeated = 0;
        }

        public void saveGame()
        {
            IAsyncResult r = StorageDevice.BeginShowSelector(Microsoft.Xna.Framework.PlayerIndex.One, null, null);
            r.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(r);

            IAsyncResult result = device.BeginOpenContainer("KoM-Storage", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";
            if (container.FileExists(filename))
                container.DeleteFile(filename);
            Stream stream = container.CreateFile(filename);

            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            serializer.Serialize(stream, state);
            stream.Close();
            container.Dispose();
        }

        public bool loadGame()
        {
            IAsyncResult r = StorageDevice.BeginShowSelector(Microsoft.Xna.Framework.PlayerIndex.One, null, null);
            r.AsyncWaitHandle.WaitOne();
            StorageDevice device = StorageDevice.EndShowSelector(r);

            IAsyncResult result = device.BeginOpenContainer("KoM-Storage", null, null);
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";
            if (!container.FileExists(filename))
            {
                container.Dispose();
                return false;
            }

            Stream stream = container.OpenFile(filename, FileMode.Open);
            XmlSerializer serializer = new XmlSerializer(typeof(GameData));
            GameData tempState = (GameData)serializer.Deserialize(stream);
            stream.Close();
            container.Dispose();

            state = tempState;

            return true;
        }
    }

    public struct GameData
    {
        public string playerName;
        public int currentWorld;
        public int currentNode;
        public int enemiesDefeated;
    }
}
