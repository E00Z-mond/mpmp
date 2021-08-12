using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
public class DataController: Singleton<DataController>
{
    private Game_Data _gameData = null;
    private string _path = string.Empty;
    private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();
    public void Init()
    {
        _path = Application.persistentDataPath + "/gameData.mp";
        LoadData();
    }
    public void SaveData()
    {
        try
        {
            using FileStream file = File.Create(_path);
            _binaryFormatter.Serialize(file, _gameData);
        }
        catch (Exception)
        {
            UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Failed to save data.\nPlease check the storage space.");
            UIPopup._closeEvent += Main.GameMain.QuitGame;
            return;
        }

#if UNITY_EDITOR
        Debug.Log("데이터 저장 완료");
#endif
    }
    public void LoadData()
    {
        if (File.Exists(_path))
        {
            using FileStream file = File.Open(_path, FileMode.Open);
            _gameData = (Game_Data)_binaryFormatter.Deserialize(file);
        }
        else
        {
            //최초 데이터 설정
            Dictionary<Character_id, Character_Data> characterDatas = new Dictionary<Character_id, Character_Data>
            {
                { Character_id.Character_dan, new Character_Data(Character_id.Character_dan, 1, true) },
                { Character_id.Character_siroo, new Character_Data(Character_id.Character_siroo, 1, true) },
                { Character_id.Character_lorena, new Character_Data(Character_id.Character_lorena, 1, true) }
            };

            Dictionary<int, Stage_Data> stageDatas = new Dictionary<int, Stage_Data>
            {
                { 1, new Stage_Data(1, 0) }
            };

            Game_Data game_Data = new Game_Data(200, 0, 5, characterDatas, stageDatas);
            _gameData = game_Data;

#if UNITY_EDITOR
            Debug.Log("최초 데이터 생성");
#endif
        }
    }
    public void ResetData()
    {
        File.Delete(_path);
        LoadData();
        UIManager.Instance.UpdateUI(UIEvent.UIEvent_show_popup, "Reset Data.\nPlease Restart Game.");
        UIPopup._closeEvent += Main.GameMain.QuitGame;
    }
    public Game_Data GetGameData()
    {
        return _gameData;
    }
}
