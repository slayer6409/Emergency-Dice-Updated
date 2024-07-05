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
    internal class DoorMalfunction : IEffect
    {
        public string Name => "Door malfunction";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => true;
        public string Tooltip => "Ship doors are malfunctioning!";

        public void Use()
        {
            Networker.Instance.StartMalfunctioningServerRPC();
        }

        public static void SetHangarDoorsState(bool closed)
        {
            foreach(var hangarDoor in GameObject.FindObjectsOfType<HangarShipDoor>())
            {
                hangarDoor.SetDoorButtonsEnabled(false);
                if (closed)
                    hangarDoor.SetDoorClosed();
                else
                    hangarDoor.SetDoorOpen();

                hangarDoor.doorPower = 20f;
                hangarDoor.shipDoorsAnimator.SetBool("Closed", closed);
            }
        }
    }
}
