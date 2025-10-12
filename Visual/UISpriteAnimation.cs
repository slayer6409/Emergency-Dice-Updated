using UnityEngine;
using UnityEngine.UI;

namespace MysteryDice.Visual;

[RequireComponent(typeof(Image))]
public class UISpritesAnimation : MonoBehaviour
{
    public float duration;

    [SerializeField] private Sprite[] sprites;
    
    private Image image;
    private int index = 0;
    private float timer = 0;

    void Start()
    {
        image = GetComponent<Image>();
    }
    private void Update()
    {
        if((timer+=Time.deltaTime) >= (duration / sprites.Length))
        {
            timer = 0;
            image.sprite = sprites[index];
            index = (index + 1) % sprites.Length;
        }
    }
}