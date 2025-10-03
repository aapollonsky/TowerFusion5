using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;

public static class CreateMapSelectorScene
{
    [MenuItem("Tools/Tower Fusion/Create Map Selector Scene")]
    public static void CreateScene()
    {
        // Create scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MapSelector";

        // Create Canvas
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

    // Create a Camera so the scene has at least one camera (prevents "Display 1: No cameras rendering" warning)
    var cameraGO = new GameObject("Main Camera", typeof(Camera));
    var cam = cameraGO.GetComponent<Camera>();
    cam.tag = "MainCamera";
    cam.clearFlags = CameraClearFlags.Color;
    cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
    cameraGO.transform.position = new Vector3(0, 0, -10);

        // Create EventSystem if missing
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            EditorSceneManager.MarkSceneDirty(scene);
        }

        // Panel
        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var img = panel.GetComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0.25f);
        rect.anchorMax = new Vector2(0.75f, 0.75f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        // Title
        var titleGO = new GameObject("Title", typeof(TextMeshProUGUI));
        titleGO.transform.SetParent(panel.transform, false);
        var title = titleGO.GetComponent<TextMeshProUGUI>();
        title.text = "Select Map";
        title.alignment = TextAlignmentOptions.Center;
        var tr = title.GetComponent<RectTransform>();
        tr.anchorMin = new Vector2(0.1f, 0.75f);
        tr.anchorMax = new Vector2(0.9f, 0.95f);
        tr.offsetMin = tr.offsetMax = Vector2.zero;

    // Dropdown (with required TMP structure)
    var dropdownGO = new GameObject("MapDropdown", typeof(RectTransform), typeof(TMP_Dropdown), typeof(Image));
    dropdownGO.transform.SetParent(panel.transform, false);
    var dropdown = dropdownGO.GetComponent<TMP_Dropdown>();
    var drt = dropdownGO.GetComponent<RectTransform>();
    drt.anchorMin = new Vector2(0.1f, 0.5f);
    drt.anchorMax = new Vector2(0.9f, 0.65f);
    drt.offsetMin = drt.offsetMax = Vector2.zero;

    // Visuals for dropdown: background image
    var dropdownImg = dropdownGO.GetComponent<Image>();
    dropdownImg.color = Color.white * 0.95f;

    // Caption (label shown when closed)
    var captionGO = new GameObject("Caption", typeof(RectTransform), typeof(TextMeshProUGUI));
    captionGO.transform.SetParent(dropdownGO.transform, false);
    var captionText = captionGO.GetComponent<TextMeshProUGUI>();
    captionText.text = "Select...";
    captionText.alignment = TextAlignmentOptions.Left;
    var capRT = captionGO.GetComponent<RectTransform>();
    capRT.anchorMin = new Vector2(0.05f, 0.1f);
    capRT.anchorMax = new Vector2(0.85f, 0.9f);
    capRT.offsetMin = capRT.offsetMax = Vector2.zero;

    // Arrow
    var arrowGO = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
    arrowGO.transform.SetParent(dropdownGO.transform, false);
    var arrowImg = arrowGO.GetComponent<Image>();
    arrowImg.color = Color.black;
    var aRT = arrowGO.GetComponent<RectTransform>();
    aRT.anchorMin = new Vector2(0.9f, 0.1f);
    aRT.anchorMax = new Vector2(0.98f, 0.9f);
    aRT.offsetMin = aRT.offsetMax = Vector2.zero;

    // Template (the dropdown list) - create as sibling under the Canvas so it renders above other UI (prevents clipping)
    var templateGO = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
    templateGO.transform.SetParent(canvasGO.transform, false);
    var tplRT = templateGO.GetComponent<RectTransform>();
    tplRT.anchorMin = new Vector2(0f, 0f);
    tplRT.anchorMax = new Vector2(1f, 0f);
    tplRT.pivot = new Vector2(0.5f, 1f);
    tplRT.sizeDelta = new Vector2(0, 150);
    // Make template background transparent so the dropdown list doesn't show a solid block behind items
    var tplImg = templateGO.GetComponent<Image>();
    if (tplImg != null) tplImg.color = new Color(0, 0, 0, 0);
    // Make sure the template is top-most so it isn't hidden by other UI
    templateGO.transform.SetAsLastSibling();
    templateGO.SetActive(false);

    // Viewport (mask)
    var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
    viewportGO.transform.SetParent(templateGO.transform, false);
    var vpRT = viewportGO.GetComponent<RectTransform>();
    vpRT.anchorMin = new Vector2(0, 0);
    vpRT.anchorMax = new Vector2(1, 1);
    vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
    var vpImg = viewportGO.GetComponent<Image>();
    vpImg.color = new Color(1f, 1f, 1f, 0.95f);
    var mask = viewportGO.GetComponent<Mask>();
    mask.showMaskGraphic = false;

    // Content (container for items)
    var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
    contentGO.transform.SetParent(viewportGO.transform, false);
    var contentRT = contentGO.GetComponent<RectTransform>();
    contentRT.anchorMin = new Vector2(0, 1);
    contentRT.anchorMax = new Vector2(1, 1);
    contentRT.pivot = new Vector2(0.5f, 1f);
    contentRT.anchoredPosition = Vector2.zero;
    var vs = contentGO.GetComponent<VerticalLayoutGroup>();
    vs.childForceExpandHeight = false;
    vs.childForceExpandWidth = true;
    var csf = contentGO.GetComponent<ContentSizeFitter>();
    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

    // Item template (Toggle with label)
    var itemGO = new GameObject("Item", typeof(RectTransform), typeof(Toggle), typeof(Image));
    itemGO.transform.SetParent(contentGO.transform, false);
    var itemRT = itemGO.GetComponent<RectTransform>();
    itemRT.sizeDelta = new Vector2(0, 30);
    var itemBg = itemGO.GetComponent<Image>();
    itemBg.color = new Color(1f, 1f, 1f, 0.98f);
    var toggle = itemGO.GetComponent<Toggle>();
    // wire the toggle's target graphic so visual transitions work
    toggle.targetGraphic = itemBg;

    // Add a small checkmark image for the toggle and assign it as the graphic
    var checkGO = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
    checkGO.transform.SetParent(itemGO.transform, false);
    var checkImg = checkGO.GetComponent<Image>();
    checkImg.color = Color.black;
    var checkRT = checkGO.GetComponent<RectTransform>();
    checkRT.anchorMin = new Vector2(0.92f, 0.25f);
    checkRT.anchorMax = new Vector2(0.98f, 0.75f);
    checkRT.offsetMin = checkRT.offsetMax = Vector2.zero;
    toggle.graphic = checkImg;

    var itemLabelGO = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
    itemLabelGO.transform.SetParent(itemGO.transform, false);
    var itemLabel = itemLabelGO.GetComponent<TextMeshProUGUI>();
    itemLabel.text = "Option";
    itemLabel.alignment = TextAlignmentOptions.Left;
    itemLabel.color = Color.black;
    var ilRT = itemLabelGO.GetComponent<RectTransform>();
    ilRT.anchorMin = new Vector2(0.05f, 0);
    ilRT.anchorMax = new Vector2(0.95f, 1);
    ilRT.offsetMin = ilRT.offsetMax = Vector2.zero;

    // Wire up ScrollRect and Toggle behavior
    var scroll = templateGO.GetComponent<ScrollRect>();
    scroll.content = contentRT;
    scroll.viewport = vpRT;

    // Assign TMP_Dropdown references
    dropdown.template = tplRT;
    dropdown.captionText = captionText;
    dropdown.itemText = itemLabel;
    dropdown.captionImage = dropdownImg;
    dropdown.itemImage = itemBg;

    // Ensure Unity serializes these references when saving the scene
    EditorUtility.SetDirty(dropdown);
    EditorUtility.SetDirty(dropdownGO);
    EditorUtility.SetDirty(templateGO);
    EditorUtility.SetDirty(captionGO);
    EditorUtility.SetDirty(itemGO);
    EditorSceneManager.MarkSceneDirty(scene);

        // Launch Button
        var launchGO = new GameObject("LaunchButton", typeof(Button), typeof(Image));
        launchGO.transform.SetParent(panel.transform, false);
        var launchBtn = launchGO.GetComponent<Button>();
        var launchImg = launchGO.GetComponent<Image>();
        launchImg.color = new Color(0.2f, 0.7f, 0.3f);
        var lrt = launchGO.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.1f, 0.15f);
        lrt.anchorMax = new Vector2(0.45f, 0.3f);
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        
        // Launch button hover colors
        var launchColors = launchBtn.colors;
        launchColors.normalColor = new Color(0.2f, 0.7f, 0.3f);
        launchColors.highlightedColor = new Color(0.3f, 0.8f, 0.4f);
        launchColors.pressedColor = new Color(0.1f, 0.5f, 0.2f);
        launchBtn.colors = launchColors;
        
        var launchTextGO = new GameObject("Text", typeof(TextMeshProUGUI));
        launchTextGO.transform.SetParent(launchGO.transform, false);
        var launchTextRT = launchTextGO.GetComponent<RectTransform>();
        launchTextRT.anchorMin = Vector2.zero;
        launchTextRT.anchorMax = Vector2.one;
        launchTextRT.offsetMin = launchTextRT.offsetMax = Vector2.zero;
        var launchText = launchTextGO.GetComponent<TextMeshProUGUI>();
        launchText.text = "Launch";
        launchText.alignment = TextAlignmentOptions.Center;
        launchText.fontSize = 20;
        launchText.fontStyle = FontStyles.Bold;
        launchText.color = Color.white;

        // Quit Button
        var quitGO = new GameObject("QuitButton", typeof(Button), typeof(Image));
        quitGO.transform.SetParent(panel.transform, false);
        var quitBtn = quitGO.GetComponent<Button>();
        var quitImg = quitGO.GetComponent<Image>();
        quitImg.color = new Color(0.7f, 0.3f, 0.3f);
        var qrt = quitGO.GetComponent<RectTransform>();
        qrt.anchorMin = new Vector2(0.55f, 0.15f);
        qrt.anchorMax = new Vector2(0.9f, 0.3f);
        qrt.offsetMin = qrt.offsetMax = Vector2.zero;
        
        // Quit button hover colors
        var quitColors = quitBtn.colors;
        quitColors.normalColor = new Color(0.7f, 0.3f, 0.3f);
        quitColors.highlightedColor = new Color(0.8f, 0.4f, 0.4f);
        quitColors.pressedColor = new Color(0.5f, 0.2f, 0.2f);
        quitBtn.colors = quitColors;
        
        var quitTextGO = new GameObject("Text", typeof(TextMeshProUGUI));
        quitTextGO.transform.SetParent(quitGO.transform, false);
        var quitTextRT = quitTextGO.GetComponent<RectTransform>();
        quitTextRT.anchorMin = Vector2.zero;
        quitTextRT.anchorMax = Vector2.one;
        quitTextRT.offsetMin = quitTextRT.offsetMax = Vector2.zero;
        var quitText = quitTextGO.GetComponent<TextMeshProUGUI>();
        quitText.text = "Quit";
        quitText.alignment = TextAlignmentOptions.Center;
        quitText.fontSize = 20;
        quitText.fontStyle = FontStyles.Bold;
        quitText.color = Color.white;

        // Add MapSelectionUI component
        var selector = panel.AddComponent<global::MapSelectionUI>();
        selector.mapDropdown = dropdown;
        selector.launchButton = launchBtn;
        selector.quitButton = quitBtn;
        selector.useButtons = true; // Enable horizontal button mode by default

        // Try to find MapLibrary asset in project
        var libs = AssetDatabase.FindAssets("t:MapLibrary");
        if (libs != null && libs.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(libs[0]);
            selector.mapLibrary = AssetDatabase.LoadAssetAtPath<TowerFusion.MapLibrary>(path);
        }

        // Save scene
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MapSelector.unity");
        AssetDatabase.Refresh();
        Debug.Log("Created MapSelector scene at Assets/Scenes/MapSelector.unity");

        // Try to auto-wire MapLaunchApplier into the main scene if it exists
        string mainScenePath = "Assets/Scenes/MainScene.unity";
        if (System.IO.File.Exists(mainScenePath))
        {
            var mainScene = EditorSceneManager.OpenScene(mainScenePath, OpenSceneMode.Additive);
            var mapManager = Object.FindObjectOfType<TowerFusion.MapManager>();
            if (mapManager != null)
            {
                var ga = mapManager.gameObject;
                var existingApplier = ga.GetComponent<global::MapLaunchApplier>();
                if (existingApplier == null)
                {
                    var applier = ga.AddComponent<global::MapLaunchApplier>();
                    applier.mapManager = mapManager;
                    EditorUtility.SetDirty(ga);
                    Debug.Log("Added MapLaunchApplier to MapManager GameObject in MainScene.");
                }
                else
                {
                    Debug.Log("MainScene already contains MapLaunchApplier on the MapManager GameObject.");
                }

                EditorSceneManager.MarkSceneDirty(mainScene);
                EditorSceneManager.SaveScene(mainScene);
                // Ensure MainScene is in Build Settings
                var mainSceneAssetPath = mainScenePath;
                var existingScenes = EditorBuildSettings.scenes;
                bool inBuild = false;
                foreach (var s in existingScenes)
                {
                    if (s.path == mainSceneAssetPath) { inBuild = true; break; }
                }
                if (!inBuild)
                {
                    var list = new System.Collections.Generic.List<EditorBuildSettingsScene>(existingScenes);
                    list.Add(new EditorBuildSettingsScene(mainSceneAssetPath, true));
                    EditorBuildSettings.scenes = list.ToArray();
                    Debug.Log("Added MainScene to Build Settings.");
                }
            }
            else
            {
                Debug.LogWarning("MainScene found but no MapManager was present to attach MapLaunchApplier to.");
            }

            // Close the main scene we opened (keep only selector open)
            EditorSceneManager.CloseScene(mainScene, true);
        }
    }
}
