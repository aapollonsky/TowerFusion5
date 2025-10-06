using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerFusion
{
    /// <summary>
    /// Component that manages traits applied to a tower
    /// </summary>
    public class TowerTraitManager : MonoBehaviour
    {
        [Header("Current Traits")]
        [SerializeField] private List<TowerTrait> appliedTraits = new List<TowerTrait>();
        
        [Header("Visual Components")]
        [SerializeField] private SpriteRenderer baseRenderer;
        [SerializeField] private SpriteRenderer overlayRenderer;
        [SerializeField] private Transform effectsParent;
        
        // Cached components
        private Tower tower;
        private List<ParticleSystem> activeEffects = new List<ParticleSystem>();
        
        // Events
        public System.Action<TowerTrait> OnTraitAdded;
        public System.Action<TowerTrait> OnTraitRemoved;
        public System.Action OnTraitsChanged;
        
        // Properties
        public List<TowerTrait> AppliedTraits => appliedTraits.ToList(); // Return copy to prevent external modification
        public int TraitCount => appliedTraits.Count;
        public bool HasTraits => appliedTraits.Count > 0;
        
        private void Awake()
        {
            tower = GetComponent<Tower>();
            
            if (baseRenderer == null)
                baseRenderer = GetComponent<SpriteRenderer>();
                
            SetupOverlayRenderer();
            SetupEffectsParent();
        }
        
        private void Start()
        {
            // Apply any pre-configured traits
            if (appliedTraits.Count > 0)
            {
                RefreshAllVisuals();
                NotifyTowerOfTraitChanges();
            }
        }
        
        /// <summary>
        /// Add a trait to this tower
        /// </summary>
        public bool AddTrait(TowerTrait trait)
        {
            if (trait == null)
            {
                Debug.LogWarning("Cannot add null trait to tower");
                return false;
            }
            
            if (appliedTraits.Contains(trait))
            {
                Debug.LogWarning($"Tower already has trait: {trait.traitName}");
                return false;
            }
            
            appliedTraits.Add(trait);
            ApplyTraitVisuals(trait);
            NotifyTowerOfTraitChanges();
            
            // Show range indicator if this trait affects range
            if (trait.rangeMultiplier != 1f || trait.rangeBonus != 0f)
            {
                if (tower != null)
                {
                    tower.ShowRangeIndicatorTemporarily(3f); // Show for 3 seconds
                    Debug.Log($"Showing range indicator for {trait.traitName} (range modifier: {trait.rangeMultiplier}x + {trait.rangeBonus})");
                }
            }
            
            OnTraitAdded?.Invoke(trait);
            OnTraitsChanged?.Invoke();
            
            Debug.Log($"Added trait '{trait.traitName}' to tower at {transform.position}");
            return true;
        }
        
        /// <summary>
        /// Remove a trait from this tower
        /// </summary>
        public bool RemoveTrait(TowerTrait trait)
        {
            if (trait == null || !appliedTraits.Contains(trait))
                return false;
            
            appliedTraits.Remove(trait);
            RefreshAllVisuals();
            NotifyTowerOfTraitChanges();
            
            OnTraitRemoved?.Invoke(trait);
            OnTraitsChanged?.Invoke();
            
            Debug.Log($"Removed trait '{trait.traitName}' from tower at {transform.position}");
            return true;
        }
        
        /// <summary>
        /// Remove all traits from this tower
        /// </summary>
        public void ClearAllTraits()
        {
            if (appliedTraits.Count == 0) return;
            
            appliedTraits.Clear();
            RefreshAllVisuals();
            NotifyTowerOfTraitChanges();
            
            OnTraitsChanged?.Invoke();
            Debug.Log($"Cleared all traits from tower at {transform.position}");
        }
        
        /// <summary>
        /// Check if tower has a specific trait
        /// </summary>
        public bool HasTrait(TowerTrait trait)
        {
            return appliedTraits.Contains(trait);
        }
        
        /// <summary>
        /// Check if tower has any trait of a specific category
        /// </summary>
        public bool HasTraitOfCategory(TraitCategory category)
        {
            return appliedTraits.Any(trait => trait.category == category);
        }
        
        /// <summary>
        /// Get all traits of a specific category
        /// </summary>
        public List<TowerTrait> GetTraitsOfCategory(TraitCategory category)
        {
            return appliedTraits.Where(trait => trait.category == category).ToList();
        }
        
        /// <summary>
        /// Calculate combined stats from all traits
        /// </summary>
        public TowerStats CalculateModifiedStats(TowerStats baseStats)
        {
            TowerStats modifiedStats = baseStats;
            
            Debug.Log($"Base stats - Range: {baseStats.range}, Damage: {baseStats.damage}, AttackSpeed: {baseStats.attackSpeed}, ChargeTime: {baseStats.chargeTime}");
            
            foreach (var trait in appliedTraits)
            {
                TowerStats beforeStats = modifiedStats;
                modifiedStats = trait.ApplyToStats(modifiedStats);
                
                Debug.Log($"Trait '{trait.traitName}': Range {beforeStats.range} -> {modifiedStats.range} (multiplier: {trait.rangeMultiplier}, bonus: {trait.rangeBonus})");
            }
            
            Debug.Log($"Final modified stats - Range: {modifiedStats.range}, Damage: {modifiedStats.damage}, AttackSpeed: {modifiedStats.attackSpeed}, ChargeTime: {modifiedStats.chargeTime}");
            
            return modifiedStats;
        }
        
        /// <summary>
        /// Get combined visual effects from all traits
        /// </summary>
        public Color GetCombinedOverlayColor()
        {
            if (appliedTraits.Count == 0)
                return Color.clear;
            
            Color combined = Color.clear;
            float totalAlpha = 0f;
            
            foreach (var trait in appliedTraits)
            {
                Color traitColor = trait.overlayColor;
                traitColor.a = trait.overlayAlpha;
                
                if (combined == Color.clear)
                {
                    combined = traitColor;
                }
                else
                {
                    // Blend colors additively for better visibility
                    combined = Color.Lerp(combined, traitColor, 0.6f);
                }
                
                totalAlpha += trait.overlayAlpha;
            }
            
            // Ensure minimum visibility and cap maximum
            combined.a = Mathf.Clamp(totalAlpha, 0.3f, 0.9f);
            
            Debug.Log($"Combined overlay color for {name}: {combined} (from {appliedTraits.Count} traits)");
            return combined;
        }
        
        #region Visual Management
        
        private void SetupOverlayRenderer()
        {
            if (overlayRenderer == null)
            {
                GameObject overlayObj = new GameObject("TraitOverlay");
                overlayObj.transform.SetParent(transform);
                overlayObj.transform.localPosition = Vector3.zero;
                overlayObj.transform.localScale = Vector3.one;
                
                overlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
                
                // Ensure we have a base renderer reference
                if (baseRenderer == null)
                    baseRenderer = GetComponent<SpriteRenderer>();
                
                if (baseRenderer != null)
                {
                    overlayRenderer.sortingOrder = baseRenderer.sortingOrder + 1;
                    // Copy the main sprite to the overlay initially
                    overlayRenderer.sprite = baseRenderer.sprite;
                    Debug.Log($"Setup trait overlay for {name}: base sorting order {baseRenderer.sortingOrder}, overlay {overlayRenderer.sortingOrder}");
                }
                else
                {
                    overlayRenderer.sortingOrder = 1; // Default sorting order
                    Debug.LogWarning($"No base SpriteRenderer found for {name} - overlay may not display correctly");
                }
                
                overlayRenderer.color = Color.clear;
            }
        }
        
        private void SetupEffectsParent()
        {
            if (effectsParent == null)
            {
                GameObject effectsObj = new GameObject("TraitEffects");
                effectsObj.transform.SetParent(transform);
                effectsObj.transform.localPosition = Vector3.zero;
                effectsParent = effectsObj.transform;
            }
        }
        
        /// <summary>
        /// Create a trait badge icon for the tower
        /// </summary>
        private void CreateTraitBadge(TowerTrait trait)
        {
            Debug.Log($"CreateTraitBadge called for trait '{trait.traitName}'");
            
            if (effectsParent == null)
            {
                Debug.LogError("effectsParent is null! Cannot create badge.");
                return;
            }
            
            // Create badge container
            GameObject badgeObj = new GameObject($"TraitBadge_{trait.traitName}");
            badgeObj.transform.SetParent(effectsParent);
            Debug.Log($"  - Badge parent set to: {effectsParent.name}");
            
            // Position based on number of existing badges (arrange them in a small arc)
            int badgeCount = effectsParent.childCount - 1; // -1 because this badge isn't counted yet
            float angle = badgeCount * 45f; // Spread badges around the tower
            Vector3 badgePosition = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * trait.badgeOffset.x,
                Mathf.Sin(angle * Mathf.Deg2Rad) * trait.badgeOffset.y,
                0f
            );
            badgeObj.transform.localPosition = badgePosition;
            badgeObj.transform.localScale = Vector3.one * trait.badgeScale;
            Debug.Log($"  - Badge positioned at: {badgePosition}, scale: {trait.badgeScale}, angle: {angle}°");
            
            // Add SpriteRenderer for the badge icon
            SpriteRenderer badgeRenderer = badgeObj.AddComponent<SpriteRenderer>();
            
            // Use assigned sprite or create a fallback based on trait name
            if (trait.traitBadge != null)
            {
                badgeRenderer.sprite = trait.traitBadge;
                Debug.Log($"  - Using assigned sprite: {trait.traitBadge.name}");
            }
            else
            {
                // Create a simple colored circle as fallback
                Debug.Log($"  - Creating fallback sprite for '{trait.traitName}' with color {trait.overlayColor}");
                badgeRenderer.sprite = CreateFallbackBadgeSprite(trait);
                Debug.Log($"  - Fallback sprite created: {(badgeRenderer.sprite != null ? "Success" : "Failed")}");
            }
            
            // Ensure badges render on top of everything
            if (baseRenderer != null)
            {
                badgeRenderer.sortingLayerName = baseRenderer.sortingLayerName;
                badgeRenderer.sortingOrder = baseRenderer.sortingOrder + 10; // Well above tower
                Debug.Log($"  - Badge render setup: layer={badgeRenderer.sortingLayerName}, order={badgeRenderer.sortingOrder} (tower order: {baseRenderer.sortingOrder})");
            }
            else
            {
                badgeRenderer.sortingLayerName = "Default";
                badgeRenderer.sortingOrder = 100;
                Debug.Log($"  - Badge render setup (no baseRenderer): layer={badgeRenderer.sortingLayerName}, order={badgeRenderer.sortingOrder}");
            }
            
            // Add subtle glow background
            GameObject glowObj = new GameObject("BadgeGlow");
            glowObj.transform.SetParent(badgeObj.transform);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localScale = Vector3.one * 1.2f; // Slightly bigger than the badge
            
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            // Create a simple circle sprite for glow (you might want to use a pre-made glow sprite)
            if (baseRenderer != null && baseRenderer.sprite != null)
            {
                glowRenderer.sprite = baseRenderer.sprite; // Temporary - use tower sprite as glow base
            }
            glowRenderer.color = new Color(trait.overlayColor.r, trait.overlayColor.g, trait.overlayColor.b, 0.3f);
            
            // Ensure glow renders behind badge but above tower
            if (baseRenderer != null)
            {
                glowRenderer.sortingLayerName = baseRenderer.sortingLayerName;
                glowRenderer.sortingOrder = baseRenderer.sortingOrder + 9; // Behind badge, above tower
            }
            else
            {
                glowRenderer.sortingLayerName = "Default";
                glowRenderer.sortingOrder = 99;
            }
            
            // Add animation if enabled
            if (trait.animateBadge)
            {
                StartCoroutine(AnimateTraitBadge(badgeObj.transform, glowRenderer));
            }
            
            Debug.Log($"Created trait badge '{trait.traitName}' at local position {badgePosition} with scale {trait.badgeScale}");
            Debug.Log($"  - World position: {badgeObj.transform.position}");
            Debug.Log($"  - Sorting layer: {badgeRenderer.sortingLayerName}, Order: {badgeRenderer.sortingOrder}");
            Debug.Log($"  - Badge sprite: {(badgeRenderer.sprite != null ? badgeRenderer.sprite.name : "null")}");
        }
        
        /// <summary>
        /// Animate trait badges with subtle float and pulse effects
        /// </summary>
        private System.Collections.IEnumerator AnimateTraitBadge(Transform badgeTransform, SpriteRenderer glowRenderer)
        {
            Vector3 basePosition = badgeTransform.localPosition;
            Vector3 baseScale = badgeTransform.localScale;
            Color baseGlowColor = glowRenderer.color;
            
            while (badgeTransform != null && appliedTraits.Count > 0)
            {
                float time = Time.time;
                
                // Gentle float animation
                Vector3 floatOffset = new Vector3(0f, Mathf.Sin(time * 1.5f) * 0.1f, 0f);
                badgeTransform.localPosition = basePosition + floatOffset;
                
                // Subtle scale pulse
                float scalePulse = 1f + Mathf.Sin(time * 2f) * 0.05f;
                badgeTransform.localScale = baseScale * scalePulse;
                
                // Glow pulse
                float glowPulse = 0.3f + Mathf.Sin(time * 1.8f) * 0.1f;
                Color glowColor = baseGlowColor;
                glowColor.a = glowPulse;
                glowRenderer.color = glowColor;
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Create a simple fallback badge sprite for traits without assigned icons
        /// </summary>
        private Sprite CreateFallbackBadgeSprite(TowerTrait trait)
        {
            // Create a larger, more visible 64x64 texture with a colored circle and border
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float outerRadius = size / 2f - 2f;
            float innerRadius = outerRadius - 4f; // Border thickness
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= outerRadius)
                    {
                        Color color;
                        if (distance > innerRadius)
                        {
                            // White border for visibility
                            color = Color.white;
                        }
                        else
                        {
                            // Trait color center with full opacity
                            color = trait.overlayColor;
                            color.a = 1f;
                        }
                        pixels[y * size + x] = color;
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Create sprite from texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            Debug.Log($"Created fallback badge sprite for '{trait.traitName}' - Size: {size}x{size}, Color: {trait.overlayColor}");
            return sprite;
        }
        
        /// <summary>
        /// Create a simple glow effect for trait visualization
        /// </summary>
        private void CreateTraitGlow(Color glowColor)
        {
            GameObject glowObj = new GameObject("TraitGlow");
            glowObj.transform.SetParent(effectsParent);
            glowObj.transform.localPosition = Vector3.zero;
            glowObj.transform.localScale = Vector3.one * 1.3f; // Slightly larger than base
            
            SpriteRenderer glowRenderer = glowObj.AddComponent<SpriteRenderer>();
            
            // Use base sprite for glow shape
            if (baseRenderer != null && baseRenderer.sprite != null)
            {
                glowRenderer.sprite = baseRenderer.sprite;
            }
            
            // Set glow color and properties
            glowColor.a = 0.4f;
            glowRenderer.color = glowColor;
            glowRenderer.sortingOrder = baseRenderer.sortingOrder - 1; // Behind base sprite
            
            // Add pulsing animation
            StartCoroutine(PulseGlow(glowRenderer));
            
            Debug.Log($"Created trait glow effect: {glowColor}");
        }
        
        /// <summary>
        /// Animate the glow effect
        /// </summary>
        private System.Collections.IEnumerator PulseGlow(SpriteRenderer glowRenderer)
        {
            float baseAlpha = glowRenderer.color.a;
            Color baseColor = glowRenderer.color;
            
            while (glowRenderer != null && appliedTraits.Count > 0)
            {
                float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 1f; // Pulse between 0.8 and 1.2
                Color newColor = baseColor;
                newColor.a = baseAlpha * pulse;
                glowRenderer.color = newColor;
                
                Vector3 scale = Vector3.one * (1.2f + Mathf.Sin(Time.time * 1.5f) * 0.1f);
                glowRenderer.transform.localScale = scale;
                
                yield return null;
            }
        }
        
        private void ApplyTraitVisuals(TowerTrait trait)
        {
            Debug.Log($"Applied visual for trait '{trait.traitName}' on {name}:");
            
            // Always create trait badge icon (with fallback if no sprite assigned)
            CreateTraitBadge(trait);
            if (trait.traitBadge != null)
            {
                Debug.Log($"  - Created trait badge: {trait.traitBadge.name}");
            }
            else
            {
                Debug.Log($"  - Created fallback badge for '{trait.traitName}'");
            }
            
            // Add particle effects
            if (trait.effectPrefab != null)
            {
                GameObject effectInstance = Instantiate(trait.effectPrefab.gameObject, effectsParent);
                ParticleSystem particles = effectInstance.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    activeEffects.Add(particles);
                    particles.Play();
                    Debug.Log($"  - Added particle effect: {trait.effectPrefab.name}");
                }
            }
            
            // Hide color overlay system (keeping it for fallback)
            if (overlayRenderer != null)
            {
                overlayRenderer.gameObject.SetActive(false);
            }
        }
        
        private void RefreshAllVisuals()
        {
            // Clear existing effects
            foreach (var effect in activeEffects)
            {
                if (effect != null)
                    DestroyImmediate(effect.gameObject);
            }
            activeEffects.Clear();
            
            // Clear existing trait badges
            if (effectsParent != null)
            {
                for (int i = effectsParent.childCount - 1; i >= 0; i--)
                {
                    Transform child = effectsParent.GetChild(i);
                    if (child.name.StartsWith("TraitBadge_"))
                    {
                        DestroyImmediate(child.gameObject);
                    }
                }
            }
            
            // Reset overlay (keeping for fallback)
            if (overlayRenderer != null)
            {
                overlayRenderer.color = Color.clear;
                overlayRenderer.sprite = null;
                overlayRenderer.gameObject.SetActive(false);
            }
            
            // Reset base renderer to original color
            if (baseRenderer != null)
            {
                baseRenderer.color = Color.white;
            }
            
            // Reapply all trait visuals
            foreach (var trait in appliedTraits)
            {
                ApplyTraitVisuals(trait);
            }
            
            // Also apply trait color tint to base renderer for better visibility
            if (appliedTraits.Count > 0 && baseRenderer != null)
            {
                Color combinedColor = GetCombinedOverlayColor();
                Color baseColorTint = Color.Lerp(Color.white, combinedColor, 0.3f);
                baseRenderer.color = baseColorTint;
                Debug.Log($"Applied base color tint to {name}: {baseColorTint}");
            }
        }
        
        private void NotifyTowerOfTraitChanges()
        {
            if (tower != null)
            {
                // Tell the tower to recalculate its stats
                tower.OnTraitsChanged();
            }
        }
        
        #endregion
        
        #region Combat Integration
        
        /// <summary>
        /// Apply trait effects when tower attacks an enemy
        /// </summary>
        public void ApplyTraitEffectsOnAttack(Enemy target, float damage)
        {
            if (target == null) return;
            
            Debug.Log($"Applying trait effects on attack to {target.name} with {appliedTraits.Count} traits");
            
            foreach (var trait in appliedTraits)
            {
                Debug.Log($"Applying trait: {trait.traitName} (hasChainEffect: {trait.hasChainEffect})");
                ApplyTraitEffect(trait, target, damage);
            }
        }
        
        /// <summary>
        /// Apply trait effects when tower kills an enemy
        /// </summary>
        public void ApplyTraitEffectsOnKill(Enemy target)
        {
            if (target == null) return;
            
            foreach (var trait in appliedTraits)
            {
                if (trait.hasGoldReward)
                {
                    GameManager.Instance?.AddGold(trait.goldPerKill);
                    Debug.Log($"Harvest trait: +{trait.goldPerKill} gold from kill");
                }
            }
        }
        
        private void ApplyTraitEffect(TowerTrait trait, Enemy target, float damage)
        {
            // Apply burn effect
            if (trait.hasBurnEffect)
            {
                target.ApplyBurnEffect(trait.burnDamagePerSecond, trait.burnDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied burn effect to {target.name}");
            }
            
            // Apply slow effect
            if (trait.hasSlowEffect)
            {
                target.ApplySlowEffect(trait.slowMultiplier, trait.slowDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied slow effect to {target.name} ({trait.slowMultiplier}x speed)");
            }
            
            // Apply brittle effect
            if (trait.hasBrittleEffect)
            {
                target.ApplyBrittleEffect(trait.brittleDamageMultiplier, trait.brittleDuration);
                Debug.Log($"Trait '{trait.traitName}': Applied brittle effect to {target.name} ({trait.brittleDamageMultiplier}x damage taken)");
            }
            
            // Apply chain effect
            if (trait.hasChainEffect)
            {
                Debug.Log($"Trait '{trait.traitName}' has chain effect - applying to {target.name}");
                ApplyChainEffect(trait, target, damage);
            }
        }
        
        private void ApplyChainEffect(TowerTrait trait, Enemy primaryTarget, float damage)
        {
            Debug.Log($"Applying chain effect from {primaryTarget.name} with range {trait.chainRange}");
            
            // Find nearby enemies for chaining - use OverlapCircleAll without layer mask first
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(
                primaryTarget.transform.position, 
                trait.chainRange
            );
            
            Debug.Log($"Found {nearbyColliders.Length} colliders in range");
            
            List<Enemy> chainTargets = new List<Enemy>();
            foreach (var collider in nearbyColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy != primaryTarget)
                {
                    chainTargets.Add(enemy);
                    Debug.Log($"Found chain target: {enemy.name} at distance {Vector3.Distance(primaryTarget.transform.position, enemy.transform.position):F2}");
                }
            }
            
            // Sort by distance and take closest targets
            chainTargets = chainTargets
                .OrderBy(e => Vector3.Distance(primaryTarget.transform.position, e.transform.position))
                .Take(trait.chainTargets)
                .ToList();
            
            // Apply chain damage
            float chainDamage = damage * trait.chainDamageMultiplier;
            Enemy lastTarget = primaryTarget;
            
            Debug.Log($"Chain targets found: {chainTargets.Count}, will deal {chainDamage} damage each");
            
            foreach (var chainTarget in chainTargets)
            {
                chainTarget.TakeDamage(chainDamage, DamageType.Magic); // Chain lightning is magical damage
                
                // Create lightning visual effect
                StartCoroutine(CreateLightningEffect(lastTarget.transform.position, chainTarget.transform.position));
                
                // Create a simple visual effect as backup
                StartCoroutine(CreateSimpleLightningFlash(chainTarget.transform.position));
                
                // Debug visualization (this shows in Scene view)
                Debug.DrawLine(lastTarget.transform.position, chainTarget.transform.position, Color.yellow, 2f);
                Debug.Log($"Chain lightning: {chainDamage} damage to {chainTarget.name} from {lastTarget.name}");
                
                lastTarget = chainTarget;
            }
        }
        
        private System.Collections.IEnumerator CreateLightningEffect(Vector3 startPos, Vector3 endPos)
        {
            Debug.Log($"Creating lightning effect from {startPos} to {endPos}");
            
            // Create temporary lightning line effect
            GameObject lightningLine = new GameObject("LightningEffect");
            LineRenderer lineRenderer = lightningLine.AddComponent<LineRenderer>();
            
            // Configure line renderer with a more visible setup
            Material lightningMaterial = new Material(Shader.Find("Sprites/Default"));
            lightningMaterial.color = Color.yellow;
            lineRenderer.material = lightningMaterial;
            lineRenderer.startWidth = 0.2f; // Make thicker for visibility
            lineRenderer.endWidth = 0.2f;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            
            // Try to set sorting layer, but don't fail if it doesn't exist
            try
            {
                lineRenderer.sortingLayerName = "Effects";
                lineRenderer.sortingOrder = 10;
            }
            catch
            {
                // Fallback - just use a high sorting order
                lineRenderer.sortingOrder = 100;
            }
            
            // Set positions
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            // Animate lightning effect
            float duration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                Color color = Color.yellow;
                color.a = alpha;
                lineRenderer.material.color = color;
                
                // Add slight random offset for lightning effect
                Vector3 direction = (endPos - startPos).normalized;
                Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * UnityEngine.Random.Range(-0.2f, 0.2f);
                Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f) + perpendicular;
                
                lineRenderer.positionCount = 3;
                lineRenderer.SetPosition(0, startPos);
                lineRenderer.SetPosition(1, midPoint);
                lineRenderer.SetPosition(2, endPos);
                
                yield return null;
            }
            
            // Clean up
            if (lightningLine != null)
                DestroyImmediate(lightningLine);
        }
        
        private System.Collections.IEnumerator CreateSimpleLightningFlash(Vector3 position)
        {
            // Create a simple white flash effect at the target position
            GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            flash.transform.position = position;
            flash.transform.localScale = Vector3.one * 0.5f;
            
            // Remove collider as we don't need it
            Collider flashCollider = flash.GetComponent<Collider>();
            if (flashCollider != null) DestroyImmediate(flashCollider);
            
            Collider2D flashCollider2D = flash.GetComponent<Collider2D>();
            if (flashCollider2D != null) DestroyImmediate(flashCollider2D);
            
            // Set up renderer
            Renderer renderer = flash.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = Color.yellow;
            }
            
            // Flash animation
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 originalScale = flash.transform.localScale;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Scale up then down
                float scaleMultiplier = Mathf.Sin(progress * Mathf.PI) * 2f;
                flash.transform.localScale = originalScale * scaleMultiplier;
                
                // Fade out
                if (renderer != null)
                {
                    Color color = Color.yellow;
                    color.a = 1f - progress;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            // Clean up
            if (flash != null)
                DestroyImmediate(flash);
        }
        
        #endregion
    }
}