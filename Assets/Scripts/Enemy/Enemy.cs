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
            
            // Store original color for status effect restoration
            if (spriteRenderer != null && !originalColorStored)
            {
                originalColor = spriteRenderer.color;
                originalColorStored = true;
                Debug.Log($"Stored original color for {name}: {originalColor}");
            }
            
            SetupVisuals();
            SetupMovement();
            
            if (enemyData.isRegenerating && enemyData.regenerationRate > 0)
            {
                StartCoroutine(RegenerateHealth());
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
                    // Start with right-facing sprite (default direction)
                    UpdateDirectionalSprite(0f); // 0° = facing right
                }
                else if (enemyData.enemySprite != null)
                {
                    spriteRenderer.sprite = enemyData.enemySprite;
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
            }
        }
        
        private void Update()
        {
            if (!isAlive || hasReachedEnd)
                return;
            
            MoveAlongPath();
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
            
            // Update sprite animation if enabled
            UpdateSpriteAnimation();
            
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
                }
            }
        }
        
        /// <summary>
        /// Update movement direction and sprite orientation
        /// </summary>
        private void UpdateMovementDirection(Vector3 previousPosition)
        {
            Vector3 movementVector = transform.position - previousPosition;
            
            // Only update if we're actually moving
            if (movementVector.magnitude > 0.01f)
            {
                // Calculate angle in degrees (0° = right, 90° = down, 180° = left, 270° = up)
                float angle = Mathf.Atan2(movementVector.y, movementVector.x) * Mathf.Rad2Deg;
                
                // Convert to Unity's coordinate system where down is positive Y
                angle = -angle; // Flip Y axis
                
                currentMovementAngle = angle;
                
                // Update sprite to match movement direction
                UpdateDirectionalSprite(currentMovementAngle);
                
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
                    // Don't update sprite here - let UpdateSpriteAnimation handle it
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