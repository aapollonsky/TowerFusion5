using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// ScriptableObject for defining enemy data
    /// </summary>
    [CreateAssetMenu(fileName = "New Enemy", menuName = "Tower Fusion/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName;
        public string description;
        public Sprite enemySprite;
        
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
        
        [Header("Visual")]
        public Color enemyColor = Color.white;
        public Vector3 scale = Vector3.one;
        
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