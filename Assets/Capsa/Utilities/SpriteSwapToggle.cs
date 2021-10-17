using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SpriteSwapToggle : MonoBehaviour
{
    public Sprite onSprite;
    public Sprite offSprite;
    public Image preview;

    private void Start()
    {
        if(!preview)
            preview = GetComponent<Image>();
    }

    internal void SetToggle(bool v)
    {
        preview.sprite = v ? onSprite : offSprite;
    }
}