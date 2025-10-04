using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// ScriptableObject for defining tower traits
    /// </summary>
    [CreateAssetMenu(fileName = "New Tower Trait", menuName = "Tower Fusion/Tower Trait")]
    public class TowerTrait : ScriptableObject
    {
        [Header("Basic Info")]
        public string traitName;
        [TextArea(3, 5)]
        public string description;
        public Sprite traitIcon;
        public TraitCategory category;
        
        [Header("Visual Effects")]
        public Color overlayColor = Color.white;
        [Range(0f, 1f)]
        public float overlayAlpha = 0.3f;
        public Sprite overlaySprite; // Optional overlay sprite for the tower
        public ParticleSystem effectPrefab; // Optional particle effect
        
        [Header("Stat Modifications")]
        public float damageMultiplier = 1f;
        public float damageBonus = 0f;
        public float rangeMultiplier = 1f;
        public float rangeBonus = 0f;
        public float attackSpeedMultiplier = 1f;
        public float attackSpeedBonus = 0f;
        public float chargeTimeBonus = 0f; // Additional charge time (for Sniper trait)
        
        [Header("Special Effects")]
        public bool hasBurnEffect = false;
        public float burnDamagePerSecond = 10f;
        public float burnDuration = 3f;
        
        public bool hasSlowEffect = false;
        public float slowMultiplier = 0.7f; // 0.7 = 30% speed reduction
        public float slowDuration = 2f;
        
        public bool hasBrittleEffect = false;
        public float brittleDamageMultiplier = 1.25f; // +25% incoming damage
        public float brittleDuration = 3f;
        
        public bool hasChainEffect = false;
        public int chainTargets = 2;
        public float chainDamageMultiplier = 1f;
        public float chainRange = 2f;
        
        public bool hasGoldReward = false;
        public int goldPerKill = 1;
        
        /// <summary>
        /// Apply trait modifications to tower stats
        /// </summary>
        public TowerStats ApplyToStats(TowerStats baseStats)
        {
            TowerStats modifiedStats = new TowerStats
            {
                damage = (baseStats.damage + damageBonus) * damageMultiplier,
                range = (baseStats.range + rangeBonus) * rangeMultiplier,
                attackSpeed = (baseStats.attackSpeed + attackSpeedBonus) * attackSpeedMultiplier,
                chargeTime = baseStats.chargeTime + chargeTimeBonus
            };
            
            return modifiedStats;
        }
    }
    
    public enum TraitCategory
    {
        Elemental,
        Range,
        Utility,
        Support
    }
    
    /// <summary>
    /// Helper struct for tower stats calculations
    /// </summary>
    [System.Serializable]
    public struct TowerStats
    {
        public float damage;
        public float range;
        public float attackSpeed;
        public float chargeTime;
        
        public TowerStats(float damage, float range, float attackSpeed, float chargeTime = 0f)
        {
            this.damage = damage;
            this.range = range;
            this.attackSpeed = attackSpeed;
            this.chargeTime = chargeTime;
        }
    }
}