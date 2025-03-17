using System.Linq;
using MysteryDice.Dice;

namespace MysteryDice.Effects
{
    internal class ThreesCompany : IEffect
    {
        public string Name => "Three`s Company";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Uh Oh Oh Oh";

        public void Use()
        {
            int roll = -1;
            string who = StartOfRound.Instance.localPlayerController.playerUsername;
            
            IEffect effect = DieBehaviour.AllowedEffects.ToList().OrderBy(_ => UnityEngine.Random.value).First();
            MysteryDice.CustomLogger.LogDebug("Rolling Effect in Three`s Company: "+ effect.Name);
            effect.Use(); 
            Networker.Instance.LogEffectsToOwnerServerRPC(who, effect.Name, roll);
            
            IEffect effect2 = DieBehaviour.AllowedEffects.ToList().OrderBy(_ => UnityEngine.Random.value).First();
            MysteryDice.CustomLogger.LogDebug("Rolling Effect in Three`s Company: "+ effect2.Name);
            effect2.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(who, effect2.Name, roll);
            
            IEffect effect3 = DieBehaviour.AllowedEffects.ToList().OrderBy(_ => UnityEngine.Random.value).First();
            MysteryDice.CustomLogger.LogDebug("Rolling Effect in Three`s Company: "+ effect3.Name);
            effect3.Use();
            Networker.Instance.LogEffectsToOwnerServerRPC(who, effect3.Name, roll);
        }
    }
}
