using UnityEngine;
using UnityEngine.UI;

public class CrossHairs : MonoBehaviour
{
    public bool updateSettings = false;
    public int pixelGap = 10;
    public int hairLength = 20;
    public int hairWidth = 20;
    public int outlineThickness = 2;
    public Color color = Color.green;

    public Image vertTop;
    public Image vertTopOutline;
    public Image vertBot;
    public Image vertBotOutline;

    public Image horzRight;
    public Image horzRightOutline;
    public Image horzLeft;
    public Image horzLeftOutline;

    private void Awake() {
        updateSettings = true;
    }

    private void Update() {
        
        if (updateSettings) {
            updateSettings = false;

            vertTop.color = color;
            vertBot.color = color;
            horzLeft.color = color;
            horzRight.color = color;

            vertTopOutline.rectTransform.localPosition = new Vector2(0f, pixelGap + hairLength / 2f);
            vertBotOutline.rectTransform.localPosition = new Vector2(0f, -pixelGap - hairLength / 2f);
            horzLeftOutline.rectTransform.localPosition = new Vector2(pixelGap + hairLength / 2f, 0f);
            horzRightOutline.rectTransform.localPosition = new Vector2(-pixelGap - hairLength / 2f, 0f);

            vertTop.rectTransform.sizeDelta = new Vector2(hairWidth, hairLength);
            vertBot.rectTransform.sizeDelta = new Vector2(hairWidth, hairLength);
            horzLeft.rectTransform.sizeDelta = new Vector2(hairLength, hairWidth);
            horzRight.rectTransform.sizeDelta = new Vector2(hairLength, hairWidth);

            Vector2 w = 2 * outlineThickness * Vector2.one;
            vertTopOutline.rectTransform.sizeDelta = w + new Vector2(hairWidth, hairLength);
            vertBotOutline.rectTransform.sizeDelta = w + new Vector2(hairWidth, hairLength);
            horzLeftOutline.rectTransform.sizeDelta = w + new Vector2(hairLength, hairWidth);
            horzRightOutline.rectTransform.sizeDelta = w +new Vector2(hairLength, hairWidth);
        }
    }
}
