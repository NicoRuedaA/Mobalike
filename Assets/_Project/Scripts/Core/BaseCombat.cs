using System;
using UnityEngine;

namespace MobaGameplay.Core
{
    public abstract class BaseCombat : MonoBehaviour
    {
        // Evento que se dispara cuando el personaje realiza un ataque básico
        public event Action OnBasicAttack;

        public virtual void BasicAttack()
        {
            OnBasicAttack?.Invoke();
        }
    }
}
