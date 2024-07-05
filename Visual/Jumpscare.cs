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

        Vector2 BaseSize = new Vector2(4f, 4f);
        void Start()
        {
            if (JumpscareGlitch.PussyMode)
                BaseSize = new Vector2(1f, 1f);

            NotScaryFace = transform.GetChild(0).gameObject;
            ScaryFace = transform.GetChild(1).gameObject;
            ScaryFace.SetActive(false);
            NotScaryFace.SetActive(false);
        }

        public void Scare()
        {
            StartCoroutine(ScareTime());
        }
        void Update()
        {
            if (!IsScaring) return;

            ScaryFace.transform.localScale = new Vector3(BaseSize.x + UnityEngine.Random.Range(0f, 0.2f), BaseSize.y + UnityEngine.Random.Range(0f, 0.2f),1f);
        }

        IEnumerator ScareTime()
        {
            AudioClip sfx = JumpscareGlitch.PussyMode ? MysteryDice.PurrSFX : MysteryDice.JumpscareSFX;

            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
            AudioSource.PlayClipAtPoint(sfx, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);

            GameObject faceToShow = JumpscareGlitch.PussyMode ? NotScaryFace : ScaryFace;

            IsScaring = true;
            faceToShow.SetActive(true);
            yield return new WaitForSeconds(1.3f);
            IsScaring = false;
            faceToShow.SetActive(false);
        }
    }
}
