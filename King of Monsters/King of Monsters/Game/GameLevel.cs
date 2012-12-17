using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using kom.Engine;
using kom.Engine.Graphics;

using kom.Game.Puzzle;

namespace kom.Game
{
    class GameLevel : GameState, IPausable
    {
        LevelParameters parameters;
        public int currentEntrance;

        Color bgColor;

        GameEntity player;
        LevelMap map;

        public Camera2d camera;

        public int currentLayer;
        List<LevelMap> layerMap;
        List<Dictionary<String, List<GameEntity>>> layers;
        Dictionary<String, List<GameEntity>> commonEntities;

        bool paused;

        int timer;
        int toLayer;
        Fader transitionFader;

        Stamp sprPoint;

        public enum State { Init, Gameplay, Pause, Transition };
        public State state;

        public GameLevel(int world, int level)
            : base()
        {
            parameters = new LevelParameters(world, level);
        }

        override public void init()
        {
            base.init();

            timer = -1;
            paused = false;

            state = State.Init;

            // Init entities containers
            // Each layer contains its own dictionary of entities by type
            layers = new List<Dictionary<String, List<GameEntity>>>();
            // Each layer has its own map
            layerMap = new List<LevelMap>();
            // Common entitites hold inter-layer entities
            commonEntities = new Dictionary<string, List<GameEntity>>();
            // Player related entities are inter-layered
            commonEntities.Add("player", new List<GameEntity>());

            // Fetch from other source
            int LAYERS = parameters.layers;
            for (int i = 0; i < LAYERS; i++)
            {
                Dictionary<String, List<GameEntity>> d = new Dictionary<String, List<GameEntity>>();
                d.Add("enemy", new List<GameEntity>());
                d.Add("solid", new List<GameEntity>());
                d.Add("stairs", new List<GameEntity>());
                d.Add("weapon", new List<GameEntity>());
                d.Add("onewaysolid", new List<GameEntity>());
                d.Add("misc", new List<GameEntity>());
                d.Add("entries", new List<GameEntity>());
                layers.Add(d);
            }

            Random r = new Random();
            bgColor = new Color(r.Next(256), r.Next(256), r.Next(256));

            for (int i = 0; i < LAYERS; i++)
            {
                // Load layer data
                switchToLayer(i, true);

                map = new LevelMap(parameters.layerFiles[i]);
                _add(map, "solid");

                layerMap.Add(map);

                handleEntities(map.entities);
            }

            // switchToLayer(0);

            // find current entrance and create player
            bool done = false;
            int layer = -1;
            foreach (Dictionary<string, List<GameEntity>> es in layers)
            {
                layer += 1;
                foreach (String k in es.Keys)
                {
                    foreach (GameEntity e in es[k])
                    {
                        if (e is LevelEntrance)
                        {
                            if ((e as LevelEntrance).entranceId == currentEntrance)
                            {
                                switchToLayer(layer);
                                _add(new Player(e.x, e.y), "player");
                                done = true;
                                break;
                            }
                        }
                    }
                    if (done)
                        break;
                }
                if (done)
                    break;
            }

            camera = new Camera2d(game.graphicsDevice);
            camera.bounds = new Rectangle(map.x, map.y, map.tilemap.width, map.tilemap.height);

            sprPoint = new Stamp(game.Content.Load<Texture2D>("rect"));

            state = State.Gameplay;
        }

