using UnityEngine;
using System.Collections;

namespace TowerFusion
{
    /// <summary>
    /// Individual tower behavior controller
    /// </summary>
    public class Tower : MonoBehaviour
    {
        [Header("Tower Configuration")]
        [SerializeField] private TowerData towerData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform firePoint;
        
        [Header("Range Visualization")]
        [SerializeField] private GameObject rangeIndicator;
        [SerializeField] private CircleCollider2D rangeCollider;
        
        // Current state
        private Enemy currentTarget;
        private float lastAttackTime;
        private int currentUpgradeLevel = 0;
        
        // Modified stats (after upgrades)
        private float modifiedDamage;
        private float modifiedRange;
        private float modifiedAttackSpeed;
        private bool hasModifiedEffects;
        
        // Events
        public System.Action<Tower, Enemy> OnTowerAttack;
        public System.Action<Tower> OnTowerUpgraded;
        
        // Properties
        public TowerData TowerData => towerData;
        public Enemy CurrentTarget => currentTarget;
        public int UpgradeLevel => currentUpgradeLevel;
        public float ModifiedDamage => modifiedDamage;
        public float ModifiedRange => modifiedRange;
        public Vector3 Position => transform.position;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (rangeCollider == null)
                rangeCollider = GetComponent<CircleCollider2D>();
        }
        
        /// <summary>
        /// Initialize tower with data
        /// </summary>
        public void Initialize(TowerData data)
        {
            towerData = data;
            
            // Initialize modified stats with base values
            modifiedDamage = towerData.damage;
            modifiedRange = towerData.attackRange;
            modifiedAttackSpeed = towerData.attackSpeed;
            
            SetupVisuals();
            SetupRangeCollider();
            
            lastAttackTime = -towerData.GetAttackCooldown(); // Allow immediate first attack
        }
        
