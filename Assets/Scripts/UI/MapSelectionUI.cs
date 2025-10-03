using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Data")]
    public TowerFusion.MapLibrary mapLibrary;

    [Header("UI")]
    public TMP_Dropdown mapDropdown;
    public Button launchButton;
    public Button quitButton;

    [Header("Scene")]
    public string sceneToLoad = "MainScene"; // change to your gameplay scene name if different

    [Header("Debug")]
    public bool debugOpenDropdownOnStart = false;
    [Tooltip("If true, create a simple runtime fallback dropdown UI when the TMP_Dropdown template doesn't render correctly.")]
    public bool createRuntimeFallback = true;

    [Tooltip("If true, attempt to auto-fix TMP_Dropdown.template positioning so the list isn't hidden under parent masks.")]
    public bool autoFixTemplate = true;

    // runtime objects for fallback
    private GameObject runtimeListRoot;
    private bool runtimeListShown = false;
    // Button-per-map mode
    [Header("Buttons")]
    public bool useButtons = false;
    private System.Collections.Generic.List<UnityEngine.UI.Button> mapButtons = new System.Collections.Generic.List<UnityEngine.UI.Button>();
    private int selectedIndex = 0;
    private GameObject buttonsContainer;

    private void Start()
    {
        if (mapLibrary == null)
        {
            Debug.LogError("MapSelectionUI: MapLibrary not assigned in inspector.");
            return;
        }

        PopulateDropdown();

        if (useButtons)
        {
            CreateButtonsForMaps();
            // Hide the dropdown when using buttons
            if (mapDropdown != null)
            {
                mapDropdown.gameObject.SetActive(false);
            }
        }

        if (mapDropdown != null)
        {
            mapDropdown.onValueChanged.RemoveAllListeners();
            mapDropdown.onValueChanged.AddListener(OnSelectionChanged);
            // ensure shown value is correct at start
            mapDropdown.value = Mathf.Clamp(mapDropdown.value, 0, Mathf.Max(0, mapDropdown.options.Count - 1));
            mapDropdown.RefreshShownValue();
            // Log options for debugging
            Debug.Log($"MapSelectionUI: dropdown has {mapDropdown.options.Count} options.");
            for (int i = 0; i < mapDropdown.options.Count; i++)
            {
                Debug.Log($"Option {i}: {mapDropdown.options[i].text}");
            }

            if (debugOpenDropdownOnStart)
            {
                // Temporarily open the dropdown so you can inspect the list visually
                mapDropdown.Show();
                StartCoroutine(HideDropdownAfterDelay(1f));
            }
            // Create a runtime fallback list if requested
            if (createRuntimeFallback)
                CreateRuntimeFallbackIfNeeded();

            // Try to fix TMP template positioning if requested
            if (autoFixTemplate)
                TryAutoFixDropdownTemplate();
        }

        if (launchButton != null)
            launchButton.onClick.AddListener(OnLaunchClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void PopulateDropdown()
    {
        if (mapDropdown == null) return;
        mapDropdown.ClearOptions();
        var optionData = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
        int count = 0;
        foreach (var m in mapLibrary.maps)
        {
            string name = m != null ? m.mapName : "<empty>";
            optionData.Add(new TMP_Dropdown.OptionData(name));
            count++;
        }
        mapDropdown.AddOptions(optionData.ConvertAll(o => o.text));
        Debug.Log($"MapSelectionUI: Populated dropdown with {count} maps.");
        if (count > 0)
        {
            mapDropdown.value = 0;
            mapDropdown.RefreshShownValue();
        }
        // If using buttons mode, reset selected index
        selectedIndex = 0;
    }

    private void CreateButtonsForMaps()
    {
        // destroy existing container
        if (buttonsContainer != null) Destroy(buttonsContainer);

        // find panel as parent (MapSelectionUI is added to panel in scene creator)
        var panel = GetComponent<RectTransform>();
        Transform parent = transform;
        if (panel == null)
        {
            var p = GetComponentInParent<RectTransform>();
            if (p != null) parent = p.transform;
        }

        buttonsContainer = new GameObject("MapButtonsContainer", typeof(RectTransform));
        buttonsContainer.transform.SetParent(parent, false);
        var brt = buttonsContainer.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.1f, 0.45f);
        brt.anchorMax = new Vector2(0.9f, 0.65f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;

        var hlg = buttonsContainer.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 10f;
        hlg.padding = new RectOffset(10, 10, 10, 10);

        mapButtons.Clear();
        for (int i = 0; i < mapLibrary.maps.Count; i++)
        {
            int idx = i;
            var btnGO = new GameObject($"MapButton_{i}", typeof(RectTransform), typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.Image));
            btnGO.transform.SetParent(buttonsContainer.transform, false);
            
            // Set button size (remove fixed height for horizontal layout)
            var btnRT = btnGO.GetComponent<RectTransform>();
            // Let HorizontalLayoutGroup control sizing
            
            var btnImg = btnGO.GetComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.25f, 0.35f, 0.45f, 0.9f);
            
            var btn = btnGO.GetComponent<UnityEngine.UI.Button>();
            
            // Set up button colors for hover/press effects
            var colors = btn.colors;
            colors.normalColor = new Color(0.25f, 0.35f, 0.45f, 0.9f);
            colors.highlightedColor = new Color(0.35f, 0.45f, 0.55f, 1f);
            colors.pressedColor = new Color(0.15f, 0.25f, 0.35f, 1f);
            colors.selectedColor = new Color(0.2f, 0.6f, 0.3f, 1f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            btn.colors = colors;
            
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGO.transform.SetParent(btnGO.transform, false);
            var txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.offsetMin = new Vector2(15, 0);
            txtRT.offsetMax = new Vector2(-15, 0);
            
            var txt = txtGO.GetComponent<TextMeshProUGUI>();
            txt.text = (mapLibrary.maps[i] != null) ? mapLibrary.maps[i].mapName : "<empty>";
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 18;
            txt.fontStyle = FontStyles.Bold;
            
            btn.onClick.AddListener(() => {
                selectedIndex = idx;
                mapDropdown.value = idx;
                mapDropdown.RefreshShownValue();
                UpdateButtonSelectionVisuals();
                Debug.Log($"Selected map: {txt.text}");
            });
            mapButtons.Add(btn);
        }

        UpdateButtonSelectionVisuals();
    }

    private void UpdateButtonSelectionVisuals()
    {
        for (int i = 0; i < mapButtons.Count; i++)
        {
            var btn = mapButtons[i];
            var img = btn.GetComponent<UnityEngine.UI.Image>();
            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            
            if (i == selectedIndex)
            {
                // Selected button styling
                img.color = new Color(0.2f, 0.7f, 0.3f, 1f);
                if (txt != null)
                {
                    txt.color = Color.white;
                    txt.fontStyle = FontStyles.Bold;
                }
            }
            else
            {
                // Normal button styling
                img.color = new Color(0.25f, 0.35f, 0.45f, 0.9f);
                if (txt != null)
                {
                    txt.color = new Color(0.9f, 0.9f, 0.9f);
                    txt.fontStyle = FontStyles.Normal;
                }
            }
        }
    }

    private void OnSelectionChanged(int index)
    {
        Debug.Log($"MapSelectionUI: Selected map index {index}");
        // placeholder: could update a preview image here using mapLibrary.maps[index].mapPreview
    }

    private System.Collections.IEnumerator HideDropdownAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (mapDropdown != null)
            mapDropdown.Hide();
    }

    // Runtime fallback helpers
    private void CreateRuntimeFallbackIfNeeded()
    {
        if (mapDropdown == null || mapDropdown.options == null || mapDropdown.options.Count <= 1)
            return; // nothing to do

        // If the TMP dropdown's template is not working (user reports only one visible item), create a simple list
        if (runtimeListRoot == null)
            BuildRuntimeList();
    }

    private void ToggleRuntimeList()
    {
        if (runtimeListRoot == null) BuildRuntimeList();
        runtimeListShown = !runtimeListShown;
        runtimeListRoot.SetActive(runtimeListShown);
    }

    private void BuildRuntimeList()
    {
        // find canvas
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        runtimeListRoot = new GameObject("RuntimeMapList", typeof(RectTransform));
        runtimeListRoot.transform.SetParent(canvas.transform, false);
        var rt = runtimeListRoot.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.1f);
        rt.anchorMax = new Vector2(0.4f, 0.4f);
        rt.anchoredPosition = Vector2.zero;

        var bg = runtimeListRoot.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0.95f, 0.95f, 0.95f, 1f);

        var layout = runtimeListRoot.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        for (int i = 0; i < mapDropdown.options.Count; i++)
        {
            int idx = i;
            var btnGO = new GameObject($"Option_{i}", typeof(RectTransform), typeof(UnityEngine.UI.Button), typeof(UnityEngine.UI.Image));
            btnGO.transform.SetParent(runtimeListRoot.transform, false);
            var btnImg = btnGO.GetComponent<UnityEngine.UI.Image>();
            btnImg.color = Color.white;
            var btn = btnGO.GetComponent<UnityEngine.UI.Button>();
            var txtGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtGO.transform.SetParent(btnGO.transform, false);
            var txt = txtGO.GetComponent<TextMeshProUGUI>();
            txt.text = mapDropdown.options[i].text;
            txt.color = Color.black;
            txt.alignment = TextAlignmentOptions.Left;
            btn.onClick.AddListener(() => {
                mapDropdown.value = idx;
                mapDropdown.RefreshShownValue();
                ToggleRuntimeList();
            });
        }

        runtimeListRoot.SetActive(false);
    }

    private void DestroyRuntimeList()
    {
        if (runtimeListRoot != null)
            Destroy(runtimeListRoot);
        runtimeListRoot = null;
    }

    private void TryAutoFixDropdownTemplate()
    {
        if (mapDropdown == null || mapDropdown.template == null)
            return;

        var templateRT = mapDropdown.template;
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // Reparent template under canvas root so it won't be clipped by parent masks
        templateRT.SetParent(canvas.transform, false);
        templateRT.SetAsLastSibling();

        // Ensure it has a Canvas so we can force it to render on top
        var templateGO = templateRT.gameObject;
        var c = templateGO.GetComponent<Canvas>();
        if (c == null) c = templateGO.AddComponent<Canvas>();
        c.overrideSorting = true;
        c.sortingOrder = 1000;

        // Match width to the dropdown RectTransform
        var ddRT = mapDropdown.GetComponent<RectTransform>();
        if (ddRT != null)
        {
            templateRT.anchorMin = new Vector2(0, 0);
            templateRT.anchorMax = new Vector2(0, 0);
            templateRT.pivot = new Vector2(0, 1);
            var worldPos = ddRT.TransformPoint(new Vector3(0, -ddRT.rect.height, 0));
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Camera.main.WorldToScreenPoint(worldPos), canvas.worldCamera, out localPos);
            templateRT.anchoredPosition = localPos;
            templateRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ddRT.rect.width);
        }

        Debug.Log("MapSelectionUI: Attempted to auto-fix TMP_Dropdown template position and sorting.");
    }

    private void OnLaunchClicked()
    {
        if (mapDropdown == null || mapLibrary == null)
            return;

        int index = mapDropdown.value;
        // Store selection and load main scene
        MapLaunchBridge.SelectedMapIndex = index;
        // Load configured scene
#if UNITY_EDITOR
        // Editor-time helpful warning if scene isn't in build settings
        bool found = false;
        foreach (var s in UnityEditor.EditorBuildSettings.scenes)
        {
            if (s.path.EndsWith(sceneToLoad + ".unity") || s.path.Contains("/" + sceneToLoad + ".unity"))
            {
                found = true; break;
            }
        }
        if (!found)
        {
            Debug.LogWarning($"MapSelectionUI: Scene '{sceneToLoad}' is not in Build Settings. Add it via File → Build Settings... or set sceneToLoad to a scene that is included.");
        }
#endif
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