        public override void update(GameTime dt)
        {
            base.update(dt);

            if (timer >= 0)
            {
                if (--timer < 0)
                    this.onTimer();
            }

            if (KoM.input.pressed(Keys.D0))
                switchToLayer(0);
            else if (KoM.input.pressed(Keys.D1))
                switchToLayer(1);

            /* Update */
            foreach (GameEntity ge in entities["solid"])
                ge.update();
            foreach (GameEntity e in entities["onewaysolid"])
                e.update();
            foreach (GameEntity ge in entities["enemy"])
                ge.update();
            foreach (GameEntity p in entities["player"])
                p.update();
            foreach (GameEntity w in entities["weapon"])
                w.update();
            foreach (GameEntity m in entities["misc"])
                m.update();
            foreach (GameEntity m in entities["entries"])
                m.update();

            /* Collisions */
            // Player vs solids
            /*foreach (GameEntity gs in entities["solid"])
                if (player.collides(gs))
                {
                    player.onCollision("solid", gs);
                    gs.onCollision("player", player);
                }*/
            // Enemies
            //  vs player
            //  vs solid
            //  vs enemy
            foreach (GameEntity ge in entities["enemy"])
            {
                if (player.collides(ge))
                {
                    player.onCollision("enemy", ge);
                    ge.onCollision("player", player);
                }

                foreach (GameEntity gs in entities["solid"])
                {
                    if (ge.collides(gs))
                    {
                        ge.onCollision("solid", gs);
                        gs.onCollision("enemy", ge);
                    }
                }

                foreach (GameEntity ee in entities["enemy"])
                {
                    if (ee != ge && ge.collides(ee))
                    {
                        ge.onCollision("enemy", ee);
                        ee.onCollision("enemy", ge);
                    }
                }
            }

            foreach (GameEntity w in entities["weapon"])
            {
                foreach (GameEntity s in entities["solid"])
                    if (w.collides(s))
                    {
                        w.onCollision("solid", s);
                        s.onCollision("weapon", w);
                    }

                foreach (GameEntity e in entities["enemy"])
                    if (w.collides(e))
                    {
                        e.onCollision("weapon", w);
                        w.onCollision("enemy", e);
                    }
            }

            /* Remove flagged entities */
            foreach (GameEntity e in deathRow)
            {
                actuallyRemove(e);
            }
            deathRow.Clear();

            foreach (Pair<GameEntity, String> e in birthRow)
                _add(e.first, e.second);
            birthRow.Clear();

            Vector2 cp = new Vector2(player.x + 8, player.y + 8);
            camera.Pos = cp;

            if (state == State.Transition)
                transitionFader.update();

            if (KoM.input.pressed(Buttons.Back))
                game.returnToWorld();

            if (KoM.input.pressed(Keys.F11))
                game.screenshot();

            Vector2 vector = GamePad.GetState(PlayerIndex.One).ThumbSticks.Right;
            if (vector.Length() > 0.5)
            {
                vector.Normalize();
                double angle = Math.Asin(vector.Y);
                camera.Rotation = (float)angle;
            }
        }

        override public void render(GameTime dt, SpriteBatch sb, Matrix matrix)
        {
            // TODO: Call this here?
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

            foreach (GameEntity e in entities["solid"])
                e.render(dt, sb);
            foreach (GameEntity e in entities["onewaysolid"])
                e.render(dt, sb);
            foreach (GameEntity m in entities["misc"])
                m.render(dt, sb);
            foreach (GameEntity e in entities["enemy"])
                e.render(dt, sb);
            foreach (GameEntity e in entities["entries"])
                e.render(dt, sb);

            if (state == State.Transition)
                transitionFader.render(sb);

            player.render(dt, sb);

            foreach (GameEntity w in entities["weapon"])
                w.render(dt, sb);

            Rectangle viewRect = camera.viewRectangle;
            sprPoint.render(sb, viewRect.X, viewRect.Y);
            sprPoint.render(sb, viewRect.Right - sprPoint.width, viewRect.Bottom - sprPoint.height);

            sb.DrawString(game.gameFont, "" + instanceNumber(typeof(PlayerWeapon)), new Vector2(viewRect.X + 10, viewRect.Y + 10), Color.NavajoWhite);
        }

