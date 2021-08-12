using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuffPrinter : MonoBehaviour
{
    public GameObject _buffBoxOrigin;
    private readonly Dictionary<Buff_id, UIBuffBox> _buffBoxes = new Dictionary<Buff_id, UIBuffBox>();
    public void AddBuffBox(Buff_id id)
    {
        Buff_Table table = (Buff_Table)TableManager.Instance.GetTableValue(Table_resources.Buff_Table, (int)id);

        if (_buffBoxes.ContainsKey(id))
        {
            UpdateBuffTurns(id, table.continuous_turns);
            return;
        }

        GameObject obj = Instantiate(_buffBoxOrigin, transform);
        obj.SetActive(true);
        UIBuffBox uIBuffBox = obj.GetComponent<UIBuffBox>();

        uIBuffBox.Register(id, table.continuous_turns, table.icon_color, table.type);
        _buffBoxes.Add(id, uIBuffBox);

        Arrange();
    }

    public void RemoveBuffBox(Buff_id id)
    {
        if (!_buffBoxes.ContainsKey(id)) return;

        _buffBoxes[id].Destroy();
        _buffBoxes.Remove(id);

        Arrange();
    }

    private void Arrange()
    {
        if (_buffBoxes.Count == 0) return;

        int index = 0;
        using var arranger = _buffBoxes.GetEnumerator();
        while (arranger.MoveNext())
        {
            Vector2 pos = new Vector2(index % 3 * 50, index / 3 * -50);
            arranger.Current.Value.ChangePos(pos);
            index++;
        }
    }

    public void UpdateBuffTurns(Buff_id id, int turns)
    {
        if (!_buffBoxes.ContainsKey(id)) return;

        _buffBoxes[id].Changeturns(turns);
    }
}
