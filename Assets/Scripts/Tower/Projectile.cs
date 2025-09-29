using UnityEngine;

namespace TowerFusion
{
    /// <summary>
    /// Projectile behavior for tower attacks
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Configuration")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private Collider2D projectileCollider;
        
        [Header("Effects")]
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private GameObject trailEffectPrefab;
        
        // Projectile data
        private Enemy targetEnemy;
        private float damage;
        private DamageType damageType;
        private float speed;
        private bool isInitialized = false;
        
        // Special effects
        private TowerData originTowerData;
        
        // Movement
        private Vector3 targetPosition;
        private bool hasReachedTarget = false;
        
        private void Awake()
        {
            if (rb2D == null)
                rb2D = GetComponent<Rigidbody2D>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (projectileCollider == null)
                projectileCollider = GetComponent<Collider2D>();
        }
        
        /// <summary>
        /// Initialize projectile with target and damage data
        /// </summary>
        public void Initialize(Enemy target, float projectileDamage, DamageType type, float projectileSpeed)
        {
            targetEnemy = target;
            damage = projectileDamage;
            damageType = type;
            speed = projectileSpeed;
            
            if (target != null)
            {
                targetPosition = target.Position;
            }
            
            isInitialized = true;
            
            // Setup movement
            SetupMovement();
            
            // Create trail effect
            if (trailEffectPrefab != null)
            {
                Instantiate(trailEffectPrefab, transform);
            }
            
            // Destroy after maximum lifetime
            Destroy(gameObject, 5f);
        }
        
        /// <summary>
        /// Set special effects data from tower
        /// </summary>
        public void SetSpecialEffects(TowerData towerData)
        {
            originTowerData = towerData;
        }
        
        /// <summary>
        /// Setup projectile movement
        /// </summary>
        private void SetupMovement()
        {
            if (rb2D == null)
                return;
            
            Vector3 direction;
            
            if (targetEnemy != null && targetEnemy.IsAlive)
            {
                // Lead the target for better accuracy
                Vector3 predictedPosition = targetEnemy.GetPredictedPosition(0.2f);
                direction = (predictedPosition - transform.position).normalized;
            }
            else
            {
                direction = (targetPosition - transform.position).normalized;
            }
            
            rb2D.velocity = direction * speed;
            
            // Rotate to face movement direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        
        private void Update()
        {
            if (!isInitialized)
                return;
            
            UpdateTargeting();
            CheckForImpact();
        }
        
        /// <summary>
        /// Update targeting for homing projectiles
        /// </summary>
        private void UpdateTargeting()
        {
            // For now, projectiles don't home. This can be extended for homing missiles
            return;
        }
        
        /// <summary>
        /// Check if projectile has reached its target
        /// </summary>
        private void CheckForImpact()
        {
            if (hasReachedTarget)
                return;
            
            if (targetEnemy != null && targetEnemy.IsAlive)
            {
                float distance = Vector3.Distance(transform.position, targetEnemy.Position);
                if (distance <= 0.1f)
                {
                    Impact(targetEnemy);
                }
            }
            else
            {
                // Target is dead or null, check if we've reached the target position
                float distance = Vector3.Distance(transform.position, targetPosition);
                if (distance <= 0.1f)
                {
                    Impact(null);
                }
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasReachedTarget)
                return;
            
            Enemy hitEnemy = other.GetComponent<Enemy>();
            if (hitEnemy != null && hitEnemy.IsAlive)
            {
                // Hit an enemy
                if (targetEnemy == null || hitEnemy == targetEnemy)
                {
                    Impact(hitEnemy);
                }
            }
        }
        
        /// <summary>
        /// Handle projectile impact
        /// </summary>
        private void Impact(Enemy hitEnemy)
        {
            if (hasReachedTarget)
                return;
            
            hasReachedTarget = true;
            
            // Stop movement
            if (rb2D != null)
            {
                rb2D.velocity = Vector3.zero;
            }
            
            // Deal damage
            if (hitEnemy != null && hitEnemy.IsAlive)
            {
                hitEnemy.TakeDamage(damage, damageType);
                ApplySpecialEffects(hitEnemy);
            }
            
            // Handle splash damage
            if (originTowerData != null && originTowerData.hasSplashDamage)
            {
                ApplySplashDamage();
            }
            
            // Create impact effect
            CreateImpactEffect();
            
            // Destroy projectile
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Apply special effects to hit enemy
        /// </summary>
        private void ApplySpecialEffects(Enemy enemy)
        {
            if (originTowerData == null || enemy == null)
                return;
            
            // TODO: Implement slow, poison, and other effects
            if (originTowerData.hasSlowEffect)
            {
                // Apply slow effect
                Debug.Log($"Applied slow effect to {enemy.EnemyData?.enemyName}");
            }
            
            if (originTowerData.hasPoisonEffect)
            {
                // Apply poison effect
                Debug.Log($"Applied poison effect to {enemy.EnemyData?.enemyName}");
            }
        }
        
        /// <summary>
        /// Apply splash damage to nearby enemies
        /// </summary>
        private void ApplySplashDamage()
        {
            if (originTowerData == null)
                return;
            
            var nearbyEnemies = EnemyManager.Instance?.FindEnemiesInRange(transform.position, originTowerData.splashRadius);
            if (nearbyEnemies == null)
                return;
            
            float splashDamage = damage * originTowerData.splashDamageMultiplier;
            
            foreach (Enemy enemy in nearbyEnemies)
            {
                if (enemy != null && enemy.IsAlive && enemy != targetEnemy)
                {
                    enemy.TakeDamage(splashDamage, damageType);
                }
            }
            
            Debug.Log($"Splash damage dealt to {nearbyEnemies.Count} enemies");
        }
        
        /// <summary>
        /// Create visual impact effect
        /// </summary>
        private void CreateImpactEffect()
        {
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        
        /// <summary>
        /// Set custom projectile sprite
        /// </summary>
        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer != null && sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
        
        /// <summary>
        /// Set custom projectile color
        /// </summary>
        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }
    }
}