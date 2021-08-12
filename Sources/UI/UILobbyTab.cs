using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyTab : UIBase
{
    public Image[] _tabMenus;
    public static Action<LobbyMenu> _changeLobbyMenuEvent;
    public Sprite select;
    public Sprite deselect;
    
    public override void Init()
    {
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_Lobby_Menu_select, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_Lobby_Menu_deselect, this);
    }

    public override void Release()
    {
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_Lobby_Menu_select, this);
        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_Lobby_Menu_deselect, this);
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_Lobby_Menu_select:
                _tabMenus[(int)values[0]].sprite = select;
                break;
            case UIEvent.UIEvent_Lobby_Menu_deselect:
                _tabMenus[(int)values[0]].sprite = deselect;
                break;
        }
    }

    public void SelectLobbyMenu(int menu)
    {
        LobbyMenu lobbyMenu = (LobbyMenu)menu;

        _changeLobbyMenuEvent?.Invoke(lobbyMenu);
    }
}