        protected override bool _add(GameEntity e, string category)
        {
            switch (category)
            {
                case "player":
                    entities["player"].Add(e);
                    player = e;
                    break;
                case "enemy":
                    entities["enemy"].Add(e);
                    break;
                case "solid":
                    entities["solid"].Add(e);
                    break;
                default:
                    if (entities.ContainsKey(category))
                    {
                        entities[category].Add(e);
                        return base._add(e, category);
                    }
                    else
                        return false;
            }

            return base._add(e, category);
        }

        public void handleEntities(List<GameEntity> list)
        {
            foreach (GameEntity e in list)
            {
                if (e == null)
                    continue;
                if (e is Player)
                    _add(e, "player");
                else if (e is Enemy)
                    _add(e, "enemy");
                else if (e is Stairs)
                    _add(e, "stairs");
                else if (e is Beast)
                    _add(e, "onewaysolid");
                else if (e is PickableBlock)
                    _add(e, "solid");
                else if (e is OneWayPlatform)
                    _add(e, "onewaysolid");
                else if (e is ActivableDisplay || e is AreaTrigger || e is Door)
                    _add(e, "misc");
                else if (e is LevelEntrance)
                    _add(e, "entries");
                else
                    Console.WriteLine("Unkown entity in handleEntities");
            }
        }

        public void moveToLayer(int layer)
        {
            
            this.paused = true;
            this.toLayer = layer;
            timer = 16;
            transitionFader = new Fader(game, Color.Black, 0.0f, 1.0f / 16);
            state = State.Transition;
        }

        public void onTimer()
        {
            // First time, change layer and set timer
            if (this.currentLayer != toLayer)
            {
                switchToLayer(toLayer);
                timer = 16;
                transitionFader = new Fader(game, Color.Black, 1.0f, -1.0f / 16);
            }
            // Second time, unpause
            else
            {
                paused = false;
                transitionFader = null;
                state = State.Gameplay;
            }
        }

        public void switchToLayer(int layer, bool notGameplay = false)
        {
            currentLayer = layer;
            entities = layers[layer];
            foreach (String key in commonEntities.Keys)
                if (!entities.Keys.Contains<String>(key))
                    entities.Add(key, commonEntities[key]);

            // Notify entities about switch
            foreach (String key in entities.Keys)
                foreach (GameEntity e in entities[key])
                    if (e is IListener)
                        (e as IListener).onEvent("layerChange");

            if (!notGameplay)
            {
                if (layerMap.Count > layer)
                    map = layerMap[layer];

                if (camera != null)
                    camera.bounds = new Rectangle(map.x, map.y, map.tilemap.width, map.tilemap.height);
            }
        }

        public bool isPaused()
        {
            return paused;
        }

        public void onPause()
        {
        }

        public void onResume()
        {
        }

        public void pause()
        {
            if (!paused)
            {
                paused = true;
                onPause();
            }
        }

        public void resume()
        {
            if (paused)
            {
                paused = false;
                onResume();
            }
        }

        override public bool isInstanceInView(GameEntity e)
        {
            return camera.viewRectangle.Contains(e.mask.rect);
        }
    }

    public class Fader
    {
        Texture2D tex;
        KoM game;
        Color color;
        float alpha;
        float delta;

        public Fader(KoM game, Color color, float alpha = 1.0f, float delta = 0.0f)
        {
            this.game = game;

            this.color = color;
            this.alpha = alpha;
            this.delta = delta;

            tex = game.Content.Load<Texture2D>("rect");
            color.A = (byte)(alpha * 255);
        }

        public void update(float a = -1.0f)
        {
            if (a != -1.0f)
                alpha = a;
            else if (delta != 0)
                alpha += delta;

            alpha = Math.Min(Math.Max(alpha, 0), 1);
            color.A = (byte)(alpha * 255);
        }

        public void render(SpriteBatch sb)
        {
            System.Diagnostics.Debug.WriteLine(alpha);
            sb.Draw(tex, new Rectangle(0, 0, game.graphicsDevice.DisplayMode.Width, game.graphicsDevice.DisplayMode.Height), color);
        }
    }

    public interface IListener
    {
        void onEvent(String eventName);
    }
}
