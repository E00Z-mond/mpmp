using UnityEngine;
public class Main : MonoBehaviour
{
    public static Main GameMain;
    public AudioSource _bgmSource;

    public TimeController _timeController = new TimeController();
    public InputController _inputController = new InputController();
    public CameraController _cameraManager;
    public CoroutineHandler _coroutineHandler;

    private readonly BattleState _battleState = new BattleState();
    private readonly LobbyState _lobbyState = new LobbyState();
    private GameStateBase _curState;

    private void Awake()
    {
        GameMain = this;

        TableManager.Instance.Init();
        UIManager.Instance.Init();
        FXManager.Instance.Init();
        DataController.Instance.Init();
        SoundManager.Instance.Init(_bgmSource);

        _timeController.Init();
        _cameraManager.Init();

        GameObject chObj = new GameObject { name = "Coroutine_Handler" };
        _coroutineHandler = chObj.AddComponent<CoroutineHandler>();

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        ChangeState(GameState.LobbyState);
    }

    public void ChangeState(GameState gameState, object value = null)
    {
        if(_curState != null)
        {
            if (_curState.GetGameState() == gameState) return;
            _curState.Release();
            FXManager.Instance.Release();
            SoundManager.Instance.Release();
            _coroutineHandler.StopAllCoroutinesCustom();
        }

        switch (gameState)
        {
            case GameState.LobbyState:
                _curState = _lobbyState;
                break;
            case GameState.BattleState:
                _curState = _battleState;
                break;
        }

        _curState.Init(value);
    }

    private void Update()
    {
        _timeController.Update();

        if (_curState == null) return;
        if (_curState.GetGameState() == GameState.LobbyState)
        {
            FXManager.Instance.CheckTouchEffect();
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            _timeController.ResetTimer(true);
        }
    }
}
