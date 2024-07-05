using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blinking : MonoBehaviour
{
    public bool Glow { get; private set; }
    public float BlinkingTime;

    private float BlinkingTimer = 0f;
    private GameObject GlowSign, NormalSign;
    private bool Stop = false;
    void Start()
    {
        BlinkingTime = 0.5f;
        Glow = false;
        NormalSign = transform.Find("Emergency Sign").gameObject;
        GlowSign = transform.Find("Emergency Sign Glowing").gameObject;
    }

    public void HideSigns()
    {
        Stop = true;
        NormalSign.SetActive(false);
        GlowSign.SetActive(false);
    }

    void Update()
    {
        if (Stop) return;

        BlinkingTimer -= Time.deltaTime;
        if (BlinkingTimer <= 0f)
        {
            BlinkingTimer = BlinkingTime;
            if (Glow)
            {
                Glow = false;
                NormalSign.SetActive(true);
                GlowSign.SetActive(false);
            }
            else
            {
                Glow = true;
                NormalSign.SetActive(false);
                GlowSign.SetActive(true);
            }
        }
    }
}
