using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerFusion.UI
{
    /// <summary>
    /// UI button for building towers
    /// </summary>
    public class TowerButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image towerIcon;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI costText;
        
        private TowerData towerData;
        
        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
            
            if (button != null)
            {
                button.onClick.AddListener(BuildTower);
            }
        }
        
        /// <summary>
        /// Initialize button with tower data
        /// </summary>
        public void Initialize(TowerData data)
        {
            towerData = data;
            UpdateDisplay();
        }
        
        /// <summary>
        /// Update button display
        /// </summary>
        private void UpdateDisplay()
        {
            if (towerData == null)
                return;
            
            // Update icon
            if (towerIcon != null && towerData.towerSprite != null)
            {
                towerIcon.sprite = towerData.towerSprite;
            }
            
            // Update name
            if (towerNameText != null)
            {
                towerNameText.text = towerData.towerName;
            }
            
            // Update cost
            if (costText != null)
            {
                costText.text = $"{towerData.buildCost}g";
            }
        }
        
        private void Update()
        {
            UpdateButtonState();
        }
        
        /// <summary>
        /// Update button interactable state based on gold
        /// </summary>
        private void UpdateButtonState()
        {
            if (button == null || towerData == null)
                return;
            
            bool canAfford = GameManager.Instance != null && 
                           GameManager.Instance.CurrentGold >= towerData.buildCost;
            
            bool wasInteractable = button.interactable;
            button.interactable = canAfford;
            
            // Debug when button state changes
            if (wasInteractable != canAfford)
            {
                Debug.Log($"TowerButton ({towerData.towerName}): Button interactable changed to {canAfford}. " +
                         $"Gold: {(GameManager.Instance != null ? GameManager.Instance.CurrentGold : 0)}, " +
                         $"Cost: {towerData.buildCost}");
            }
        }
        
        /// <summary>
        /// Build tower button callback
        /// </summary>
        private void BuildTower()
        {
            if (towerData == null)
            {
                Debug.LogWarning("TowerButton: towerData is null");
                return;
            }
            
            Debug.Log($"TowerButton: BuildTower clicked for {towerData.towerName}");
            TowerManager.Instance?.StartPlacingTower(towerData);
        }
    }
}