        /// <summary>
        /// Setup visual appearance
        /// </summary>
        private void SetupVisuals()
        {
            if (spriteRenderer != null && towerData.towerSprite != null)
            {
                spriteRenderer.sprite = towerData.towerSprite;
                spriteRenderer.color = towerData.towerColor;
            }
            
            transform.localScale = towerData.scale;
            
            // Setup range indicator
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(false);
                rangeIndicator.transform.localScale = Vector3.one * (modifiedRange * 2f);
            }
        }
        
        /// <summary>
        /// Setup range detection collider
        /// </summary>
        private void SetupRangeCollider()
        {
            if (rangeCollider != null)
            {
                rangeCollider.radius = modifiedRange;
                rangeCollider.isTrigger = true;
            }
        }
        
        private void Update()
        {
            if (towerData == null)
                return;
            
            UpdateTargeting();
            TryAttack();
        }
        
        /// <summary>
        /// Update current target based on targeting mode
        /// </summary>
        private void UpdateTargeting()
        {
            if (currentTarget != null && IsValidTarget(currentTarget))
                return; // Keep current target if still valid
            
            currentTarget = FindTarget();
        }
        
        /// <summary>
        /// Find a new target based on targeting mode
        /// </summary>
        private Enemy FindTarget()
        {
            var enemiesInRange = EnemyManager.Instance?.FindEnemiesInRange(transform.position, modifiedRange);
            if (enemiesInRange == null || enemiesInRange.Count == 0)
                return null;
            
            // Filter enemies based on tower capabilities
            enemiesInRange.RemoveAll(enemy => !CanTargetEnemy(enemy));
            
            if (enemiesInRange.Count == 0)
                return null;
            
            // Select target based on targeting mode
            Enemy target = null;
            
            switch (towerData.targetingMode)
            {
                case TargetingMode.First:
                    // TODO: Implement path-based targeting
                    target = enemiesInRange[0];
                    break;
                    
                case TargetingMode.Last:
                    // TODO: Implement path-based targeting
                    target = enemiesInRange[enemiesInRange.Count - 1];
                    break;
                    
                case TargetingMode.Closest:
                    float closestDistance = float.MaxValue;
                    foreach (var enemy in enemiesInRange)
                    {
                        float distance = Vector3.Distance(transform.position, enemy.Position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            target = enemy;
                        }
                    }
                    break;
                    
                case TargetingMode.Strongest:
                    float maxHealth = 0f;
                    foreach (var enemy in enemiesInRange)
                    {
                        if (enemy.CurrentHealth > maxHealth)
                        {
                            maxHealth = enemy.CurrentHealth;
                            target = enemy;
                        }
                    }
                    break;
                    
                case TargetingMode.Weakest:
                    float minHealth = float.MaxValue;
                    foreach (var enemy in enemiesInRange)
                    {
                        if (enemy.CurrentHealth < minHealth)
                        {
                            minHealth = enemy.CurrentHealth;
                            target = enemy;
                        }
                    }
                    break;
            }
            
            return target;
        }
        
        /// <summary>
        /// Check if tower can target a specific enemy
        /// </summary>
        private bool CanTargetEnemy(Enemy enemy)
        {
            if (enemy == null || !enemy.IsAlive)
                return false;
            
            if (enemy.EnemyData.canFly && !towerData.canTargetFlying)
                return false;
            
            if (!enemy.EnemyData.canFly && !towerData.canTargetGround)
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Check if target is still valid
        /// </summary>
        private bool IsValidTarget(Enemy target)
        {
            if (target == null || !target.IsAlive)
                return false;
            
            float distance = Vector3.Distance(transform.position, target.Position);
            if (distance > modifiedRange)
                return false;
            
            return CanTargetEnemy(target);
        }
        
        /// <summary>
        /// Try to attack current target
        /// </summary>
        private void TryAttack()
        {
            if (currentTarget == null)
                return;
            
            float timeSinceLastAttack = Time.time - lastAttackTime;
            float attackCooldown = 1f / modifiedAttackSpeed;
            
            if (timeSinceLastAttack >= attackCooldown)
            {
                Attack(currentTarget);
                lastAttackTime = Time.time;
            }
        }
        
        /// <summary>
        /// Attack the target
        /// </summary>
        private void Attack(Enemy target)
        {
            if (target == null)
                return;
            
            // Rotate towards target
            RotateTowardsTarget(target);
            
            if (towerData.isHitscan)
            {
                // Instant hit
                DealDamageToTarget(target);
            }
            else
            {
                // Create projectile
                CreateProjectile(target);
            }
            
            OnTowerAttack?.Invoke(this, target);
        }
        
        /// <summary>
        /// Rotate tower to face target
        /// </summary>
        private void RotateTowardsTarget(Enemy target)
        {
            Vector3 direction = target.Position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
        
        /// <summary>
        /// Deal damage directly to target (hitscan)
        /// </summary>
        private void DealDamageToTarget(Enemy target)
        {
            target.TakeDamage(modifiedDamage, towerData.damageType);
            
            // Apply special effects
            ApplySpecialEffects(target);
        }
        
        /// <summary>
        /// Create projectile towards target
        /// </summary>
        private void CreateProjectile(Enemy target)
        {
            if (towerData.projectilePrefab == null)
            {
                // Fallback to hitscan if no projectile
                DealDamageToTarget(target);
                return;
            }
            
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            GameObject projectileObj = Instantiate(towerData.projectilePrefab, spawnPosition, transform.rotation);
            
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(target, modifiedDamage, towerData.damageType, towerData.projectileSpeed);
                projectile.SetSpecialEffects(towerData);
            }
        }
        
        /// <summary>
        /// Apply special effects to target
        /// </summary>
        private void ApplySpecialEffects(Enemy target)
        {
            // TODO: Implement special effects like slow, poison, etc.
        }
        
        /// <summary>
        /// Upgrade the tower
        /// </summary>
        public bool TryUpgrade()
        {
            if (currentUpgradeLevel >= towerData.upgrades.Length)
                return false; // Max level reached
            
            TowerUpgrade upgrade = towerData.upgrades[currentUpgradeLevel];
            
            if (!GameManager.Instance.SpendGold(upgrade.cost))
                return false; // Not enough gold
            
            ApplyUpgrade(upgrade);
            currentUpgradeLevel++;
            
            OnTowerUpgraded?.Invoke(this);
            
            return true;
        }
        
        /// <summary>
        /// Apply upgrade effects
        /// </summary>
        private void ApplyUpgrade(TowerUpgrade upgrade)
        {
            modifiedDamage += upgrade.damageIncrease;
            modifiedRange += upgrade.rangeIncrease;
            modifiedAttackSpeed += upgrade.attackSpeedIncrease;
            
            // Update visual and collider
            SetupRangeCollider();
            
            if (upgrade.upgradeSprite != null)
            {
                spriteRenderer.sprite = upgrade.upgradeSprite;
            }
            
            Debug.Log($"Tower upgraded to level {currentUpgradeLevel}: {upgrade.upgradeName}");
        }
        
        /// <summary>
        /// Show/hide range indicator
        /// </summary>
        public void SetRangeIndicatorVisible(bool visible)
        {
            if (rangeIndicator != null)
            {
                rangeIndicator.SetActive(visible);
            }
        }
        
        /// <summary>
        /// Get upgrade cost for next level
        /// </summary>
        public int GetNextUpgradeCost()
        {
            if (currentUpgradeLevel >= towerData.upgrades.Length)
                return -1; // Max level
            
            return towerData.upgrades[currentUpgradeLevel].cost;
        }
        
        /// <summary>
        /// Get next upgrade info
        /// </summary>
        public TowerUpgrade GetNextUpgrade()
        {
            if (currentUpgradeLevel >= towerData.upgrades.Length)
                return null;
            
            return towerData.upgrades[currentUpgradeLevel];
        }
    }
}