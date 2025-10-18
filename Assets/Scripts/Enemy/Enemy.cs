using UnityEngine;
using System.Collections;

namespace TowerFusion
{
    /// <summary>
    /// Individual enemy behavior controller
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Enemy Configuration")]
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb2D;
        
        [Header("Health Bar")]
        [SerializeField] private Transform healthBarContainer;
        [SerializeField] private Transform healthBarFill;
        
        // Current state
        private float currentHealth;
        private int currentPathIndex = 0;
        private bool isAlive = true;
        private bool hasReachedEnd = false;
        
        // Status effects
        private float speedMultiplier = 1f;
        private float damageMultiplier = 1f;
        private bool isSlowed = false;
        private bool isBrittle = false;
        private bool isBurning = false;
        private Color originalColor = Color.white;
        private bool originalColorStored = false;
        
        // Movement
        private Vector3 targetPosition;
        private Vector3[] pathPoints;
        private Vector3 lastPosition;
        private float currentMovementAngle;
        
        // Enemy role and corn stealing
        private EnemyRole assignedRole = EnemyRole.Attacker;
        private bool hasCorn = false;
        private GameObject cornVisual;
        private Vector3 spawnPoint;
        private float cornGrabTimer = 0f;
        
        // Tower attacking
        private Tower currentTowerTarget;
        private float lastTowerAttackTime;
        private bool isAttackingTower = false;
        private enum EnemyBehaviorState { MovingToEnd, SeekingTower, AttackingTower, MovingToCorn, GrabbingCorn, ReturningWithCorn }
        private EnemyBehaviorState behaviorState = EnemyBehaviorState.SeekingTower;
        
        // Sprite animation
        private float animationTimer = 0f;
        private int currentFrameIndex = 0;
        private Sprite[] currentAnimationFrames;
        
        // Events
        public System.Action<Enemy> OnEnemyKilled;
        public System.Action<Enemy> OnEnemyReachedEnd;
        
        // Properties
        public EnemyData EnemyData => enemyData;
        public float CurrentHealth => currentHealth;
        public float MaxHealth => enemyData?.maxHealth ?? 100f;
        public bool IsAlive => isAlive;
        public Vector3 Position => transform.position;
        public float CurrentSpeed => enemyData.moveSpeed * speedMultiplier;
        public bool IsSlowed => isSlowed;
        public bool IsBrittle => isBrittle;
        public bool IsBurning => isBurning;
        public EnemyRole Role => assignedRole;
        public bool HasCorn => hasCorn;
        
