using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Character : MonoBehaviour
{
    private Character_state _currentState = Character_state.Character_state_none;
    private Animator _anim = null;
    public Transform _transform = null;
    private Coroutine _moveCoroutine = null;
    private Coroutine _aniCoroutine = null;

    private bool _isPlaying = false;
    private bool _isMoving = false;
    private static readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
    public void Init()
    {
        _transform = transform;
        _anim = GetComponentInChildren<Animator>();
    }

    public void ChangeState(Character_state state)
    {
        if (_currentState == state) return;
        if (IsPlaying() == true)
        {
            StopCoroutine(_aniCoroutine);
            _isPlaying = false;
        }

        _currentState = state;
        _anim.SetInteger("character_state", (int)_currentState);

        if (_currentState != Character_state.Character_state_idle && _currentState != Character_state.Character_state_move)
        {
            _isPlaying = true;
            _aniCoroutine = StartCoroutine(AnimPlayingCheck());
        }
    }

    private IEnumerator AnimPlayingCheck()
    {
        yield return _waitForEndOfFrame;

        var state = _anim.GetCurrentAnimatorStateInfo(0);

        float _duration = state.length;
        float elapsed = 0.0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _isPlaying = false;
        if(_currentState != Character_state.Character_state_die)
        {
            ChangeState(Character_state.Character_state_idle);
        }
    }

    public bool IsPlaying()
    {
        if (_isPlaying)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Move(Vector2 destination)
    {
        if (_isMoving) StopMoving();
        _isMoving = true;
        ChangeState(Character_state.Character_state_move);
        _moveCoroutine = StartCoroutine(MoveCoroutine(destination));
    }

    public void StopMoving()
    {
        if(_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        ChangeState(Character_state.Character_state_idle);
        _transform.localScale = Vector3.one;
        _isMoving = false;
        _moveCoroutine = null;
    }

    public void ResetPos()
    {
        StopMoving();
        _transform.localPosition = Vector3.zero;
    }

    private IEnumerator MoveCoroutine(Vector2 destination)
    {
        float dir = destination.x - _transform.localPosition.x;

        if(dir > 0)
        {
            Vector3 scale = _transform.localScale;
            _transform.localScale = new Vector3(scale.x * -1, scale.y, scale.z);
        }

        while (true)
        {
            Vector2 current_position = _transform.localPosition;

            if (Vector2.Distance(current_position, destination) < 0.01f)
            {
                StopMoving();
            }

            _transform.localPosition = Vector2.MoveTowards(current_position, destination, Const.DEFAULT_SPEED * Time.deltaTime);
            yield return null;
        }
    }

    public bool IsMoving()
    {
        if (_isMoving)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
