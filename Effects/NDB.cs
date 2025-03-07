using MysteryDice.Patches;
using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class NutsBaby : IEffect
    {
        public string Name => "NDB";
        public EffectType Outcome => EffectType.Mixed;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Non-deductible Breakfast";

        public void Use()
        {
            if (GetEnemies.Maneater == null)
                return;
            Networker.Instance.SpawnDongServerRPC();
        }
    } 
    public class ManeaterPatchThing : MonoBehaviour
    {
        CaveDwellerAI _instance;
        public void Update() 
        {
            if (_instance == null)
            {
                gameObject.TryGetComponent<CaveDwellerAI>(out _instance);
            }
            else
            {
                _instance.growthMeter = 0f;
            }
        }
    }
}
