using System.Collections;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    public Coroutine StartCoroutineCustom(IEnumerator enumerator)
    {
        return StartCoroutine(enumerator);
    }

    public void StopCoroutineCustom(Coroutine enumerator)
    {
        StopCoroutine(enumerator);
    }

    public void StopAllCoroutinesCustom()
    {
        StopAllCoroutines();
    }
}
