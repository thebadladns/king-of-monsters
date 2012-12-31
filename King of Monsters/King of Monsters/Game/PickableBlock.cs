using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using BananaEngine;
using BananaEngine.Graphics;

namespace kom.Game
{
    class PickableBlock : GameEntity, IPusher, IPausable
    {
        public Stamp graphic;
        public int speed;
        public float vspeed;
        public float gravity;
        public String statusString;

        public Vector2 initialPosition;
        public Vector2 moveTo;

        public PickableBlock(int x, int y) : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            mask.w = 16; mask.h = 16;

            graphic = new Stamp(game.Content.Load<Texture2D>("block"));

            vspeed = 0;
            speed = 0;
            gravity = 0.5f;

            statusString = "";
        }

        virtual public void onInitCarry(GameEntity other)
        {
            collidable = false;
        }

        virtual public void onCarry(Vector2 to)
        {
            x = (int) to.X;
            y = (int) to.Y - graphic.height;
        }

        virtual public bool canDrop(Vector2 at)
        {
            Vector2 ath = at;
            ath.Y -= mask.h;
            return !placeMeeting(ath, "solid");
        }

        public Vector2 movementTo(Vector2 to)
        {
            // Simulate a movement towards to checking collisions
            // against solids, and return the remainer
            Vector2 currentPos = pos;
            to.Y -= (mask.h + mask.offsety);
            Vector2 r = moveToContact(to, "solid");
            pos = currentPos;
            to.Y += (mask.h + mask.offsety);
            return to - r;
        }

        virtual public void onThrow(float hspeed, float vspeed)
        {
            collidable = true;
            speed = (int)hspeed;
            this.vspeed = vspeed;
        }

        virtual public void onEndCarry()
        {
            collidable = true;
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            Vector2 t = pos;
            t.Y -= 16;
            // sb.DrawString(game.gameFont, statusString, pos, color);
            if (statusString == "on air")
                graphic.color = Color.Red;
            else if (statusString == "carried")
                graphic.color = Color.Aqua;
            else
                graphic.color = Color.White;
            graphic.render(sb, x, y);
        }

        public override void update()
        {
            if (isPaused())
                return;

            initialPosition = pos;
            moveTo = pos;

            if (collidable)
            {
                moveTo.Y += vspeed;

                bool onair = !placeMeeting(x, y + 1, "solid");
                if (onair)
                    onair = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);

                if (onair)
                {
                    statusString = "on air";
                    moveTo.X += speed;
                    vspeed += gravity;

                    // Bounce over player
                    if (placeMeeting(x, y + 1, "player"))
                        vspeed = -2;
                    /*else if (placeMeeting(x, y + 1, "onewaysolid"))
                    {
                        vspeed = -1;
                        speed = 0;
                    }*/

                    Vector2 r = handleMovement();
                    if (r.X != 0 && vspeed > 0)
                        speed = 0;
                    if (r.Y != 0 && vspeed < 0)
                        vspeed = 0;
                    else if (r.Y != 0 && vspeed >= 0)
                    {
                        x = (x / 8) * 8;
                    }
                }
                else
                {
                    vspeed = 0;
                    speed = 0;
                    statusString = "on ground";

                    handleDynamicSolidMovement();
                    handleMovement();
                }
            }
            else
            {
                statusString = "carried";
            }

            base.update();
        }

        protected Vector2 handleMovement()
        {
            Vector2 remnant;
            // Check wether we collide first with a solid or a onewaysolid,
            // and use that data to position the player character.
            Vector2 oldPos = pos;
            Vector2 remnantOneWay = moveToContact(moveTo, "onewaysolid", onewaysolidCondition);
            Vector2 posOneWay = pos;
            pos = oldPos;
            Vector2 remnantSolid = moveToContact(moveTo, "solid");
            Vector2 posSolid = pos;
            if (remnantOneWay.Length() > remnantSolid.Length())
            {
                remnant = remnantOneWay;
                pos = posOneWay;
            }
            else
            {
                remnant = remnantSolid;
                pos = posSolid;
            }

            return remnant;
        }

        protected bool handleDynamicSolidMovement()
        {
            GameEntity dynamicSolid = instancePlace(x, y + 1, "onewaysolid", "dynamic-solid");
            if (dynamicSolid != null && onewaysolidCondition(this, dynamicSolid))
            {
                if (dynamicSolid is IDynamicSolid)
                {
                    if ((dynamicSolid as IDynamicSolid).hasMoved())
                    {
                        // moveTo += (dynamicSolid as IDynamicSolid).getStepMovement();
                        moveTo = (dynamicSolid.pos);
                    }
                    else
                        Console.WriteLine("Dynamic solid has not moved yet!");
                    return true;
                }
            }
            return false;
        }

        public bool onewaysolidCondition(GameEntity me, GameEntity other)
        {
            if (me is PickableBlock)
            {
                PickableBlock p = me as PickableBlock;
                return (p.initialPosition.Y + p.mask.offsety + me.mask.h <= other.mask.y);
            }
            else
                return true;
        }

        public Vector2 getMovement()
        {
            return moveTo;
        }

        public Vector2 getMovementDelta()
        {
            return moveTo - initialPosition;
        }

        public bool isPaused()
        {
            return (world as GameLevel).isPaused();
        }
    }

    public interface ICarrier
    {
        bool isFlipped();
    }

    public interface IPusher
    {
        Vector2 getMovement();
        Vector2 getMovementDelta();
    }
}
