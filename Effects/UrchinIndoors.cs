using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class UrchinIndoors : IEffect
    {
        public string Name => "Indoor Urchins";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Spikey boyos, but inside!";

        public void Use()
        {
            int UrchinSpawn = UnityEngine.Random.Range(2, 4);
            if (GetEnemies.Urchin == null)
                return;
            Misc.SpawnEnemyForced(GetEnemies.Urchin, UrchinSpawn, true);
        }
    }
}
