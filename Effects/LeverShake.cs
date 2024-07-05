using Unity.Netcode;
using Unity.Netcode.Transports;
using UnityEngine;

namespace MysteryDice.Effects
{
    internal class LeverShake : IEffect
    {
        public string Name => "Lever Shake";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Ship's lever starts moving!";

        public static bool IsShaking = false;
        public static Vector3 InitialLevelTriggerLocalPosition = Vector3.zero;
        public static GameObject ShipLeverTrigger = null;
        public static GameObject ShipLever = null;

        public void Use()
        {
            Networker.Instance.LeverShakeServerRPC();
        }

        public static void ServerUse()
        {
           
        }

        public static void ClientsUse()
        {
            IsShaking = true;
            ShipLeverTrigger = GameObject.Find("StartGameLever");
            ShipLever = GameObject.Find("HangarDoorLever");
            //InitialLevelTriggerPosition = ShipLeverTrigger.transform.position;
            InitialLevelTriggerLocalPosition = ShipLeverTrigger.transform.localPosition;
        }

        public static void FixedUpdate()
        {
            if (ShipLeverTrigger == null || IsShaking == false) return;

            ShipLever.transform.position = ShipLeverTrigger.transform.position;

            ShipLeverTrigger.transform.localPosition = InitialLevelTriggerLocalPosition;

            float noiseA = Mathf.PerlinNoise1D(Time.time) * 0.2f;
            float noiseB = Mathf.PerlinNoise1D(Time.time*0.5f)*0.1f;

            ShipLeverTrigger.transform.position += 
                ShipLeverTrigger.transform.right * Mathf.Sin(Time.time) * 0.3f +
                ShipLeverTrigger.transform.up * (Mathf.Cos(Time.time * (2f + noiseA)) * 0.5f + 1f) +
                ShipLeverTrigger.transform.forward * Mathf.Sin(Time.time * (1.2f+ noiseB)) * 1.3f;
        }

    }
}
