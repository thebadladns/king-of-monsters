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
    class Beast : PickableBlock, IDynamicSolid
    {
        public Spritemap _graphic;
        public bool moved;
        public Vector2 movement;

        public enum Dir { Left, Right };
        public Dir facing;

        public int hspeed;

        public bool isolated;

        public GameEntity carrierEntity;

        public Beast(int x, int y) : base(x, y)
        {
        }

        public override void init()
        {
            base.init();

            mask.w = 16; mask.h = 15; mask.offsety = 1;
            _graphic = new Spritemap(game.Content.Load<Texture2D>("beast"), 16, 16);
            int[] fs = {0, 1, 2, 3};
            _graphic.add(new Anim("walk", fs, 0.12f));
            int[] fss = { 4, 5 };
            _graphic.add(new Anim("carried", fss, 0.05f));

            _graphic.play("walk");

            moved = false;
            facing = (Dir) new Random().Next(2);
            hspeed = 1;
            vspeed = 0;
            gravity = 0.5f;

            isolated = false;

            attributes.Add("dynamic-solid");
        }

        override public void onInitCarry(GameEntity other)
        {
            _graphic.play("carried");
            collidable = false;
            if (other is ICarrier)
                carrierEntity = other;
        }

        override public void onCarry(Vector2 to)
        {
            x = (int) to.X;
            y = (int) to.Y - _graphic.height;
        }

        override public void onThrow(float hspeed, float vspeed)
        {
            speed = (int) hspeed;
            this.vspeed = vspeed;
        }

        override public void onEndCarry()
        {
            _graphic.play("walk");
            collidable = true;
            carrierEntity = null;
        }

        public override void render(Microsoft.Xna.Framework.GameTime dt, SpriteBatch sb)
        {
            // base.render(dt, sb);
            Vector2 tpos = pos;
            tpos.Y -= _graphic.height/2;
            // sb.DrawString(game.gameFont, pos.X + ", " + pos.Y, tpos, color);
            _graphic.render(sb, x, y);
        }

        public override void update()
        {
            if (isPaused())
                return;

            initialPosition = pos;
            moveTo = pos;

            _graphic.update();

            moved = false;
            isolated = false;

            if (collidable)
            {
                bool onair = !placeMeeting(x, y + 1, "solid");
                if (onair)
                    onair = !placeMeeting(x, y+1, "onewaysolid", onewaysolidCondition);
                
                if (onair)
                {
                    moveTo.X += speed;
                    moveTo.Y += vspeed;
                    vspeed += gravity;

                    // Bounce on player
                    if (placeMeeting(x, y + 1, "player"))
                        vspeed = -2;

                    Vector2 remnant = handleMovement();
                    // Vector2 r = moveToContact(moveTo, "solid");
                    if (remnant.X != 0 && vspeed > 0)
                        speed = 0;
                    if (remnant.Y != 0 && vspeed < 0)
                        vspeed = 0;
                }
                else
                {
                    
                    GameEntity dynamicSolid = instancePlace(x, y + 1, "onewaysolid", "dynamic-solid");
                    bool carried = (dynamicSolid != null && onewaysolidCondition(this, dynamicSolid));

                    if (carried)
                    {
                        handleDynamicSolidMovement();
                    }
                    else
                    {
                        // vspeed = 1;
                        if (canAdvance(facing))
                        {
                            moveTo.X += hspeed * (facing == Dir.Left ? -1 : 1);
                        }
                        else if (canAdvance((Dir)((((int)facing) + 1) % 2)))
                        {
                            facing = (Dir)((((int)facing) + 1) % 2);
                        }
                        else
                        {
                            // isolated
                            isolated = true;
                        }
                    }

                    /*if (placeMeeting(moveTo, "player"))
                        facing = (Dir)(((int)facing + 1) % 2);
                    else
                    {*/
                        Vector2 remnant = handleMovement();

                        // Vector2 r = moveToContact(moveTo, "solid");
                        // If a solid is found in horizontal movement, change direction
                        if (remnant.X != 0)
                            facing = (Dir)(((int)facing + 1) % 2);
                    //}
                }

                _graphic.color = color;
                _graphic.flipped = (facing == Dir.Right);
                if (isolated)
                    _graphic.currentAnim.frameIndex = 0;
                movement = pos - initialPosition;
                moved = true;
            }
            else
            {
                if (carrierEntity != null)
                    _graphic.flipped = (carrierEntity as ICarrier).isFlipped();
            }

            mask.update(x, y);
        }

        public Vector2 getStepMovement()
        {
            Vector2 v = Vector2.Zero;
            v.X = movement.X;
            return v;
        }

        public bool hasMoved()
        {
            return moved;
        }

        public bool groundAvailable(Dir direction)
        {
            bool solidGround = placeMeeting((int)moveTo.X + mask.w * (direction == Dir.Left ? -1 : 1), (int)moveTo.Y + 1, "solid");
            bool onewayGround = placeMeeting((int) moveTo.X + mask.w * (direction == Dir.Left ? -1 : 1), (int) moveTo.Y + 1, "onewaysolid", onewaysolidCondition);
            return (solidGround || onewayGround);
        }

        public bool canAdvance(Dir direction)
        {
            bool isGround = groundAvailable(direction);
            bool canMove = !placeMeeting((int)moveTo.X, (int)moveTo.Y, "solid");
            return isGround && canMove;
        }

        public void handlePusherMovement()
        {
            IPusher pusher = (instancePlace(x, y, "solid", null, (GameEntity me, GameEntity other) => other is IPusher) as IPusher);
            if (pusher != null)
            {
                moveTo += pusher.getMovementDelta();
            }
        }
    }
}
