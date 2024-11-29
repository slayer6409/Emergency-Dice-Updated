using LethalLib.Modules;
using MysteryDice.Effects;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MysteryDice.Visual
{
    public class Jumpscare : MonoBehaviour
    {
        bool IsScaring = false;
        GameObject ScaryFace = null;
        GameObject NotScaryFace = null;
        GameObject Emergency = null;

        Vector2 BaseSize = new Vector2(4f, 4f);
        void Start()
        {
            if (MysteryDice.pussyMode.Value)
                BaseSize = new Vector2(1f, 1f);

            NotScaryFace = transform.GetChild(0).gameObject;
            ScaryFace = transform.GetChild(1).gameObject;
            Emergency = transform.GetChild(2).gameObject;
            ScaryFace.SetActive(false);
            NotScaryFace.SetActive(false);
            Emergency.SetActive(false);
        }

        public void Scare()
        {
            StartCoroutine(ScareTime());
        }
        public void EmergencyMeeting()
        {
            StartCoroutine(ScareTime(true));
        }
        void Update()
        {
            if (!IsScaring) return;

            ScaryFace.transform.localScale = new Vector3(BaseSize.x + UnityEngine.Random.Range(0f, 0.2f), BaseSize.y + UnityEngine.Random.Range(0f, 0.2f),1f);
        }

        IEnumerator ScareTime(bool meeting = false)
        {
            
            //AudioClip sfx = MysteryDice.pussyMode.Value ? MysteryDice.PurrSFX : MysteryDice.JumpscareSFX;
            //if(meeting) sfx = MysteryDice.MeetingSFX;
            MysteryDice.sounds.TryGetValue(MysteryDice.pussyMode.Value ? "purr" : "glitch", out AudioClip sfx);
            if(meeting) MysteryDice.sounds.TryGetValue("Meeting_Sound",out sfx);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);

            GameObject faceToShow = MysteryDice.pussyMode.Value ? NotScaryFace : ScaryFace;
            if (meeting) faceToShow = Emergency;
            IsScaring = true;
            faceToShow.SetActive(true);
            yield return new WaitForSeconds(1.3f);
            IsScaring = false;
            faceToShow.SetActive(false);
        }
    }
}
