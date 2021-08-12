using System;
using System.Globalization;
using System.Net;
using UnityEngine;

public class TimeController
{
    private const string _webPath = "http://www.google.com";
    private Game_Data _gameData;
    private float _elapesd = 0.0f;
    private int _interval = 0;
    public void Init()
    {
        _gameData = DataController.Instance.GetGameData();
    }
    public void Update()
    {
        if (_gameData.horn < Const.MAX_HORN)
        {
            _elapesd += Time.unscaledDeltaTime;
            if (_elapesd >= _interval) ChargeHorn();
        }
    }
    public DateTime GetBaseTime()
    {
        using var req = WebRequest.Create(_webPath).GetResponse();
        DateTime dateTime = DateTime.ParseExact(req.Headers["date"], "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture.DateTimeFormat);
        return dateTime;
    }

    public void ChargeHorn()
    {
        while(_elapesd >= _interval)
        {
            _gameData.horn++;
            _gameData.horn_charging_time = _gameData.horn_charging_time.AddSeconds(Const.HORN_CHARGING_INTERVAL_SECONDS);

            _interval += Const.HORN_CHARGING_INTERVAL_SECONDS;
            if (_gameData.horn == Const.MAX_HORN) break;
        }

        DataController.Instance.SaveData();
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_update_horn);
        ResetTimer(false);
    }

    public void SaveHornTime()
    {
        DateTime nowBaseTime = GetBaseTime();

        _gameData.horn_charging_time = nowBaseTime;
        DataController.Instance.SaveData();

        ResetTimer(false);
    }

    public void ResetTimer(bool calculateInterval)
    {
        if (calculateInterval)
        {
            DateTime goal = _gameData.horn_charging_time.AddSeconds(Const.HORN_CHARGING_INTERVAL_SECONDS);
            DateTime now = GetBaseTime();
            if (goal > now) _interval = (int)(goal - now).TotalSeconds;
            else _interval = -(int)(now - goal).TotalSeconds;
        }
        else
        {
            _interval = Const.HORN_CHARGING_INTERVAL_SECONDS;
        }

        _elapesd = 0;
    }

    public float GetRemainingSeconds()
    {
        return _interval - _elapesd;
    }
}
