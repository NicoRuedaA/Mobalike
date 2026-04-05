using UnityEngine;

namespace MobaGameplay.Core
{
    [RequireComponent(typeof(BaseEntity))]
    public abstract class BaseController : MonoBehaviour
    {
        protected BaseEntity entity;

        protected virtual void Awake()
        {
            entity = GetComponent<BaseEntity>();
        }
    }
}
