using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using kom.Engine;
using kom.Engine.Graphics;

namespace kom.Game
{
    class Player : GameEntity, ICarrier, IPausable
    {
        public Spritemap graphic;
        public int hspeed;
        public float vspeed, gravity;

        public enum MovementState { Idle, Walk, Jump, Ladder, Thrown };
        public enum ActionState { None, Attack, Carry }
        public enum Dir { Left, Right };

        public MovementState state;
        public ActionState action;
        public Dir facing;

        // Step variables
        public Vector2 initialPosition;
        public Vector2 moveTo;

        public PickableBlock carriedEntity;

        public Player(int x, int y) : base(x, y)
        {
        }

        override public void init()
        {
            base.init();

            mask = new Mask(0, 0, 14, 15, 1, 1);
            mask.game = game;
            attributes.Add("player");
            attributes.Add("moveable");

            graphic = new Spritemap(game.Content.Load<Texture2D>("krop-gplay"), 16, 16);
            int[] fs = {0, 1};
            graphic.add(new Anim("idle", fs, 0.1f));
            graphic.add(new Anim("walk", fs, 10f));
            int[] fss = {9};
            graphic.add(new Anim("jump", fss, 0.0f));
            int[] fsss = {10, 11};
            graphic.add(new Anim("ladder", fsss, 0.1f));
            int[] fssss = {2, 3};
            graphic.add(new Anim("idle-carry", fssss, 0.1f));
            int[] fsssss = { 5 };
            graphic.add(new Anim("jump-carry", fsssss, 0.0f));
            int[] ff = { 16 };
            graphic.add(new Anim("throw", ff, 0.0f));

            graphic.play("idle");

            hspeed = 2;
            vspeed = 0f;
            gravity = 0.5f;

            state = MovementState.Idle;
            action = ActionState.None;
            facing = Dir.Right;

            carriedEntity = null;
        }

