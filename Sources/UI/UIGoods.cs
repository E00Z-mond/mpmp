using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class UIGoods : UIBase, IUnityAdsListener
{
    public Text _hornText;
    public Text _hornTimerText;
    public Text _goldText;
    public Text _cloverText;
    public GameObject _adsPopup;

    private Game_Data _gameData = null;
    private const string _gameID = "4246013";
    private const string _adsType = "Rewarded_Android";
    private readonly WaitForSeconds _waitFor = new WaitForSeconds(1f);

    public override void Init()
    {
        _gameData = DataController.Instance.GetGameData();
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_goods, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_horn, this);

#if UNITY_EDITOR
        Advertisement.Initialize(_gameID, true);
#else
        Advertisement.Initialize(_gameID, false);
#endif
        Advertisement.AddListener(this);
    }

    public override void Release()
    {
        Advertisement.RemoveListener(this);

        UIManager.Instance.DetachUIEvent(UIEvent.UIEvent_update_goods, this);
        UIManager.Instance.AttachUIEvent(UIEvent.UIEvent_update_horn, this);

        _hornTimerText.gameObject.SetActive(false);

        StopAllCoroutines();
    }

    public override void UpdateUI(UIEvent uiEvent, params object[] values)
    {
        switch (uiEvent)
        {
            case UIEvent.UIEvent_update_goods:
                _hornText.text = _gameData.horn.ToString();
                _goldText.text = _gameData.gold.ToString();
                _cloverText.text = _gameData.clover.ToString();
                break;
            case UIEvent.UIEvent_update_horn:
                _hornText.text = _gameData.horn.ToString();
                break;
        }
    }

    public void ShowHornTimer()
    {
        if (_gameData.horn < Const.MAX_HORN) StartCoroutine(HornTimer());
    }

    private IEnumerator HornTimer()
    {
        _hornTimerText.gameObject.SetActive(true);
        float remainingSeconds = Main.GameMain._timeController.GetRemainingSeconds();

        for (int i = 0; i < 5; i++)
        {
            int min = Mathf.FloorToInt(remainingSeconds / 60);
            int sec = Mathf.FloorToInt(remainingSeconds % 60);

            _hornTimerText.text = string.Format("{0} : {1:00}", min, sec);
            remainingSeconds--;
            yield return _waitFor;
        }

        _hornTimerText.gameObject.SetActive(false);
    }

    public void ActiveAdsPoppup()
    {
        transform.SetAsLastSibling();
        _adsPopup.SetActive(true);
    }

    public void ShowAds()
    {
        if (Advertisement.IsReady(_adsType))
        {
            Advertisement.Show(_adsType);
        }
        else
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Unable to load ads.\nPlease try again later!");
        }
    }

    public void OnUnityAdsDidError(string message)
    {
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Unable to load ads.\nPlease try again later!");
#if UNITY_EDITOR
        Debug.Log("@ OnUnityAdsDidError: " + message);
#endif
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        if (placementId == _adsType)
        {
            _adsPopup.SetActive(false);
        }
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        switch (showResult)
        {
            case ShowResult.Finished:
                _gameData.horn += 2;
                DataController.Instance.SaveData();
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Congratulations!\nYou've got two horns!");
                _hornText.text = _gameData.horn.ToString();
                break;
            case ShowResult.Failed:
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Failed!\n" + "The advertisement was not completed normally.");
                break;
            case ShowResult.Skipped:
                UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Skipped!\n" + "The advertisement was not\ncompleted normally.");
                break;
        }

        Main.GameMain._timeController.ResetTimer(true);
    }

    public void OnUnityAdsReady(string placementId)
    {
        if (placementId == _adsType)
        {
#if UNITY_EDITOR
            Debug.Log("@ OnUnityAdsReady");
#endif
        }
    }
}
