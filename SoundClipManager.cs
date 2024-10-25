using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice
{
    internal class SoundClipManager
    {

        public static void playSound(string soundName)
        {
            AudioClip audioClip = null;
            switch (soundName) 
            {
                case "Jaws":
                    audioClip = MysteryDice.JawsSFX;
                    break;
                case "Dawg":
                    audioClip = MysteryDice.DawgSFX;
                    break;
            }
            AudioSource.PlayClipAtPoint(audioClip, GameNetworkManager.Instance.localPlayerController.transform.position, 10f);
        }
    }
}