        override public void update()
        {
            if (isPaused())
                return;

            color = Color.White;

            initialPosition = pos;
            moveTo = pos;

            Stairs ladder = (Stairs) instancePlace(x, y, "stairs");

            bool onair = !placeMeeting(x, y + 1, "solid");
            if (onair)
                onair = !placeMeeting(x, y + 1, "onewaysolid", onewaysolidCondition);
            bool onladder = ladder != null;

            bool toLadder = false;

            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    if (action != ActionState.Attack)
                    {
                        if (input.left())
                        {
                            moveTo.X -= hspeed;
                            facing = Dir.Left;
                        }
                        else if (input.right())
                        {
                            moveTo.X += hspeed;
                            facing = Dir.Right;
                        }
                    }

                    if (onair)
                    {
                        state = MovementState.Jump;
                    }
                    else
                    {
                        vspeed = 0f;

                        state = MovementState.Idle;

                        if (input.pressed(Buttons.A))
                        {
                            state = MovementState.Jump;
                            vspeed = -4f;
                        }
                        else if (input.pressed(Buttons.X))
                        {
                            if (action == ActionState.Carry)
                            {
                                // Drop or throw carried entity
                                bool drop = false;
                                Vector2 target = moveTo;

                                // if (carriedEntity is Beast)
                                // {
                                    float hsp = moveTo.X - pos.X;
                                    carriedEntity.onThrow((2f * (facing == Dir.Left ? -1 : 1)) + hsp * 0.85f, -(3.5f - (Math.Abs(hsp)/hspeed)));
                                    drop = true;
                                    state = MovementState.Thrown;
                                    timer[0] = 15;
                                // }
                                /* Drop on ground instead of throw */
                                /*else 
                                {
                                    // Choose target position
                                    if (facing == Dir.Left)
                                        target.X -= 16;
                                    else
                                        target.X += 16;
                                    target.Y += 8;
                                    // Check if can be dropped there
                                    if (carriedEntity.canDrop(target))
                                    {
                                        drop = true;
                                        carriedEntity.onCarry(target);
                                    }
                                }*/

                                if (drop)
                                {
                                    action = ActionState.None;

                                    // Notify it about not being carried
                                    carriedEntity.onEndCarry();

                                    carriedEntity = null;
                                }
                            }
                            else
                            {
                                if (action == ActionState.None)
                                {
                                    // Grab pickable entity
                                    if (input.down())
                                    {
                                        GameEntity entity = instancePlace(x, y + 1, "solid");
                                        if (entity == null || !(entity is PickableBlock))
                                            entity = instancePlace(x, y + 1, "onewaysolid", null, onewaysolidCondition);
                                        if (entity is PickableBlock/* && onewaysolidCondition(this, entity)*/)
                                        {
                                            carriedEntity = entity as PickableBlock;
                                            carriedEntity.onInitCarry(this);
                                            action = ActionState.Carry;
                                        }
                                    }
                                    else
                                    {
                                        // Attack
                                        if (world.instanceNumber(typeof(PlayerWeapon)) < 3)
                                        {
                                            handleAttack();
                                        }
                                    }
                                }
                            }
                        }

                        if (action == ActionState.None)
                        {
                            if (onladder)
                            {
                                if (input.up() || input.down() && (!input.left() && !input.right()))
                                    state = MovementState.Ladder;
                            }
                            else if (placeMeeting(x, y + 1, "stairs") && input.down())
                            {
                                moveTo.Y += 2;
                                GameEntity g = (GameEntity) instancePlace(moveTo, "stairs");
                                if (g != null)
                                    moveTo.X = g.x;
                                toLadder = true;
                                state = MovementState.Ladder;
                            }
                        }

                        handleDynamicSolidMovement();
                    }

                    if (action == ActionState.Carry && carriedEntity != null)
                    {
                        moveTo = carriedEntity.movementTo(moveTo);
                    }

                    break;
                case MovementState.Ladder:
                    if (onladder)
                    {
                        moveTo.X = ladder.x;

                        if (input.up())
                            moveTo.Y -= hspeed;
                        else if (input.down())
                            moveTo.Y += hspeed;
                        if ((input.left() || input.right()) && !(input.down() || input.up()))
                            if (placeMeeting(x, y+1, "solid") || placeMeeting(x, y+1, "onewaysolid", onewaysolidCondition))
                                state = MovementState.Idle;
                    }
                    else
                    {
                        state = MovementState.Jump;
                        if (input.up())
                            vspeed = -2.0f;
                        else
                            vspeed = 0;
                    }
                    break;
                case MovementState.Jump:
                    if (onair)
                    {
                        if (input.released(Buttons.A) && vspeed < 0)
                            vspeed /= 2;

                        vspeed += gravity;
                        if (input.left())
                        {
                            moveTo.X -= hspeed;
                            facing = Dir.Left;
                        }
                        else if (input.right())
                        {
                            moveTo.X += hspeed;
                            facing = Dir.Right;
                        }

                        if (input.pressed(Buttons.X) && action == ActionState.None)
                        {
                            handleAttack();
                        }
                    }
                    // Go to standing state, acknowleding the case in which
                    // the player is moving through a one way platform
                    else if (vspeed >= 0)
                    {
                        state = MovementState.Idle;
                    }
                    break;
                case MovementState.Thrown:
                    handleDynamicSolidMovement();
                    if (onair)
                        state = MovementState.Jump;
                    break;
            }
            
            moveTo.Y += vspeed;

            if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y < -0.5)
            {
                moveTo.Y = pos.Y + 2;
                vspeed = 0;
            }
            else if (GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y > 0.5)
            {
                moveTo.Y = pos.Y - 2;
                vspeed = 0;
            }

            IPusher pusher = (instancePlace(x, y, "solid", null, pusherCondition) as IPusher);
            if (pusher != null)
            {
                moveTo += pusher.getMovementDelta();
            }

