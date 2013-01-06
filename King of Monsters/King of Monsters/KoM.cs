using System;
using System.Collections.Generic;
using System.Linq;

using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using bEngine;
using kom.Game;
using kom.Game.Data;

namespace kom
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class KoM : bGame
    {
        // Data management
        public GameDataManager dataManager;

        // Gameplay
        protected bGameState worldMap;

        public KoM() : base()
        {
            horizontalZoom = 3;
            verticalZoom = 3;

            Resolution.Init(ref graphics, 256, 240);
            Resolution.SetVirtualResolution(256, 240);
            Resolution.SetResolution(256*3, 240*3, false);

            Content.RootDirectory = "Content";

            dataManager = new GameDataManager();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            if (!dataManager.loadGame())
                dataManager.startNewGame();

            // TODO: Create Initial Level
            worldMap = new GameWorld(this);
            changeWorld(worldMap);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load game-wide available sprite fonts
            gameFont = Content.Load<SpriteFont>("font0");

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Control time flow (30fps)
            timeSinceLastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (timeSinceLastUpdate < millisecondsPerFrame)
                return;

            timeSinceLastUpdate = 0;

            // Update inputstate
            input.update();

            // Allows the game to exit
            if (/*input.pressed(Buttons.Back) ||*/ input.pressed(Keys.Escape))
                this.Exit();
            else if (input.pressed(Keys.F4))
            {
                int rw, rh;
                if (graphics.IsFullScreen)
                {
                    rw = 256 * (int) horizontalZoom;
                    rh = 240 * (int) verticalZoom;
                }
                else
                {
                    rw = GraphicsDevice.DisplayMode.Width;
                    rh = GraphicsDevice.DisplayMode.Height;
                }

                Resolution.SetResolution(rw, rh, !graphics.IsFullScreen);
            }
            else if (input.pressed(Keys.R))
            {
                if (world != null)
                {
                    world.end();
                    dataManager.startNewGame();
                    world.init();
                }
            }
            else if (input.pressed(Keys.Add))
            {
                millisecondsPerFrame += 5.0;
            }
            else if (input.pressed(Keys.Subtract))
            {
                millisecondsPerFrame -= 5.0;
            }

            if (input.pressed(Buttons.Y))
                bConfig.DEBUG = !bConfig.DEBUG;
            
            if (world != null)
                world.update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Resolution.BeginDraw();
            // Generate resolution render matrix 
            Matrix matrix = Resolution.getTransformationMatrix();
            
            // Render world
            if (world != null)
                world.render(gameTime, spriteBatch, matrix);

            // Game level draw calls
            Vector2 pos = Vector2.Zero;
            if (world is GameLevel)
            {
                Rectangle viewRectangle = (world as GameLevel).camera.viewRectangle;
                pos = new Vector2(viewRectangle.X, viewRectangle.Y);
            }
            spriteBatch.DrawString(gameFont, "mpf: " + Math.Round(millisecondsPerFrame, 2, MidpointRounding.ToEven), pos, Color.PapayaWhip);

            // Finish drawing
            spriteBatch.End();

            // Not needed to call parent
            // base.Draw(gameTime);
        }

        public void goToLevel(int world, int level, int entrance)
        {
            GameLevel l = new GameLevel(world, level);
            l.currentEntrance = entrance;
            changeWorld(l);
        }

        public void returnToWorld()
        {
            changeWorld(worldMap);
        }
    }
}
