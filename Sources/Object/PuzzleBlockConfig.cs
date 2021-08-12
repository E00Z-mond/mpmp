using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/PuzzleBlockConfig", fileName = "PuzzleBlockConfig")]
public class PuzzleBlockConfig : ScriptableObject
{
    public Sprite[] puzzleBlockFrames;
    public Sprite[] puzzleBlockIcons;
}
