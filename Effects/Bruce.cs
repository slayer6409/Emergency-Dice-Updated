using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class Bruce : IEffect
    {
        public string Name => "Bruce";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Fish are friends, not food";

        public void Use()
        {
            int bruceSpawn = UnityEngine.Random.Range(1, 3);
            if (GetEnemies.Bruce == null)
                return;
            Networker.Instance.PlaySoundServerRPC("Jaws");
            Misc.SpawnEnemyForced(GetEnemies.Bruce, bruceSpawn, false);
        }
    }
}
