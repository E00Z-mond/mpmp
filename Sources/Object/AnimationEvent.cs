using UnityEngine;
using UnityEngine.Events;
public class AnimationEvent : MonoBehaviour
{
    [SerializeField]
    UnityEvent[] events;
    public void EventCall(int index)
    {
        if (index >= 0 && index < events.Length)
        {
            events[index]?.Invoke();

        }
        else
        {
            Debug.LogError("Index out of range");
        }
    }
}
