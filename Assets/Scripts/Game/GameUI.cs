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
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        
        [Header("Trait Selection")]
        [SerializeField] private GameObject traitCardDialog;
        [SerializeField] private TextMeshProUGUI traitNameText;
        [SerializeField] private TextMeshProUGUI traitDescriptionText;
        [SerializeField] private Image traitIconImage;
        [SerializeField] private Button traitDoneButton;
        
        [Header("Available Traits")]
        [SerializeField] private TowerTrait[] availableTraits = new TowerTrait[5];
        [SerializeField] private float[] traitProbabilities = {0.2f, 0.2f, 0.2f, 0.2f, 0.2f};
        
        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private Button restartButton;
        
        // Trait system state
        private bool traitButtonUsedThisWave = false;
        private TowerTrait selectedTrait = null;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
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
            if (towerInfoPanel == null)
                return;
            
            if (tower == null)
            {
                towerInfoPanel.SetActive(false);
                return;
            }
            
            towerInfoPanel.SetActive(true);
            
            // Update tower info
            if (towerNameText != null)
            {
                towerNameText.text = tower.TowerData.towerName;
            }
            
            if (towerStatsText != null)
            {
                towerStatsText.text = $"Damage: {tower.ModifiedDamage:F1}\\nRange: {tower.ModifiedRange:F1}\\nLevel: {tower.UpgradeLevel + 1}";
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
            if (traitButtonUsedThisWave || availableTraits == null || availableTraits.Length == 0)
                return;
            
            // Generate random trait based on probabilities
            selectedTrait = GenerateRandomTrait();
            
            if (selectedTrait == null)
            {
                Debug.LogWarning("No trait could be generated!");
                return;
            }
            
            // Update UI with trait info
            if (traitNameText != null)
                traitNameText.text = selectedTrait.traitName;
            
            if (traitDescriptionText != null)
                traitDescriptionText.text = selectedTrait.description;
            
            if (traitIconImage != null && selectedTrait.traitIcon != null)
                traitIconImage.sprite = selectedTrait.traitIcon;
            
            // Show dialog
            if (traitCardDialog != null)
                traitCardDialog.SetActive(true);
            
            // Disable trait button for this wave
            traitButtonUsedThisWave = true;
            if (traitButton != null)
                traitButton.interactable = false;
        }
        
        /// <summary>
        /// Accept the selected trait and close dialog
        /// </summary>
        private void AcceptTrait()
        {
            if (selectedTrait == null)
                return;
            
            // Add trait to available traits for towers
            // This trait will be available for assignment to towers
            Debug.Log($"Trait '{selectedTrait.traitName}' is now available for assignment!");
            
            // TODO: Add trait to tower selection system
            // For now, we'll just log it
            
            // Hide dialog
            if (traitCardDialog != null)
                traitCardDialog.SetActive(false);
            
            selectedTrait = null;
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
    }
}