using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public SpriteRenderer _skyBox;
    private Coroutine _curCoroutine = null;
    private Transform _transform = null;
    public void Init()
    {
        _transform = transform;
    }
    public void Move(Vector2 destination)
    {
        if (_curCoroutine != null) StopCoroutine(_curCoroutine);
        _curCoroutine = StartCoroutine(MoveToTargetCoroutine(destination));
    }

    private IEnumerator MoveToTargetCoroutine(Vector2 destination)
    {
        while (true)
        {
            Vector2 currentPosition = _transform.localPosition;

            if (Vector3.Distance(currentPosition, destination) < 0.01f)
            {
                _curCoroutine = null;
                yield break;
            }

            _transform.localPosition = Vector3.MoveTowards(currentPosition, destination, Const.DEFAULT_SPEED * Time.deltaTime);
            yield return null;
        }
    }

    public void ResetCameraPos()
    {
        if (_curCoroutine != null) StopCoroutine(_curCoroutine);
        _transform.localPosition = Vector2.zero;
    }

    public void ChangeSkyColor(Color color)
    {
        _skyBox.color = color;
    }
}
