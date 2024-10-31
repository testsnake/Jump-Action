using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackgroundManager : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public Image backgroundImage;

    void Start()
    {
        // Set the background image to appear behind the text
        backgroundImage.transform.SetSiblingIndex(tmpText.transform.GetSiblingIndex() - 1);
    }
}
