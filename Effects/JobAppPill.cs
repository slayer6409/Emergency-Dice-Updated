using GameNetcodeStuff;
using LethalLib.Modules;
using MysteryDice.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace MysteryDice.Effects
{
    internal class JobAppPill : GalEffect
    {
        public string Name => "Now Hiring";
        public EffectType RealOutcome => EffectType.Awful;
        public EffectType NoGalOutcome => EffectType.Bad;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Are you looking for a job?";

        public void Use()
        {
            Networker.Instance.doRedPillStuffServerRPC(false);
        }
        
    } 
    internal class GlitchPill : GalEffect
    {
        public string Name => "Bald and Angry";
        public EffectType RealOutcome => EffectType.Awful;
        public EffectType NoGalOutcome => EffectType.Bad;
        public EffectType Outcome => MysteryDice.DisableGal.Value ? NoGalOutcome : RealOutcome;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "He is in denial";

        public void Use()
        {
            Networker.Instance.doRedPillStuffServerRPC(true);
        }
        
    }

    public class RedPillChanger : MonoBehaviour
    {
        public GameObject RedPill;
        public Material RedPillMaterial;
        public bool isBald = false;

        public void Awake()
        {
            StartCoroutine(doStartStuff());
        }

        public IEnumerator doStartStuff()
        {
            yield return new WaitUntil(() => RedPill != null && RedPillMaterial != null);
            var renderer = RedPill.transform.Find("Texture").GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogWarning("RedPill has no Renderer!");
                yield break;
            }
            renderer.material = RedPillMaterial;
        }
    }
}
