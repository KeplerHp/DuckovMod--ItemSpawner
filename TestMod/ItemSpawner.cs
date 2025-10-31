using System;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ItemSpawner
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        Canvas? _canvas;
        GameObject? _panel;
        CanvasGroup? _panelGroup;
        TMP_InputField? _input;
        Button? _spawnButton;
        TextMeshProUGUI? _status;

        // 面板宽度占屏幕宽度的比例（0.1 - 0.95）
        const float PanelWidthPercent = 0.40f;

        void Awake()
        {
            CreateUI();
        }

        void OnDestroy()
        {
            if (_canvas != null)
                Destroy(_canvas.gameObject);
        }

        void Update()
        {
            // 按 F10 切换显示/隐藏
            if (Input.GetKeyDown(KeyCode.F10))
            {
                if (_panel != null)
                {
                    bool newState = !_panel.activeSelf;
                    _panel.SetActive(newState);

                    if (newState)
                    {
                        // 显示时启用交互并聚焦输入框
                        if (_panelGroup != null)
                        {
                            _panelGroup.interactable = true;
                            _panelGroup.blocksRaycasts = true;
                        }

                        // 确保有 EventSystem
                        if (EventSystem.current == null)
                            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

                        // 聚焦输入框（ActivateInputField + 设置选中）
                        if (_input != null)
                        {
                            _input.ActivateInputField();
                            EventSystem.current?.SetSelectedGameObject(_input.gameObject);
                        }
                    }
                    else
                    {
                        // 隐藏时关闭交互并清除焦点，停用输入框
                        if (_panelGroup != null)
                        {
                            _panelGroup.interactable = false;
                            _panelGroup.blocksRaycasts = false;
                        }

                        _input?.DeactivateInputField();
                        EventSystem.current?.SetSelectedGameObject(null);
                    }
                }
            }
        }

        void CreateUI()
        {
            // Canvas
            var canvasGO = new GameObject("ItemSpawnerCanvas");
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // 添加 CanvasScaler 以支持不同分辨率下的自适应
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(canvasGO);

            // Panel
            _panel = new GameObject("ItemSpawnerPanel");
            var rt = _panel.AddComponent<RectTransform>();
            _panel.AddComponent<CanvasRenderer>();
            var img = _panel.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.6f);
            _panel.transform.SetParent(canvasGO.transform, false);

            // 添加 CanvasGroup 控制交互
            _panelGroup = _panel.AddComponent<CanvasGroup>();
            _panelGroup.interactable = false;
            _panelGroup.blocksRaycasts = false;

            // 计算并限制宽度比例，确保在合理范围内
            float pct = Mathf.Clamp(PanelWidthPercent, 0.1f, 0.95f);
            float half = pct * 0.5f;

            // 使用锚点来决定面板在父画布中的宽度（根据屏幕宽度百分比自适应）
            rt.anchorMin = new Vector2(0.5f - half, 0.5f); // 水平起点
            rt.anchorMax = new Vector2(0.5f + half, 0.5f); // 水平终点
            // 当 anchors 在水平上有跨度时，sizeDelta.x = 0 表示宽度由父级宽度 * (anchorMax.x - anchorMin.x) 决定
            rt.sizeDelta = new Vector2(0f, 120f); // 固定高度 120
            rt.anchoredPosition = new Vector2(0, -100);

            // Input (TMP)
            var inputGO = new GameObject("ItemIdInput");
            inputGO.transform.SetParent(_panel.transform, false);
            var inputRT = inputGO.AddComponent<RectTransform>();
            // 拉伸到 panel 的宽度并留出左右间距（通过 sizeDelta 为负）
            inputRT.anchorMin = new Vector2(0f, 1f);
            inputRT.anchorMax = new Vector2(1f, 1f);
            inputRT.pivot = new Vector2(0.5f, 1f);
            inputRT.anchoredPosition = new Vector2(0, -10);
            inputRT.sizeDelta = new Vector2(-20f, 36f);

            _input = inputGO.AddComponent<TMP_InputField>();
            _input.contentType = TMP_InputField.ContentType.IntegerNumber;
            _input.lineType = TMP_InputField.LineType.SingleLine;

            var inputTextGO = new GameObject("Text");
            inputTextGO.transform.SetParent(inputGO.transform, false);
            var inputText = inputTextGO.AddComponent<TextMeshProUGUI>();
            inputText.raycastTarget = false;
            inputText.fontSize = 18;
            inputText.margin = new Vector4(5, 5, 5, 5);
            inputText.enableAutoSizing = false;
            inputText.text = "";
            inputText.color = Color.white;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;

            var placeholderGO = new GameObject("Placeholder");
            placeholderGO.transform.SetParent(inputGO.transform, false);
            var placeholder = placeholderGO.AddComponent<TextMeshProUGUI>();
            placeholder.raycastTarget = false;
            placeholder.fontSize = 18;
            placeholder.text = "输入物品 ID（整数），回车或点击“生成”";
            placeholder.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;

            _input.textComponent = inputText;
            _input.placeholder = placeholder;

            // Button
            var btnGO = new GameObject("SpawnButton");
            btnGO.transform.SetParent(_panel.transform, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.5f, 0f);
            btnRT.anchorMax = new Vector2(0.5f, 0f);
            btnRT.pivot = new Vector2(0.5f, 0f);
            btnRT.anchoredPosition = new Vector2(0, 10);
            btnRT.sizeDelta = new Vector2(140f, 36f);

            var buttonImg = btnGO.AddComponent<Image>();
            buttonImg.color = new Color(0.2f, 0.6f, 0.2f, 1f);
            _spawnButton = btnGO.AddComponent<Button>();

            var btnTextGO = new GameObject("ButtonText");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            var btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
            btnText.text = "生成并交给玩家";
            btnText.fontSize = 16;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;
            var btnTextRT = btnTextGO.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.offsetMin = Vector2.zero;
            btnTextRT.offsetMax = Vector2.zero;

            _spawnButton.onClick.AddListener(SpawnFromInput);

            // 支持回车提交
            _input.onSubmit.AddListener(_ => SpawnFromInput());

            // Status 文本
            var statusGO = new GameObject("StatusText");
            statusGO.transform.SetParent(_panel.transform, false);
            var statusRT = statusGO.AddComponent<RectTransform>();
            statusRT.anchorMin = new Vector2(0f, 0f);
            statusRT.anchorMax = new Vector2(1f, 0f);
            statusRT.pivot = new Vector2(0.5f, 0f);
            statusRT.anchoredPosition = new Vector2(0, 52);
            statusRT.sizeDelta = new Vector2(-20f, 18f);

            _status = statusGO.AddComponent<TextMeshProUGUI>();
            _status.fontSize = 14;
            _status.alignment = TextAlignmentOptions.Center;
            _status.text = "按 F10 切换面板显示。";
            _status.color = Color.white;
        }

        void SpawnFromInput()
        {
            // 保护：若面板已隐藏或不允许交互，阻止触发生成逻辑
            if (_panel == null || !_panel.activeSelf || (_panelGroup != null && !_panelGroup.interactable))
            {
                SetStatus("面板不可用，不能生成物品。");
                return;
            }

            if (_input == null) return;

            var raw = _input.text?.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                SetStatus("请输入物品 ID。");
                return;
            }

            if (!int.TryParse(raw, out var id))
            {
                SetStatus("ID 必须是整数。");
                return;
            }

            try
            {
                Item? item = ItemAssetsCollection.InstantiateSync(id);
                if (item == null)
                {
                    SetStatus($"未找到 ID={id} 对应的物品。");
                    return;
                }

                // 将物品送给玩家（按需可设置参数）
                ItemUtilities.SendToPlayer(item, dontMerge: false, sendToStorage: true);
                SetStatus($"已生成并发送物品 ID={id} 给玩家。");
                _input.text = string.Empty;
            }
            catch (Exception ex)
            {
                SetStatus($"生成或发送物品时出错: {ex.Message}");
                Debug.LogException(ex);
            }
        }

        void SetStatus(string text)
        {
            if (_status != null)
                _status.text = text;
        }
    }
}