        private void Awake()
        {
            if (rb2D == null)
                rb2D = GetComponent<Rigidbody2D>();
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        /// <summary>
        /// Initialize enemy with data
        /// </summary>
        public void Initialize(EnemyData data)
        {
            enemyData = data;
            currentHealth = enemyData.maxHealth;
            isAlive = true;
            hasReachedEnd = false;
            currentPathIndex = 0;
            hasCorn = false;
            
            // Set default role from data
            assignedRole = enemyData.defaultRole;
            
            // Store spawn point for return journey
            spawnPoint = transform.position;
            
            // Store original color for status effect restoration
            if (spriteRenderer != null && !originalColorStored)
            {
                originalColor = spriteRenderer.color;
                originalColorStored = true;
                Debug.Log($"Stored original color for {name}: {originalColor}");
            }
            
            SetupVisuals();
            SetupMovement();
            
            // Determine initial behavior based on role
            if (assignedRole == EnemyRole.Stealer)
            {
                behaviorState = EnemyBehaviorState.MovingToCorn;
                Debug.Log($"{name} assigned as STEALER - heading to corn storage");
            }
            else
            {
                behaviorState = EnemyBehaviorState.SeekingTower;
                Debug.Log($"{name} assigned as ATTACKER - seeking towers");
            }
            
            if (enemyData.isRegenerating && enemyData.regenerationRate > 0)
            {
                StartCoroutine(RegenerateHealth());
            }
        }
        
        /// <summary>
        /// Set the enemy's role (called by WaveManager for dynamic assignment)
        /// </summary>
        public void SetRole(EnemyRole role)
        {
            assignedRole = role;
            
            // Update behavior state based on role
            if (assignedRole == EnemyRole.Stealer)
            {
                behaviorState = EnemyBehaviorState.MovingToCorn;
                Debug.Log($"{name} role set to STEALER");
            }
            else
            {
                behaviorState = EnemyBehaviorState.SeekingTower;
                Debug.Log($"{name} role set to ATTACKER");
            }
        }
        
        /// <summary>
        /// Setup visual appearance
        /// </summary>
        private void SetupVisuals()
        {
            if (spriteRenderer != null)
            {
                // Try to use directional sprites first, fallback to single sprite
                if (enemyData.directionalSprites != null && enemyData.directionalSprites.HasDirectionalSprites())
                {
                    // Initialize animation state
                    currentFrameIndex = 0;
                    animationTimer = 0f;
                    
                    // Start with right-facing sprite (default direction)
                    currentMovementAngle = 0f; // Initialize angle
                    UpdateDirectionalSprite(0f); // 0° = facing right
                    
                    Debug.Log($"Initialized directional sprites for {enemyData.enemyName}: useAnimation={enemyData.directionalSprites.useAnimation}");
                }
                else if (enemyData.enemySprite != null)
                {
                    spriteRenderer.sprite = enemyData.enemySprite;
                    Debug.Log($"Using single sprite for {enemyData.enemyName}");
                }
                
                spriteRenderer.color = enemyData.enemyColor;
            }
            
            transform.localScale = enemyData.scale;
            lastPosition = transform.position;
            
            // Setup health bar
            UpdateHealthBar();
        }
        
        /// <summary>
        /// Setup movement path
        /// </summary>
        private void SetupMovement()
        {
            pathPoints = MapManager.Instance?.GetPathPoints();
            
            if (pathPoints != null && pathPoints.Length > 0)
            {
                transform.position = MapManager.Instance.GetEnemySpawnPoint();
                targetPosition = pathPoints[0];
                
                // Immediately set initial direction towards first waypoint
                Vector3 initialDirection = (targetPosition - transform.position).normalized;
                if (initialDirection.magnitude > 0.1f)
                {
                    float initialAngle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
                    initialAngle = -initialAngle; // Flip Y axis
                    currentMovementAngle = initialAngle;
                    UpdateDirectionalSprite(currentMovementAngle);
                }
            }
        }
        
        private void Update()
        {
            if (!isAlive || hasReachedEnd)
                return;
            
            // Update behavior based on role and state
            switch (behaviorState)
            {
                case EnemyBehaviorState.MovingToCorn:
                    MoveTowardsCornStorage();
                    break;
                    
                case EnemyBehaviorState.GrabbingCorn:
                    GrabCorn();
                    break;
                    
                case EnemyBehaviorState.ReturningWithCorn:
                    ReturnToSpawn();
                    break;
                    
                case EnemyBehaviorState.SeekingTower:
                case EnemyBehaviorState.AttackingTower:
                case EnemyBehaviorState.MovingToEnd:
                    // Attacker behavior
                    if (enemyData.canAttackTowers)
                    {
                        UpdateTowerTargeting();
                        HandleTowerBehavior();
                    }
                    else
                    {
                        MoveAlongPath();
                    }
                    break;
            }
            
            // Always update animation, even if not moving
            if (enemyData.directionalSprites.useAnimation)
            {
                UpdateSpriteAnimation();
            }
        }
        
        /// <summary>
        /// Update tower targeting for enemies that attack towers
        /// </summary>
        private void UpdateTowerTargeting()
        {
            // Check if current tower target is still valid
            if (currentTowerTarget != null && (!currentTowerTarget.IsAlive || currentTowerTarget == null))
            {
                // Unassign from distributor
                if (EnemyTargetDistributor.Instance != null)
                {
                    EnemyTargetDistributor.Instance.UnassignEnemy(this);
                }
                
                currentTowerTarget = null;
                isAttackingTower = false;
            }
            
            // If we don't have a target or are moving to end, look for towers
            if (behaviorState != EnemyBehaviorState.MovingToEnd && currentTowerTarget == null)
            {
                FindDistributedTower();
                
                if (currentTowerTarget != null)
                {
                    behaviorState = EnemyBehaviorState.SeekingTower;
                    
                    // Register with distributor
                    if (EnemyTargetDistributor.Instance != null)
                    {
                        EnemyTargetDistributor.Instance.AssignEnemyToTower(this, currentTowerTarget);
                    }
                }
                else
                {
                    // No towers found, head to end point
                    behaviorState = EnemyBehaviorState.MovingToEnd;
                }
            }
        }
        
        /// <summary>
        /// Handle behavior based on current state
        /// </summary>
        private void HandleTowerBehavior()
        {
            switch (behaviorState)
            {
                case EnemyBehaviorState.SeekingTower:
                    if (currentTowerTarget != null)
                    {
                        MoveTowardsTower(currentTowerTarget);
                        
                        // Check if in attack range
                        float distanceToTower = Vector3.Distance(transform.position, currentTowerTarget.Position);
                        if (distanceToTower <= enemyData.towerAttackRange)
                        {
                            behaviorState = EnemyBehaviorState.AttackingTower;
                            isAttackingTower = true;
                        }
                    }
                    else
                    {
                        behaviorState = EnemyBehaviorState.MovingToEnd;
                    }
                    break;
                    
                case EnemyBehaviorState.AttackingTower:
                    if (currentTowerTarget != null && currentTowerTarget.IsAlive)
                    {
                        TryAttackTower();
                        
                        // Check if still in range
                        float distanceToTower = Vector3.Distance(transform.position, currentTowerTarget.Position);
                        if (distanceToTower > enemyData.towerAttackRange)
                        {
                            behaviorState = EnemyBehaviorState.SeekingTower;
                            isAttackingTower = false;
                        }
                    }
                    else
                    {
                        // Tower destroyed or invalid
                        currentTowerTarget = null;
                        isAttackingTower = false;
                        behaviorState = EnemyBehaviorState.SeekingTower;
                    }
                    break;
                    
                case EnemyBehaviorState.MovingToEnd:
                    MoveAlongPath();
                    break;
            }
        }
        
        /// <summary>
        /// Find tower using distribution system to spread enemies across different towers
        /// </summary>
        private void FindDistributedTower()
        {
            if (EnemyTargetDistributor.Instance != null)
            {
                // Use distributor to get best tower assignment
                currentTowerTarget = EnemyTargetDistributor.Instance.SelectTargetForEnemy(
                    this, 
                    transform.position, 
                    enemyData.towerDetectionRange
                );
            }
            else
            {
                // Fallback to nearest tower if no distributor
                FindNearestTowerFallback();
            }
        }
        
        /// <summary>
        /// Fallback method to find nearest tower when distributor is unavailable
        /// </summary>
        private void FindNearestTowerFallback()
        {
            if (TowerManager.Instance == null)
                return;
            
            var towers = TowerManager.Instance.GetActiveTowers();
            if (towers == null || towers.Count == 0)
            {
                currentTowerTarget = null;
                return;
            }
            
            Tower nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var tower in towers)
            {
                if (tower == null || !tower.IsAlive)
                    continue;
                
                float distance = Vector3.Distance(transform.position, tower.Position);
                if (distance <= enemyData.towerDetectionRange && distance < nearestDistance)
                {
                    nearest = tower;
                    nearestDistance = distance;
                }
            }
            
            currentTowerTarget = nearest;
        }
        
