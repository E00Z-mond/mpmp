using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FXObj
{
    public FX_Table _info;
    public ParticleSystem _ps = null;
    public Transform _transform = null;
    public FXObj(FX_Table fxTable, ParticleSystem particleSystem, Transform transform)
    {
        _info = fxTable;
        _ps = particleSystem;
        _transform = transform;
    }
}
public class FXManager : Singleton<FXManager>
{
    private const string _fxPath = "Prefabs/FX/";

    private readonly Dictionary<FX_id, FXObj> _singleFXPool = new Dictionary<FX_id, FXObj>();
    private readonly Dictionary<FX_id, Stack<FXObj>> _multiFXPool = new Dictionary<FX_id, Stack<FXObj>>();

    private Transform _fxGroup = null;
    private Transform _touchFXgroup = null;
    private Transform _canvas = null;
    private Image _fade = null;
    private InputController _inputController = null;
    private Coroutine _fadeCoroutine = null;
    private Vector2 _touchEffectAnchor = Vector2.zero;
    public void Init()
    {
        GameObject fxGroup = new GameObject{ name = "FXGroup" };
        _fxGroup = fxGroup.transform;
        _fxGroup.localPosition = new Vector3(0, 0, 5);

        _inputController = Main.GameMain._inputController;
        GameObject touchFXGroup = new GameObject { name = "TouchFXGroup" };
        _touchFXgroup = touchFXGroup.transform;
        _touchFXgroup.localPosition = Vector3.zero;
        _touchFXgroup.localScale = Vector3.one;

        _canvas = UIManager.Instance.GetCanvas().transform;
        _touchFXgroup.SetParent(_canvas, false);
    }
    public void Release()
    {
        for (int i = 0; i < _touchFXgroup.childCount; i++)
        {
            Object.Destroy(_touchFXgroup.GetChild(i).gameObject);
        }

        for (int i = 0; i < _fxGroup.childCount; i++)
        {
            Object.Destroy(_fxGroup.GetChild(i).gameObject);
        }

        _multiFXPool.Clear();
        _singleFXPool.Clear();
    }

    public void PreLoadSingleFX(FX_id fx)
    {
        FX_Table table = (FX_Table)TableManager.Instance.GetTableValue(Table_resources.FX_Table, (int)fx);

        if (table.use_type == FX_use_type.FX_use_type_single)
        {
            if (_singleFXPool.ContainsKey(fx)) return;

            GameObject fxPrefab = Object.Instantiate(Resources.Load<GameObject>(_fxPath + fx.ToString()), _fxGroup, false);
            FXObj fxObj = new FXObj(table, fxPrefab.GetComponent<ParticleSystem>(), fxPrefab.transform);
            fxPrefab.SetActive(false);
            _singleFXPool.Add(fx, fxObj);
        }
    }

    public void MakeWorldFX(FX_id fx, Vector3 pos, int dir = 0)
    {
        FX_Table table = (FX_Table)TableManager.Instance.GetTableValue(Table_resources.FX_Table, (int)fx);

        FXObj fxObj = null;
        switch (table.use_type)
        {
            case FX_use_type.FX_use_type_single:
                if (_singleFXPool.ContainsKey(fx))
                {
                    fxObj = _singleFXPool[fx];
                    _singleFXPool.Remove(fx);
                }
                else
                {
                    GameObject fxPrefab = Object.Instantiate(Resources.Load<GameObject>(_fxPath + fx.ToString()), _fxGroup, false);
                    fxObj = new FXObj(table, fxPrefab.GetComponent<ParticleSystem>(), fxPrefab.transform);
                }
                break;
            case FX_use_type.FX_use_type_multi:
                if (_multiFXPool.ContainsKey(fx))
                {
                    Stack<FXObj> stack = _multiFXPool[fx];
                    fxObj = stack.Pop();
                    if (stack.Count == 0) _multiFXPool.Remove(fx);
                }
                else
                {
                    GameObject fxPrefab;
                    if (fx == FX_id.TouchEffect) fxPrefab = Object.Instantiate(Resources.Load<GameObject>(_fxPath + fx.ToString()), _touchFXgroup);
                    else fxPrefab = Object.Instantiate(Resources.Load<GameObject>(_fxPath + fx.ToString()), _fxGroup, false);
                    fxObj = new FXObj(table, fxPrefab.GetComponent<ParticleSystem>(), fxPrefab.transform);
                }
                break;
        }

        if (dir < 0)
        {
            Vector3 apply = Vector3.one;
            switch (fxObj._info.dir_type)
            {
                case FX_dir_type.FX_dir_type_invert_to_x:
                    apply = new Vector3(dir, 1, 1);
                    break;
                case FX_dir_type.FX_dir_type_invert_to_z:
                    apply = new Vector3(1, 1, dir);
                    break;
            }
            fxObj._transform.localScale = apply;
        }
        else
        {
            fxObj._transform.localScale = Vector3.one;
        }

        pos.z = 0;
        fxObj._transform.localPosition += pos;
        fxObj._transform.gameObject.SetActive(true);

        Main.GameMain._coroutineHandler.StartCoroutineCustom(ReturnFX(fxObj, pos));
    }

