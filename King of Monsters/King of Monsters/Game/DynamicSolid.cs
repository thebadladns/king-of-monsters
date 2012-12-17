using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

namespace kom.Game
{
    public interface IDynamicSolid
    {
        bool hasMoved();
        Vector2 getStepMovement();
        Vector2 movementTo(Vector2 to);
    }
}

