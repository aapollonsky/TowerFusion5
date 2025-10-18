using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// ScriptableObject for defining tower data
    /// </summary>
    [CreateAssetMenu(fileName = "New Tower", menuName = "Tower Fusion/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("Basic Info")]
        public string towerName;
        public string description;
        public Sprite towerSprite;
        public int buildCost = 50;
        
        [Header("Combat Stats")]
        public float damage = 25f;
        public float attackRange = 3f;
        public float attackSpeed = 1f; // Attacks per second
        public DamageType damageType = DamageType.Physical;
        
        [Header("Defense")]
        public float maxHealth = 100f;
        [Tooltip("Can this tower be destroyed by enemies?")]
        public bool isDestructible = true;
        
        [Header("Targeting")]
        public TargetingMode targetingMode = TargetingMode.First;
        public bool canTargetFlying = true;
        public bool canTargetGround = true;
        
        [Header("Projectile")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 10f;
        public bool isHitscan = false; // Instant hit vs projectile
        
        [Header("Special Effects")]
        public bool hasSplashDamage = false;
        public float splashRadius = 1f;
        public float splashDamageMultiplier = 0.5f;
        
        public bool hasSlowEffect = false;
        public float slowStrength = 0.5f; // 0.5 = 50% speed reduction
        public float slowDuration = 2f;
        
        public bool hasPoisonEffect = false;
        public float poisonDamage = 5f;
        public float poisonDuration = 3f;
        
        [Header("Upgrades")]
        public TowerUpgrade[] upgrades;
        
        [Header("Visual")]
        public Color towerColor = Color.white;
        public Vector3 scale = Vector3.one;
        
        [Header("Rotation")]
        public bool useRotationSprites = false;
        [Tooltip("Sprites for different rotation angles. Should be in order: 0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°")]
        public Sprite[] rotationSprites;
        [Tooltip("Angles corresponding to rotation sprites. Default: 0, 45, 90, 135, 180, 225, 270, 315")]
        public float[] spriteAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
        
        /// <summary>
        /// Get attack cooldown in seconds
        /// </summary>
        public float GetAttackCooldown()
        {
            return 1f / attackSpeed;
        }
    }
    
    public enum TargetingMode
    {
        First,      // First enemy in path
        Last,       // Last enemy in path
        Closest,    // Closest enemy
        Strongest,  // Enemy with most health
        Weakest     // Enemy with least health
    }
    
    [System.Serializable]
    public class TowerUpgrade
    {
        public string upgradeName;
        public string description;
        public int cost;
        public Sprite upgradeSprite;
        
        [Header("Stat Modifications")]
        public float damageIncrease = 0f;
        public float rangeIncrease = 0f;
        public float attackSpeedIncrease = 0f;
        
        [Header("New Abilities")]
        public bool addsSplashDamage = false;
        public bool addsSlowEffect = false;
        public bool addsPoisonEffect = false;
        public bool addsArmorPiercing = false;
    }
}