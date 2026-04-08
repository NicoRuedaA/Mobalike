namespace MobaGameplay.AI
{
    /// <summary>
    /// Estados del AI de enemigo.
    /// </summary>
    public enum EnemyState
    {
        /// <summary>No tiene objetivo, esperando.</summary>
        Idle,
        
        /// <summary>Moviéndose entre puntos de patrulla.</summary>
        Patrol,
        
        /// <summary>Persiguiendo al jugador.</summary>
        Chase,
        
        /// <summary>En rango de ataque, atacando.</summary>
        Attack,
        
        /// <summary>Baja vida, huyendo.</summary>
        Retreat,
        
        /// <summary>Enemigo muerto, limpieza.</summary>
        Dead
    }
}
