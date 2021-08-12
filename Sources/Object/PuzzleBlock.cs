using System.Collections;
using UnityEngine;

public class PuzzleBlock : MonoBehaviour
{
    public Transform _transform = null;
    public SpriteRenderer _frameSprite = null;
    public SpriteRenderer _iconSprite = null;
    public Animator _anim = null;
    public PuzzleBlockConfig _puzzleBlockConfig = null;

    private Character_type _type = Character_type.Character_type_none;
    private Block_state _current_state = Block_state.Block_state_idle;
    private float _duration = 0.0f; 
    private bool _IsPlaying = false;
    public void Init(Character_type type)
    {
        _type = type;
        _transform = transform;
        _frameSprite.sprite = _puzzleBlockConfig.puzzleBlockFrames[(int)_type];
        _iconSprite.sprite = _puzzleBlockConfig.puzzleBlockIcons[(int)_type];
    }

    public Character_type GetBlockType()
    {
        return _type;
    }

    public void ChangeState(Block_state state)
    {
        if (_current_state == state) return;
        if (IsPlaying() == true) return;

        _current_state = state;
        _anim.SetInteger("block_state", (int)_current_state);

        if (_current_state != Block_state.Block_state_idle && _current_state != Block_state.Block_state_focus)
        {
            _IsPlaying = true;
            StartCoroutine(AnimPlayingCheck());
        }
    }

    private IEnumerator AnimPlayingCheck()
    {
        yield return new WaitForEndOfFrame();

        _duration = _anim.GetCurrentAnimatorStateInfo(0).length;

        float elapsed = 0.0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _IsPlaying = false;
        ChangeState(Block_state.Block_state_idle);
    }

    public bool IsPlaying()
    {
        if (_IsPlaying)
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
        StartCoroutine(MoveCoroutine(destination));
    }

    private IEnumerator MoveCoroutine(Vector2 destination)
    {
        while (true)
        {
            Vector2 current_position = _transform.localPosition;

            if (Vector2.Distance(current_position, destination) < 0.01f)
            {
                break;
            }

            _transform.localPosition = Vector2.MoveTowards(current_position, destination, Const.DEFAULT_SPEED * Time.deltaTime);
            yield return null;
        }
    }
}
