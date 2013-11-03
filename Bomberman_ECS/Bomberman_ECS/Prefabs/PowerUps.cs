using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Bomberman_ECS.Core;
using Bomberman_ECS.Components;

namespace Bomberman_ECS.Prefabs
{
    static partial class Prefabs
    {
        private static void MakePowerUps()
        {
            MakePowerUp(PowerUpType.BombUp, "BombUp", "PowerUp_BombUp", "PU_BombUp", GoodColor);
            MakePowerUp(PowerUpType.FireUp, "FireUp", "PowerUp_FireUp", "PU_FireUp", GoodColor);
            MakePowerUp(PowerUpType.SpeedUp, "SpeedUp", "PowerUp_SpeedUp", "PU_SpeedUp", GoodColor);
            MakePowerUp(PowerUpType.FullFire, "FullFire", "PowerUp_FullFire", "PU_FullFire", GoodColor);

            MakePowerUp(PowerUpType.PowerBomb, "PowerBomb", "PowerUp_PowerBomb", "PU_PowerBomb", GoodColor);
            MakePowerUp(PowerUpType.DangerousBomb, "DangerousBomb", "PowerUp_DangerousBomb", "PU_DangerousBomb", GoodColor);
            MakePowerUp(PowerUpType.PassThroughBomb, "PassThroughBomb", "PowerUp_PassThroughBomb", "PU_PassThroughBomb", GoodColor);
            MakePowerUp(PowerUpType.RemoteBomb, "RemoteBomb", "PowerUp_RemoteBomb", "PU_RemoteControlBomb", GoodColor);

            MakePowerUp(PowerUpType.LandMineBomb, "LandMine", "PowerUp_LandMine", "PU_LandMine", GoodColor);
            MakePowerUp(PowerUpType.BombDown, "BombDown", "PowerUp_BombDown", "PU_BombDown", BadColor);
            MakePowerUp(PowerUpType.FireDown, "FireDown", "PowerUp_FireDown", "PU_FireDown", BadColor);
            MakePowerUp(PowerUpType.SpeedDown, "SpeedDown", "PowerUp_SpeedDown", "PU_SpeedDown", BadColor);
        }

        private static Color GoodColor = new Color(255, 196, 128, 255);
        private static Color BadColor = new Color(0, 128, 196, 255);

        private static void MakePowerUp(PowerUpType type, string name, string model, string sound, Color tint)
        {
            ScriptContainer scriptContainer = new ScriptContainer("Wiggle".CRC32Hash()) { };
            Scripts.Scripts.Wiggle_Init(scriptContainer, period: GameConstants.PowerUpWigglePeriod, extent: GameConstants.PowerUpWiggleExtent);

            EntityTemplateManager.AddTemplate(
                new EntityTemplate(
                    name,
                    new Placement()
                    {
                        Layer = 2,
                        Visible = true,
                    },
                    new Aspect()
                    {
                        ModelNameId = model.CRC32Hash(),
                        Tint = tint,
                        Size = new Vector2(1.12f),
                    },
                    new PowerUp()
                    {
                        Type = type,
                        SoundId = sound.CRC32Hash()
                    },
                    new ExplosionImpact()
                    {
                        Barrier = ExplosionBarrier.None,
                        ShouldSendMessage = true,
                    },
                    scriptContainer
                    )
                );
        }
    }
}
