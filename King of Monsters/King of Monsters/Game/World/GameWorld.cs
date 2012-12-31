using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BananaEngine;

using kom.Game.Data;

namespace kom.Game
{
    class GameWorld : GameState
    {
        WorldParameters parameters;
        
        Color bgColor;
        WorldMap map;

        Camera2d camera;

        LevelNode currentNode, nextNode;
        int nextNodePosition;
        PlayerLocationMarker playerMaker;

        GameDataManager data;

        public GameWorld(KoM game) : base()
        {
            this.game = game;
            data = game.dataManager;
            parameters = new WorldParameters(0);
        }

        protected override bool _add(GameEntity e, string category)
        {
            entities[category].Add(e);

            return base._add(e, category);
        }

        override public void init()
        {
            base.init();

            entities.Clear();
            entities.Add("solid", new List<GameEntity>());
            entities.Add("nodes", new List<GameEntity>());

            map = new WorldMap(parameters.mapfile);
            _add(map, "solid");

            foreach (LevelNode n in map.nodes)
            {
                _add(n, "nodes");
            }

            foreach (LevelNode n in entities["nodes"])
                n.setData();

            currentNode = entities["nodes"][data.state.currentNode] as LevelNode;
            nextNode = null;
            nextNodePosition = 0;

            bgColor = Color.Black;

            playerMaker = new PlayerLocationMarker(0, 0);
            _add(playerMaker, "nodes");
            playerMaker.placeAt(currentNode);

            camera = new Camera2d(game.graphicsDevice);
            camera.Pos = new Vector2(128, 120);

            (game as KoM).dataManager.state.currentWorld = 0;
            (game as KoM).dataManager.state.currentNode = currentNode.id;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            List<LevelNode> available = currentNode.links;
            nextNode = available[nextNodePosition];
            playerMaker.placeAt(currentNode);
            if (KoM.input.pressed(Microsoft.Xna.Framework.Input.Buttons.B))
                nextNodePosition = (nextNodePosition + 1) % available.Count;
            else if (KoM.input.pressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                currentNode = nextNode;
                nextNodePosition = 0;
                (game as KoM).dataManager.state.currentNode = currentNode.id;
            }
            else if (KoM.input.pressed(Microsoft.Xna.Framework.Input.Buttons.Start))
            {
                (game as KoM).goToLevel(0, currentNode.level, currentNode.entrance);
            }

            if (KoM.input.pressed(Microsoft.Xna.Framework.Input.Keys.S))
                data.saveGame();

            foreach (GameEntity ge in entities["solid"])
                ge.update();
            foreach (GameEntity ge in entities["nodes"])
                ge.update();
        }

        public override void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            base.render(dt, sb, matrix);

            matrix *= camera.get_transformation();

            sb.Begin(SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    SamplerState.PointClamp,
                    null,
                    RasterizerState.CullCounterClockwise,
                    null,
                    matrix);

            game.GraphicsDevice.Clear(bgColor);

            currentNode.graphic.color = Color.BlueViolet;
            if (nextNode != null)
                nextNode.graphic.color = Color.GreenYellow;

            foreach (GameEntity ge in entities["solid"])
                ge.render(dt, sb);
            foreach (GameEntity ge in entities["nodes"])
                ge.render(dt, sb);

            currentNode.graphic.color = Color.White;
            if (nextNode != null)
                nextNode.graphic.color = Color.White;

            sb.DrawString(game.gameFont, data.state.playerName, Vector2.Zero, Color.White);
            sb.DrawString(game.gameFont, data.state.currentNode.ToString(), new Vector2(0, 16), Color.White);
        }
    }
}
