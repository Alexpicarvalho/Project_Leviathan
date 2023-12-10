using System;
using UnityEngine.UI;

[Serializable]
public class UIImageDetails
{
    public Image Image;
    public float StartAlpha { get; private set; }
    public float CurrentAlpha { get; set; }
    

    public UIImageDetails(Image image, float startAlpha)
    {
        Image = image;
        StartAlpha = startAlpha;
        CurrentAlpha = startAlpha;
    }
}
