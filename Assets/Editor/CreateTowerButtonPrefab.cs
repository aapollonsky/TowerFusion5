using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class CreateTowerButtonPrefab
{
    [MenuItem("Tools/Tower Fusion/Create Tower Button Prefab")]
    public static void CreatePrefab()
    {
        // Create Canvas if none exists
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // Parent for the button (temporary)
        GameObject parent = new GameObject("TowerButton_Prefab_Holder");

        // Create Button
        GameObject buttonGO = new GameObject("TowerButton", typeof(RectTransform), typeof(Button), typeof(Image));
        buttonGO.transform.SetParent(parent.transform, false);

        Image img = buttonGO.GetComponent<Image>();
        img.color = Color.white;

        // Add TowerButton script if present
        var towerButtonScript = buttonGO.AddComponent(typeof(TowerFusion.UI.TowerButton));

        // Create icon child
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(buttonGO.transform, false);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0f, 0.5f);
        iconRT.anchorMax = new Vector2(0f, 0.5f);
        iconRT.sizeDelta = new Vector2(40, 40);
        iconRT.anchoredPosition = new Vector2(30, 0);

        // Create name text
        GameObject nameGO = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.text = "Tower";
        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0f, 0.5f);
        nameRT.anchorMax = new Vector2(0f, 0.5f);
        nameRT.anchoredPosition = new Vector2(80, 10);
        nameRT.sizeDelta = new Vector2(140, 20);

        // Create cost text
        GameObject costGO = new GameObject("Cost", typeof(RectTransform), typeof(TextMeshProUGUI));
        costGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI costText = costGO.GetComponent<TextMeshProUGUI>();
        costText.text = "0g";
        RectTransform costRT = costGO.GetComponent<RectTransform>();
        costRT.anchorMin = new Vector2(0f, 0.5f);
        costRT.anchorMax = new Vector2(0f, 0.5f);
        costRT.anchoredPosition = new Vector2(80, -10);
        costRT.sizeDelta = new Vector2(140, 20);

        // Wire up TowerButton serialized fields via reflection
        // Note: This tries to assign fields named 'button','towerIcon','towerNameText','costText'
        var tb = buttonGO.GetComponent(typeof(TowerFusion.UI.TowerButton));
        if (tb != null)
        {
            var tbType = tb.GetType();
            var buttonField = tbType.GetField("button", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var iconField = tbType.GetField("towerIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var nameField = tbType.GetField("towerNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var costField = tbType.GetField("costText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (buttonField != null)
                buttonField.SetValue(tb, buttonGO.GetComponent<Button>());
            if (iconField != null)
                iconField.SetValue(tb, iconGO.GetComponent<Image>());
            if (nameField != null)
                nameField.SetValue(tb, nameText);
            if (costField != null)
                costField.SetValue(tb, costText);
        }

        // Ensure Prefabs/UI folder exists
        string prefabDir = "Assets/Prefabs/UI";
        if (!Directory.Exists(prefabDir))
            Directory.CreateDirectory(prefabDir);

        string prefabPath = prefabDir + "/TowerButton.prefab";

        // Save as prefab
        Object prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(buttonGO, prefabPath, InteractionMode.UserAction);
        if (prefab != null)
        {
            Debug.Log($"Created TowerButton prefab at {prefabPath}");
        }
        else
        {
            Debug.LogError("Failed to create TowerButton prefab");
        }

        // Cleanup temporary parent
        Object.DestroyImmediate(parent);
    }
}
