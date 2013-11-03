using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Bomberman_ECS.Core;
using Bomberman_ECS.Levels;
using Bomberman_ECS.Systems;
using Bomberman_ECS.Components;
using Bomberman_ECS.InputHandlerCallbacks;
using Bomberman_ECS.MessageHandlers;
using Bomberman_ECS.Prefabs;
using Bomberman_ECS.Scripts;

namespace Bomberman_ECS
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            InitializeEntityManager();

            Scripts.Scripts.Initialize();
            Prefabs.Prefabs.Initialize();
            Callbacks.Initialize();
            Handlers.Initialize();

            camera = new Camera("Main", this.GraphicsDevice.PresentationParameters.BackBufferWidth, this.GraphicsDevice.PresentationParameters.BackBufferHeight);
            camera.LookAt = new Vector3(-2, 0, 0);
            camera.DirectionToViewer = new Vector3(0, 0, 1);
            camera.Update();

            base.Initialize();
        }

        private EntityManager entityManager;
        private Camera camera;

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            levelLoader = new LevelLoader(entityManager, Universe.TopLevelGroupUniqueIdBase);

            SpriteMapper spriteMapper = InitializeSprites();
            InitializeSystems(spriteMapper);

            needToLoadLevel = true;

            MediaPlayer.Play(Content.Load<Song>(@"Music\SpellRolemusic"));
            // Creative commons
            // Attribution:
            // <div xmlns:cc="http://creativecommons.org/ns#" xmlns:dct="http://purl.org/dc/terms/" about="http://freemusicarchive.org/music/Rolemusic/Straw_Fields/01_rolemusic_-_spell"><span property="dct:title">Spell</span> (<a rel="cc:attributionURL" property="cc:attributionName" href="http://freemusicarchive.org/music/Rolemusic/">Rolemusic</a>) / <a rel="license" href="http://creativecommons.org/licenses/by-nc-sa/3.0/us/">CC BY-NC-SA 3.0</a></div>
            MediaPlayer.Volume = 0.3f;
            MediaPlayer.IsRepeating = true;
        }

        private void InitializeEntityManager()
        {
            entityManager = new EntityManager(new Universe());
            entityManager.AddComponentType<Placement>(ComponentTypeIds.Placement, 400);
            entityManager.AddComponentType<Aspect>(ComponentTypeIds.Aspect, 400);
            entityManager.AddComponentType<Physics>(ComponentTypeIds.Physics, 400);
            entityManager.AddComponentType<InputMap>(ComponentTypeIds.InputMap, 10);
            entityManager.AddComponentType<PlayerInfo>(ComponentTypeIds.Player, 10);
            entityManager.AddComponentType<ExplosionImpact>(ComponentTypeIds.ExplosionImpact, 400);
            entityManager.AddComponentType<InputHandlers>(ComponentTypeIds.InputHandlers, 10);
            entityManager.AddComponentType<Bomb>(ComponentTypeIds.Bomb, 20);
            entityManager.AddComponentType<MessageHandler>(ComponentTypeIds.MessageHandler, 100);
            entityManager.AddComponentType<PowerUp>(ComponentTypeIds.PowerUp, 100);
            entityManager.AddComponentType<Explosion>(ComponentTypeIds.Explosion, 200);
            entityManager.AddComponentType<ScriptContainer>(ComponentTypeIds.ScriptContainer, 100);
            entityManager.AddComponentType<FrameAnimation>(ComponentTypeIds.FrameAnimation, 200);
            entityManager.AddComponentType<GameState>(ComponentTypeIds.GameState, 10);
        }

        private SpriteMapper InitializeSprites()
        {
            // Preload sprites
            SpriteMapper spriteMapper = new SpriteMapper();
            spriteMapper.AddTexture("Brick", Content.Load<Texture2D>(@"Textures\Brick"));
            spriteMapper.AddTexture("EndBrick", Content.Load<Texture2D>(@"Textures\EndBrick"));
            spriteMapper.AddTexture("SoftBrick", Content.Load<Texture2D>(@"Textures\SoftBrick"));
            spriteMapper.AddTexture("Man", Content.Load<Texture2D>(@"Textures\Man"));
            spriteMapper.AddTexture("Bomb", Content.Load<Texture2D>(@"Textures\Bomb"));
            spriteMapper.AddTexture("BombDangerous", Content.Load<Texture2D>(@"Textures\BombDangerous"));
            spriteMapper.AddTexture("BombSpiked", Content.Load<Texture2D>(@"Textures\BombSpiked"));
            spriteMapper.AddTexture("BombPower", Content.Load<Texture2D>(@"Textures\BombPower"));
            spriteMapper.AddTexture("BombRC", Content.Load<Texture2D>(@"Textures\BombRC"));
            spriteMapper.AddTexture("Explosion", Content.Load<Texture2D>(@"Textures\Explosion"), 8, 1);
            spriteMapper.AddTexture("ExplosionBlue", Content.Load<Texture2D>(@"Textures\ExplosionBlue"), 8, 1);
            spriteMapper.AddTexture("LandMine", Content.Load<Texture2D>(@"Textures\LandMine"), 4, 1);

            SpriteSheet ssPowerUps = new SpriteSheet(Content.Load<Texture2D>(@"Textures\PowerUps"), 4, 4);
            spriteMapper.AddTexture("PowerUp_BombUp", ssPowerUps, 0, 1, 0, 1);
            spriteMapper.AddTexture("PowerUp_BombDown", ssPowerUps, 1, 1, 3, 1);
            spriteMapper.AddTexture("PowerUp_FireUp", ssPowerUps, 1, 1, 0, 1);
            spriteMapper.AddTexture("PowerUp_FireDown", ssPowerUps, 2, 1, 3, 1);
            spriteMapper.AddTexture("PowerUp_SpeedUp", ssPowerUps, 2, 1, 0, 1);
            spriteMapper.AddTexture("PowerUp_SpeedDown", ssPowerUps, 3, 1, 3, 1);
            spriteMapper.AddTexture("PowerUp_FullFire", ssPowerUps, 3, 1, 0, 1);
            spriteMapper.AddTexture("PowerUp_PowerBomb", ssPowerUps, 0, 1, 2, 1);
            spriteMapper.AddTexture("PowerUp_DangerousBomb", ssPowerUps, 1, 1, 2, 1);
            spriteMapper.AddTexture("PowerUp_RemoteBomb", ssPowerUps, 3, 1, 2, 1);
            spriteMapper.AddTexture("PowerUp_PassThroughBomb", ssPowerUps, 2, 1, 2, 1);
            spriteMapper.AddTexture("PowerUp_LandMine", ssPowerUps, 0, 1, 3, 1);

            return spriteMapper;
        }

        private void InitializeSystems(SpriteMapper spriteMapper)
        {
            // Prepare the systems
            SystemManager systemManager = new SystemManager(entityManager);
            systemManager.AddSystem(new RenderSystem(this, camera, spriteBatch, spriteMapper));
            systemManager.AddSystem(new InputSystem());
            GridSystem gridSystem = new GridSystem();
            entityGrid = gridSystem.EntityGrid;
            systemManager.AddSystem(gridSystem);
#if DEBUG
            systemManager.AddSystem(new CollisionSystem(GraphicsDevice, Content, camera));
#else
            systemManager.AddSystem(new CollisionSystem());
#endif
            systemManager.AddSystem(new PowerUpSystem(entityGrid));
            systemManager.AddSystem(new ExplosionSystem(entityGrid));
            systemManager.AddSystem(new BombSystem());
            systemManager.AddSystem(new MessageHandlerSystem());
            systemManager.AddSystem(new SoundSystem(Content));
            systemManager.AddSystem(new ScriptsSystem());
            systemManager.AddSystem(new FrameAnimationSystem(spriteMapper));
            systemManager.AddSystem(new GameStateSystem(this, entityGrid, spriteBatch, Content.Load<SpriteFont>("GUI")));
            systemManager.AddSystem(new PlayerSystem());
        }

        private LevelLoader levelLoader;

        bool needToLoadLevel = false;
        public void LoadLevelNextCycle()
        {
            needToLoadLevel = true;
        }

        private void LoadLevel()
        {
            needToLoadLevel = false;

            // Clear out everything.
            entityManager.FreeAllEntities();

            Entity mainGameEntity = entityManager.AllocateForGeneratedContent("MainGame", Universe.TopLevelGroupUniqueIdBase);

            Rectangle bounds;
            levelLoader.Load(0, out bounds);

            GameState gameState = (GameState)entityManager.GetComponent(mainGameEntity, ComponentTypeIds.GameState);
            gameState.Bounds = bounds;

            // Make bombermen
            Entity eMan = entityManager.AllocateForGeneratedContent("Player1", Universe.TopLevelGroupUniqueIdBase);
            Placement placement = (Placement)entityManager.GetComponent(eMan, ComponentTypeIds.Placement);
            placement.Position = new Vector3(bounds.X + 1, bounds.Y + 1, 0);

            eMan = entityManager.AllocateForGeneratedContent("Player2", Universe.TopLevelGroupUniqueIdBase);
            placement = (Placement)entityManager.GetComponent(eMan, ComponentTypeIds.Placement);
            placement.Position = new Vector3(bounds.Right - 2, bounds.Y + 1, 0);

            eMan = entityManager.AllocateForGeneratedContent("Player3", Universe.TopLevelGroupUniqueIdBase);
            placement = (Placement)entityManager.GetComponent(eMan, ComponentTypeIds.Placement);
            placement.Position = new Vector3(bounds.X + 1, bounds.Bottom - 2, 0);
        }

        private EntityGrid entityGrid;

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (needToLoadLevel)
            {
                LoadLevel();
            }

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            entityManager.UpdateSystems(gameTime);

            base.Update(gameTime);

            /*
            if (Keyboard.GetState().IsKeyDown(Keys.Z) && once)
            {
                once = false;
                MemoryStream stream = new MemoryStream();
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    liveIdWorker.Clear();
                    entityManager.EnumerateEntitiesOwnedBy(Universe.TopLevelGroupUniqueIdBase, liveIdWorker, EntityEnumeration.Shallow);
                    writer.Write(liveIdWorker.Count);
                    foreach (int liveId in liveIdWorker)
                    {
                        entityManager.SerializeEntity(writer, entityManager.GetEntityByLiveId(liveId), null, EntityEnumeration.Shallow, null);
                    }

                    // Done.
                    entityManager.FreeAllEntities();

                    stream.Seek(0, SeekOrigin.Begin);
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            entityManager.DeserializeEntity(reader);
                        }
                    }
                }
            }      */      
        }
        private bool once = true;
        private List<int> liveIdWorker = new List<int>();

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Texture2D grass = Content.Load<Texture2D>(@"Textures\Grass");
            Vector2 scale = Vector2.One;
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointWrap, null, null);
            spriteBatch.Draw(grass, Vector2.Zero, new Rectangle(0, 0, 1024, 768), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            spriteBatch.End();

            entityManager.DrawSystems(gameTime);

            base.Draw(gameTime);
        }
    }
}
