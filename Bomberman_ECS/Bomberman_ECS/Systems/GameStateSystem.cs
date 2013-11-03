using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Bomberman_ECS.Components;
using Bomberman_ECS.Core;
using Bomberman_ECS.Levels;

namespace Bomberman_ECS.Systems
{
    class GameStateSystem : EntitySystem
    {
        public GameStateSystem(Game1 game, EntityGrid entityGrid, SpriteBatch spriteBatch, SpriteFont font)
            : base(SystemOrders.Update.GameState,
                new int[] { ComponentTypeIds.GameState },
                new uint[] { Messages.LoadLevel  }
                )
        {
            this.spriteBatch = spriteBatch;
            this.entityGrid = entityGrid;
            DrawOrder = SystemOrders.Draw.GameState;
            this.font = font;
            this.game = game;
        }

        private Game1 game;
        private SpriteFont font;
        private SpriteBatch spriteBatch;
        private EntityGrid entityGrid;

        protected override void Initialize()
        {
            gameStateComponents = EntityManager.GetComponentManager<GameState>(ComponentTypeIds.GameState);
        }

        private IComponentMapper<GameState> gameStateComponents;

        private List<Point> deathBlockSpiralPoints = new List<Point>();
        private Rectangle cachedBounds = Rectangle.Empty;
        private bool timeRunningOut = false;

        protected override void OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            float ellapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            foreach (int liveId in entityIdCollection)
            {
                GameState gameState = gameStateComponents.GetComponentFor(liveId);
                entityGrid.Bounds = gameState.Bounds;
                gameState.TimeRemaining = Math.Max(0, gameState.TimeRemaining - ellapsed); ;

                if (cachedBounds != gameState.Bounds)
                {
                    cachedBounds = gameState.Bounds;
                    Util.Helpers.CalculateDeathSpiralPoints(cachedBounds, deathBlockSpiralPoints);
                }

                MaybeSendDeathBlock(gameState);

                // Let's do some other tasks.
                if (!gameState.IsGameOver)
                {
                    MessageData data = new MessageData();
                    EntityManager.SendMessage(Messages.QueryWinningPlayer, ref data, null);
                    if (data.Handled)
                    {
                        gameState.WinningPlayerUniqueId = data.Int32;
                        // The game is over. This is the uniqueId of the player that won.
                        if (gameState.WinningPlayerUniqueId != EntityManager.InvalidEntityUniqueId)
                        {
                            gameState.IsGameOver = true;
                            bool allocatedNew;
                            InputHandlers ih = (InputHandlers)EntityManager.AddComponentToEntity(EntityManager.GetEntityByLiveId(liveId), ComponentTypeIds.InputHandlers, out allocatedNew);
                            ih.InputHandlerIds.Add("RestartGame".CRC32Hash());
                        }
                    }
                }
            }
        }

        private List<int> uniqueIdWorker = new List<int>();
        private void MaybeSendDeathBlock(GameState gameState)
        {
            // How many spots for death blocks?
            int count = (gameState.Bounds.Width - 2) * (gameState.Bounds.Height - 2);
            // How many seconds before they start appearing?
            float secondsToAppear = GameConstants.DeathBlocksArrivePeriod * count;

            Rectangle bounds = gameState.Bounds;
            float timeAppearing = Math.Max(0, secondsToAppear - gameState.TimeRemaining);
            int numberAppearing = (int)(timeAppearing / GameConstants.DeathBlocksArrivePeriod);
            Debug.Assert(numberAppearing <= count);

            // Show a warning?
            timeRunningOut = ((numberAppearing == 0) && (secondsToAppear < GameConstants.TimeRunningOutWarningTime));

            while (numberAppearing > gameState.DeathBlockCount)
            {
                // Send a block.
                Entity deathBlock = EntityManager.AllocateForGeneratedContent("DeathBlock", Universe.TopLevelGroupUniqueIdBase);
                // Figure out where! That's the hard part.
                Point position = deathBlockSpiralPoints[gameState.DeathBlockCount];
                Placement placement = (Placement)EntityManager.GetComponent(deathBlock, ComponentTypeIds.Placement);
                placement.Position = new Vector3(position.X, position.Y, 0);
                EntityManager.SendMessage(deathBlock, Messages.PlaySound, "Thud".CRC32Hash(), GameConstants.ThudVolume, null);

                // Send a kill messages to any entity here.
                entityGrid.QueryUniqueIdsAt(position, uniqueIdWorker);
                foreach (int uniqueIdHere in uniqueIdWorker)
                {
                    EntityManager.SendMessage(EntityManager.GetEntityByUniqueId(uniqueIdHere), Messages.DirectKill, null);
                }

                gameState.DeathBlockCount++;
            }
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                case Messages.LoadLevel:
                    {
                        game.LoadLevelNextCycle();
                    }
                    break;
            }
            return 0;
        }

        private StringBuilder sb = new StringBuilder();
        protected override void OnDraw(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            spriteBatch.Begin();
            foreach (int liveId in this.GetEntityLiveIds())
            {
                GameState gameState = gameStateComponents.GetComponentFor(liveId);
                sb.Clear();
                float displayTime = gameState.TimeRemaining + 0.99f;
                int minutes = (int)(displayTime / 60f);
                int seconds = (int)(displayTime % 60f);
                sb.AppendFormat("{0}:{1:00}", minutes, seconds);
                spriteBatch.DrawString(font, sb, new Vector2(10), Color.White);

                if (gameState.IsGameOver)
                {
                    int winnerNumber = -1;
                    // REVIEW: If the last player is destroy, this could happen.
                    // This needs work.
                    Entity winner = EntityManager.TryGetEntityByUniqueId(gameState.WinningPlayerUniqueId);
                    if (winner != null)
                    {
                        winnerNumber = ((PlayerInfo)EntityManager.GetComponent(winner, ComponentTypeIds.Player)).PlayerNumber;
                    }

                    sb.Clear();
                    sb.AppendFormat("GAME OVER. Player {0} wins!", winnerNumber);
                    spriteBatch.DrawString(font, sb, new Vector2(50), Color.White);
                    spriteBatch.DrawString(font, "Press space to start a new game", new Vector2(50, 100), Color.White);
                }

                if (timeRunningOut)
                {
                    spriteBatch.DrawString(font, "Time is running out!", new Vector2(301, 361), Color.Black);
                    spriteBatch.DrawString(font, "Time is running out!", new Vector2(300, 360), Color.DarkRed);
                }
            }
            spriteBatch.End();
        }
    }
}