            if (input.pressed(Buttons.Y))
                world.add(new Beast(x, y - 32), "onewaysolid");

            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl) || input.check(Buttons.B) || 
                toLadder)
                pos = moveTo;
            else
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
                // The y movement was stopped
                if (remnant.Y != 0 && vspeed < 0)
                    // Touched ceiling
                    vspeed = 0;
            }

            if (action == ActionState.Carry && carriedEntity != null)
            {
                carriedEntity.onCarry(pos);
            }

            switch (state)
            {
                case MovementState.Idle:
                case MovementState.Walk:
                    graphic.color = Color.White;
                    if (action == ActionState.Carry)
                        graphic.play("idle-carry");
                    else if (action == ActionState.Attack)
                        graphic.play("throw");
                    else if (state == MovementState.Idle)
                        graphic.play("idle");
                    else if (state == MovementState.Walk)
                        graphic.play("walk");

                    if (facing == Dir.Right)
                        graphic.flipped = true;
                    else
                        graphic.flipped = false;
                    break;
                case MovementState.Jump:
                    graphic.color = Color.Red;
                    if (action == ActionState.Carry)
                        graphic.play("jump-carry");
                    else
                        graphic.play("jump");
                    if (vspeed < 0)
                        graphic.currentAnim.frameIndex = 0;
                    else
                        graphic.currentAnim.frameIndex = 1;
                    if (facing == Dir.Right)
                        graphic.flipped = true;
                    else
                        graphic.flipped = false;
                    break;
                case MovementState.Ladder:
                    graphic.play("ladder");
                    graphic.flipped = false;
                    if (moveTo.Y - initialPosition.Y != 0)
                        graphic.currentAnim.resume();
                    else
                        graphic.currentAnim.pause();

                    break;
                case MovementState.Thrown:
                    graphic.play("throw");
                    graphic.flipped = (facing == Dir.Right);
                    break;
            }

            base.update();
            graphic.update();
        }

        protected void handleAttack()
        {
            float hsp = moveTo.X - pos.X;

            action = ActionState.Attack;

            PlayerWeapon w = new PlayerWeapon(x + (facing == Dir.Right ? 14 : 0), y + 4);
            world.add(w, "weapon");
            w.hspeed = 3f * (facing == Dir.Left ? -1 : 1) + hsp * 0.95f;
            w.vspeed = -2f;

            if (state == MovementState.Walk)
                moveTo.X = pos.X;

            timer[1] = 5;
        }

        protected void handleDynamicSolidMovement()
        {
            GameEntity dynamicSolid = instancePlace(x, y + 1, "onewaysolid", "dynamic-solid", onewaysolidCondition);
            if (dynamicSolid != null/* && onewaysolidCondition(this, dynamicSolid)*/)
                if (dynamicSolid is IDynamicSolid)
                {
                    if ((dynamicSolid as IDynamicSolid).hasMoved())
                        moveTo += (dynamicSolid as IDynamicSolid).getStepMovement();
                    else
                        Console.WriteLine("Dynamic solid has not moved yet!");
                }
        }

        public bool onewaysolidCondition(GameEntity me, GameEntity other)
        {
            if (me is Player)
            {
                Player p = me as Player;
                return (p.initialPosition.Y + p.mask.offsety + me.mask.h <= other.mask.y);
            }
            else
                return true;
        }

        public bool pusherCondition(GameEntity me, GameEntity other)
        {
            return (other is IPusher);
        }

        public override void onTimer(int n)
        {
            switch (n)
            {
                case 0:
                    if (state == MovementState.Thrown)
                        state = MovementState.Idle;
                    break;
                case 1:
                    if (action == ActionState.Attack)
                        action = ActionState.None;
                    break;
            }
        }

        public override void onCollision(string type, GameEntity other)
        {
            if (type == "enemy")
                color = Color.Red;
            else if (type == "solid")
                color = Color.Yellow;
        }

        override public void render(GameTime dt, SpriteBatch sb)
        {
            base.render(dt, sb);
            graphic.render(sb, pos);
        }

        public bool isFlipped()
        {
            return graphic.flipped;
        }

        public bool isPaused()
        {
            return (world as GameLevel).isPaused();
        }
    }
}