        /// <summary>
        /// Move towards the targeted tower
        /// </summary>
        private void MoveTowardsTower(Tower tower)
        {
            if (tower == null)
                return;
            
            Vector3 previousPosition = transform.position;
            Vector3 direction = (tower.Position - transform.position).normalized;
            float currentSpeed = enemyData.moveSpeed * speedMultiplier;
            float moveDistance = currentSpeed * Time.deltaTime;
            
            transform.position += direction * moveDistance;
            
            // Update sprite direction based on movement
            UpdateMovementDirection(previousPosition);
        }
        
        /// <summary>
        /// Try to attack current tower target
        /// </summary>
        private void TryAttackTower()
        {
            if (currentTowerTarget == null || !currentTowerTarget.IsAlive)
                return;
            
            float timeSinceLastAttack = Time.time - lastTowerAttackTime;
            if (timeSinceLastAttack >= enemyData.towerAttackCooldown)
            {
                AttackTower(currentTowerTarget);
                lastTowerAttackTime = Time.time;
            }
        }
        
        /// <summary>
        /// Attack a specific tower
        /// </summary>
        private void AttackTower(Tower tower)
        {
            if (tower == null || !tower.IsAlive)
                return;
            
            tower.TakeDamage(enemyData.towerAttackDamage);
            Debug.Log($"{enemyData.enemyName} attacked {tower.TowerData.towerName} for {enemyData.towerAttackDamage} damage!");
            
            // Check if tower was destroyed
            if (!tower.IsAlive)
            {
                // Unassign from distributor
                if (EnemyTargetDistributor.Instance != null)
                {
                    EnemyTargetDistributor.Instance.UnassignEnemy(this);
                }
                
                currentTowerTarget = null;
                isAttackingTower = false;
                behaviorState = EnemyBehaviorState.SeekingTower;
            }
        }
        
