using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private readonly Dictionary<UIEvent, List<UIBase>> _activeUIEvents = new Dictionary<UIEvent, List<UIBase>>();
    private readonly Dictionary<UIPrefab, UIBase> _UIBases = new Dictionary<UIPrefab, UIBase>();
    private readonly Dictionary<string, Sprite> _spriteResources = new Dictionary<string, Sprite>();

    private const string _uiPath = "Prefabs/UI/";
    private const string _spritePath = "Textures/";

    public void Init()
    {
        _canvas = Object.FindObjectOfType<Canvas>();
        _canvasRect = _canvas.gameObject.GetComponent<RectTransform>();
    }

    public void ActiveUI(UIPrefab ui)
    {
        if(_UIBases.ContainsKey(ui) == false)
        {
            CreateUI(ui);
        }

        UIBase uiBase = _UIBases[ui];
        if(uiBase != null)
        {
            uiBase.gameObject.SetActive(true);
            uiBase.Init();
        }
    }

    private void CreateUI(UIPrefab ui)
    {
        GameObject uiObj = Object.Instantiate(Resources.Load<GameObject>(_uiPath + ui.ToString()), _canvas.transform, false);
        RectTransform rect = uiObj.GetComponent<RectTransform>();

        rect.anchoredPosition = Vector3.zero;
        rect.localScale = Vector3.one;

        UIBase uiBase = uiObj.GetComponent<UIBase>();
        _UIBases.Add(ui, uiBase);
    }

    public void DeactiveUI(UIPrefab ui)
    {
        if(_UIBases.ContainsKey(ui) == false)
        {
            return;
        }

        UIBase uiBase = _UIBases[ui];
        if(uiBase != null)
        {
            uiBase.Release();
            uiBase.gameObject.SetActive(false);
        }
    }

    public void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        if (_activeUIEvents.ContainsKey(uiEvent))
        {
            List<UIBase> uiBases = _activeUIEvents[uiEvent];

            for (int i = 0; i < uiBases.Count; i++)
            {
                uiBases[i].UpdateUI(uiEvent, values);
            }
        }
    }

    public void AttachUIEvent(UIEvent uiEvent, UIBase uiBase)
    {
        List<UIBase> uiBases;
        if (_activeUIEvents.ContainsKey(uiEvent))
        {
            uiBases = _activeUIEvents[uiEvent];
            if (uiBases.Contains(uiBase)) return;

            uiBases.Add(uiBase);
        }
        else
        {
            uiBases = new List<UIBase>();
            uiBases.Add(uiBase);
            _activeUIEvents.Add(uiEvent, uiBases);
        }
    }

    public void DetachUIEvent(UIEvent uiEvent, UIBase uiBase)
    {
        if (_activeUIEvents.ContainsKey(uiEvent) == false) return;

        List<UIBase> uiBases = _activeUIEvents[uiEvent];

        uiBases.Remove(uiBase);

        if (uiBases.Count == 0) _activeUIEvents.Remove(uiEvent);
    }

    public Sprite GetSpriteResource(string value)
    {
        if (_spriteResources.ContainsKey(value)) return _spriteResources[value];
        else
        {
            Sprite sprite = Resources.Load<Sprite>(_spritePath + value);
            _spriteResources.Add(value, sprite);
            return sprite;
        }
    }

    public Canvas GetCanvas()
    {
        return _canvas;
    }

    public Vector2 GetCanvasSize()
    {
        return _canvasRect.sizeDelta;
    }

    public Vector2 GetDynamicPositionByWorldPos(Vector2 worldPos)
    {
        Vector2 view = Camera.main.WorldToViewportPoint(worldPos);
        Vector2 canSize = _canvasRect.sizeDelta;
        return new Vector2(canSize.x * view.x, canSize.y * view.y);
    }

    public Vector2 GetDynamicPositionByScreenPos(Vector2 screenPos)
    {
        Vector2 view = Camera.main.ScreenToViewportPoint(screenPos);
        Vector2 canSize = _canvasRect.sizeDelta;
        return new Vector2(canSize.x * view.x, canSize.y * view.y);
    }
}

