using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class MineExploder : IEffect
    {
        public string Name => "Mine Exploder";
        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Explode all mines";

        public void Use()
        {
            Networker.Instance.turretMineExploderServerRPC(false);
        }

        public static void explodeAllMines()
        {
            var landmines = GameObject.FindObjectsByType<Landmine>(FindObjectsSortMode.None);
            foreach (var landmine in landmines)
            {
                landmine.ExplodeMineServerRpc();
            }
        }
        
    }
    
    internal class TurretExploder : GalEffect
    {
        public string Name => "Turret Exploder";
        public EffectType RealOutcome => EffectType.Great;
        public EffectType NoGalOutcome => EffectType.Great;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Explode all turrets";

        public void Use()
        {
            Networker.Instance.turretMineExploderServerRPC(true);
        }

        public static void explodeAllMines()
        {
            var turrets = GameObject.FindObjectsByType<Turret>(FindObjectsSortMode.None);
            foreach (var turret in turrets)
            {
                Landmine.SpawnExplosion(turret.transform.position, true, 0f, 0f, 0, 2);
                if(turret) turret.GetComponent<NetworkObject>().Despawn(true);
            }
        }
        
    }
}
