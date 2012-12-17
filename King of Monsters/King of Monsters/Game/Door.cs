using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using kom.Engine;
using kom.Engine.Graphics;

namespace kom.Game
{
    class Door : GameEntity
    {
        public enum Location {Back, Front};

        Location placement;
        int width;
        int height;

        public Door(int x, int y, int w, int h, Location at)
            : base(x, y)
        {
            this.width = w;
            this.height = h;
            placement = at;
        }

        public override void init()
        {
            base.init();

            mask.w = width;
            mask.h = height;
        }

        public override void update()
        {
            base.update();

            if (!(world as GameLevel).isPaused())
            {
                if (placeMeeting(x, y, "player"))
                {
                    GameLevel gl = (world as GameLevel);
                    int toLayer = gl.currentLayer;
                    switch (placement)
                    {
                        case Location.Back:
                            if (input.up())
                                toLayer++;
                            break;
                        case Location.Front:
                            if (input.down())
                                toLayer--;
                            break;
                    }

                    if (toLayer != gl.currentLayer)
                        gl.moveToLayer(toLayer);
                }
            }
        }
    }
}