        /// <summary>
        /// Move enemy along the path
        /// </summary>
        private void MoveAlongPath()
        {
            if (pathPoints == null || pathPoints.Length == 0)
                return;
            
            // Store last position for direction calculation
            Vector3 previousPosition = transform.position;
            
            // Move towards target
            Vector3 direction = (targetPosition - transform.position).normalized;
            float currentSpeed = enemyData.moveSpeed * speedMultiplier;
            float moveDistance = currentSpeed * Time.deltaTime;
            
            transform.position += direction * moveDistance;
            
            // Update sprite direction based on movement
            UpdateMovementDirection(previousPosition);
            
            // Check if reached current target
            if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
            {
                currentPathIndex++;
                
                if (currentPathIndex >= pathPoints.Length)
                {
                    // Reached the end
                    ReachEnd();
                }
                else
                {
                    targetPosition = pathPoints[currentPathIndex];
                    
                    // Immediately update sprite direction to face the new target
                    Vector3 newDirection = (targetPosition - transform.position).normalized;
                    if (newDirection.magnitude > 0.1f)
                    {
                        float newAngle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
                        newAngle = -newAngle; // Flip Y axis
                        currentMovementAngle = newAngle;
                        UpdateDirectionalSprite(currentMovementAngle);
                    }
                }
            }
        }
        
        /// <summary>
        /// Update movement direction and sprite orientation
        /// </summary>
        private void UpdateMovementDirection(Vector3 previousPosition)
        {
            // Use target direction for immediate response, fallback to actual movement
            Vector3 targetDirection = (targetPosition - transform.position).normalized;
            Vector3 movementVector = transform.position - previousPosition;
            
            Vector3 directionToUse = Vector3.zero;
            
            // Use target direction if we have a clear target and are moving towards it
            if (targetDirection.magnitude > 0.1f && Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                directionToUse = targetDirection;
            }
            // Otherwise use actual movement if we moved enough
            else if (movementVector.magnitude > 0.001f) // Reduced threshold for more responsiveness
            {
                directionToUse = movementVector.normalized;
            }
            
            if (directionToUse != Vector3.zero)
            {
                // Calculate angle in degrees (0° = right, 90° = down, 180° = left, 270° = up)
                float angle = Mathf.Atan2(directionToUse.y, directionToUse.x) * Mathf.Rad2Deg;
                
                // Convert to Unity's coordinate system where down is positive Y
                angle = -angle; // Flip Y axis
                
                // Only update if direction changed significantly (reduce jitter)
                float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentMovementAngle, angle));
                if (angleDifference > 10f) // 10-degree threshold to prevent jitter
                {
                    currentMovementAngle = angle;
                    
                    // Update sprite to match movement direction
                    UpdateDirectionalSprite(currentMovementAngle);
                }
                
