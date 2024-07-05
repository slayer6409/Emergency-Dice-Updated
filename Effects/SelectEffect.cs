using GameNetcodeStuff;
using MysteryDice.Dice;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

namespace MysteryDice.Effects
{
    internal class SelectEffect : IEffect
    {
        public string Name => "Select Effect";
        public EffectType Outcome => EffectType.Great;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "You can select an effect";

        public static GameObject EffectMenu = null;

        public void Use()
        {
            ShowSelectMenu();
        }

        public static void ShowSelectMenu()
        {
            if (EffectMenu != null)
            {
                GameObject.Destroy(EffectMenu);
                EffectMenu = null;
            }
                
            EffectMenu = GameObject.Instantiate(MysteryDice.EffectMenuPrefab);

            Transform scrollContent = EffectMenu.transform.Find("Panel/Panel/Scroll View/Viewport/Content");
            Button exitButton = EffectMenu.transform.Find("Panel/Exit").GetComponent<Button>();
            exitButton.onClick.AddListener(() =>
            {
                CloseSelectMenu();
            });

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            foreach (IEffect effect in DieBehaviour.AllowedEffects)
            {
                GameObject effectObj = GameObject.Instantiate(MysteryDice.EffectMenuButtonPrefab, scrollContent);
                TMP_Text buttonText = effectObj.transform.GetChild(0).GetComponent<TMP_Text>();
                buttonText.text = $"{effect.Name} [{effect.Outcome}]";

                Button button = effectObj.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    CloseSelectMenu();
                    effect.Use();
                });
                
            }
        }
        public static void CloseSelectMenu()
        {
            if (EffectMenu != null)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                GameObject.Destroy(EffectMenu);
            }
                
        }
    }
}
