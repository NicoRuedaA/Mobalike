using UnityEngine;
using MobaGameplay.UI.Targeting;

namespace MobaGameplay.Core
{
    public class HeroEntity : BaseEntity
    {
        // En el futuro, aquí irá la lógica específica de héroes: Subir de Nivel, Inventario, Oro, etc.
        protected override void Awake()
        {
            base.Awake();
            
            // Ensure TargetingManager knows about this player
            if (TargetingManager.Instance != null)
            {
                TargetingManager.Instance.Initialize(transform);
            }
        }
    }
}
