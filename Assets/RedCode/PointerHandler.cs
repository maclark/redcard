using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public System.Action<PointerEventData> onEnter;
    public System.Action<PointerEventData> onExit;

    public void OnPointerEnter(PointerEventData eventData) {
        onEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        onExit?.Invoke(eventData);
    }
}
