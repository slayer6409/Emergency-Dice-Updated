using HarmonyLib;
using LethalLib.Modules;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        public static void TerminalStart(Terminal __instance)
        {
            AddDiceSynonym("emergencydie");
            AddDiceSynonym("dice");
            AddDiceSynonym("die");
            AddDiceSynonym("edie");
        }

        public static void AddDiceSynonym(string synonym)
        {
            TerminalKeyword diceWord = GetKeyword("emergency-die");
            if (diceWord == null) return;

            if (GetKeyword(synonym) != null)
                return;

            TerminalKeyword newKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            newKeyword.isVerb = diceWord.isVerb;
            newKeyword.defaultVerb = diceWord.defaultVerb;
            newKeyword.compatibleNouns = diceWord.compatibleNouns;
            newKeyword.specialKeywordResult = diceWord.specialKeywordResult;
            newKeyword.accessTerminalObjects = diceWord.accessTerminalObjects;
            newKeyword.word = synonym;

            Items.terminal.terminalNodes.allKeywords = Items.terminal.terminalNodes.allKeywords.AddItem(newKeyword).ToArray();

            for (int i = 0; i < Items.terminal.terminalNodes.allKeywords.Length; i++)
            {
                TerminalKeyword kw = Items.terminal.terminalNodes.allKeywords[i];

                if (kw.word == "buy")
                {
                    CompatibleNoun die = GetDiceCompatibleNoun();
                    CompatibleNoun compatibleNoun = new CompatibleNoun() { noun = newKeyword, result = die.result };
                    kw.compatibleNouns = kw.compatibleNouns.AddItem(compatibleNoun).ToArray();
                    return;
                }
            }
        }
        public static TerminalKeyword GetKeyword(string keyword)
        {
            foreach(var item in Items.terminal.terminalNodes.allKeywords)
            {
                if (item.word == keyword)
                    return item;
            }
            return null;
        }

        public static CompatibleNoun GetDiceCompatibleNoun()
        {
            foreach(var tKey in Items.terminal.terminalNodes.allKeywords)
            {
                if (tKey.word == "buy")
                {
                    foreach (var noun in tKey.compatibleNouns)
                    {
                        if (noun.noun.word == "emergency-die")
                            return noun;
                    }
                }
            }
            return null;
        }
    }
}
