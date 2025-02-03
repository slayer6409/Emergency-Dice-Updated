using MysteryDice.Effects;
using System.Collections.Generic;

namespace MysteryDice.Dice
{
    public class RustyDie : DieBehaviour
    {
        public override void SetupRollToEffectMapping()
        {
            RollToEffect.Add(1, new EffectType[] { EffectType.Awful });
            RollToEffect.Add(2, new EffectType[] { EffectType.Bad });
            RollToEffect.Add(3, new EffectType[] { });
            RollToEffect.Add(4, new EffectType[] { });
            RollToEffect.Add(5, new EffectType[] { });
            RollToEffect.Add(6, new EffectType[] { });
        }

        public override void Roll()
        {
            int diceRoll = UnityEngine.Random.Range(1,7);
            int diceRoll2 = UnityEngine.Random.Range(1,15);

            IEffect randomEffect = GetRandomEffect(diceRoll, Effects);

            if(randomEffect == null)
            {
                switch (diceRoll)
                {
                    case 3:
                        Networker.Instance.JackpotServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(1, 2));
                        Misc.SafeTipMessage($"Rolled 3", "Spawning some scrap");
                        break;
                    case 4:
                        Networker.Instance.JackpotServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(3, 4));
                        Misc.SafeTipMessage($"Rolled 4", "Spawning scrap");
                        break;
                    case 5:
                        Networker.Instance.JackpotServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(5, 6));
                        Misc.SafeTipMessage($"Rolled 5", "Spawning more scrap");
                        break;
                    case 6:
                        RoundManager RM = RoundManager.Instance;
                        List<int> weightList = new List<int>(RM.currentLevel.spawnableScrap.Count);
                        for (int j = 0; j < RM.currentLevel.spawnableScrap.Count; j++)
                        {
                            weightList.Add(RM.currentLevel.spawnableScrap[j].rarity);
                        }
                        int[] weights = weightList.ToArray();
                        switch (diceRoll2)
                        {
                            case 1:
                                Networker.Instance.SameScrapServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(4, 6), "Easter egg");
                                Misc.SafeTipMessage($"Hop Hop", "Explosive Eggs?");
                                break;
                            case 2:
                                Networker.Instance.SameScrapServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(4, 7),"Gift");
                                Misc.SafeTipMessage($"HO HO HO", "Christmas Time!");
                                break;
                            case 3:
                                var item = RM.currentLevel.spawnableScrap[RM.GetRandomWeightedIndex(weights)].spawnableItem;
                                Networker.Instance.SameScrapServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(6, 8),item.itemName);
                                Misc.SafeTipMessage($"Wat?", "It's all the same?!?");
                                break;
                            default:
                                Networker.Instance.JackpotServerRPC(PlayerUser.playerClientId, UnityEngine.Random.Range(7, 8));
                                Misc.SafeTipMessage($"Rolled 6", "Spawning a lot of scrap!");
                                break;
                        }
                       
                        break;
                }
            }
            else
            {
                PlaySoundBasedOnEffect(randomEffect.Outcome);
                randomEffect.Use();
                Networker.Instance.LogEffectsToOwnerServerRPC(PlayerUser.playerUsername, randomEffect.Name);
                ShowDefaultTooltip(randomEffect, diceRoll);
            }
            
        }
    }
}
