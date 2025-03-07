using HarmonyLib;
using MysteryDice.Dice;
using MysteryDice.Effects;
using System;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class ChatPatch
    {
        const string BaseCommand = "~edice ";
        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatTesting(HUDManager __instance)
        {
            string txt = __instance.chatTextField.text.ToLower();
            if(txt == "/visor"||txt == "\\visor")
            {
                ToggleVisor();
                Misc.SafeTipMessage("Visor Toggled", "Run the command again to toggle it back");
            }
            if(txt == "/fixhud"||txt == "\\fixhud")
            {
                HUDManager.Instance.enabled=true;
                HUDManager.Instance.ToggleHUD(true);
            }
            // if((txt == "/size" || txt == "\\size") && SizeDifferenceSwitcher.canSwitch)
            // {
            //     SizeDifferenceSwitcher.BecomeSmall(GameNetworkManager.Instance.localPlayerController.actualClientId);
            //     Misc.SafeTipMessage("Size Switched", "Run the command again to toggle it back");
            // }

            if (!MysteryDice.allowChatCommands.Value) return;

            if (txt == BaseCommand + "enemies")
            {
                MysteryDice.CustomLogger.LogMessage("Registered enemies:");
                foreach (var enemy in RoundManager.Instance.currentLevel.Enemies)
                    MysteryDice.CustomLogger.LogMessage(enemy.enemyType.enemyName);
            }

            if (txt.Contains(BaseCommand + "dice"))
            {
                string[] args = txt.Split(' ');
                if (args.Length < 3) return;
                
                if(Int32.TryParse(args[2], out int diceNum))
                {

                    GameObject selectedDie = MysteryDice.DieEmergency.spawnPrefab;
                    switch (diceNum)
                    {
                        case 1:
                            selectedDie = MysteryDice.DieEmergency.spawnPrefab;
                            break;
                        case 2:
                            selectedDie = MysteryDice.DieGambler.spawnPrefab;
                            break;
                        case 3:
                            selectedDie = MysteryDice.DieChronos.spawnPrefab;
                            break;
                        case 4:
                            selectedDie = MysteryDice.DieSacrificer.spawnPrefab;
                            break;
                        case 5:
                            selectedDie = MysteryDice.DieSaint.spawnPrefab;
                            break;
                        case 6:
                            selectedDie = MysteryDice.DieRusty.spawnPrefab;
                            break;
                    }

                    GameObject obj = UnityEngine.Object.Instantiate
                    (
                        selectedDie,
                        GameNetworkManager.Instance.localPlayerController.transform.position,
                        Quaternion.identity,
                        RoundManager.Instance.playersManager.propsContainer
                    );

                    obj.GetComponent<GrabbableObject>().fallTime = 0f;
                    obj.GetComponent<NetworkObject>().Spawn();
                    obj.GetComponent<GrabbableObject>().EnableItemMeshes(true);
                }
            }

            if (txt.Contains(BaseCommand + "menu"))
                MysteryDice.DebugMenu(true);

            foreach (IEffect effect in DieBehaviour.AllEffects)
            {
                if (txt.Contains(BaseCommand + "effect " + effect.Name.ToLower()) || txt.Contains(BaseCommand + "e " + effect.Name.ToLower()))
                {
                    effect.Use();
                }
            }
        }
        public static void ToggleVisor()
        {
            GameObject visor = GameObject.Find("Systems/Rendering/PlayerHUDHelmetModel/ScavengerHelmet");
            MeshRenderer component = visor.GetComponent<MeshRenderer>();
            component.enabled = !component.enabled;
        }
        
    }

    
}
