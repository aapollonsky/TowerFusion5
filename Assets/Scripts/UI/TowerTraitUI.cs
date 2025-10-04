using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TowerFusion.UI
{
    /// <summary>
    /// UI component for selecting and managing tower traits
    /// </summary>
    public class TowerTraitUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject traitSelectionPanel;
        [SerializeField] private Transform traitButtonsParent;
        [SerializeField] private Button traitButtonPrefab;
        [SerializeField] private TextMeshProUGUI selectedTowerName;
        [SerializeField] private Transform appliedTraitsParent;
        [SerializeField] private Button closeButton;
        
        [Header("Available Traits")]
        [SerializeField] private List<TowerTrait> availableTraits = new List<TowerTrait>();
        
        // Current state
        private Tower selectedTower;
        private List<Button> traitButtons = new List<Button>();
        private List<GameObject> appliedTraitIcons = new List<GameObject>();
        
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseTraitSelection);
                
            // Hide panel initially
            if (traitSelectionPanel != null)
                traitSelectionPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show trait selection UI for a specific tower
        /// </summary>
        public void ShowTraitSelection(Tower tower)
        {
            if (tower == null) return;
            
            selectedTower = tower;
            
            if (selectedTowerName != null)
                selectedTowerName.text = $"Select Trait for {tower.TowerData.towerName}";
            
            RefreshTraitButtons();
            RefreshAppliedTraits();
            
            if (traitSelectionPanel != null)
                traitSelectionPanel.SetActive(true);
        }
        
        /// <summary>
        /// Close trait selection UI
        /// </summary>
        public void CloseTraitSelection()
        {
            selectedTower = null;
            
            if (traitSelectionPanel != null)
                traitSelectionPanel.SetActive(false);
        }
        
        /// <summary>
        /// Set the list of available traits
        /// </summary>
        public void SetAvailableTraits(List<TowerTrait> traits)
        {
            availableTraits = traits ?? new List<TowerTrait>();
            
            if (selectedTower != null)
                RefreshTraitButtons();
        }
        
        private void RefreshTraitButtons()
        {
            // Clear existing buttons
            foreach (var button in traitButtons)
            {
                if (button != null)
                    DestroyImmediate(button.gameObject);
            }
            traitButtons.Clear();
            
            if (traitButtonPrefab == null || traitButtonsParent == null)
                return;
            
            // Create buttons for each available trait
            foreach (var trait in availableTraits)
            {
                CreateTraitButton(trait);
            }
        }
        
        private void CreateTraitButton(TowerTrait trait)
        {
            GameObject buttonObj = Instantiate(traitButtonPrefab.gameObject, traitButtonsParent);
            Button button = buttonObj.GetComponent<Button>();
            
            // Setup button text
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = trait.traitName;
            }
            
            // Setup button color based on trait category
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = GetCategoryColor(trait.category);
            }
            
            // Setup button click handler
            button.onClick.AddListener(() => OnTraitButtonClicked(trait));
            
            // Check if trait is already applied
            if (selectedTower != null && selectedTower.HasTrait(trait))
            {
                button.interactable = false;
                if (buttonText != null)
                    buttonText.text += " (Applied)";
            }
            
            traitButtons.Add(button);
        }
        
        private void OnTraitButtonClicked(TowerTrait trait)
        {
            if (selectedTower == null || trait == null) return;
            
            // Try to add the trait
            if (selectedTower.AddTrait(trait))
            {
                Debug.Log($"Added trait '{trait.traitName}' to tower");
                RefreshTraitButtons();
                RefreshAppliedTraits();
            }
            else
            {
                Debug.LogWarning($"Failed to add trait '{trait.traitName}' to tower");
            }
        }
        
        private void RefreshAppliedTraits()
        {
            // Clear existing trait icons
            foreach (var icon in appliedTraitIcons)
            {
                if (icon != null)
                    DestroyImmediate(icon);
            }
            appliedTraitIcons.Clear();
            
            if (selectedTower == null || appliedTraitsParent == null)
                return;
            
            // Create icons for applied traits
            var appliedTraits = selectedTower.GetAppliedTraits();
            foreach (var trait in appliedTraits)
            {
                CreateAppliedTraitIcon(trait);
            }
        }
        
        private void CreateAppliedTraitIcon(TowerTrait trait)
        {
            GameObject iconObj = new GameObject($"Trait_{trait.traitName}");
            iconObj.transform.SetParent(appliedTraitsParent);
            iconObj.transform.localScale = Vector3.one;
            
            // Add image component
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = GetCategoryColor(trait.category);
            
            // Add text component
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(iconObj.transform);
            textObj.transform.localScale = Vector3.one;
            textObj.transform.localPosition = Vector3.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = trait.traitName;
            text.fontSize = 12;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            // Add button for removal
            Button removeButton = iconObj.AddComponent<Button>();
            removeButton.onClick.AddListener(() => RemoveTrait(trait));
            
            // Add tooltip (simple version)
            var tooltip = iconObj.AddComponent<TooltipTrigger>();
            tooltip.tooltipText = trait.description;
            
            appliedTraitIcons.Add(iconObj);
        }
        
        private void RemoveTrait(TowerTrait trait)
        {
            if (selectedTower == null || trait == null) return;
            
            if (selectedTower.RemoveTrait(trait))
            {
                Debug.Log($"Removed trait '{trait.traitName}' from tower");
                RefreshTraitButtons();
                RefreshAppliedTraits();
            }
        }
        
        private Color GetCategoryColor(TraitCategory category)
        {
            switch (category)
            {
                case TraitCategory.Elemental:
                    return new Color(1f, 0.5f, 0.2f, 0.8f); // Orange
                case TraitCategory.Range:
                    return new Color(0.2f, 1f, 0.2f, 0.8f); // Green
                case TraitCategory.Utility:
                    return new Color(1f, 1f, 0.2f, 0.8f); // Yellow
                case TraitCategory.Support:
                    return new Color(0.2f, 0.5f, 1f, 0.8f); // Blue
                default:
                    return Color.gray;
            }
        }
    }
    
    /// <summary>
    /// Simple tooltip component for trait descriptions
    /// </summary>
    public class TooltipTrigger : MonoBehaviour, UnityEngine.EventSystems.IPointerEnterHandler, UnityEngine.EventSystems.IPointerExitHandler
    {
        public string tooltipText;
        
        public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
        {
            // Show tooltip - this would connect to a tooltip system
            Debug.Log($"Tooltip: {tooltipText}");
        }
        
        public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
        {
            // Hide tooltip
        }
    }
}