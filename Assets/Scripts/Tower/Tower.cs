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
        
        [Header("Rotation Sprites")]
        [SerializeField] private Sprite[] rotationSprites; // Array of sprites for different angles (0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°)
        [SerializeField] private float[] spriteAngles = {0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f}; // Corresponding angles
        [SerializeField] private bool useRotationSprites = false; // Toggle to enable/disable sprite rotation
        
        // Components
        private TowerTraitManager traitManager;
        
        // Current state
        private Enemy currentTarget;
        private float lastAttackTime;
        private int currentUpgradeLevel = 0;
        
        // Charge time mechanics for Sniper trait
        private bool isCharging = false;
        private float chargeStartTime;
        private float currentChargeTime = 0f;
        
        // Modified stats (after upgrades)
        private float modifiedDamage;
        private float modifiedRange;
        private float modifiedAttackSpeed;
        private float modifiedChargeTime;
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
        public TowerTraitManager TraitManager => traitManager;
        public bool IsCharging => isCharging;
        public float ChargeProgress => isCharging ? Mathf.Clamp01((Time.time - chargeStartTime) / modifiedChargeTime) : 0f;
        public float ChargeTime => modifiedChargeTime;
        
        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            
            if (rangeCollider == null)
                rangeCollider = GetComponent<CircleCollider2D>();
                
            // Get or create trait manager
            traitManager = GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                traitManager = gameObject.AddComponent<TowerTraitManager>();
            }
            
            // Ensure tower has a click collider for mouse detection
            EnsureClickCollider();
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
            modifiedChargeTime = 0f; // Base towers have no charge time
            
            // Initialize rotation sprites from TowerData
            if (towerData.useRotationSprites && towerData.rotationSprites != null && towerData.rotationSprites.Length > 0)
            {
                useRotationSprites = true;
                rotationSprites = towerData.rotationSprites;
                spriteAngles = towerData.spriteAngles;
            }
            
            SetupVisuals();
            SetupRangeCollider();
            RecalculateStats();
            
            lastAttackTime = -towerData.GetAttackCooldown(); // Allow immediate first attack
        }
        
        /// <summary>
        /// Called by TowerTraitManager when traits change
        /// </summary>
        public void OnTraitsChanged()
        {
            RecalculateStats();
            SetupRangeCollider(); // Update range collider with new range
        }
        
        /// <summary>
        /// Recalculate tower stats including trait modifications
        /// </summary>
        private void RecalculateStats()
        {
            // Start with base stats
            TowerStats baseStats = new TowerStats(
                towerData.damage,
                towerData.attackRange,
                towerData.attackSpeed,
                0f
            );
            
            // Apply trait modifications
            if (traitManager != null)
            {
                TowerStats modifiedStats = traitManager.CalculateModifiedStats(baseStats);
                
                // Store old values for comparison
                float oldRange = modifiedRange;
                
                modifiedDamage = modifiedStats.damage;
                modifiedRange = modifiedStats.range;
                modifiedAttackSpeed = modifiedStats.attackSpeed;
                modifiedChargeTime = modifiedStats.chargeTime;
                
                // Debug log if range changed
                if (Mathf.Abs(oldRange - modifiedRange) > 0.1f)
                {
                    Debug.Log($"{name}: Range updated from {oldRange} to {modifiedRange}");
                }
            }
            else
            {
                modifiedDamage = baseStats.damage;
                modifiedRange = baseStats.range;
                modifiedAttackSpeed = baseStats.attackSpeed;
                modifiedChargeTime = baseStats.chargeTime;
            }
            
            // Update visual elements that depend on modified stats
            UpdateRangeIndicator();
            SetupRangeCollider();
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
        
        /// <summary>
        /// Update range indicator to show current modified range
        /// </summary>
        private void UpdateRangeIndicator()
        {
            // Create range indicator if it doesn't exist
            if (rangeIndicator == null)
            {
                CreateRangeIndicator();
            }
            
            if (rangeIndicator != null)
            {
                // Update LineRenderer circle to match the current modified range
                LineRenderer lineRenderer = rangeIndicator.GetComponent<LineRenderer>();
                if (lineRenderer != null)
                {
                    // Update circle points with correct radius
                    CreateCirclePoints(lineRenderer, modifiedRange, lineRenderer.positionCount);
                    
                    // Update color based on if range is modified
                    bool rangeEnhanced = Mathf.Abs(modifiedRange - towerData.attackRange) > 0.1f;
                    Color indicatorColor = rangeEnhanced ? new Color(0f, 1f, 0f, 0.8f) : new Color(1f, 1f, 1f, 0.8f);
                    lineRenderer.material.color = indicatorColor;
                    
                    // Ensure high sorting order for visibility
                    lineRenderer.sortingOrder = Mathf.Max(lineRenderer.sortingOrder, 1000);
                }
                else
                {
                    // Fallback for sprite-based indicators
                    rangeIndicator.transform.localScale = Vector3.one * (modifiedRange * 2f);
                    
                    SpriteRenderer rangeRenderer = rangeIndicator.GetComponent<SpriteRenderer>();
                    if (rangeRenderer != null)
                    {
                        bool rangeEnhanced = Mathf.Abs(modifiedRange - towerData.attackRange) > 0.1f;
                        rangeRenderer.color = rangeEnhanced ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 1f, 1f, 0.5f);
                        rangeRenderer.sortingOrder = Mathf.Max(rangeRenderer.sortingOrder, 1000);
                    }
                }
            }
        }
        
        /// <summary>
        /// Create a simple range indicator if one doesn't exist
        /// </summary>
        private void CreateRangeIndicator()
        {
            // Create range indicator using LineRenderer for better visibility
            GameObject indicator = new GameObject("RangeIndicator");
            indicator.transform.SetParent(transform);
            indicator.transform.localPosition = Vector3.zero;
            
            LineRenderer lineRenderer = indicator.AddComponent<LineRenderer>();
            
            // Configure LineRenderer for circle
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material.color = new Color(1f, 1f, 1f, 0.8f);
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            
            // Try to set sorting layer for visibility
            try
            {
                lineRenderer.sortingLayerName = "UI";
                lineRenderer.sortingOrder = 1000;
            }
            catch
            {
                lineRenderer.sortingOrder = 1000;
            }
            
            // Create circle points
            int segments = 64;
            lineRenderer.positionCount = segments;
            CreateCirclePoints(lineRenderer, 1f, segments); // Will be scaled later
            
            rangeIndicator = indicator;
            rangeIndicator.SetActive(false); // Start hidden
        }
        
        /// <summary>
        /// Create circle points for LineRenderer
        /// </summary>
        private void CreateCirclePoints(LineRenderer lineRenderer, float radius, int segments)
        {
            float angleIncrement = 2 * Mathf.PI / segments;
            
            for (int i = 0; i < segments; i++)
            {
                float angle = i * angleIncrement;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        
        /// <summary>
        /// Create a simple circle texture for range indicator
        /// </summary>
        private Texture2D CreateCircleTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.5f - 2f; // Slightly smaller for clean edge
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    // Create a circle outline
                    if (distance >= radius - 2f && distance <= radius)
                    {
                        pixels[y * size + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        private void Update()
        {
            if (towerData == null)
                return;
            
            UpdateTargeting();
            TryAttack();
            
            // Handle mouse click for range display
            HandleMouseClick();
        }
        
        /// <summary>
        /// Handle mouse clicks on the tower to show range indicator
        /// </summary>
        private void HandleMouseClick()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0f;
                
                // Check if click is on this tower (simple distance check)
                float clickDistance = Vector3.Distance(mouseWorldPos, transform.position);
                if (clickDistance <= 1f) // Within 1 unit of the tower
                {
                    ShowRangeIndicatorTemporarily(3f);
                    Debug.Log($"Clicked on {name} - showing range indicator (Range: {modifiedRange})");
                }
            }
        }
        
        /// <summary>
        /// Unity's built-in mouse click detection (requires collider)
        /// </summary>
        private void OnMouseDown()
        {
            ShowRangeIndicatorTemporarily(3f);
            Debug.Log($"Clicked on {name} - showing range indicator (Range: {modifiedRange})");
        }
        
        /// <summary>
        /// Ensure tower has a collider for click detection
        /// </summary>
        private void EnsureClickCollider()
        {
            // Check if we have a non-trigger collider for click detection
            Collider2D[] colliders = GetComponents<Collider2D>();
            bool hasClickCollider = false;
            
            foreach (var collider in colliders)
            {
                if (!collider.isTrigger)
                {
                    hasClickCollider = true;
                    break;
                }
            }
            
            if (!hasClickCollider)
            {
                // Add a small collider for click detection
                CircleCollider2D clickCollider = gameObject.AddComponent<CircleCollider2D>();
                clickCollider.radius = 0.5f; // Small radius around tower sprite
                clickCollider.isTrigger = false; // Must be false for OnMouseDown to work
                Debug.Log($"Added click collider to {name}");
            }
        }
        
        /// <summary>
        /// Update current target based on targeting mode
        /// </summary>
        private void UpdateTargeting()
        {
            Enemy previousTarget = currentTarget;
            
            if (currentTarget != null && IsValidTarget(currentTarget))
            {
                // Update rotation to track current target
                if (useRotationSprites && currentTarget != null)
                    UpdateRotationSprite(currentTarget.transform.position);
                return; // Keep current target if still valid
            }
            
            currentTarget = FindTarget();
            
            // Update rotation when target changes
            if (useRotationSprites && currentTarget != previousTarget && currentTarget != null)
                UpdateRotationSprite(currentTarget.transform.position);
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
            {
                // Cancel charging if target is lost
                if (isCharging)
                {
                    isCharging = false;
                    Debug.Log($"{name}: Target lost, canceling charge");
                }
                return;
            }
            
            float timeSinceLastAttack = Time.time - lastAttackTime;
            float attackCooldown = 1f / modifiedAttackSpeed;
            
            if (timeSinceLastAttack >= attackCooldown)
            {
                // Check if this tower has charge time (Sniper trait)
                if (modifiedChargeTime > 0f)
                {
                    if (!isCharging)
                    {
                        // Start charging
                        isCharging = true;
                        chargeStartTime = Time.time;
                        Debug.Log($"{name}: Started charging (charge time: {modifiedChargeTime}s)");
                    }
                    else
                    {
                        // Check if charge is complete
                        float chargeElapsed = Time.time - chargeStartTime;
                        if (chargeElapsed >= modifiedChargeTime)
                        {
                            // Charge complete, fire!
                            Attack(currentTarget);
                            lastAttackTime = Time.time;
                            isCharging = false;
                            Debug.Log($"{name}: Charge complete, firing!");
                        }
                    }
                }
                else
                {
                    // No charge time, attack immediately
                    Attack(currentTarget);
                    lastAttackTime = Time.time;
                }
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
            //RotateTowardsTarget(target);
            
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
            float damage = modifiedDamage;
            float healthBefore = target.CurrentHealth;
            
            target.TakeDamage(damage, towerData.damageType);
            
            // Apply trait effects on attack
            if (traitManager != null)
            {
                traitManager.ApplyTraitEffectsOnAttack(target, damage);
                
                // Apply trait effects on kill if enemy died
                if (healthBefore > 0 && target.CurrentHealth <= 0)
                {
                    traitManager.ApplyTraitEffectsOnKill(target);
                }
            }
            
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
                projectile.Initialize(target, modifiedDamage, towerData.damageType, towerData.projectileSpeed, this);
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
        /// Update tower sprite rotation based on target position
        /// </summary>
        private void UpdateRotationSprite(Vector3 targetPosition)
        {
            if (!useRotationSprites || rotationSprites == null || rotationSprites.Length == 0)
                return;
            
            // Calculate angle to target
            Vector3 direction = targetPosition - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Normalize angle to 0-360 range
            if (angle < 0) angle += 360f;
            
            // Find closest sprite angle
            int closestIndex = GetClosestSpriteIndex(angle);
            
            // Update sprite if we have a valid index and sprite
            if (closestIndex >= 0 && closestIndex < rotationSprites.Length && rotationSprites[closestIndex] != null)
            {
                spriteRenderer.sprite = rotationSprites[closestIndex];
            }
        }
        
        /// <summary>
        /// Get the index of the sprite that best matches the target angle
        /// </summary>
        private int GetClosestSpriteIndex(float targetAngle)
        {
            if (spriteAngles == null || spriteAngles.Length == 0)
                return 0;
            
            int closestIndex = 0;
            float closestDifference = Mathf.Abs(Mathf.DeltaAngle(targetAngle, spriteAngles[0]));
            
            for (int i = 1; i < spriteAngles.Length && i < rotationSprites.Length; i++)
            {
                float difference = Mathf.Abs(Mathf.DeltaAngle(targetAngle, spriteAngles[i]));
                if (difference < closestDifference)
                {
                    closestDifference = difference;
                    closestIndex = i;
                }
            }
            
            return closestIndex;
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
                if (visible)
                {
                    UpdateRangeIndicator(); // Ensure it shows the correct range when made visible
                }
            }
        }
        
        /// <summary>
        /// Show range indicator temporarily (useful for trait application feedback)
        /// </summary>
        public void ShowRangeIndicatorTemporarily(float duration = 2f)
        {
            if (rangeIndicator != null)
            {
                SetRangeIndicatorVisible(true);
                StartCoroutine(HideRangeIndicatorAfterDelay(duration));
            }
        }
        
        private System.Collections.IEnumerator HideRangeIndicatorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetRangeIndicatorVisible(false);
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
        
        #region Trait Management
        
        /// <summary>
        /// Add a trait to this tower
        /// </summary>
        public bool AddTrait(TowerTrait trait)
        {
            if (traitManager == null) return false;
            return traitManager.AddTrait(trait);
        }
        
        /// <summary>
        /// Remove a trait from this tower
        /// </summary>
        public bool RemoveTrait(TowerTrait trait)
        {
            if (traitManager == null) return false;
            return traitManager.RemoveTrait(trait);
        }
        
        /// <summary>
        /// Check if tower has a specific trait
        /// </summary>
        public bool HasTrait(TowerTrait trait)
        {
            if (traitManager == null) return false;
            return traitManager.HasTrait(trait);
        }
        
        /// <summary>
        /// Check if tower has any trait of a specific category
        /// </summary>
        public bool HasTraitOfCategory(TraitCategory category)
        {
            if (traitManager == null) return false;
            return traitManager.HasTraitOfCategory(category);
        }
        
        /// <summary>
        /// Get all applied traits
        /// </summary>
        public System.Collections.Generic.List<TowerTrait> GetAppliedTraits()
        {
            if (traitManager == null) return new System.Collections.Generic.List<TowerTrait>();
            return traitManager.AppliedTraits;
        }
        
        /// <summary>
        /// Clear all traits from this tower
        /// </summary>
        public void ClearAllTraits()
        {
            if (traitManager != null)
                traitManager.ClearAllTraits();
        }
        
        #endregion
    }
}