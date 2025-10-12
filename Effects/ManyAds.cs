using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks.Data;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MysteryDice.Effects
{
    internal class ManyAds : IEffect
    {
        public string Name => "ManyAds";
        public EffectType Outcome => EffectType.Bad;
        public bool ShowDefaultTooltip => false;
        public string Tooltip => "Coming to you soon!";
        
        private static Queue<QueuedAd> adQueue = new();
        
        private static bool adPlaying = false;
        
        public static List<string> neutralText = new List<string>();
        public static List<string> topText = new List<string>();
        public static List<string> bottomText = new List<string>();

        public void Use()
        {
            Networker.Instance.TriggerManyAdsServerRpc();
        }
        
        public static IEnumerator LoadRemoteTextLists()
        {
            topText.Clear();
            bottomText.Clear();
            neutralText.Clear();
            yield return LoadListFromURL(GetURLWithTimestamp("TopText.txt"), topText, InitializeTopText, "TopText");
            yield return LoadListFromURL(GetURLWithTimestamp("BottomText.txt"), bottomText, InitializeBottomText, "BottomText");
            yield return LoadListFromURL(GetURLWithTimestamp("NeutralText.txt"), neutralText, InitializeNeutralText, "NeutralText");
        }

        private static IEnumerator LoadListFromURL(string url, List<string> targetList, Action fallback, string name)
        {
            MysteryDice.ExtendedLogging($"Loading List from url: {url} to target list: {name}");

            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                www.useHttpContinue = false;
                www.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
                www.SetRequestHeader("Pragma", "no-cache");
                www.SetRequestHeader("Expires", "0");

                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    MysteryDice.CustomLogger.LogWarning($"Failed to load ad text from {url}: {www.error}");
                    fallback?.Invoke();
                }
                else
                {
                    targetList.Clear();
                    string rawText = www.downloadHandler.text;
                    MysteryDice.ExtendedLogging($"[RawText] {name}: {rawText}");
                    string[] lines = rawText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    targetList.AddRange(lines.Select(line => line.Trim()).Where(line => !string.IsNullOrWhiteSpace(line)));
                }
            }
        }
        private static string GetURLWithTimestamp(string fileName)
        {
            string baseUrl = $"https://slayer6409.github.io/Emergency-Dice-Updated/{fileName}";
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            return $"{baseUrl}?cachebust={Guid.NewGuid()}";
        }
        private static void InitializeTopText()
        {
            MysteryDice.ExtendedLogging("Fallback Top Text");
            topText = new List<string>()
                {
                    "This fuckin thing",
                    "top text",
                    "Receding Hairline",
                    "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
                    "According to all known laws of aviation, there is no way a bee should be able to fly. Its wings are too small to get its fat little body off the ground. The bee, of course, flies anyway because bees don't care what humans think is impossible. Yellow, black. Yellow, black. Yellow, black. Yellow, black. Ooh, black and yellow! Let's shake it up a little. Barry! Breakfast is ready! Ooming! Hang on a second. Hello? - Barry? - Adam? - Oan you believe this is happening? - I can't. I'll pick you up. Looking sharp. Use the stairs. Your father paid good money for those. Sorry. I'm excited. Here's the graduate. We're very proud of you, son. A perfect report card, all B's. Very proud. Ma! I got a thing going here. - You got lint on your fuzz. - Ow! That's me! - Wave to us! We'll be in row 118,000. - Bye! Barry, I told you, stop flying in the house! - Hey, Adam. - Hey, Barry. - Is that fuzz gel? - A little. Special day, graduation. Never thought I'd make it. Three days grade school, three days high school. Those were awkward. Three days college. I'm glad I took a day and hitchhiked around the hive. You did come back different.",
                    "Open Gale",
                    "I scream",
                    "My cat",
                    "My dog",
                    "Zeekerss",
                    "Hot gals",
                    "Sapsucker",
                    "Ogopogo",
                    "Dice",
                    "Jimothy",
                    "Scrap-E",
                    "Biodiversity",
                    "Surfaced",
                    "Code rebirth",
                    "Hi I'm Chris Hansen"
                };
        }

        private static void InitializeBottomText()
        {
            MysteryDice.ExtendedLogging("Fallback Bottom Text");
            bottomText = new List<string>()
            {
                "bottom text", 
                "I wonder whose?",
                "Take a bath",
                "Import local mod",
                "Is a milf",
                "In your area",
                "You",
                "Buy 1 get 1",
                "Ive seen better",
                "sucks",
                "is cancelled",
                "The least of your concern",
                "is entertaining",
                "I wish they were real",
                "Not safe for work",
                "Happy birthday!",
                "Most likely to die next",
                "It burns when I pee",
                "Wheres the beef",
                "Spin spin spin",
                "is dead"
            };
        }

        private static void InitializeNeutralText()
        {
            MysteryDice.ExtendedLogging("Fallback Neutral Text");
            neutralText = new List<string>()
            {
                "Breaking News!",
                "Fuck",
                "Heheheheheheh",
                "Not the best",
                "?????????????????????",
                "What the fuck?",
                "Hehehehehehehe",
                ">:D",
                "Wut",
                "Ewwwww",
                "Happy Birthday",
                "Mu Mu Mu",
                "Whose child is this?",
                "Oh no",
                "RUN",
                "I hate it",
                "Go away",
                "Got ran over by a car",
                "Go Scarab",
                "Whatever",
                "Sussy",
                "Nice wig",
                "Smash.",
                "Skill Issue",
                "Someone clip that",
                "is Bald",
                "is Stinky",
                "is Short",
                "Glitch is Bald",
                "Mu is Bald",
                "Cross is Stinky",
                "Beef is Short",
                "The Short, Bald, and Stinky",
                "wi wi wi",
                "",
                "There is a config you should change :D"
            };
        }

        public static void logAll()
        {
            MysteryDice.ExtendedLogging("Logging All");
            foreach (var VARIABLE in topText)
            {
                MysteryDice.ExtendedLogging("Top " + VARIABLE);
            }
            foreach (var VARIABLE in bottomText)
            {
                MysteryDice.ExtendedLogging("Bottom " + VARIABLE);
            }
            foreach (var VARIABLE in neutralText)
            {
                MysteryDice.ExtendedLogging("Neutral " + VARIABLE);
            }
        }
        
        public static string getTopText()
        {
            logAll();
            List<string> tempList = new List<string>(topText);
            tempList.AddRange(StartOfRound.Instance.allPlayerScripts
                .Where(x=>x.isPlayerControlled||x.isPlayerDead)
                .Select(p => p.playerUsername)
                .ToList());
            tempList.AddRange(neutralText);
            return tempList[Random.Range(0, tempList.Count)];
        }

        public static string getBottomText()
        {
            List<string> tempList = new List<string>(bottomText);
            tempList.AddRange(StartOfRound.Instance.allPlayerScripts
                .Where(x=>x.isPlayerControlled||x.isPlayerDead)
                .Select(p => p.playerUsername)
                .ToList());
            tempList.AddRange(neutralText);
            return tempList[Random.Range(0, tempList.Count)];
        }
        
        public static void QueueAd(bool isTool, string name, string top, string bottom, bool isObject=false, GameObject prefab=null)
        {
            adQueue.Enqueue(new QueuedAd(isTool, name, top, bottom, isObject, prefab));

            if (!adPlaying)
                HUDManager.Instance.StartCoroutine(PlayAdQueue());
        }
        private static IEnumerator PlayAdQueue()
        {
            adPlaying = true;

            while (adQueue.Count > 0)
            {
                QueuedAd ad = adQueue.Dequeue();

                if (ad.isTool)
                {
                    showItemAd(ad.name, ad.topText, ad.bottomText);
                }
                else if(!ad.isObject)
                {
                    showUnlockableAd(ad.name, ad.topText, ad.bottomText);
                }
                else
                {
                    showPrefabAd(ad.prefabObject, ad.topText, ad.bottomText);
                }

                yield return new WaitForSeconds(14.2f);
            }
            adPlaying = false;
        }

        public static void showPrefabAd(GameObject prefab, string top, string bottom)
        {
            CreatePrefabAdModel(prefab);
            HUDManager.Instance.BeginDisplayAd(top, bottom);
        }
        
        public static void showItemAd(string itemName, string top, string bottom)
        {
            var item = StartOfRound.Instance.allItemsList.itemsList.Find(x=>x.itemName==itemName);
            if (item == null)
            {
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogWarning("Show Item Ad: Item is Null");
                return;
            } 
            if (item.spawnPrefab == null) {
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogWarning("Show Item Ad: Item spawn prefab is null");
                return;
            } 
            HUDManager.Instance.CreateToolAdModel(-100, item);
            doAdStuff(top, bottom);
        }

        public static void doAdStuff(string top, string bottom)
        {
            MysteryDice.ExtendedLogging($"Do Ad Stuff: {top} {bottom}");
            var hm = HUDManager.Instance;
            hm.advertTopText.text = top;
            hm.advertBottomText.text = bottom;
            if (hm.displayAdCoroutine != null)
            {
                hm.StopCoroutine(hm.displayAdCoroutine);
            }
            hm.displayAdCoroutine = hm.StartCoroutine(hm.displayAd());
        }
        
        public static void CreatePrefabAdModel(GameObject prefab)
        {
            GameObject adPrefab = GameObject.Instantiate(prefab, HUDManager.Instance.advertItemParent);
            foreach (var comp in prefab.GetComponents<MonoBehaviour>())
            {
                Object.Destroy(comp);
            }
            var netObj = adPrefab.GetComponent<NetworkObject>();
            if (netObj != null) Object.Destroy(netObj);

            var grabbable = adPrefab.GetComponent<GrabbableObject>();
            if (grabbable != null) Object.Destroy(grabbable);

            var collider = adPrefab.GetComponent<Collider>();
            if (collider != null) Object.Destroy(collider);
            adPrefab.transform.localPosition = Vector3.zero;
            adPrefab.transform.localScale = adPrefab.transform.localScale * 155f;
            adPrefab.transform.rotation = Quaternion.identity;
            Renderer[] componentsInChildren2 = adPrefab.GetComponentsInChildren<Renderer>();
            SetLayerRecursively(adPrefab, 23);
            adPrefab.SetActive(value: true);
        }
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
                SetLayerRecursively(child.gameObject, layer);
        }

        public static void showUnlockableAd(string unlockableName, string top, string bottom)
        {
            var unlockable = StartOfRound.Instance.unlockablesList.unlockables.Find(x=>x.unlockableName==unlockableName);
            if (unlockable == null)
            {
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogWarning("Show Unlockable Ad: Unlockable is Null. Huh");
                return;
            } 
            if( unlockable.prefabObject == null) 
            {
                if (MysteryDice.DebugLogging.Value) MysteryDice.CustomLogger.LogWarning("Show Unlockable Ad: Unlockable prefabObject is Null. Huh");
                return;
            } 
            HUDManager.Instance.CreateFurnitureAdModel(unlockable);
            doAdStuff(top, bottom);
        }
        
        public static void showRandomAd()
        {
            var topTextSelection = getTopText();
            var bottomTextSelection = getBottomText();
            bool tool = Random.value > 0.5f;
            if (tool)
            {
                var validItems = StartOfRound.Instance.allItemsList.itemsList
                    .Where(x=>x!=null && x.spawnPrefab!=null)
                    .ToList();
                var itemChoice = Random.Range(0, validItems.Count);
                var item = validItems[itemChoice];
                Networker.Instance.AdServerRPC(true, item.itemName, topTextSelection, bottomTextSelection);
            }
            else
            {
                var validUnlockables = StartOfRound.Instance.unlockablesList.unlockables
                    .Where(x => x.prefabObject != null)
                    .ToList();
                var unlockable = validUnlockables[Random.Range(0, validUnlockables.Count)];
                Networker.Instance.AdServerRPC(false, unlockable.unlockableName, topTextSelection, bottomTextSelection);
            }
        }
    }

    public class QueuedAd
    {
        public bool isTool;
        public bool isObject;
        public GameObject prefabObject;
        public string name;
        public string topText;
        public string bottomText;

        public QueuedAd(bool isTool, string name, string topText, string bottomText, bool isObject = false, GameObject prefabObject = null)
        {
            this.isTool = isTool;
            this.isObject = isObject;
            this.prefabObject = prefabObject;
            this.name = name;
            this.topText = topText;
            this.bottomText = bottomText;
        }
    }
}
