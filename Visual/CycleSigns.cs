using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MysteryDice;
public class CycleSigns : MonoBehaviour
{
    class DiceVisuals
    {
        public Sprite Sprite;
        public Color ModelColor;
        public Color EmissionColor;
        public float Emission;

        public DiceVisuals(Sprite sprite, Color color, Color emissionColor, float emission)
        {
            Sprite = sprite;
            ModelColor = color;
            EmissionColor = emissionColor;
            Emission = emission;
        }
    }

    public float CycleTime = 1f;

    private float CurrentTimer = 0f;
    private int CurrentSprite = 0;
    private bool Stop = false;

    private SpriteRenderer SignSpriteRenderer;
    private Renderer DiceRenderer;

    List<DiceVisuals> Visuals = new List<DiceVisuals>();
    void Start()
    {
        Visuals.Add(new DiceVisuals(MysteryDice.MysteryDice.WarningJester, Color.yellow, Color.yellow,100f));
        Visuals.Add(new DiceVisuals(MysteryDice.MysteryDice.WarningBracken, Color.yellow, Color.yellow,100f));
        Visuals.Add(new DiceVisuals(MysteryDice.MysteryDice.WarningDeath, Color.red, Color.red,100f));
        Visuals.Add(new DiceVisuals(MysteryDice.MysteryDice.WarningLuck, Color.green, Color.green,300f));

        SignSpriteRenderer = transform.Find("Emergency Sign").gameObject.GetComponent<SpriteRenderer>();
        DiceRenderer = gameObject.GetComponent<Renderer>();
    }
    void Update()
    {
        if (Stop) return;
        CurrentTimer -=  Time.deltaTime;
        if(CurrentTimer <= 0f)
        {
            CurrentTimer = CycleTime;
            CycleSprite();
        }
    }

    void CycleSprite()
    {
        CurrentSprite++;
        if (CurrentSprite >= Visuals.Count)
            CurrentSprite = 0;

        SignSpriteRenderer.sprite = Visuals[CurrentSprite].Sprite;
        DiceRenderer.material.SetColor("_BaseColor", Visuals[CurrentSprite].ModelColor);
        DiceRenderer.material.SetColor("_EmissiveColor", Visuals[CurrentSprite].EmissionColor * Visuals[CurrentSprite].Emission);
    }

    public void HideSigns()
    {
        Stop = true;
        SignSpriteRenderer.gameObject.SetActive(false);
    }
}
