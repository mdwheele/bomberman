using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Bomberman_ECS.Core;
using Bomberman_ECS.InputHandlerCallbacks;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    // Maps physical inputs to actions and states, and passes those actions and states to input handlers.
    class InputSystem : EntitySystem
    {
        public InputSystem()
            : base(SystemOrders.Update.Input, 
                new int[] { ComponentTypeIds.InputMap, ComponentTypeIds.InputHandlers }
                )
        {
        }

        protected override void Initialize()
        {
            inputMapComponents = EntityManager.GetComponentManager<InputMap>(ComponentTypeIds.InputMap);
            inputHandlersComponents = EntityManager.GetComponentManager<InputHandlers>(ComponentTypeIds.InputHandlers);
        }

        private IComponentMapper<InputMap> inputMapComponents;
        private IComponentMapper<InputHandlers> inputHandlersComponents;

        private KeyboardState previousKeyboardState;
        private KeyboardState currentKeyboardState;
        private GamePadState previousGamePadState;
        private GamePadState currentGamePadState;

        private List<int> statesWorker = new List<int>();
        private List<int> actionsWorker = new List<int>();

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // REVIEW: We invoke the handlers as we're enumerating through components.
            // This will be a problem if the components are added/remove during the enumeration in response
            // to a handler.
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                statesWorker.Clear();
                actionsWorker.Clear();
                InputMap inputMap = inputMapComponents.GetComponentFor(liveId);
                
                foreach (KeyValuePair<Keys, int> pair in inputMap.KeyToAction)
                {
                    if (!previousKeyboardState.IsKeyDown(pair.Key) && currentKeyboardState.IsKeyDown(pair.Key))
                    {
                        actionsWorker.Add(pair.Value);
                    }
                }
                foreach (KeyValuePair<Keys, int> pair in inputMap.KeyToState)
                {
                    if (currentKeyboardState.IsKeyDown(pair.Key))
                    {
                        statesWorker.Add(pair.Value);
                    }
                }

                foreach (KeyValuePair<Buttons, int> pair in inputMap.ButtonToAction)
                {
                    if (!previousGamePadState.IsButtonDown(pair.Key) && currentGamePadState.IsButtonDown(pair.Key))
                    {
                        actionsWorker.Add(pair.Value);
                    }
                }
                foreach (KeyValuePair<Buttons, int> pair in inputMap.ButtonToState)
                {
                    if (currentGamePadState.IsButtonDown(pair.Key))
                    {
                        statesWorker.Add(pair.Value);
                    }
                }

                InputHandlers inputHandlers = inputHandlersComponents.GetComponentFor(liveId);
                foreach (int callbackId in inputHandlers.InputHandlerIds)
                {
                    Callbacks.GetCallback(callbackId)(EntityManager, liveId, actionsWorker, statesWorker);
                }
            }

            previousKeyboardState = currentKeyboardState;
            previousGamePadState = currentGamePadState;
        }
    }
}
