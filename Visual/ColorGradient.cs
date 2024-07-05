using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ColorGradient : MonoBehaviour
{
    public Renderer CubeRenderer, MoonRenderer, SunRenderer;
    public Color NightColor = new Color(103f / 255f, 99f / 255f, 226f / 255f);
    public Color DayColor = new Color(255f / 255f, 209f / 255f, 0f);

    private float ColorTimer = 0f;

    void Start()
    {
        CubeRenderer = GetComponent<Renderer>();
        SunRenderer = transform.Find("Sun").GetComponent<Renderer>();
        MoonRenderer = transform.Find("Moon").GetComponent<Renderer>();
    }
    void Update()
    {
        ColorTimer -= Time.deltaTime;

        if (ColorTimer >= 0f) return; //reducing load
        ColorTimer = 1f;

        Color newColor = DayColor + (NightColor - DayColor) * TimeOfDay.Instance.normalizedTimeOfDay;
        CubeRenderer.material.SetColor("_BaseColor", newColor);
        CubeRenderer.material.SetColor("_EmissiveColor", newColor * 3f);
        float moonAlpha = TimeOfDay.Instance.normalizedTimeOfDay;
        float sunAlpha = 1f - moonAlpha;
        MoonRenderer.material.SetColor("_BaseColor", new Color(1f,1f,1f, moonAlpha));
        SunRenderer.material.SetColor("_BaseColor", new Color(1f, 1f, 1f, sunAlpha));

        MoonRenderer.material.SetColor("_UnlitColor", new Color(1f, 1f, 1f, moonAlpha));
        SunRenderer.material.SetColor("_UnlitColor", new Color(1f, 1f, 1f, sunAlpha));

        MoonRenderer.material.SetColor("_MainColor", new Color(1f, 1f, 1f, moonAlpha));
        SunRenderer.material.SetColor("_MainColor", new Color(1f, 1f, 1f, sunAlpha));
    }
}
