using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace kom.Engine.Graphics
{
    public class Spritemap : Graphic
    {
        public Texture2D image;
        public int spriteWidth, spriteHeight;
        public int columns, rows;

        public Anim currentAnim;
        public Dictionary<string, Anim> animations;

        public bool flipped = false;

        int _width, _height;

        public new int width { get { return spriteWidth; } }
        public new int height { get { return spriteHeight; } }

        public Spritemap(Texture2D image, int spriteWidth, int spriteHeight)
        {
            this.image = image;
            this._width = image.Width;
            this._height = image.Height;

            this.spriteWidth = spriteWidth;
            this.spriteHeight = spriteHeight;

            columns = _width / spriteWidth;
            rows = _height / spriteHeight;

            animations = new Dictionary<string, Anim>();
            currentAnim = null;
        }

        public void add(Anim animation)
        {
            animations.Add(animation.name, animation);
        }

        public void play(string anim)
        {
            if (currentAnim == null || currentAnim.name != anim)
            {
                currentAnim = animations[anim];
                if (currentAnim != null)
                {
                    currentAnim.play();
                }
            }
        }

        public void update()
        {
            if (currentAnim != null)
                currentAnim.update();
        }

        protected Rectangle getFrame(int frame)
        {
            Rectangle rect = new Rectangle((frame % columns) * spriteWidth,
                                           (frame / columns) * spriteHeight,
                                           spriteWidth, spriteHeight);
            return rect;
        }

        override public void render(SpriteBatch sb, Vector2 position)
        {
            Rectangle to = new Rectangle((int) position.X, (int) position.Y, spriteWidth, spriteHeight);
            sb.Draw(image, to, getFrame(currentAnim.frame), color, 0, Vector2.Zero, (flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 0);
        }
    }

    public class Anim
    {
        public string name;
        public int[] frames;
        public float speed;
        public bool loop;

        public bool playing;
        public int frame 
        {
            set { currentFrame = value; }
            get { currentFrame = frames[currentFrameIndex]; return currentFrame; }
        }
        protected int currentFrame;
        public bool finished;
        public int frameIndex
        {
            get { return currentFrameIndex; }
            set 
            { 
                currentFrameIndex = Math.Max(0, Math.Min(value, frames.Length-1)); 
                actualFrameIndex = currentFrameIndex; 
            }
        }

        protected int currentFrameIndex;
        protected float actualFrameIndex;

        public Anim(String name, int[] frames, float speed = 1.0f, bool loop = true)
        {
            this.name = name;
            this.frames = frames;
            this.speed = speed;
            this.loop = loop;

            currentFrameIndex = 0;
            currentFrame = frames[currentFrameIndex];
            finished = false;
            playing = false;
        }

        public void play(int from = 0)
        {
            finished = false;
            playing = true;
            currentFrameIndex = from;
            currentFrame = frames[currentFrameIndex];
            actualFrameIndex = from;
        }

        public void pause()
        {
            playing = false;
        }

        public void resume()
        {
            playing = true;
        }

        public void stop()
        {
            playing = false;
            finished = true;
        }

        public void update()
        {
            if (playing && !finished)
            {
                actualFrameIndex += speed;
                if ((int)Math.Floor(actualFrameIndex) >= currentFrameIndex+1)
                {
                    // Next frame reached
                    currentFrameIndex++;
                    actualFrameIndex = currentFrameIndex;
                    // Finished?
                    if (currentFrameIndex >= frames.Length)
                    {
                        if (loop)
                        {
                            currentFrameIndex = 0;
                            actualFrameIndex = currentFrameIndex;
                        }
                        else
                        {
                            playing = false;
                            finished = true;
                            currentFrameIndex = frames[frames.Length - 1];
                        }
                    }

                    currentFrame = frames[currentFrameIndex];
                }
            }
        }
    }
}
