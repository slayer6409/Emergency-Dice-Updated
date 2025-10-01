using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class Overweight : IEffect
    {
        public string Name => "Overweight";
        public EffectType Outcome => EffectType.GalMixed;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Heavy but Valuable";

        public void Use()
        {
            for (int i = 0; i < 4; i++)
            {
                var item = StartOfRound.Instance.allItemsList
                    .itemsList[Random.Range(0, StartOfRound.Instance.allItemsList.itemsList.Count)].itemName;
                Networker.Instance.SameScrapAdvancedServerRPC(Array.IndexOf(StartOfRound.Instance.allPlayerScripts,GameNetworkManager.Instance.localPlayerController), 1, item, weightMod: 5, scrapValueMod: 4);
            }
        }
    }
}
