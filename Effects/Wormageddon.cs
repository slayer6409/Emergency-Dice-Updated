using MysteryDice.Patches;

namespace MysteryDice.Effects
{
    internal class Wormageddon : IEffect
    {
        public string Name => "Wormageddon";
        public EffectType Outcome => EffectType.Awful;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Be ware of worms";

        public void Use()
        {
            Networker.Instance.SpawnWormssServerRPC();
        }

        public static void SpawnWorms()
        {
            for(int i = 0; i < UnityEngine.Random.Range(3,8); i++)
                Misc.SpawnOutsideEnemy(GetEnemies.Sandworm);
        }
    }
}
