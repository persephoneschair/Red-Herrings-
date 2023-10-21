using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Led : MonoBehaviour
{
    // Start is called before the first frame update
    private Image image;
    public Color[] colors;

    void Awake()
    {
        GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(20f, 160f));
        image = GetComponent<Image>();
    }

    public enum BulbColor
    {
        Red,
        Green,
        Blue,
        Yellow,
        Orange,
        Purple
    }

    public void SetBulbColor(BulbColor col)
    {
        image.color = colors[(int)col];
    }
}
