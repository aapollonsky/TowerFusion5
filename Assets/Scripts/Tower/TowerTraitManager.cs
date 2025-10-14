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
        private List<TraitProjectileSystem> projectileSystems = new List<TraitProjectileSystem>();
        
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
            
            // Setup independent projectile system if trait has one
            if (trait.hasIndependentProjectile)
            {
                SetupTraitProjectileSystem(trait);
            }
            
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
            
            // Remove projectile system if trait had one
            if (trait.hasIndependentProjectile)
            {
                RemoveTraitProjectileSystem(trait);
            }
            
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
            
            // Clear all projectile systems
            ClearAllProjectileSystems();
            
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
            
            // Ensure minimum badge scale for visibility
            float actualScale = trait.badgeScale > 0 ? trait.badgeScale : 1.2f;
            if (actualScale < 1.0f) actualScale = 1.2f; // Force larger badges
            
            badgeObj.transform.localScale = Vector3.one * actualScale;
            Debug.Log($"  - Badge positioned at: {badgePosition}, trait scale: {trait.badgeScale}, actual scale used: {actualScale}, angle: {angle}°");
            
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
            
            // Add animation if enabled
            if (trait.animateBadge)
            {
                StartCoroutine(AnimateTraitBadge(badgeObj.transform));
            }
            
            Debug.Log($"Created trait badge '{trait.traitName}' at local position {badgePosition} with scale {trait.badgeScale}");
            Debug.Log($"  - World position: {badgeObj.transform.position}");
            Debug.Log($"  - Sorting layer: {badgeRenderer.sortingLayerName}, Order: {badgeRenderer.sortingOrder}");
            Debug.Log($"  - Badge sprite: {(badgeRenderer.sprite != null ? badgeRenderer.sprite.name : "null")}");
        }
        
        /// <summary>
        /// Animate trait badges with subtle float and pulse effects
        /// </summary>
        private System.Collections.IEnumerator AnimateTraitBadge(Transform badgeTransform)
        {
            Vector3 basePosition = badgeTransform.localPosition;
            Vector3 baseScale = badgeTransform.localScale;
            
            while (badgeTransform != null && appliedTraits.Count > 0)
            {
                float time = Time.time;
                
                // Gentle float animation
                Vector3 floatOffset = new Vector3(0f, Mathf.Sin(time * 1.5f) * 0.1f, 0f);
                badgeTransform.localPosition = basePosition + floatOffset;
                
                // Subtle scale pulse
                float scalePulse = 1f + Mathf.Sin(time * 2f) * 0.05f;
                badgeTransform.localScale = baseScale * scalePulse;
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Create a trait-specific badge sprite based on the trait name
        /// </summary>
        private Sprite CreateFallbackBadgeSprite(TowerTrait trait)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            // Initialize with transparent background
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            Color traitColor = trait.overlayColor;
            traitColor.a = 1f;
            
            // Create trait-specific icons based on trait name
            string traitName = trait.traitName.ToLower();
            
            if (traitName.Contains("ice") || traitName.Contains("frost") || traitName.Contains("freeze"))
            {
                CreateSnowflakeIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("fire") || traitName.Contains("burn") || traitName.Contains("flame"))
            {
                CreateFireIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("lightning") || traitName.Contains("electric") || traitName.Contains("shock"))
            {
                CreateLightningIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("poison") || traitName.Contains("toxic") || traitName.Contains("venom"))
            {
                CreatePoisonIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("harvest") || traitName.Contains("gold") || traitName.Contains("coin"))
            {
                CreateCoinIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("speed") || traitName.Contains("fast") || traitName.Contains("rapid"))
            {
                CreateSpeedIcon(pixels, size, center, traitColor);
            }
            else if (traitName.Contains("power") || traitName.Contains("damage") || traitName.Contains("strength"))
            {
                CreatePowerIcon(pixels, size, center, traitColor);
            }
            else
            {
                // Default: simple diamond shape
                CreateDiamondIcon(pixels, size, center, traitColor);
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            // Create sprite from texture
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            Debug.Log($"Created trait-specific badge for '{trait.traitName}' - Type: {GetIconType(traitName)}");
            return sprite;
        }
        
        private string GetIconType(string traitName)
        {
            if (traitName.Contains("ice") || traitName.Contains("frost") || traitName.Contains("freeze")) return "Snowflake";
            if (traitName.Contains("fire") || traitName.Contains("burn") || traitName.Contains("flame")) return "Fire";
            if (traitName.Contains("lightning") || traitName.Contains("electric") || traitName.Contains("shock")) return "Lightning";
            if (traitName.Contains("poison") || traitName.Contains("toxic") || traitName.Contains("venom")) return "Poison";
            if (traitName.Contains("harvest") || traitName.Contains("gold") || traitName.Contains("coin")) return "Coin";
            if (traitName.Contains("speed") || traitName.Contains("fast") || traitName.Contains("rapid")) return "Speed";
            if (traitName.Contains("power") || traitName.Contains("damage") || traitName.Contains("strength")) return "Power";
            return "Diamond";
        }
        
        private void CreateSnowflakeIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw a 6-pointed snowflake
            float armLength = size * 0.35f;
            
            // Draw 6 arms at 60-degree intervals
            for (int arm = 0; arm < 6; arm++)
            {
                float angle = arm * 60f * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                // Main arm
                DrawLine(pixels, size, center, center + direction * armLength, color, 2);
                
                // Small branches
                Vector2 branchPoint = center + direction * (armLength * 0.6f);
                Vector2 branchDir1 = new Vector2(Mathf.Cos(angle + 0.5f), Mathf.Sin(angle + 0.5f));
                Vector2 branchDir2 = new Vector2(Mathf.Cos(angle - 0.5f), Mathf.Sin(angle - 0.5f));
                DrawLine(pixels, size, branchPoint, branchPoint + branchDir1 * (armLength * 0.3f), color, 1);
                DrawLine(pixels, size, branchPoint, branchPoint + branchDir2 * (armLength * 0.3f), color, 1);
            }
            
            // Center dot
            DrawCircle(pixels, size, center, 3, color);
        }
        
        private void CreateFireIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw flame shape
            int points = 8;
            Vector2[] flamePoints = new Vector2[points];
            
            for (int i = 0; i < points; i++)
            {
                float angle = (i / (float)points) * 2f * Mathf.PI;
                float radius = size * 0.25f + Mathf.Sin(angle * 3f) * size * 0.1f; // Wavy edge
                if (i % 2 == 1) radius *= 1.2f; // Alternate radius for flame effect
                
                flamePoints[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            
            // Fill the flame shape
            FillPolygon(pixels, size, flamePoints, color);
            
            // Add inner flame detail
            Color innerColor = new Color(1f, 1f, 0.8f, 1f); // Lighter center
            DrawCircle(pixels, size, center, (int)(size * 0.15f), innerColor);
        }
        
        private void CreateLightningIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw zigzag lightning bolt
            Vector2[] lightningPoints = new Vector2[]
            {
                center + new Vector2(-size * 0.1f, size * 0.3f),
                center + new Vector2(size * 0.05f, size * 0.1f),
                center + new Vector2(-size * 0.05f, size * 0.1f),
                center + new Vector2(size * 0.15f, -size * 0.1f),
                center + new Vector2(0f, -size * 0.1f),
                center + new Vector2(size * 0.1f, -size * 0.3f)
            };
            
            // Draw thick lightning bolt
            for (int i = 0; i < lightningPoints.Length - 1; i++)
            {
                DrawLine(pixels, size, lightningPoints[i], lightningPoints[i + 1], color, 3);
            }
        }
        
        private void CreatePoisonIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw skull-like poison symbol (simplified drop shape with cross)
            DrawCircle(pixels, size, center, (int)(size * 0.25f), color);
            
            // Draw poison drop dripping down
            Vector2 dropBottom = center + new Vector2(0, -size * 0.3f);
            DrawLine(pixels, size, center + new Vector2(0, -size * 0.25f), dropBottom, color, 3);
            
            // Small X in the center
            Vector2 crossSize = new Vector2(size * 0.1f, size * 0.1f);
            DrawLine(pixels, size, center - crossSize, center + crossSize, Color.black, 2);
            DrawLine(pixels, size, center + new Vector2(-crossSize.x, crossSize.y), center + new Vector2(crossSize.x, -crossSize.y), Color.black, 2);
        }
        
        private void CreateCoinIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw coin with $ symbol
            DrawCircle(pixels, size, center, (int)(size * 0.3f), color);
            DrawCircle(pixels, size, center, (int)(size * 0.28f), Color.black); // Border
            DrawCircle(pixels, size, center, (int)(size * 0.25f), color);
            
            // Draw $ symbol
            Color symbolColor = Color.black;
            // Vertical line
            DrawLine(pixels, size, center + new Vector2(0, size * 0.2f), center + new Vector2(0, -size * 0.2f), symbolColor, 2);
            // S curves (simplified as horizontal lines)
            DrawLine(pixels, size, center + new Vector2(-size * 0.1f, size * 0.1f), center + new Vector2(size * 0.1f, size * 0.1f), symbolColor, 2);
            DrawLine(pixels, size, center + new Vector2(-size * 0.1f, -size * 0.1f), center + new Vector2(size * 0.1f, -size * 0.1f), symbolColor, 2);
        }
        
        private void CreateSpeedIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw arrow pointing right with motion lines
            Vector2[] arrowPoints = new Vector2[]
            {
                center + new Vector2(-size * 0.2f, 0f),
                center + new Vector2(size * 0.2f, 0f),
                center + new Vector2(size * 0.1f, size * 0.1f),
                center + new Vector2(size * 0.2f, 0f),
                center + new Vector2(size * 0.1f, -size * 0.1f)
            };
            
            // Draw arrow shaft
            DrawLine(pixels, size, arrowPoints[0], arrowPoints[1], color, 3);
            // Draw arrowhead
            DrawLine(pixels, size, arrowPoints[1], arrowPoints[2], color, 3);
            DrawLine(pixels, size, arrowPoints[1], arrowPoints[4], color, 3);
            
            // Motion lines behind arrow
            for (int i = 1; i <= 3; i++)
            {
                Vector2 lineStart = center + new Vector2(-size * 0.3f - i * size * 0.05f, 0f);
                Vector2 lineEnd = lineStart + new Vector2(size * 0.1f, 0f);
                DrawLine(pixels, size, lineStart, lineEnd, color, 1);
            }
        }
        
        private void CreatePowerIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw burst/explosion pattern
            int rays = 8;
            for (int i = 0; i < rays; i++)
            {
                float angle = (i / (float)rays) * 2f * Mathf.PI;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                
                float length = (i % 2 == 0) ? size * 0.3f : size * 0.2f; // Alternating lengths
                DrawLine(pixels, size, center, center + direction * length, color, 2);
            }
            
            // Center star
            DrawCircle(pixels, size, center, (int)(size * 0.08f), color);
        }
        
        private void CreateDiamondIcon(Color[] pixels, int size, Vector2 center, Color color)
        {
            // Draw diamond shape
            Vector2[] diamondPoints = new Vector2[]
            {
                center + new Vector2(0, size * 0.25f),       // Top
                center + new Vector2(size * 0.2f, 0),        // Right
                center + new Vector2(0, -size * 0.25f),      // Bottom
                center + new Vector2(-size * 0.2f, 0)        // Left
            };
            
            FillPolygon(pixels, size, diamondPoints, color);
        }
        
        // Helper methods for drawing
        private void DrawLine(Color[] pixels, int size, Vector2 start, Vector2 end, Color color, int thickness)
        {
            Vector2 dir = (end - start).normalized;
            float distance = Vector2.Distance(start, end);
            
            for (float t = 0; t <= distance; t += 0.5f)
            {
                Vector2 pos = start + dir * t;
                DrawPixelThick(pixels, size, (int)pos.x, (int)pos.y, color, thickness);
            }
        }
        
        private void DrawCircle(Color[] pixels, int size, Vector2 center, int radius, Color color)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int px = (int)(center.x + x);
                        int py = (int)(center.y + y);
                        if (px >= 0 && px < size && py >= 0 && py < size)
                        {
                            pixels[py * size + px] = color;
                        }
                    }
                }
            }
        }
        
        private void FillPolygon(Color[] pixels, int size, Vector2[] points, Color color)
        {
            // Simple polygon fill - just fill the bounding area for now
            float minX = points[0].x, maxX = points[0].x;
            float minY = points[0].y, maxY = points[0].y;
            
            foreach (var point in points)
            {
                minX = Mathf.Min(minX, point.x);
                maxX = Mathf.Max(maxX, point.x);
                minY = Mathf.Min(minY, point.y);
                maxY = Mathf.Max(maxY, point.y);
            }
            
            for (int y = (int)minY; y <= (int)maxY; y++)
            {
                for (int x = (int)minX; x <= (int)maxX; x++)
                {
                    if (x >= 0 && x < size && y >= 0 && y < size)
                    {
                        // Simple point-in-polygon test (simplified)
                        Vector2 testPoint = new Vector2(x, y);
                        if (IsPointInPolygon(testPoint, points))
                        {
                            pixels[y * size + x] = color;
                        }
                    }
                }
            }
        }
        
        private bool IsPointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            int j = polygon.Length - 1;
            
            for (int i = 0; i < polygon.Length; i++)
            {
                Vector2 pi = polygon[i];
                Vector2 pj = polygon[j];
                
                if (((pi.y > point.y) != (pj.y > point.y)) &&
                    (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x))
                {
                    inside = !inside;
                }
                j = i;
            }
            
            return inside;
        }
        
        private void DrawPixelThick(Color[] pixels, int size, int x, int y, Color color, int thickness)
        {
            for (int dy = -thickness / 2; dy <= thickness / 2; dy++)
            {
                for (int dx = -thickness / 2; dx <= thickness / 2; dx++)
                {
                    int px = x + dx;
                    int py = y + dy;
                    if (px >= 0 && px < size && py >= 0 && py < size)
                    {
                        pixels[py * size + px] = color;
                    }
                }
            }
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
            
            Debug.Log($"<color=yellow>>>> ApplyTraitEffectsOnAttack: {target.name}, Traits count: {appliedTraits.Count}</color>");
            
            foreach (var trait in appliedTraits)
            {
                Debug.Log($"<color=yellow>  → Checking trait: {trait.traitName}</color>");
                Debug.Log($"     hasEarthTrapEffect: {trait.hasEarthTrapEffect}");
                Debug.Log($"     hasExplosionEffect: {trait.hasExplosionEffect}");
                Debug.Log($"     hasChainEffect: {trait.hasChainEffect}");
                Debug.Log($"     hasBurnEffect: {trait.hasBurnEffect}");
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
        
        /// <summary>
        /// Apply a single trait's effects (for trait-specific projectiles)
        /// </summary>
        public void ApplySingleTraitEffect(TowerTrait trait, Enemy target, float damage)
        {
            if (trait == null || target == null) return;
            ApplyTraitEffect(trait, target, damage);
        }
        
        /// <summary>
        /// Apply a single trait's kill effects (for trait-specific projectiles)
        /// </summary>
        public void ApplySingleTraitKillEffect(TowerTrait trait, Enemy target)
        {
            if (trait == null || target == null) return;
            
            if (trait.hasGoldReward)
            {
                GameManager.Instance?.AddGold(trait.goldPerKill);
                Debug.Log($"Trait '{trait.traitName}': +{trait.goldPerKill} gold from kill");
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
            
            // Apply explosion effect
            if (trait.hasExplosionEffect)
            {
                Debug.Log($"Trait '{trait.traitName}' has explosion effect - applying at {target.transform.position}");
                ApplyExplosionEffect(trait, target, damage);
            }
            
            // Apply earth trap effect - converts hit enemy into a hole trap
            if (trait.hasEarthTrapEffect)
            {
                Debug.Log($"<color=magenta>═══ EARTH TRAIT TRIGGERED ═══</color>");
                Debug.Log($"<color=cyan>Trait: '{trait.traitName}'</color>");
                Debug.Log($"<color=cyan>Target: {target.name}</color>");
                Debug.Log($"<color=cyan>Target Position: {target.transform.position}</color>");
                Debug.Log($"<color=cyan>Target Health: {target.CurrentHealth}/{target.MaxHealth}</color>");
                
                // Store position before destroying enemy
                Vector3 holePosition = target.transform.position;
                
                // Instantly kill and destroy the hit enemy
                Debug.Log($"<color=yellow>Killing enemy with overkill damage...</color>");
                target.TakeDamage(target.MaxHealth * 100f, DamageType.Magic); // Overkill to ensure death
                
                // Create hole at the enemy's position
                Debug.Log($"<color=yellow>Creating black disk at {holePosition}...</color>");
                CreateEarthHole(trait, holePosition);
                
                // Destroy the enemy GameObject immediately (it becomes the hole)
                if (target != null && target.gameObject != null)
                {
                    Debug.Log($"<color=yellow>Destroying enemy GameObject in 0.1s...</color>");
                    Destroy(target.gameObject, 0.1f); // Small delay to let damage register
                }
                
                Debug.Log($"<color=magenta>═══ EARTH TRAIT COMPLETE ═══</color>");
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
        
        private void ApplyExplosionEffect(TowerTrait trait, Enemy target, float damage)
        {
            Vector3 explosionCenter = target.transform.position;
            Debug.Log($"Creating explosion at {explosionCenter} with radius {trait.explosionRadius}");
            
            // Find all enemies in explosion radius
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(explosionCenter, trait.explosionRadius);
            Debug.Log($"Found {hitColliders.Length} colliders in explosion radius");
            
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var collider in hitColliders)
            {
                Enemy enemy = collider.GetComponent<Enemy>();
                if (enemy != null && enemy != target && enemy.IsAlive)
                {
                    enemiesInRange.Add(enemy);
                    Debug.Log($"Enemy in explosion: {enemy.name}");
                }
            }
            
            // Apply explosion damage
            float explosionDamage = damage * trait.explosionDamageMultiplier;
            foreach (var enemy in enemiesInRange)
            {
                enemy.TakeDamage(explosionDamage, DamageType.Fire);
                Debug.Log($"Explosion: {explosionDamage} damage to {enemy.name}");
            }
            
            // Create visual explosion effect
            StartCoroutine(CreateExplosionVisual(explosionCenter, trait.explosionRadius));
        }
        
        private System.Collections.IEnumerator CreateExplosionVisual(Vector3 center, float radius)
        {
            // Create expanding circle effect
            GameObject explosionObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            explosionObj.name = "ExplosionEffect";
            explosionObj.transform.position = center;
            explosionObj.transform.localScale = Vector3.zero;
            
            // Remove colliders
            Collider col = explosionObj.GetComponent<Collider>();
            if (col != null) DestroyImmediate(col);
            
            Collider2D col2D = explosionObj.GetComponent<Collider2D>();
            if (col2D != null) DestroyImmediate(col2D);
            
            // Set up material
            Renderer renderer = explosionObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Sprites/Default"));
                renderer.material.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
            }
            
            // Animate explosion
            float duration = 0.5f;
            float elapsed = 0f;
            float maxScale = radius * 2f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Expand quickly, then fade
                float scale = Mathf.Lerp(0f, maxScale, Mathf.Sqrt(progress));
                explosionObj.transform.localScale = Vector3.one * scale;
                
                // Fade out
                if (renderer != null)
                {
                    Color color = new Color(1f, 0.5f - progress * 0.3f, 0f, 1f - progress);
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            // Clean up
            if (explosionObj != null)
                DestroyImmediate(explosionObj);
        }
        
        private void CreateEarthHole(TowerTrait trait, Vector3 position)
        {
            Debug.Log($"<color=cyan>>>> CreateEarthHole called at position {position}</color>");
            GameObject holeObj;
            
            // Try to use prefab if assigned
            if (trait.trapPrefab != null)
            {
                holeObj = Instantiate(trait.trapPrefab, position, Quaternion.identity);
                Debug.Log($"<color=green>✓ Created Earth Hole from prefab at {position}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>No prefab assigned, creating basic hole visual...</color>");
                // Create basic hole visual
                holeObj = CreateBasicHoleVisual(position);
                Debug.Log($"<color=green>✓ Created basic black disk at {position}</color>");
            }
            
            if (holeObj == null)
            {
                Debug.LogError("<color=red>✗ Failed to create hole object!</color>");
                return;
            }
            
            Debug.Log($"<color=cyan>Hole GameObject name: {holeObj.name}, Active: {holeObj.activeSelf}</color>");
            
            // Add or get EarthTrap component
            EarthTrap holeComponent = holeObj.GetComponent<EarthTrap>();
            if (holeComponent == null)
            {
                Debug.Log("<color=yellow>Adding EarthTrap component...</color>");
                holeComponent = holeObj.AddComponent<EarthTrap>();
            }
            
            if (holeComponent == null)
            {
                Debug.LogError("<color=red>✗ Failed to add EarthTrap component!</color>");
                return;
            }
            
            Debug.Log($"<color=cyan>Initializing hole with duration={trait.trapDuration}s, radius={trait.trapRadius}</color>");
            
            // Initialize hole
            holeComponent.Initialize(trait.trapDuration, trait.trapRadius);
            
            Debug.Log($"<color=green>✓✓✓ Earth Hole fully created and initialized!</color>");
        }
        
        private GameObject CreateBasicHoleVisual(Vector3 position)
        {
            Debug.Log($"<color=cyan>>>> CreateBasicHoleVisual at {position}</color>");
            
            GameObject holeObj = new GameObject("EarthTrap_BrownDisk");
            holeObj.transform.position = position;
            
            Debug.Log($"Created GameObject '{holeObj.name}' at {holeObj.transform.position}");
            
            // Add sprite renderer for the brown semi-transparent disk
            SpriteRenderer diskRenderer = holeObj.AddComponent<SpriteRenderer>();
            
            Debug.Log("Creating 128x128 brown semi-transparent disk texture...");

            // Create a proper brown semi-transparent disk texture with smooth edges
            int size = 128; // Higher resolution for smoother appearance
            float diskTransparency = 0.8f; // 80% opacity

            Texture2D diskTexture = new Texture2D(size, size);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;
            
            // Brown semi-transparent disk with smooth anti-aliased edges
            Color brown = new Color(0.55f, 0.35f, 0.2f, diskTransparency); // Brown with 80% opacity
            Color transparent = new Color(0.55f, 0.35f, 0.2f, 0f);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius - 2f)
                    {
                        // Solid brown semi-transparent disk core
                        pixels[y * size + x] = brown;
                    }
                    else if (distance <= radius)
                    {
                        // Smooth anti-aliased edge (2 pixel fade)
                        float edgeFade = (radius - distance) / 2f;
                        pixels[y * size + x] = Color.Lerp(transparent, brown, edgeFade);
                    }
                    else
                    {
                        // Outside the disk - transparent
                        pixels[y * size + x] = transparent;
                    }
                }
            }
            
            diskTexture.SetPixels(pixels);
            diskTexture.Apply();
            
            Debug.Log("Texture created and applied");
            
            // Create sprite from texture
            Sprite diskSprite = Sprite.Create(
                diskTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), // Center pivot
                100f // Pixels per unit
            );
            
            diskRenderer.sprite = diskSprite;
            diskRenderer.sortingLayerName = "Default"; // Use default sorting layer
            diskRenderer.sortingOrder = 5; // Above ground (0-4) but below enemies (10+)
            diskRenderer.color = new Color(0.55f, 0.35f, 0.2f, diskTransparency); // Brown semi-transparent
            
            Debug.Log($"Sprite assigned to renderer. SortingLayer={diskRenderer.sortingLayerName}, SortingOrder={diskRenderer.sortingOrder}, Color={diskRenderer.color}");
            
            // Optional: Add a subtle glow/shadow effect for depth
            GameObject shadowObj = new GameObject("Shadow");
            shadowObj.transform.SetParent(holeObj.transform);
            shadowObj.transform.localPosition = new Vector3(0, -0.1f, 0);
            shadowObj.transform.localScale = Vector3.one * 1.1f; // Slightly larger
            
            SpriteRenderer shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = diskSprite;
            shadowRenderer.sortingLayerName = "Default";
            shadowRenderer.sortingOrder = 4; // Just below the main disk
            shadowRenderer.color = new Color(0.3f, 0.2f, 0.1f, 0.4f); // Darker brown shadow
            
            Debug.Log($"<color=green>✓ Brown semi-transparent disk visual created with shadow layer</color>");
            Debug.Log($"<color=cyan>Main disk: SortingOrder=5 (brown semi-transparent), Shadow: SortingOrder=4</color>");
            
            return holeObj;
        }
        
        #endregion
        
        #region Trait Projectile Systems
        
        /// <summary>
        /// Setup an independent projectile system for a trait
        /// </summary>
        private void SetupTraitProjectileSystem(TowerTrait trait)
        {
            if (tower == null)
            {
                Debug.LogError($"Cannot setup projectile system for trait '{trait.traitName}' - no tower reference");
                return;
            }
            
            // Create projectile system GameObject
            GameObject systemObj = new GameObject($"TraitProjectileSystem_{trait.traitName}");
            systemObj.transform.SetParent(transform);
            systemObj.transform.localPosition = Vector3.zero;
            
            // Add and initialize component
            TraitProjectileSystem system = systemObj.AddComponent<TraitProjectileSystem>();
            Transform firePoint = tower.transform; // Could be customized per trait
            system.Initialize(trait, tower, firePoint);
            
            projectileSystems.Add(system);
            
            Debug.Log($"<color=green>✓ Setup projectile system for trait '{trait.traitName}'</color>");
        }
        
        /// <summary>
        /// Remove projectile system for a specific trait
        /// </summary>
        private void RemoveTraitProjectileSystem(TowerTrait trait)
        {
            TraitProjectileSystem systemToRemove = projectileSystems.FirstOrDefault(s => s.GetTrait() == trait);
            
            if (systemToRemove != null)
            {
                projectileSystems.Remove(systemToRemove);
                Destroy(systemToRemove.gameObject);
                Debug.Log($"<color=orange>Removed projectile system for trait '{trait.traitName}'</color>");
            }
        }
        
        /// <summary>
        /// Clear all projectile systems
        /// </summary>
        private void ClearAllProjectileSystems()
        {
            foreach (var system in projectileSystems)
            {
                if (system != null)
                    Destroy(system.gameObject);
            }
            projectileSystems.Clear();
            Debug.Log("Cleared all trait projectile systems");
        }
        
        #endregion
    }
}