using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Systems
{
    class SoundSystem : EntitySystem
    {
        public SoundSystem(ContentManager content)
            : base(SystemOrders.Update.Sound,
                new int[] { ComponentTypeIds.SoundLoop },
                new uint[] { Messages.PlaySound })
        {
            soundEffects["Explosion".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\Explosion");
            soundEffects["DropBomb".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\DropBomb");
            soundEffects["FootStep".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\FootStep");
            soundEffects["PowerUp".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PowerUp");
            soundEffects["Buzzer".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\Buzzer");
            soundEffects["Thud".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\Thud");

            // Powerups sounds.
            soundEffects["PU_BombUp".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_BombUp");
            soundEffects["PU_FireUp".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_FireUp");
            soundEffects["PU_SpeedUp".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_SpeedUp");
            soundEffects["PU_FullFire".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_FullFire");
            soundEffects["PU_PowerBomb".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_PowerBomb");
            soundEffects["PU_DangerousBomb".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_DangerousBomb");
            soundEffects["PU_PassThroughBomb".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_PassThroughBomb");
            soundEffects["PU_RemoteControlBomb".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_RemoteControlBomb");
            soundEffects["PU_LandMine".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_LandMine");
            soundEffects["PU_BombDown".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_BombDown");
            soundEffects["PU_FireDown".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_FireDown");
            soundEffects["PU_SpeedDown".CRC32Hash()] = content.Load<SoundEffect>(@"Audio\PU_SpeedDown");
        }

        protected override void Initialize()
        {
            soundLoopComponents = EntityManager.GetComponentManager<SoundLoop>(ComponentTypeIds.SoundLoop);
        }

        private Dictionary<int, SoundEffect> soundEffects = new Dictionary<int, SoundEffect>();

        private IComponentMapper<SoundLoop> soundLoopComponents;

        private SoundEffectInstance[] soundEffectInstance = new SoundEffectInstance[EntityManager.MaxEntities];

        protected override void OnEntityRemoved(Entity entity, int liveId)
        {
            base.OnEntityRemoved(entity, liveId);

            SoundEffectInstance sei = soundEffectInstance[liveId];
            if (sei != null)
            {
                soundEffectInstance[liveId] = null;
                sei.Dispose();
            }
        }

        protected override int OnHandleMessage(Entity target, uint message, ref MessageData data, object sender)
        {
            switch (message)
            {
                case Messages.PlaySound:
                    soundEffects[data.Int32].Play(data.Single, data.Single2, 0f);
                    data.Handled = true;
                    break;
            }
            return 0;
        }

        protected override void OnEntityAdded(Entity entity, int liveId)
        {
            base.OnEntityAdded(entity, liveId);

            SoundLoop soundLoop = soundLoopComponents.GetComponentFor(entity);

            soundEffectInstance[liveId] = soundEffects[soundLoop.CueNameId].CreateInstance();
            soundEffectInstance[liveId].IsLooped = true;    // REVIEW: support non-looped.
            UpdateSound(liveId, soundLoop);
            soundEffectInstance[liveId].Play();
        }

        private void UpdateSound(int liveId, SoundLoop soundLoop)
        {
            soundEffectInstance[liveId].Volume = soundLoop.Volume;
            soundEffectInstance[liveId].Pitch = soundLoop.Pitch;
        }

        protected override void  OnProcessEntities(GameTime gameTime, IEnumerable<int> entityIdCollection)
        {
            foreach (int liveId in entityIdCollection)
            {
                SoundLoop soundLoop = soundLoopComponents.GetComponentFor(liveId);
                UpdateSound(liveId, soundLoop);
            }
        }
    }
}