    private IEnumerator ReturnFX(FXObj fxObj, Vector3 pos)
    {
        yield return new WaitForSeconds(fxObj._ps.main.duration);

        if (fxObj._transform == null) yield break;

        fxObj._transform.gameObject.SetActive(false);
        fxObj._transform.localPosition -= pos;

        FX_Table table = fxObj._info;
        switch (table.use_type)
        {
            case FX_use_type.FX_use_type_single:
                if (_singleFXPool.ContainsKey(table.id))
                {
                    //싱글 FX가 다중 호출된 예외 상황
                    Object.Destroy(fxObj._transform.gameObject);
                }
                else
                {
                    _singleFXPool.Add(table.id, fxObj);
                }
                break;
            case FX_use_type.FX_use_type_multi:
                if (_multiFXPool.ContainsKey(table.id))
                {
                    Stack<FXObj> stack = _multiFXPool[table.id];
                    stack.Push(fxObj);
                }
                else
                {
                    Stack<FXObj> stack = new Stack<FXObj>();
                    stack.Push(fxObj);
                    _multiFXPool.Add(table.id, stack);
                }
                break;
        }
    }
    public void FadeIn()
    {
        if (_fadeCoroutine != null) Main.GameMain._coroutineHandler.StopCoroutineCustom(_fadeCoroutine);
        _fadeCoroutine = Main.GameMain._coroutineHandler.StartCoroutineCustom(FadeInFlow());
    }
    public IEnumerator FadeInFlow()
    {
        if(_fade == null)
        {
            GameObject fxObj = Object.Instantiate(Resources.Load<GameObject>(_fxPath + FX_id.Fade.ToString()), _canvas);
            _fade = fxObj.GetComponent<Image>();
        }

        _fade.color = new Color(0, 0, 0, 1);
        while (_fade.color.a > 0)
        {
            _fade.gameObject.transform.SetAsLastSibling();
            float a = _fade.color.a - Time.deltaTime * 2;
            _fade.color = new Color(0, 0, 0, a);
            yield return null;
        }

        _fadeCoroutine = null;
    }
    public void ResetFade()
    {
        if (_fadeCoroutine != null) Main.GameMain._coroutineHandler.StopCoroutineCustom(_fadeCoroutine);
        _fade.color = new Color(0, 0, 0, 0);
    }


    public void CheckTouchEffect()
    {
        if (_touchEffectAnchor == Vector2.zero)
        {
            Vector2 canSize = UIManager.Instance.GetCanvasSize();
            _touchEffectAnchor = new Vector2(canSize.x * 0.5f, canSize.y * 0.5f);
        }

        if (_inputController.IsInputDown)
        {
            Vector2 input = _inputController.InputPosition;
            Vector2 pos = UIManager.Instance.GetDynamicPositionByScreenPos(input);
            MakeWorldFX(FX_id.TouchEffect, pos - _touchEffectAnchor);
        }
    }
}
