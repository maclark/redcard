using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public System.Action<PointerEventData> onClick;
    public System.Action<PointerEventData> onEnter;
    public System.Action<PointerEventData> onExit;

    public void OnPointerClick(PointerEventData eventData) {
        onClick?.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        onEnter?.Invoke(eventData);
    }

    public void OnPointerExit(PointerEventData eventData) {
        onExit?.Invoke(eventData);
    }
}
