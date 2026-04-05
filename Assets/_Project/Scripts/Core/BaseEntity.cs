using UnityEngine;

namespace MobaGameplay.Core
{
    public abstract class BaseEntity : MonoBehaviour
    {
        public BaseMovement Movement { get; private set; }

        protected virtual void Awake()
        {
            Movement = GetComponent<BaseMovement>();
        }
    }
}
