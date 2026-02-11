using UnityEngine;


namespace RedCard {
    public interface IBaseEvent { }

    public class PlayerThrowInEvent : IBaseEvent {
        public PlayerThrowInEvent(Jugador jugador) { }
    }
}

