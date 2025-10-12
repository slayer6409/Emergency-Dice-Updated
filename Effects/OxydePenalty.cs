using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CodeRebirth.src.MiscScripts;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class OxydePenalty : IEffect
    {
        public string Name => "Oxyde Penalty";
        public EffectType Outcome => EffectType.SpecialPenalty;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You shouldn't have done that";


        public void Use()
        {
            Networker.Instance.doOxydePenaltyServerRpc();
        }

        [MethodImpl(MethodImplOptions.NoInlining|MethodImplOptions.NoOptimization)]
        public static void doPenalty()
        {
            foreach (var enemyLevelSpawner in EnemyLevelSpawner.enemyLevelSpawners)
            {
               if(enemyLevelSpawner is CodeRebirth.src.Content.Maps.CompactorEnemyLevelSpawner) continue;
               enemyLevelSpawner.spawnTimer = 0f;
            }
        }
        
    }
}
