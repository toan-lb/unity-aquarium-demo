using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Aquarium.Composition;
using Aquarium.Simulation;
using Aquarium.Presentation;

public static class SetupAquariumScene
{
    [MenuItem("Aquarium/Setup Main Scene")]
    public static void CreateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 1. Camera adjustments for aquarium ambiance
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0, 0, -12);
            mainCam.backgroundColor = new Color(0.03f, 0.07f, 0.15f); // Deep oceanic dark blue
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.fieldOfView = 50f;
        }

        // 2. Directional Light for aquarium feel
        var light = Object.FindFirstObjectByType<Light>();
        if (light != null)
        {
            light.color = new Color(0.85f, 0.95f, 1.0f); // Cool underwater light
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // 3. Fish Spawner (Simulation with 30 ambient fish)
        var spawnerGo = new GameObject("FishSpawner");
        var spawner = spawnerGo.AddComponent<FishSpawner>();

        // 4. UI System (Canvas & EventSystem)
        var eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<EventSystem>();
        eventSystemGo.AddComponent<StandaloneInputModule>();

        var canvasGo = new GameObject("Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        // HUD Container (Top Left)
        var hudGo = new GameObject("CurrencyHUD");
        hudGo.transform.SetParent(canvasGo.transform, false);
        var hudRect = hudGo.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(25, -20);
        hudRect.sizeDelta = new Vector2(400, 80);

        // Balance Text
        var balTextGo = new GameObject("BalanceText");
        balTextGo.transform.SetParent(hudGo.transform, false);
        var balText = balTextGo.AddComponent<Text>();
        balText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        balText.fontSize = 28;
        balText.fontStyle = FontStyle.Bold;
        balText.color = new Color(1.0f, 0.85f, 0.2f); // Gold yellow
        balText.text = "Gold: 200";
        var balRect = balTextGo.GetComponent<RectTransform>();
        balRect.anchorMin = new Vector2(0, 0.5f);
        balRect.anchorMax = new Vector2(1, 1);
        balRect.sizeDelta = Vector2.zero;

        // Stats Text (Fish count & FPS)
        var statsTextGo = new GameObject("StatsText");
        statsTextGo.transform.SetParent(hudGo.transform, false);
        var statsText = statsTextGo.AddComponent<Text>();
        statsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        statsText.fontSize = 17;
        statsText.color = new Color(0.45f, 0.85f, 1.0f); // Cyan
        statsText.text = "Active Fish: 30 | FPS: 60";
        var statsRect = statsTextGo.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0, 0);
        statsRect.anchorMax = new Vector2(1, 0.5f);
        statsRect.sizeDelta = Vector2.zero;

        var currencyHUD = hudGo.AddComponent<CurrencyHUD>();
        var soHud = new SerializedObject(currencyHUD);
        soHud.FindProperty("_balanceText").objectReferenceValue = balText;
        soHud.FindProperty("_statsText").objectReferenceValue = statsText;
        soHud.ApplyModifiedProperties();

        // Shop View Container (Bottom Center)
        var shopGo = new GameObject("FishShopView");
        shopGo.transform.SetParent(canvasGo.transform, false);
        var shopRect = shopGo.AddComponent<RectTransform>();
        shopRect.anchorMin = new Vector2(0.5f, 0);
        shopRect.anchorMax = new Vector2(0.5f, 0);
        shopRect.pivot = new Vector2(0.5f, 0);
        shopRect.anchoredPosition = new Vector2(0, 30);
        shopRect.sizeDelta = new Vector2(720, 110);

        // Feedback / Status Text
        var feedbackGo = new GameObject("StatusFeedbackText");
        feedbackGo.transform.SetParent(shopGo.transform, false);
        var fbText = feedbackGo.AddComponent<Text>();
        fbText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        fbText.fontSize = 17;
        fbText.color = new Color(0.9f, 0.95f, 1.0f);
        fbText.text = "Welcome to Aquarium Simulator! Buy fish or a school to expand.";
        fbText.alignment = TextAnchor.MiddleCenter;
        var fbRect = feedbackGo.GetComponent<RectTransform>();
        fbRect.anchorMin = new Vector2(0, 0.6f);
        fbRect.anchorMax = new Vector2(1, 1);
        fbRect.sizeDelta = Vector2.zero;

        // Button Container (Row of 4 buttons)
        var btnRowGo = new GameObject("ButtonRow");
        btnRowGo.transform.SetParent(shopGo.transform, false);
        var btnRowRect = btnRowGo.AddComponent<RectTransform>();
        btnRowRect.anchorMin = new Vector2(0, 0);
        btnRowRect.anchorMax = new Vector2(1, 0.6f);
        btnRowRect.sizeDelta = Vector2.zero;

        // 4 Action Buttons
        var btnGuppyGo = CreateButton("BuyGuppyButton", "Guppy (30G)", btnRowGo.transform, new Vector2(-255, 0), new Color(0.85f, 0.45f, 0.1f));
        var btnTetraGo = CreateButton("BuyTetraButton", "Tetra (60G)", btnRowGo.transform, new Vector2(-85, 0), new Color(0.1f, 0.65f, 0.85f));
        var btnSchoolGo = CreateButton("BuySchoolButton", "+10 School (150G)", btnRowGo.transform, new Vector2(85, 0), new Color(0.15f, 0.65f, 0.4f));
        var btnAddGoldGo = CreateButton("AddGoldButton", "+100 Gold", btnRowGo.transform, new Vector2(255, 0), new Color(0.6f, 0.35f, 0.8f));

        var shopView = shopGo.AddComponent<FishShopView>();
        var soShop = new SerializedObject(shopView);
        soShop.FindProperty("_buyGuppyButton").objectReferenceValue = btnGuppyGo.GetComponent<Button>();
        soShop.FindProperty("_buyTetraButton").objectReferenceValue = btnTetraGo.GetComponent<Button>();
        soShop.FindProperty("_buySchoolButton").objectReferenceValue = btnSchoolGo.GetComponent<Button>();
        soShop.FindProperty("_addGoldButton").objectReferenceValue = btnAddGoldGo.GetComponent<Button>();
        soShop.FindProperty("_statusFeedbackText").objectReferenceValue = fbText;
        soShop.ApplyModifiedProperties();

        // 5. Composition Root
        var rootGo = new GameObject("AquariumCompositionRoot");
        var compRoot = rootGo.AddComponent<AquariumCompositionRoot>();
        var soRoot = new SerializedObject(compRoot);
        soRoot.FindProperty("_spawner").objectReferenceValue = spawner;
        soRoot.FindProperty("_currencyHUD").objectReferenceValue = currencyHUD;
        soRoot.FindProperty("_shopView").objectReferenceValue = shopView;
        soRoot.FindProperty("_defaultStartingGold").intValue = 250;
        soRoot.ApplyModifiedProperties();

        // 6. Save Scene
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainAquarium.unity");

        // Add to EditorBuildSettings
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/MainAquarium.unity", true) };
        Debug.Log("✅ Scene 'Assets/Scenes/MainAquarium.unity' generated with bustling aquarium features!");
    }

    private static GameObject CreateButton(string name, string label, Transform parent, Vector2 pos, Color baseColor)
    {
        var btnGo = new GameObject(name);
        btnGo.transform.SetParent(parent, false);

        var image = btnGo.AddComponent<Image>();
        image.color = baseColor;

        var button = btnGo.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = baseColor * 1.25f;
        colors.pressedColor = baseColor * 0.75f;
        button.colors = colors;

        var rect = btnGo.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta = new Vector2(155, 42);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = label;
        text.fontSize = 14;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        return btnGo;
    }
}
