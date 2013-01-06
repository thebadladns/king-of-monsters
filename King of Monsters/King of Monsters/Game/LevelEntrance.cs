﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using bEngine;

namespace kom.Game
{
    class LevelEntrance : bEntity
    {
        public int entranceId;

        public LevelEntrance(int x, int y)
            : base(x, y)
        {

        }

        public override void init()
        {
            base.init();

            mask.w = 16;
            mask.h = 16;
        }
    }
}