                lastPosition = transform.position;
            }
        }
        
        /// <summary>
        /// Update sprite based on movement direction
        /// </summary>
        private void UpdateDirectionalSprite(float angleDegrees)
        {
            if (spriteRenderer == null || enemyData.directionalSprites == null)
                return;
            
            if (!enemyData.directionalSprites.HasDirectionalSprites())
            {
                // Fall back to single sprite if no directional sprites
                if (enemyData.enemySprite != null)
                {
                    spriteRenderer.sprite = enemyData.enemySprite;
                }
                return;
            }
            
            // Check if animation is enabled
            if (enemyData.directionalSprites.useAnimation && 
                enemyData.directionalSprites.animationFrames != null)
            {
                // Update animation frames for current direction
                Sprite[] frames = enemyData.directionalSprites.animationFrames.GetFramesForAngle(angleDegrees);
                if (frames != null && frames.Length > 0)
                {
                    currentAnimationFrames = frames;
                    currentFrameIndex = 0; // Reset to first frame when direction changes
                    animationTimer = 0f; // Reset timer
                    
                    // Set initial frame
                    if (frames[0] != null)
                    {
                        spriteRenderer.sprite = frames[0];
                    }
                    return;
                }
            }
            
            // Use static directional sprite
            Sprite directionSprite = enemyData.directionalSprites.GetSpriteForAngle(angleDegrees);
            if (directionSprite != null)
            {
                spriteRenderer.sprite = directionSprite;
            }
        }
        
        /// <summary>
        /// Update sprite animation if enabled
        /// </summary>
        private void UpdateSpriteAnimation()
        {
            if (!enemyData.directionalSprites.useAnimation || 
                currentAnimationFrames == null || 
                currentAnimationFrames.Length == 0 ||
                spriteRenderer == null)
                return;
            
            // Update animation timer
            animationTimer += Time.deltaTime;
            
            float frameRate = enemyData.directionalSprites.animationFrames.frameRate;
            float frameDuration = 1f / frameRate;
            
            if (animationTimer >= frameDuration)
            {
                animationTimer = 0f;
                int previousFrameIndex = currentFrameIndex;
                currentFrameIndex++;
                
                // Handle looping
                if (currentFrameIndex >= currentAnimationFrames.Length)
                {
                    if (enemyData.directionalSprites.animationFrames.looping)
                    {
                        currentFrameIndex = 0;
                    }
                    else
                    {
                        currentFrameIndex = currentAnimationFrames.Length - 1; // Stay on last frame
                    }
                }
                
                // Update sprite to current frame
                if (currentFrameIndex < currentAnimationFrames.Length && 
                    currentAnimationFrames[currentFrameIndex] != null)
                {
                    spriteRenderer.sprite = currentAnimationFrames[currentFrameIndex];
                }
            }
        }
        
        /// <summary>
        /// Take damage from towers
        /// </summary>
        public void TakeDamage(float damage, DamageType damageType)
        {
            if (!isAlive)
                return;
            
            float actualDamage = enemyData.CalculateDamage(damage, damageType);
            
            // Apply brittle effect (increased damage taken)
            if (isBrittle)
            {
                actualDamage *= damageMultiplier;
            }
            
            currentHealth -= actualDamage;
            
            // Update health bar
            UpdateHealthBar();
            
            // Create damage number effect
            CreateDamageNumber(actualDamage);
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// Enemy dies and gives rewards
        /// </summary>
        private void Die()
        {
            if (!isAlive)
                return;
            
            isAlive = false;
            
            // Drop corn if carrying
            if (hasCorn)
            {
                DropCorn();
            }
            
            // Unassign from tower distributor
            if (EnemyTargetDistributor.Instance != null)
            {
                EnemyTargetDistributor.Instance.UnassignEnemy(this);
            }
            
            // Give gold reward
            GameManager.Instance?.AddGold(enemyData.goldReward);
            
            // Notify systems
            OnEnemyKilled?.Invoke(this);
            EnemyManager.Instance?.OnEnemyKilled(this);
            
            // Create death effect
            CreateDeathEffect();
            
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Enemy reaches the end and damages player
        /// </summary>
        private void ReachEnd()
        {
            if (hasReachedEnd)
                return;
            
            hasReachedEnd = true;
            
            // Damage player
            GameManager.Instance?.ModifyHealth(-enemyData.damageToPlayer);
            
            // Notify systems
            OnEnemyReachedEnd?.Invoke(this);
            EnemyManager.Instance?.OnEnemyReachedEnd(this);
            
            Destroy(gameObject);
        }
        
        #region Corn Stealing Behavior
        
        /// <summary>
        /// Move towards corn storage
        /// </summary>
        private void MoveTowardsCornStorage()
        {
            if (CornManager.Instance == null)
            {
                // No corn manager, revert to normal behavior
                behaviorState = EnemyBehaviorState.SeekingTower;
                return;
            }
            
            Vector3 cornPosition = CornManager.Instance.GetCornStoragePosition();
            Vector3 direction = (cornPosition - transform.position).normalized;
            
            // Move towards corn
            transform.position += direction * CurrentSpeed * Time.deltaTime;
            
            // Update sprite direction
            if (direction.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                angle = -angle; // Flip Y axis for Unity's coordinate system
                currentMovementAngle = angle;
                UpdateDirectionalSprite(currentMovementAngle);
            }
            
            lastPosition = transform.position;
            
            // Check if reached corn storage
            if (CornManager.Instance.IsInGrabRange(transform.position))
            {
                // Check if corn still available
                if (CornManager.Instance.Storage.HasCorn)
                {
                    behaviorState = EnemyBehaviorState.GrabbingCorn;
                    cornGrabTimer = 0f;
                    Debug.Log($"{name} reached corn storage, starting grab");
                }
                else
                {
                    // No corn left, become attacker
                    Debug.LogWarning($"{name} reached empty corn storage, switching to attacker");
                    assignedRole = EnemyRole.Attacker;
                    behaviorState = EnemyBehaviorState.SeekingTower;
                }
            }
        }
        
        /// <summary>
        /// Grab corn from storage (takes time)
        /// </summary>
        private void GrabCorn()
        {
            cornGrabTimer += Time.deltaTime;
            
            if (cornGrabTimer >= enemyData.cornGrabDuration)
            {
                // Attempt to grab corn
                CornManager.Instance.RegisterCornGrab(this);
                hasCorn = true;
                
                // Create visual indicator
                CreateCornVisual();
                
                // Apply speed penalty
                speedMultiplier *= enemyData.cornCarrySpeedMultiplier;
                
                // Start returning to spawn
                behaviorState = EnemyBehaviorState.ReturningWithCorn;
                Debug.Log($"{name} grabbed corn! Returning to spawn with {speedMultiplier}x speed");
            }
        }
        
        /// <summary>
        /// Return to spawn point with corn
        /// </summary>
        private void ReturnToSpawn()
        {
            Vector3 direction = (spawnPoint - transform.position).normalized;
            
            // Move towards spawn
            transform.position += direction * CurrentSpeed * Time.deltaTime;
            
            // Update sprite direction
            if (direction.magnitude > 0.1f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                angle = -angle; // Flip Y axis
                currentMovementAngle = angle;
                UpdateDirectionalSprite(currentMovementAngle);
            }
            
            lastPosition = transform.position;
            
            // Check if reached spawn
            float distanceToSpawn = Vector3.Distance(transform.position, spawnPoint);
            if (distanceToSpawn < 0.5f) // Close enough to spawn
            {
                // Successfully stole corn!
                CornManager.Instance.RegisterCornSteal(this);
                Debug.Log($"{name} successfully returned corn to spawn!");
                
                // Mark as complete and notify systems (like ReachEnd does)
                hasReachedEnd = true;
                OnEnemyReachedEnd?.Invoke(this);
                EnemyManager.Instance?.OnEnemyReachedEnd(this);
                
                // Destroy enemy (successful theft)
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// Drop corn when killed
        /// </summary>
        private void DropCorn()
        {
            if (!hasCorn)
                return;
            
            hasCorn = false;
            
            // Return corn to storage
            CornManager.Instance.ReturnCornToStorage();
            
            // Destroy visual
            if (cornVisual != null)
            {
                Destroy(cornVisual);
            }
            
            // Restore speed
            speedMultiplier /= enemyData.cornCarrySpeedMultiplier;
            
            Debug.Log($"{name} dropped corn - returned to storage");
        }
        
        /// <summary>
        /// Create visual indicator of corn being carried
        /// </summary>
        private void CreateCornVisual()
        {
            // Simple approach: Create a yellow circle above enemy
            // TODO: Replace with actual corn sprite
            cornVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cornVisual.transform.SetParent(transform);
            cornVisual.transform.localPosition = Vector3.up * 0.5f;
            cornVisual.transform.localScale = Vector3.one * 0.3f;
            
            // Make it yellow (corn-ish)
            Renderer renderer = cornVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.yellow;
            }
            
            // Remove collider
            Destroy(cornVisual.GetComponent<Collider>());
        }
        
        #endregion
        
        /// <summary>
        /// Update health bar visual
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBarFill != null)
            {
                float healthPercent = currentHealth / MaxHealth;
                healthBarFill.localScale = new Vector3(healthPercent, 1f, 1f);
            }
            
            if (healthBarContainer != null)
            {
                healthBarContainer.gameObject.SetActive(currentHealth < MaxHealth);
            }
        }
        
        /// <summary>
        /// Create floating damage number
        /// </summary>
        private void CreateDamageNumber(float damage)
        {
            // TODO: Implement damage number effect
            Debug.Log($"{enemyData.enemyName} took {damage:F1} damage");
        }
        
        /// <summary>
        /// Create death effect
        /// </summary>
        private void CreateDeathEffect()
        {
            // TODO: Implement death particle effect
            Debug.Log($"{enemyData.enemyName} was killed!");
        }
        
        /// <summary>
        /// Regenerate health over time
        /// </summary>
        private IEnumerator RegenerateHealth()
        {
            while (isAlive)
            {
                yield return new WaitForSeconds(1f);
                
                if (currentHealth < MaxHealth)
                {
                    currentHealth += enemyData.regenerationRate;
                    currentHealth = Mathf.Min(currentHealth, MaxHealth);
                    UpdateHealthBar();
                }
            }
        }
        
        /// <summary>
        /// Get predicted position after time
        /// </summary>
        public Vector3 GetPredictedPosition(float timeAhead)
        {
            if (!isAlive || pathPoints == null)
                return transform.position;
            
            Vector3 direction = (targetPosition - transform.position).normalized;
            float currentSpeed = enemyData.moveSpeed * speedMultiplier;
            float distance = currentSpeed * timeAhead;
            
            return transform.position + direction * distance;
        }
        
        /// <summary>
        /// Manually set sprite direction (useful for testing or special cases)
        /// </summary>
        public void SetSpriteDirection(float angleDegrees)
        {
            currentMovementAngle = angleDegrees;
            UpdateDirectionalSprite(angleDegrees);
        }
        
        /// <summary>
        /// Get current movement angle in degrees
        /// </summary>
        public float GetMovementAngle()
        {
            return currentMovementAngle;
        }
        
        /// <summary>
        /// Force start animation for testing (call this from inspector or debug)
        /// </summary>
        [ContextMenu("Force Start Animation")]
        public void ForceStartAnimation()
        {
            if (enemyData.directionalSprites.useAnimation && 
                enemyData.directionalSprites.animationFrames != null)
            {
                // Force right direction animation for testing
                SetSpriteDirection(0f); // Right direction
                Debug.Log($"Forced animation start for {enemyData.enemyName}");
            }
            else
            {
                Debug.Log($"Cannot start animation for {enemyData.enemyName}: useAnimation={enemyData.directionalSprites.useAnimation}, animationFrames={(enemyData.directionalSprites.animationFrames != null ? "assigned" : "null")}");
            }
        }
        
        /// <summary>
        /// Update sprite color based on current status effects
        /// </summary>
        private void UpdateStatusEffectColor()
        {
            if (spriteRenderer == null) return;
            
            Color targetColor = originalColor;
            
            // Apply status effect colors - burn takes priority as it's most visible
            if (isBurning)
            {
                // Red/orange tint for burning
                targetColor = Color.Lerp(originalColor, Color.red, 0.6f);
                targetColor = Color.Lerp(targetColor, new Color(1f, 0.5f, 0f), 0.3f); // Add orange tint
                
                // Apply additional effects if present
                if (isSlowed)
                {
                    targetColor = Color.Lerp(targetColor, Color.cyan, 0.2f);
                }
                if (isBrittle)
                {
                    targetColor = Color.Lerp(targetColor, Color.white, 0.1f);
                }
            }
            else if (isSlowed && isBrittle)
            {
                // Mix cyan (slow) and white (brittle) - results in light cyan-white
                targetColor = Color.Lerp(originalColor, Color.cyan, 0.3f);
                targetColor = Color.Lerp(targetColor, Color.white, 0.2f);
            }
            else if (isSlowed)
            {
                // Cyan tint for slow
                targetColor = Color.Lerp(originalColor, Color.cyan, 0.4f);
            }
            else if (isBrittle)
            {
                // White tint for brittle
                targetColor = Color.Lerp(originalColor, Color.white, 0.5f);
            }
            
            spriteRenderer.color = targetColor;
            Debug.Log($"{name} color updated: Burning={isBurning}, Slowed={isSlowed}, Brittle={isBrittle}, Color={targetColor}");
        }
        
        #region Trait Effects
        
        /// <summary>
        /// Apply burn effect to this enemy
        /// </summary>
        public void ApplyBurnEffect(float damagePerSecond, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(BurnCoroutine(damagePerSecond, duration));
            Debug.Log($"Applied burn effect: {damagePerSecond} DPS for {duration}s");
        }
        
        /// <summary>
        /// Apply slow effect to this enemy
        /// </summary>
        public void ApplySlowEffect(float speedMultiplier, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(SlowCoroutine(speedMultiplier, duration));
            Debug.Log($"Applied slow effect: {speedMultiplier}x speed for {duration}s");
        }
        
        /// <summary>
        /// Apply brittle effect to this enemy (increases incoming damage)
        /// </summary>
        public void ApplyBrittleEffect(float damageMultiplier, float duration)
        {
            if (!isAlive) return;
            
            StartCoroutine(BrittleCoroutine(damageMultiplier, duration));
            Debug.Log($"Applied brittle effect: {damageMultiplier}x damage taken for {duration}s");
        }
        
        private System.Collections.IEnumerator BurnCoroutine(float damagePerSecond, float duration)
        {
            if (isBurning)
            {
                Debug.Log($"Enemy already burning, existing effect continues");
                yield break;
            }
            
            // Apply burn effect
            isBurning = true;
            
            // Update visual indicator
            UpdateStatusEffectColor();
            
            Debug.Log($"Enemy burning: {damagePerSecond} DPS for {duration}s");
            
            float elapsed = 0f;
            float damageInterval = 0.5f; // Apply damage every 0.5 seconds
            float nextDamageTime = damageInterval;
            
            while (elapsed < duration && isAlive)
            {
                elapsed += Time.deltaTime;
                
                // Add flickering burn effect
                if (spriteRenderer != null)
                {
                    float flicker = Mathf.Sin(elapsed * 15f) * 0.3f + 1f; // Fast flicker
                    Color burnColor = Color.Lerp(originalColor, Color.red, 0.6f);
                    burnColor = Color.Lerp(burnColor, new Color(1f, 0.5f, 0f), 0.3f); // Orange tint
                    burnColor *= flicker; // Apply flicker
                    
                    // Maintain other status effect colors
                    if (isSlowed)
                    {
                        burnColor = Color.Lerp(burnColor, Color.cyan, 0.2f);
                    }
                    if (isBrittle)
                    {
                        burnColor = Color.Lerp(burnColor, Color.white, 0.1f);
                    }
                    
                    spriteRenderer.color = burnColor;
                }
                
                if (elapsed >= nextDamageTime)
                {
                    float damage = damagePerSecond * damageInterval;
                    currentHealth -= damage;
                    
                    // Create damage number with fire effect
                    CreateDamageNumber(damage);
                    
                    // Add some visual flair - could add particle effects here later
                    Debug.Log($"🔥 Burn damage: {damage} (Health: {currentHealth})");
                    
                    if (currentHealth <= 0)
                    {
                        isBurning = false;
                        Die();
                        yield break;
                    }
                    
                    UpdateHealthBar();
                    nextDamageTime += damageInterval;
                }
                
                yield return null;
            }
            
            // Remove burn effect
            isBurning = false;
            
            // Update color based on remaining status effects
            UpdateStatusEffectColor();
            
            Debug.Log("Enemy burn effect ended");
        }
        
        private System.Collections.IEnumerator SlowCoroutine(float newSpeedMultiplier, float duration)
        {
            if (isSlowed)
            {
                // If already slowed, just update the multiplier if it's more severe
                if (newSpeedMultiplier < speedMultiplier)
                {
                    speedMultiplier = newSpeedMultiplier;
                }
                Debug.Log($"Enemy already slowed, existing effect continues");
                yield break;
            }
            
            // Apply slow effect
            isSlowed = true;
            float originalSpeedMultiplier = speedMultiplier;
            speedMultiplier = newSpeedMultiplier;
            
            // Update visual indicator
            UpdateStatusEffectColor();
            
            Debug.Log($"Enemy slowed: {speedMultiplier}x speed for {duration}s");
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Remove slow effect
            speedMultiplier = originalSpeedMultiplier;
            isSlowed = false;
            
            // Update color based on remaining status effects
            UpdateStatusEffectColor();
            
            Debug.Log("Enemy slow effect ended");
        }
        
        private System.Collections.IEnumerator BrittleCoroutine(float newDamageMultiplier, float duration)
        {
            if (isBrittle)
            {
                // If already brittle, use the higher multiplier
                if (newDamageMultiplier > damageMultiplier)
                {
                    damageMultiplier = newDamageMultiplier;
                }
                Debug.Log($"Enemy already brittle, existing effect continues");
                yield break;
            }
            
            // Apply brittle effect
            isBrittle = true;
            float originalDamageMultiplier = damageMultiplier;
            damageMultiplier = newDamageMultiplier;
            
            // Update visual indicator
            UpdateStatusEffectColor();
            
            Debug.Log($"Enemy brittle: {damageMultiplier}x damage taken for {duration}s");
            
            // Wait for duration
            yield return new WaitForSeconds(duration);
            
            // Remove brittle effect
            damageMultiplier = originalDamageMultiplier;
            isBrittle = false;
            
            // Update color based on remaining status effects
            UpdateStatusEffectColor();
            
            Debug.Log("Enemy brittle effect ended");
        }
        
        #endregion
    }
}