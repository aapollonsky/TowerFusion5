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
        
        [Header("Icon Badge System")]
        public Sprite traitBadge; // Small icon to display on tower
        public Vector2 badgeOffset = new Vector2(0.8f, 0.8f); // Offset from tower center for badge placement
        public float badgeScale = 1.2f; // Size of the badge (increased for better visibility)
        public bool animateBadge = true; // Whether the badge should animate (float/pulse)
        
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
        
        public bool hasExplosionEffect = false;
        public float explosionRadius = 2f;
        public float explosionDamageMultiplier = 0.75f; // 75% damage to explosion targets
        
        public bool hasEarthTrapEffect = false;
        public float trapDuration = 4f;
        public float trapRadius = 1f;
        public GameObject trapPrefab; // Visual prefab for the trap
        
        [Header("Projectile System (Optional)")]
        [Tooltip("If set, this trait will fire its own independent projectiles")]
        public bool hasIndependentProjectile = false;
        public GameObject projectilePrefab; // Trait-specific projectile
        [Tooltip("Seconds between shots (e.g., 2.0 = fires every 2 seconds, 0.5 = fires twice per second)")]
        public float projectileCooldown = 1f; // Seconds between shots
        public float projectileSpeed = 10f;
        public float projectileDamage = 25f;
        public DamageType projectileDamageType = DamageType.Magic;
        [Tooltip("Visual effect that appears at impact")]
        public GameObject projectileImpactEffect;
        [Tooltip("If true, projectile applies trait effects; if false, projectile is just visual")]
        public bool projectileAppliesTraitEffects = true;
        
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