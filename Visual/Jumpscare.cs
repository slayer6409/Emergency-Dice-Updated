using LethalLib.Modules;
using MysteryDice.Effects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Visual
{
    public class Jumpscare : MonoBehaviour
    {
        bool IsScaring = false;
        GameObject ScaryFace = null;
        GameObject Emergency = null;
        public List<KeyValuePair<GameObject, ScareFace>> scarePairs = new();


        public int getIntNonScary()
        {
            int index = -1;

            List<int> stuff = new();
            if (MysteryDice.insideJoke.Value)
            {
                stuff = scarePairs.Select((pair, i) => (pair, i))
                    .Where(p => p.pair.Key != ScaryFace && p.pair.Key != Emergency && p.pair.Key.name!= "Bald" && p.pair.Key.name != "rigo" && p.pair.Key.name != "funo" && p.pair.Key.name != "tree")
                    .Select(p => p.i)
                    .ToList();
            }
            else
            { 
                stuff = scarePairs
                .Select((pair, i) => (pair, i))
                .Where(p => p.pair.Key != ScaryFace && p.pair.Key != Emergency)
                .Select(p => p.i)
                .ToList();
            }
           

            if (stuff.Count > 0)
                index = stuff[Random.Range(0, stuff.Count)];
            else
                MysteryDice.CustomLogger.LogWarning("getIntNonScary: No valid non-scary entries found.");
            return index;
        }

        public int getIntScary()
        {
            int index = scarePairs.FindIndex(p => p.Key == ScaryFace);
            if (index == -1)
                MysteryDice.CustomLogger.LogWarning("getIntScary: ScaryFace not found in scarePairs.");
            return index;
        }

        public int GetIntEmergency()
        {
            int index = scarePairs.FindIndex(p => p.Key == Emergency);
            if (index == -1)
                MysteryDice.CustomLogger.LogWarning("GetIntEmergency: Emergency face not found in scarePairs.");
            return index;
        }
        
        Vector2 BaseSize = new Vector2(4f, 4f);
        void Start()
        {
            if (MysteryDice.pussyMode.Value)
                BaseSize = new Vector2(1f, 1f);
            scarePairs = new();

            foreach (Transform child in transform)
            {
                GameObject face = child.gameObject;
                var sf = face.GetComponent<ScareFace>();
                face.SetActive(false);

                
                if (sf == null) continue;
                scarePairs.Add(new(face, sf));
                if (face.name.Equals("Creepy", System.StringComparison.OrdinalIgnoreCase) || face.name.Equals("ScaryFace"))
                    ScaryFace = face;
                else if (face.name.Equals("Emergency", System.StringComparison.OrdinalIgnoreCase))
                    Emergency = face;
            }
            if (ScaryFace == null)
                MysteryDice.CustomLogger.LogWarning("ScaryFace not found!");
            if (Emergency == null)
                MysteryDice.CustomLogger.LogWarning("Emergency face not found!");
        }

        
        public void Scare(int sync)
        {
            StartCoroutine(ScareTime(sync));
        }
        void Update()
        {
            if (!IsScaring) return;

            ScaryFace.transform.localScale = new Vector3(BaseSize.x + UnityEngine.Random.Range(0f, 0.2f), BaseSize.y + UnityEngine.Random.Range(0f, 0.2f),1f);
        }

        IEnumerator ScareTime(int sync, bool meeting = false)
        {
            KeyValuePair<GameObject, ScareFace> selected = scarePairs.ElementAt(sync);

            GameObject faceToShow = selected.Key;
            string clipKey = selected.Value.getRandomSoundKey();
            
            if (!MysteryDice.pussyMode.Value)
            {
                faceToShow = ScaryFace;
                clipKey = "glitch";
            }
            if (meeting)
            {
                faceToShow = Emergency;
                clipKey = "Meeting_Sound";
            }

            if (MysteryDice.sounds.TryGetValue(clipKey, out AudioClip clip) && clip != null)
            {
                GameObject tempAudio = new GameObject("TempAudio");
                tempAudio.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;

                AudioSource source = tempAudio.AddComponent<AudioSource>();
                source.clip = clip;
                source.spatialBlend = 0f;
                source.minDistance = 5f;
                source.maxDistance = 15f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.dopplerLevel = 0f;
                source.volume = MysteryDice.SoundVolume.Value;
                source.playOnAwake = false;
                source.pitch = Random.Range(0.8f, 1.2f);
                source.Play();

                GameObject.Destroy(tempAudio, clip.length);
            }
            else
            {
                MysteryDice.CustomLogger.LogWarning($"Failed to find audio clip for key: '{clipKey}'");
            }

            IsScaring = true;
            faceToShow.SetActive(true);
            yield return new WaitForSeconds(1.3f);
            IsScaring = false;
            faceToShow.SetActive(false);
        }
    }

    public class ScareFace : MonoBehaviour
    {
        public List<string> soundKeys = new List<string>();

        public string getRandomSoundKey()
        {
            if (soundKeys == null || soundKeys.Count == 0)
                return "purr";
            return soundKeys[Random.Range(0, soundKeys.Count)];
        }
    }
}
