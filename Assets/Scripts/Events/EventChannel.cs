using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "EventChannel", menuName = "Events/EventChannel")]
public class EventChannel : ScriptableObject
{
    // Sugestion: el delegado es public mutable, asi que un suscriptor podria pisar (=) en lugar de sumar (+=) todos los listeners, o invocarlo desde afuera. Declararlo como 'event' (public event UnityAction OnEventTriggered;) protege la invocacion y solo permite +=/-=. (Aplica igual a Block/Int/StatsEventChannel.)
    public UnityAction OnEventTriggered;

    public void RaiseEvent()
    {
        OnEventTriggered?.Invoke();
    }
}
