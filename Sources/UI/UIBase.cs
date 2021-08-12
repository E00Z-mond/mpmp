using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public abstract void Init();
    public abstract void Release();
    public abstract void UpdateUI(UIEvent uiEvent, params object[] values);

}
