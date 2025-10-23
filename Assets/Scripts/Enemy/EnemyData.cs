using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Container for directional sprites
    /// </summary>
    [System.Serializable]
    public class DirectionalSprites
    {
        [Header("8-Direction Sprites (optional)")]
        [Tooltip("Right direction (0°)")]
        public Sprite right;
        
        [Tooltip("Down-Right direction (45°)")]
        public Sprite downRight;
        
        [Tooltip("Down direction (90°)")]
        public Sprite down;
        
        [Tooltip("Down-Left direction (135°)")]
        public Sprite downLeft;
        
        [Tooltip("Left direction (180°)")]
        public Sprite left;
        
        [Tooltip("Up-Left direction (225°)")]
        public Sprite upLeft;
        
        [Tooltip("Up direction (270°)")]
        public Sprite up;
        
        [Tooltip("Up-Right direction (315°)")]
        public Sprite upRight;
        
        [Header("4-Direction Sprites (alternative)")]
        [Tooltip("If only 4-direction sprites available")]
        public bool useOnly4Directions = false;
        
        [Header("Animation")]
        [Tooltip("Enable sprite animation per direction")]
        public bool useAnimation = false;
        
        [Tooltip("Animation frames for each direction (experimental)")]
        public AnimationFrames animationFrames;
        
        /// <summary>
        /// Get sprite for given angle in degrees (0° = right, 90° = down, etc.)
        /// </summary>
        public Sprite GetSpriteForAngle(float angleDegrees)
        {
            // Normalize angle to 0-360 range
            while (angleDegrees < 0) angleDegrees += 360;
            while (angleDegrees >= 360) angleDegrees -= 360;
            
            if (useOnly4Directions)
            {
                return Get4DirectionSprite(angleDegrees);
            }
            else
            {
                return Get8DirectionSprite(angleDegrees);
            }
        }
        
        private Sprite Get8DirectionSprite(float angle)
        {
            // 8-direction mapping with 45-degree sectors
            if (angle >= 337.5f || angle < 22.5f) return right;
            else if (angle >= 22.5f && angle < 67.5f) return downRight;
            else if (angle >= 67.5f && angle < 112.5f) return down;
            else if (angle >= 112.5f && angle < 157.5f) return downLeft;
            else if (angle >= 157.5f && angle < 202.5f) return left;
            else if (angle >= 202.5f && angle < 247.5f) return upLeft;
            else if (angle >= 247.5f && angle < 292.5f) return up;
            else if (angle >= 292.5f && angle < 337.5f) return upRight;
            
            return right; // Fallback
        }
        
        private Sprite Get4DirectionSprite(float angle)
        {
            // 4-direction mapping with 90-degree sectors
            if (angle >= 315f || angle < 45f) return right;
            else if (angle >= 45f && angle < 135f) return down;
            else if (angle >= 135f && angle < 225f) return left;
            else if (angle >= 225f && angle < 315f) return up;
            
            return right; // Fallback
        }
        
        /// <summary>
        /// Check if any directional sprites are assigned
        /// </summary>
        public bool HasDirectionalSprites()
        {
            return right != null || down != null || left != null || up != null ||
                   downRight != null || downLeft != null || upLeft != null || upRight != null;
        }
    }
    
    /// <summary>
    /// Animation frames for directional sprites
    /// </summary>
    [System.Serializable]
    public class AnimationFrames
    {
        [Header("Animation Settings")]
        public float frameRate = 8f;
        public bool looping = true;
        
        [Header("Directional Animation Frames")]
        public Sprite[] rightFrames;
        public Sprite[] downRightFrames;
        public Sprite[] downFrames;
        public Sprite[] downLeftFrames;
        public Sprite[] leftFrames;
        public Sprite[] upLeftFrames;
        public Sprite[] upFrames;
        public Sprite[] upRightFrames;
        
        /// <summary>
        /// Get animation frames for given angle
        /// </summary>
        public Sprite[] GetFramesForAngle(float angleDegrees)
        {
            // Normalize angle
            while (angleDegrees < 0) angleDegrees += 360;
            while (angleDegrees >= 360) angleDegrees -= 360;
            
            // 8-direction mapping
            if (angleDegrees >= 337.5f || angleDegrees < 22.5f) return rightFrames;
            else if (angleDegrees >= 22.5f && angleDegrees < 67.5f) return downRightFrames;
            else if (angleDegrees >= 67.5f && angleDegrees < 112.5f) return downFrames;
            else if (angleDegrees >= 112.5f && angleDegrees < 157.5f) return downLeftFrames;
            else if (angleDegrees >= 157.5f && angleDegrees < 202.5f) return leftFrames;
            else if (angleDegrees >= 202.5f && angleDegrees < 247.5f) return upLeftFrames;
            else if (angleDegrees >= 247.5f && angleDegrees < 292.5f) return upFrames;
            else if (angleDegrees >= 292.5f && angleDegrees < 337.5f) return upRightFrames;
            
            return rightFrames; // Fallback
        }
    }

    /// <summary>
    /// ScriptableObject for defining enemy data
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Tower Fusion/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName;
        public string description;
        
        [Header("Sprites")]
        public Sprite enemySprite; // Fallback sprite if directional sprites not set
        public DirectionalSprites directionalSprites;
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 2f;
        public int goldReward = 10;
        public int damageToPlayer = 1;
        
        [Header("Resistances")]
        [Range(0f, 1f)] public float physicalResistance = 0f;
        [Range(0f, 1f)] public float magicResistance = 0f;
        [Range(0f, 1f)] public float fireResistance = 0f;
        [Range(0f, 1f)] public float iceResistance = 0f;
        
        [Header("Special Abilities")]
        public bool canFly = false;
        public bool isArmored = false;
        public bool isFast = false;
        public bool isRegenerating = false;
        public float regenerationRate = 0f;
        
        [Header("Corn Stealing")]
        [Tooltip("Time to grab corn when reaching storage")]
        public float cornGrabDuration = 1f;
        [Tooltip("Speed multiplier when carrying corn (e.g., 0.8 = 20% slower)")]
        [Range(0.1f, 2f)] public float cornCarrySpeedMultiplier = 0.8f;
        
        [Header("Tower Attacking")]
        [Tooltip("Can this enemy attack towers? (Recommended: true for Attackers, false for pure Stealers)")]
        public bool canAttackTowers = true;
        [Tooltip("Damage dealt to towers per attack")]
        public float towerAttackDamage = 10f;
        [Tooltip("Range at which enemy can attack towers")]
        public float towerAttackRange = 0.5f;
        [Tooltip("Time between attacks on towers (seconds)")]
        public float towerAttackCooldown = 1f;
        [Tooltip("Detection range for finding towers to attack")]
        public float towerDetectionRange = 10f;
        
        [Header("Visual")]
        public Color enemyColor = Color.white;
        public Vector3 scale = Vector3.one;
        
        [Header("Flocking/Separation")]
        [Tooltip("Enable separation force to prevent clumping")]
        public bool useSeparation = true;
        [Tooltip("Radius within which enemies repel each other")]
        [Range(0.1f, 5f)] public float separationRadius = 1f;
        [Tooltip("Strength of separation force")]
        [Range(0f, 10f)] public float separationStrength = 2f;
        
        /// <summary>
        /// Calculate damage after resistances
        /// </summary>
        public float CalculateDamage(float baseDamage, DamageType damageType)
        {
            float resistance = 0f;
            
            switch (damageType)
            {
                case DamageType.Physical:
                    resistance = physicalResistance;
                    break;
                case DamageType.Magic:
                    resistance = magicResistance;
                    break;
                case DamageType.Fire:
                    resistance = fireResistance;
                    break;
                case DamageType.Ice:
                    resistance = iceResistance;
                    break;
            }
            
            return baseDamage * (1f - resistance);
        }
    }
    
    public enum DamageType
    {
        Physical,
        Magic,
        Fire,
        Ice
    }
}