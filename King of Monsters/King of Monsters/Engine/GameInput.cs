using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace kom.Engine
{
    public class GameInput
    {
        protected GamePadState oldPadState;
        public GamePadState currentPadState;
        
        protected KeyboardState oldKeyState;
        public KeyboardState currentKeyState;

        public GameInput()
        {
            currentPadState = GamePad.GetState(PlayerIndex.One);
            oldPadState = currentPadState;

            currentKeyState = Keyboard.GetState();
            oldKeyState = currentKeyState;
        }

        public void update()
        {
            oldPadState = currentPadState;
            currentPadState = GamePad.GetState(PlayerIndex.One);

            oldKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();
        }

        public bool pressed(Keys key)
        {
            return currentKeyState.IsKeyDown(key) && oldKeyState.IsKeyUp(key);
        }

        public bool check(Keys key)
        {
            return currentKeyState.IsKeyDown(key);
        }

        public bool released(Keys key)
        {
            return currentKeyState.IsKeyUp(key) && oldKeyState.IsKeyDown(key);
        }

        public bool pressed(Buttons btn)
        {
            return currentPadState.IsButtonDown(btn) && oldPadState.IsButtonUp(btn);
        }

        public bool check(Buttons btn)
        {
            return currentPadState.IsButtonDown(btn);
        }

        public bool released(Buttons btn)
        {
            return currentPadState.IsButtonUp(btn) && oldPadState.IsButtonDown(btn);
        }

        public bool left()
        {
            return currentPadState.ThumbSticks.Left.X < -0.3 ||
                currentKeyState.IsKeyDown(Keys.Left);
        }

        public bool right()
        {
            return currentPadState.ThumbSticks.Left.X > 0.3 ||
                currentKeyState.IsKeyDown(Keys.Right);
        }

        public bool up()
        {
            return currentPadState.ThumbSticks.Left.Y > 0.3 ||
                currentKeyState.IsKeyDown(Keys.Up);
        }

        public bool down()
        {
            return currentPadState.ThumbSticks.Left.Y < -0.3 ||
                currentKeyState.IsKeyDown(Keys.Down);
        }
    }
}
