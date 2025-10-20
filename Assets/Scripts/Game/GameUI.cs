using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerFusion.UI
{
    /// <summary>
    /// Main game UI controller
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Game Stats UI")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        
        [Header("Wave Control")]
        [SerializeField] private Button startWaveButton;
        [SerializeField] private Button traitButton;
        [SerializeField] private TextMeshProUGUI waveProgressText;
        
        [Header("Tower Building")]
        [SerializeField] private Transform towerButtonContainer;
        [SerializeField] private GameObject towerButtonPrefab;
        [SerializeField] private TowerData[] availableTowers;
        
        [Header("Tower Info Panel")]
        [SerializeField] private GameObject towerInfoPanel;
        [SerializeField] private TextMeshProUGUI towerNameText;
        [SerializeField] private TextMeshProUGUI towerStatsText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button assignTraitButton;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        
        [Header("Trait Selection")]
        [SerializeField] private GameObject traitCardDialog;
        [Tooltip("Optional: Container for 3 trait cards. If not assigned, will auto-find or create within traitCardDialog")]
        [SerializeField] private Transform traitCardsContainer; // Container for 3 trait cards
        [Tooltip("Optional: Prefab for individual trait card. If not assigned, uses legacy single-card UI")]
        [SerializeField] private GameObject traitCardPrefab; // Prefab for individual trait card
        [Tooltip("Legacy single-card UI (used as fallback if traitCardPrefab not assigned)")]
        [SerializeField] private TextMeshProUGUI traitNameText;
        [SerializeField] private TextMeshProUGUI traitDescriptionText;
        [SerializeField] private Image traitIconImage;
        [SerializeField] private Button traitDoneButton;
        
        [Header("Available Traits")]
        [SerializeField] private TowerTrait[] availableTraits = new TowerTrait[7];
        [SerializeField] private float[] traitProbabilities = {0.15f, 0.15f, 0.15f, 0.15f, 0.15f, 0.125f, 0.125f};
        [SerializeField] private bool autoLoadTraitsFromResources = true;
        
        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button restartButton;
        
        // Trait system state
        private bool traitButtonUsedThisWave = false;
        private TowerTrait selectedTrait = null;
        private TowerTrait availableTraitForAssignment = null;
        private TowerTrait[] currentTraitOptions = new TowerTrait[3]; // 3 traits to choose from
        private GameObject[] traitCardInstances = new GameObject[3]; // UI instances
        
        private void Start()
        {
            // Auto-load traits if enabled and array is empty
            if (autoLoadTraitsFromResources && (availableTraits == null || availableTraits.Length == 0 || availableTraits[0] == null))
            {
                LoadTraitsFromResources();
            }
            
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// Load all traits from Resources folder
        /// </summary>
        private void LoadTraitsFromResources()
        {
            // Try loading from Resources/Traits folder first
            TowerTrait[] loadedTraits = Resources.LoadAll<TowerTrait>("Traits");
            
            // If not found, try Data/Traits
            if (loadedTraits == null || loadedTraits.Length == 0)
            {
                loadedTraits = Resources.LoadAll<TowerTrait>("Data/Traits");
            }
            
            if (loadedTraits != null && loadedTraits.Length > 0)
            {
                availableTraits = loadedTraits;
                Debug.Log($"Auto-loaded {loadedTraits.Length} traits from Resources: {string.Join(", ", System.Array.ConvertAll(loadedTraits, t => t.traitName))}");
                
                // Update probabilities array to match loaded traits count
                if (traitProbabilities.Length != loadedTraits.Length)
                {
                    float equalProbability = 1f / loadedTraits.Length;
                    traitProbabilities = new float[loadedTraits.Length];
                    for (int i = 0; i < traitProbabilities.Length; i++)
                    {
                        traitProbabilities[i] = equalProbability;
                    }
                    Debug.Log($"Updated trait probabilities to equal distribution: {equalProbability:P0} each");
                }
            }
            else
            {
                Debug.LogWarning("No traits found in Resources/Traits or Resources/Data/Traits! Please ensure traits are created and placed in a Resources folder.");
                Debug.LogWarning("Run: Tools > Tower Fusion > Create Default Traits, then move the traits to Assets/Resources/Traits/");
            }
        }
        
        /// <summary>
        /// Initialize UI elements
        /// </summary>
        private void InitializeUI()
        {
            // Setup buttons
            if (startWaveButton != null)
            {
                startWaveButton.onClick.AddListener(StartWave);
            }
            
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(UpgradeTower);
            }
            
            if (sellButton != null)
            {
                sellButton.onClick.AddListener(SellTower);
            }
            
            if (assignTraitButton != null)
            {
                assignTraitButton.onClick.AddListener(AssignTraitToSelectedTower);
            }
            
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(RestartGame);
            }
            
            if (traitButton != null)
            {
                traitButton.onClick.AddListener(ShowTraitCard);
            }
            
            if (traitDoneButton != null)
            {
                traitDoneButton.onClick.AddListener(AcceptTrait);
            }
            
            // Create tower building buttons
            CreateTowerButtons();
            
            // Initialize panels
            if (towerInfoPanel != null)
                towerInfoPanel.SetActive(false);
            
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
            
            if (traitCardDialog != null)
                traitCardDialog.SetActive(false);
        }
        
        /// <summary>
        /// Subscribe to game events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHealthChanged += UpdateHealthDisplay;
                GameManager.Instance.OnGoldChanged += UpdateGoldDisplay;
                GameManager.Instance.OnWaveChanged += UpdateWaveDisplay;
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
                GameManager.Instance.OnGameOver += OnGameOver;
                GameManager.Instance.OnVictory += OnVictory;
                
                // Immediately update displays with current values
                UpdateHealthDisplay(GameManager.Instance.CurrentHealth);
                UpdateGoldDisplay(GameManager.Instance.CurrentGold);
                UpdateWaveDisplay(GameManager.Instance.CurrentWave);
            }
            
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerSelected += OnTowerSelected;
                TowerManager.Instance.OnTowerUpgraded += OnTowerUpgraded;
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemySpawned += UpdateWaveProgress;
            }
        }
        
        /// <summary>
        /// Unsubscribe from game events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHealthChanged -= UpdateHealthDisplay;
                GameManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
                GameManager.Instance.OnWaveChanged -= UpdateWaveDisplay;
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
                GameManager.Instance.OnGameOver -= OnGameOver;
                GameManager.Instance.OnVictory -= OnVictory;
            }
            
            if (TowerManager.Instance != null)
            {
                TowerManager.Instance.OnTowerSelected -= OnTowerSelected;
                TowerManager.Instance.OnTowerUpgraded -= OnTowerUpgraded;
            }
            
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.OnEnemySpawned -= UpdateWaveProgress;
            }
        }
        
        /// <summary>
        /// Create tower building buttons
        /// </summary>
        private void CreateTowerButtons()
        {
            if (towerButtonContainer == null || towerButtonPrefab == null)
                return;
            
            foreach (TowerData towerData in availableTowers)
            {
                if (towerData == null)
                    continue;
                
                GameObject buttonObj = Instantiate(towerButtonPrefab, towerButtonContainer);
                TowerButton towerButton = buttonObj.GetComponent<TowerButton>();
                
                if (towerButton != null)
                {
                    towerButton.Initialize(towerData);
                }
            }
        }
        
        /// <summary>
        /// Update health display
        /// </summary>
        private void UpdateHealthDisplay(int health)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {health}";
            }
        }
        
        /// <summary>
        /// Update gold display
        /// </summary>
        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }
        
        /// <summary>
        /// Update wave display
        /// </summary>
        private void UpdateWaveDisplay(int wave)
        {
            if (waveText != null)
            {
                int totalWaves = MapManager.Instance?.GetTotalWaves() ?? 0;
                waveText.text = $"Wave: {wave}/{totalWaves}";
            }
        }
        
        /// <summary>
        /// Update wave progress display
        /// </summary>
        private void UpdateWaveProgress(int spawned, int total)
        {
            if (waveProgressText != null)
            {
                waveProgressText.text = $"Enemies: {spawned}/{total}";
            }
        }
        
        /// <summary>
        /// Handle game state changes
        /// </summary>
        private void OnGameStateChanged(GameState gameState)
        {
            if (startWaveButton != null)
            {
                startWaveButton.interactable = (gameState == GameState.Preparing);
                
                switch (gameState)
                {
                    case GameState.Preparing:
                        startWaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "start wave";
                        // Reset trait button for new wave
                        traitButtonUsedThisWave = false;
                        // Clear any unused trait from previous wave
                        availableTraitForAssignment = null;
                        UpdateTraitAssignmentUI();
                        break;
                    case GameState.WaveInProgress:
                        startWaveButton.GetComponentInChildren<TextMeshProUGUI>().text = "waving";
                        break;
                }
            }
            
            // Update trait button state
            if (traitButton != null)
            {
                traitButton.interactable = (gameState == GameState.Preparing && !traitButtonUsedThisWave);
            }
        }
        
        /// <summary>
        /// Handle tower selection
        /// </summary>
        private void OnTowerSelected(Tower tower)
        {
            Debug.Log($"OnTowerSelected called with tower: {(tower != null ? tower.name : "null")}");
            
            if (towerInfoPanel == null)
                return;
            
            if (tower == null)
            {
                Debug.Log("Hiding tower info panel (tower is null)");
                towerInfoPanel.SetActive(false);
                UpdateTraitAssignmentUI(); // Hide trait assignment button when no tower selected
                return;
            }
            
            Debug.Log("Showing tower info panel");
            towerInfoPanel.SetActive(true);
            
            // Update tower info
            if (towerNameText != null)
            {
                towerNameText.text = tower.TowerData.towerName;
            }
            
            if (towerStatsText != null)
            {
                // Format damage to show whole numbers when possible, otherwise 1 decimal place
                string damageText = tower.ModifiedDamage % 1 == 0 ? $"{tower.ModifiedDamage:F0}" : $"{tower.ModifiedDamage:F1}";
                string rangeText = tower.ModifiedRange % 1 == 0 ? $"{tower.ModifiedRange:F0}" : $"{tower.ModifiedRange:F1}";
                
                towerStatsText.text = $"Damage: {damageText}\nRange: {rangeText}\nLevel: {tower.UpgradeLevel + 1}";
            }
            
            // Update upgrade button
            if (upgradeButton != null && upgradeButtonText != null)
            {
                TowerUpgrade nextUpgrade = tower.GetNextUpgrade();
                if (nextUpgrade != null)
                {
                    upgradeButton.interactable = true;
                    upgradeButtonText.text = $"Upgrade ({nextUpgrade.cost}g)";
                }
                else
                {
                    upgradeButton.interactable = false;
                    upgradeButtonText.text = "Max Level";
                }
            }
            
            // Update trait assignment UI
            UpdateTraitAssignmentUI();
        }
        
        /// <summary>
        /// Handle tower upgrade
        /// </summary>
        private void OnTowerUpgraded(Tower tower)
        {
            // Refresh tower info if this tower is selected
            if (TowerManager.Instance?.GetSelectedTower() == tower)
            {
                OnTowerSelected(tower);
            }
        }
        
        /// <summary>
        /// Handle game over
        /// </summary>
        private void OnGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Handle victory
        /// </summary>
        private void OnVictory()
        {
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// Start wave button callback
        /// </summary>
        private void StartWave()
        {
            GameManager.Instance?.StartWave();
        }
        
        /// <summary>
        /// Upgrade tower button callback
        /// </summary>
        private void UpgradeTower()
        {
            TowerManager.Instance?.UpgradeSelectedTower();
        }
        
        /// <summary>
        /// Sell tower button callback
        /// </summary>
        private void SellTower()
        {
            TowerManager.Instance?.SellSelectedTower();
        }
        
        /// <summary>
        /// Restart game button callback
        /// </summary>
        private void RestartGame()
        {
            GameManager.Instance?.RestartGame();
            
            // Hide game over panels
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
        }
        
        /// <summary>
        /// Show trait card dialog with randomly selected trait
        /// </summary>
        private void ShowTraitCard()
        {
            if (traitButtonUsedThisWave)
            {
                Debug.Log("Trait button already used this wave");
                return;
            }
            
            if (availableTraits == null || availableTraits.Length == 0)
            {
                Debug.LogError("No traits available! Make sure traits are loaded. Check availableTraits array in GameUI inspector or enable autoLoadTraitsFromResources.");
                return;
            }
            
            // Generate 3 unique random traits
            currentTraitOptions = Generate3UniqueTraits();
            
            if (currentTraitOptions == null || currentTraitOptions[0] == null)
            {
                Debug.LogWarning("No traits could be generated! Check that availableTraits contains valid trait assets.");
                return;
            }
            
            Debug.Log($"Generated 3 trait options: {string.Join(", ", System.Array.ConvertAll(currentTraitOptions, t => t?.traitName ?? "null"))}");
            
            // Use new 3-card system if prefab is assigned, otherwise fall back to legacy single-card UI
            if (traitCardPrefab != null)
            {
                Display3TraitCards();
            }
            else
            {
                DisplayLegacySingleTraitCard();
            }
            
            // Show dialog
            if (traitCardDialog != null)
            {
                traitCardDialog.SetActive(true);
                Debug.Log("Showing trait selection dialog with 3 options");
            }
            else
            {
                Debug.LogError("traitCardDialog is null! Assign the trait dialog GameObject in GameUI inspector.");
            }
            
            // Disable trait button for this wave
            traitButtonUsedThisWave = true;
            if (traitButton != null)
                traitButton.interactable = false;
        }
        
        /// <summary>
        /// Display 3 trait cards in the new UI system
        /// </summary>
        private void Display3TraitCards()
        {
            // Ensure container exists
            EnsureTraitCardsContainer();
            
            if (traitCardsContainer == null)
            {
                Debug.LogWarning("Could not find or create trait cards container. Falling back to legacy UI.");
                DisplayLegacySingleTraitCard();
                return;
            }
            
            // Hide legacy single-card UI elements
            HideLegacyUIElements();
            
            // Clear existing cards
            ClearTraitCards();
            
            // Create 3 trait cards
            for (int i = 0; i < 3; i++)
            {
                if (currentTraitOptions[i] == null)
                    continue;
                
                GameObject cardObj = Instantiate(traitCardPrefab, traitCardsContainer);
                traitCardInstances[i] = cardObj;
                
                // Setup card UI (assuming prefab has these components)
                var nameText = cardObj.transform.Find("TraitName")?.GetComponent<TextMeshProUGUI>();
                var descText = cardObj.transform.Find("TraitDescription")?.GetComponent<TextMeshProUGUI>();
                var iconImage = cardObj.transform.Find("TraitIcon")?.GetComponent<Image>();
                var selectButton = cardObj.GetComponent<Button>();
                
                if (nameText != null)
                    nameText.text = currentTraitOptions[i].traitName;
                
                if (descText != null)
                    descText.text = currentTraitOptions[i].description;
                
                if (iconImage != null && currentTraitOptions[i].traitIcon != null)
                    iconImage.sprite = currentTraitOptions[i].traitIcon;
                
                // Add button listener to select this trait
                if (selectButton != null)
                {
                    int index = i; // Capture for closure
                    selectButton.onClick.AddListener(() => SelectTrait(index));
                }
            }
        }
        
        /// <summary>
        /// Display single trait card (legacy UI fallback)
        /// </summary>
        private void DisplayLegacySingleTraitCard()
        {
            // Use first trait from options (fallback to old behavior)
            selectedTrait = currentTraitOptions[0];
            
            // Show legacy UI elements
            ShowLegacyUIElements();
            
            if (traitNameText != null)
                traitNameText.text = selectedTrait.traitName;
            
            if (traitDescriptionText != null)
                traitDescriptionText.text = selectedTrait.description;
            
            if (traitIconImage != null && selectedTrait.traitIcon != null)
                traitIconImage.sprite = selectedTrait.traitIcon;
                
            Debug.LogWarning("Using legacy single-card UI. For 3-card selection, assign traitCardsContainer and traitCardPrefab in inspector.");
        }
        
        /// <summary>
        /// Hide legacy single-card UI elements (used when showing 3-card system)
        /// </summary>
        private void HideLegacyUIElements()
        {
            if (traitNameText != null)
                traitNameText.gameObject.SetActive(false);
            
            if (traitDescriptionText != null)
                traitDescriptionText.gameObject.SetActive(false);
            
            if (traitIconImage != null)
                traitIconImage.gameObject.SetActive(false);
            
            if (traitDoneButton != null)
                traitDoneButton.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// Show legacy single-card UI elements (used when falling back to legacy UI)
        /// </summary>
        private void ShowLegacyUIElements()
        {
            if (traitNameText != null)
                traitNameText.gameObject.SetActive(true);
            
            if (traitDescriptionText != null)
                traitDescriptionText.gameObject.SetActive(true);
            
            if (traitIconImage != null)
                traitIconImage.gameObject.SetActive(true);
            
            if (traitDoneButton != null)
                traitDoneButton.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Clear all trait card instances
        /// </summary>
        private void ClearTraitCards()
        {
            for (int i = 0; i < traitCardInstances.Length; i++)
            {
                if (traitCardInstances[i] != null)
                {
                    Destroy(traitCardInstances[i]);
                    traitCardInstances[i] = null;
                }
            }
        }
        
        /// <summary>
        /// Ensure trait cards container exists within the trait dialog
        /// Auto-finds or creates it if needed
        /// </summary>
        private void EnsureTraitCardsContainer()
        {
            // If already assigned, use it
            if (traitCardsContainer != null)
                return;
            
            // If no dialog, can't create container
            if (traitCardDialog == null)
            {
                Debug.LogError("traitCardDialog is null! Cannot find or create trait cards container.");
                return;
            }
            
            // Try to find existing container by name
            Transform existingContainer = traitCardDialog.transform.Find("TraitCardsContainer");
            if (existingContainer != null)
            {
                traitCardsContainer = existingContainer;
                Debug.Log("Found existing TraitCardsContainer in traitCardDialog");
                return;
            }
            
            // Create new container within the dialog
            GameObject containerObj = new GameObject("TraitCardsContainer");
            containerObj.transform.SetParent(traitCardDialog.transform, false);
            
            // Setup RectTransform
            RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Add HorizontalLayoutGroup for auto-layout
            var layoutGroup = containerObj.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layoutGroup.spacing = 20;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.padding = new RectOffset(20, 20, 20, 20);
            
            traitCardsContainer = containerObj.transform;
            Debug.Log("Created new TraitCardsContainer in traitCardDialog with HorizontalLayoutGroup");
        }
        
        /// <summary>
        /// User selects one of the 3 traits
        /// </summary>
        private void SelectTrait(int index)
        {
            if (index < 0 || index >= currentTraitOptions.Length || currentTraitOptions[index] == null)
            {
                Debug.LogError($"Invalid trait selection index: {index}");
                return;
            }
            
            selectedTrait = currentTraitOptions[index];
            Debug.Log($"User selected trait: {selectedTrait.traitName}");
            
            // Accept the selected trait
            AcceptTrait();
        }
        
        /// <summary>
        /// Accept the selected trait and close dialog
        /// </summary>
        private void AcceptTrait()
        {
            if (selectedTrait == null)
                return;
            
            // Store trait as available for assignment
            availableTraitForAssignment = selectedTrait;
            Debug.Log($"Trait '{selectedTrait.traitName}' is now available for assignment!");
            
            // Update UI to show trait is available
            UpdateTraitAssignmentUI();
            
            // Clean up trait card instances
            ClearTraitCards();
            
            // Hide dialog
            if (traitCardDialog != null)
                traitCardDialog.SetActive(false);
            
            selectedTrait = null;
            currentTraitOptions = new TowerTrait[3];
        }
        
        /// <summary>
        /// Generate random trait based on configured probabilities
        /// </summary>
        private TowerTrait GenerateRandomTrait()
        {
            if (availableTraits == null || availableTraits.Length == 0)
                return null;
            
            if (traitProbabilities == null || traitProbabilities.Length != availableTraits.Length)
            {
                // Fallback to equal probability
                int randomIndex = Random.Range(0, availableTraits.Length);
                return availableTraits[randomIndex];
            }
            
            // Calculate cumulative probabilities
            float totalProbability = 0f;
            for (int i = 0; i < traitProbabilities.Length; i++)
            {
                totalProbability += traitProbabilities[i];
            }
            
            // Generate random value
            float randomValue = Random.Range(0f, totalProbability);
            
            // Select trait based on probability
            float cumulativeProbability = 0f;
            for (int i = 0; i < availableTraits.Length; i++)
            {
                cumulativeProbability += traitProbabilities[i];
                if (randomValue <= cumulativeProbability && availableTraits[i] != null)
                {
                    return availableTraits[i];
                }
            }
            
            // Fallback to first available trait
            return availableTraits[0];
        }
        
        /// <summary>
        /// Generate 3 unique random traits
        /// </summary>
        private TowerTrait[] Generate3UniqueTraits()
        {
            if (availableTraits == null || availableTraits.Length == 0)
                return null;
            
            TowerTrait[] selectedTraits = new TowerTrait[3];
            System.Collections.Generic.List<TowerTrait> availablePool = new System.Collections.Generic.List<TowerTrait>();
            
            // Create pool of all available traits (excluding nulls)
            foreach (var trait in availableTraits)
            {
                if (trait != null)
                    availablePool.Add(trait);
            }
            
            if (availablePool.Count == 0)
            {
                Debug.LogError("No valid traits in availableTraits!");
                return null;
            }
            
            // If we have fewer than 3 traits total, allow duplicates
            bool allowDuplicates = availablePool.Count < 3;
            
            // Generate 3 traits
            for (int i = 0; i < 3; i++)
            {
                TowerTrait trait = GenerateRandomTrait();
                
                // If not allowing duplicates, regenerate until we get a unique one
                if (!allowDuplicates)
                {
                    int attempts = 0;
                    while (System.Array.IndexOf(selectedTraits, trait) >= 0 && attempts < 50)
                    {
                        trait = GenerateRandomTrait();
                        attempts++;
                    }
                }
                
                selectedTraits[i] = trait;
            }
            
            return selectedTraits;
        }
        
        /// <summary>
        /// Update UI to reflect trait assignment state
        /// </summary>
        private void UpdateTraitAssignmentUI()
        {
            if (assignTraitButton != null)
            {
                bool hasTraitToAssign = availableTraitForAssignment != null;
                Tower selectedTower = TowerManager.Instance?.GetSelectedTower();
                
                // Debug logging
                Debug.Log($"UpdateTraitAssignmentUI - HasTrait: {hasTraitToAssign}, SelectedTower: {(selectedTower != null ? selectedTower.name : "null")}");
                
                bool shouldShowButton = hasTraitToAssign && selectedTower != null;
                assignTraitButton.gameObject.SetActive(shouldShowButton);
                
                if (hasTraitToAssign && selectedTower != null)
                {
                    var buttonText = assignTraitButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (buttonText != null)
                    {
                        buttonText.text = $"Apply {availableTraitForAssignment.traitName}";
                    }
                }
                
                Debug.Log($"Assign trait button active: {shouldShowButton}");
            }
            else
            {
                Debug.LogWarning("assignTraitButton is null! Make sure to assign it in GameUI inspector.");
            }
            
            // Update trait button visual feedback
            if (traitButton != null)
            {
                var buttonImage = traitButton.GetComponent<Image>();
                if (buttonImage != null && availableTraitForAssignment != null)
                {
                    // Make the trait button glow when a trait is available for assignment
                    buttonImage.color = Color.yellow;
                }
                else if (buttonImage != null)
                {
                    // Normal color when no trait available
                    buttonImage.color = Color.white;
                }
            }
        }
        
        /// <summary>
        /// Assign the available trait to the currently selected tower
        /// </summary>
        private void AssignTraitToSelectedTower()
        {
            if (availableTraitForAssignment == null)
            {
                Debug.LogWarning("No trait available for assignment!");
                return;
            }
            
            // Debug tower manager state
            if (TowerManager.Instance == null)
            {
                Debug.LogError("TowerManager.Instance is null! Make sure TowerManager exists in the scene.");
                return;
            }
            
            Tower selectedTower = TowerManager.Instance.GetSelectedTower();
            Debug.Log($"Debug: TowerManager.GetSelectedTower() returned: {(selectedTower != null ? selectedTower.name : "null")}");
            
            if (selectedTower == null)
            {
                Debug.LogWarning("No tower selected for trait assignment!");
                Debug.LogWarning("Make sure to click on a tower to select it before trying to apply traits.");
                
                // Try to find towers in scene as additional debug info
                var towers = FindObjectsOfType<Tower>();
                Debug.Log($"Debug: Found {towers.Length} towers in scene: {string.Join(", ", System.Array.ConvertAll(towers, t => t.name))}");
                return;
            }
            
            // Try to apply the trait to the selected tower
            if (selectedTower.AddTrait(availableTraitForAssignment))
            {
                Debug.Log($"Successfully applied trait '{availableTraitForAssignment.traitName}' to tower '{selectedTower.TowerData.towerName}'");
                
                // Clear the available trait as it's been used
                availableTraitForAssignment = null;
                
                // Hide the trait dialog
                Debug.Log("Attempting to hide trait dialog...");
                if (traitCardDialog != null)
                {
                    Debug.Log($"Hiding dialog GameObject: '{traitCardDialog.name}' - Currently active: {traitCardDialog.activeInHierarchy}");
                    traitCardDialog.SetActive(false);
                    Debug.Log($"Dialog hidden. New active state: {traitCardDialog.activeInHierarchy}");
                }
                else
                {
                    Debug.LogWarning("traitCardDialog is null! Cannot hide dialog.");
                }
                
                // Update UI
                UpdateTraitAssignmentUI();
                
                // Deselect tower to close the tower info panel
                Debug.Log("Deselecting tower to close tower info panel...");
                TowerManager.Instance.DeselectTower();
                Debug.Log("Tower deselected.");
            }
            else
            {
                Debug.LogWarning($"Failed to apply trait '{availableTraitForAssignment.traitName}' to tower. Tower may already have this trait or reached max traits.");
            }
        }
    }
}