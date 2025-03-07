using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class LizzieDog : IEffect
    {
        public string Name => "Lizzie`s Puppy";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can pet that dog";

        public void Use()
        {
            if (GetEnemies.Dog == null)
                return;
            Networker.Instance.PlaySoundServerRPC("Dawg");
            Networker.Instance.SpawnPetDogServerRPC();
        }
    }
}
