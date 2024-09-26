using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
public class TextManager : MonoBehaviour, IPointerClickHandler
{
    public Action<string> callback;
    public string textValue = "text.missing";
    public TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        // TODO - Dynamic text setting based on language
        // Parse -> lookup -> display
        text.SetText(textValue);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[Button] " + this.textValue);
        callback?.Invoke(text.text);
    }
}
