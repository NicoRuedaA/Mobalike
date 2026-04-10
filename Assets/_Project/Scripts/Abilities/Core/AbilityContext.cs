using UnityEngine;
using MobaGameplay.Core;

namespace MobaGameplay.Abilities
{
    /// <summary>
    /// Context passed to ability behaviors at execution time.
    /// Contains everything a behavior needs to know: who cast it,
    /// where the target is, what data the ability has.
    /// </summary>
    public struct AbilityContext
    {
        /// <summary>Entity casting the ability</summary>
        public BaseEntity Owner;
        
        /// <summary>World position the player targeted</summary>
        public Vector3 TargetPosition;
        
        /// <summary>Entity under the cursor (null if targeting ground)</summary>
        public BaseEntity TargetEntity;
        
        /// <summary>Ability data (config values)</summary>
        public AbilityData Data;
        
        /// <summary>
        /// Convenience factory method.
        /// </summary>
        public static AbilityContext Create(BaseEntity owner, Vector3 targetPos, 
            BaseEntity targetEntity, AbilityData data)
        {
            return new AbilityContext
            {
                Owner = owner,
                TargetPosition = targetPos,
                TargetEntity = targetEntity,
                Data = data
            };
        }
    }
}