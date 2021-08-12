using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

struct BlockIndex
{
    public int row;
    public int col;
    public BlockIndex(int row, int col)
    {
        this.row = row;
        this.col = col;
    }
}

public delegate void PuzzlePopCallback(Character_type type, int skillLevel);
public class PuzzleBlockManager : MonoBehaviour
{
    public GameObject _puzzleBlock = null;
    public GameObject _bgForDim = null;
    private GameObject _dimBox = null;
    private InputController _inputController = null;
    private Transform _transform = null;

    private readonly PuzzleBlock[,] _puzzleBlocks = new PuzzleBlock[Const.MAX_ROW, Const.MAX_COL];
    private readonly List<BlockIndex> _matchPuzzleBlocks = new List<BlockIndex>();
    private readonly Queue<PuzzleBlock> _poppedPuzzleBlocks = new Queue<PuzzleBlock>();

    private Vector2 _startPosition = Vector2.zero;
    private Vector2 _endPosition = Vector2.zero;
    private Character_type _currentType = Character_type.Character_type_none;
    private bool _turnFlag = false;
    private bool _inProgress = false;

    public static event PuzzlePopCallback PuzzlePopEvent = null;
    public void Init()
    {
        _transform = transform;
        _inputController = Main.GameMain._inputController;
        CreatePuzzleBlocks();
    }

    public void Release()
    {
        _puzzleBlocks.Initialize();
        _matchPuzzleBlocks.Clear();
        _poppedPuzzleBlocks.Clear();
        _inputController = null;
    }
    private void CreatePuzzleBlocks()
    {
        for (int i = 0; i < Const.MAX_ROW; i++)
        {
            for (int j = 0; j < Const.MAX_COL; j++)
            {
                int random = Random.Range(0, Const.PUZZLE_BLOCK_TYPE_COUNT);

                GameObject blockObj = Instantiate(_puzzleBlock, _transform);
                PuzzleBlock blockScript = blockObj.GetComponent<PuzzleBlock>();
                blockScript.Init((Character_type)random);
                blockScript._transform.localPosition = new Vector2(j, i);

                _puzzleBlocks[i, j] = blockScript;
            }
        }

        _dimBox = Instantiate(_bgForDim, _transform);
        Transform dimTransform = _dimBox.transform;
        Vector3 dimPos = dimTransform.localPosition;
        dimPos.z = -1;
        _dimBox.transform.localPosition = dimPos;
    }
    void Update()
    {
        if (_turnFlag)
        {
            OnInputHandler();
        }
    }
    private void OnInputHandler()
    {
        if (_inputController.IsInputDown)
        {
            if (IsInsidePuzzle(_inputController.InputWorldPositionVec2) == false) return;
            _startPosition = InputPositionToPuzzlePosition(_inputController.InputPosition);
            _inProgress = true;
        }

        if (_inProgress)
        {
            if (IsInsidePuzzle(_inputController.InputWorldPositionVec2))
            {
                if (_inputController.IsInputHeld)
                {
                    _endPosition = InputPositionToPuzzlePosition(_inputController.InputPosition);
                    CheckMatchBlocks();
                }
                else if (_inputController.IsInputUp)
                {
                    StartCoroutine(PopPuzzle());
                    SetTurnFlag(false);
                    _inProgress = false;
                }
            }
            else
            {
                _inProgress = false;
                ReleaseFocus();
            }
        }
    }

    private Vector2 InputPositionToPuzzlePosition(Vector2 input)
    {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(input);
        Vector2 localPosition = _transform.InverseTransformPoint(worldPosition);
        localPosition = new Vector2(Mathf.Floor(localPosition.x), Mathf.Floor(localPosition.y));

        return localPosition;
    }

    private void CheckMatchBlocks()
    {
        ReleaseFocus();

        BlockIndex blockIndex = new BlockIndex((int)_startPosition.y, (int)_startPosition.x);
        _matchPuzzleBlocks.Add(blockIndex);

        _currentType = _puzzleBlocks[(int)_startPosition.y, (int)_startPosition.x].GetBlockType();

        int horizontalOperator = CalculateOperator(_startPosition.x, _endPosition.x);
        if (horizontalOperator != 0)
        {
            blockIndex = new BlockIndex((int)_startPosition.y, (int)_startPosition.x + horizontalOperator);
            CheckBlockType(blockIndex);
        }

        int verticalOperator = CalculateOperator(_startPosition.y, _endPosition.y);
        if (verticalOperator != 0)
        {
            blockIndex = new BlockIndex((int)_startPosition.y + verticalOperator, (int)_startPosition.x);
            CheckBlockType(blockIndex);
        }

        if (_matchPuzzleBlocks.Count == 3)
        {
            blockIndex = new BlockIndex((int)_startPosition.y + verticalOperator, (int)_startPosition.x + horizontalOperator);
            CheckBlockType(blockIndex);
        }

        if (_matchPuzzleBlocks.Count == 3)
        {
            _matchPuzzleBlocks.RemoveAt(_matchPuzzleBlocks.Count - 1);
        }

        FocusBlocks();
    }

    private int CalculateOperator(float start, float end)
    {
        int result = 0;

        if (start > end)
        {
            result = -1;
        }
        else if (start < end)
        {
            result = +1;
        }

        return result;
    }

    private void CheckBlockType(BlockIndex blockIndex)
    {
        PuzzleBlock block = _puzzleBlocks[blockIndex.row, blockIndex.col];

        if (_currentType == block.GetBlockType())
        {
            _matchPuzzleBlocks.Add(blockIndex);
        }
    }

    private void FocusBlocks()
    {
        BlockIndex blockIndex;
        PuzzleBlock block;
        for (int i = 0; i < _matchPuzzleBlocks.Count; i++)
        {
            blockIndex = _matchPuzzleBlocks[i];
            block = GetBlockByIndex(blockIndex);
            block.ChangeState(Block_state.Block_state_focus);
        }
    }

    private void PopMatchBlocks()
    {
        BlockIndex blockIndex;
        PuzzleBlock block;
        for (int i = 0; i < _matchPuzzleBlocks.Count; i++)
        {
            blockIndex = _matchPuzzleBlocks[i];
            block = GetBlockByIndex(blockIndex);
            block.ChangeState(Block_state.Block_state_pop);

            _puzzleBlocks[blockIndex.row, blockIndex.col] = null;
            _poppedPuzzleBlocks.Enqueue(block);
        }

        PuzzlePopEvent?.Invoke(_currentType, _matchPuzzleBlocks.Count);
        _matchPuzzleBlocks.Clear();
    }

    private void ReleaseFocus()
    {
        if (_matchPuzzleBlocks.Count == 0) return;
        BlockIndex blockIndex;
        PuzzleBlock block;
        for (int i = 0; i < _matchPuzzleBlocks.Count; i++)
        {
            blockIndex = _matchPuzzleBlocks[i];
            block = GetBlockByIndex(blockIndex);
            if (block != null)
            {
                block.ChangeState(Block_state.Block_state_idle);
            }
        }

        _matchPuzzleBlocks.Clear();
    }

    private bool IsInsidePuzzle(Vector2 position)
    {
        if (-5.0f < position.x && position.x < 5.0f)
        {
            if (-3.0f < position.y && position.y < -1.0f)
            {
                return true;
            }
        }
        return false;
    }

    private void ArrangeBlocks()
    {
        for (int i = 0; i < Const.MAX_ROW; i++)
        {
            // 좌측부터 최초로 빈 공간 탐색
            int col = 0;
            while (col < Const.MAX_COL)
            {
                if (_puzzleBlocks[i, col] == null) break;
                col++;
            }
            
            //빈 공간이 없는 경우
            if (col == Const.MAX_COL) continue;

            //빈 공간 기준 우측으로 유효한 블록 탐색
            int interval = 1;
            while (col + interval < Const.MAX_COL)
            {
                if (_puzzleBlocks[i, col + interval] != null) break;
                interval++;
            }

            //유효한 블록이 없는 경우, 정렬 필요 X
            if (col + interval == Const.MAX_COL) continue;

            //빈 공간으로 유효한 블록을 땡겨옴
            while(col + interval < Const.MAX_COL)
            {
                _puzzleBlocks[i, col] = _puzzleBlocks[i, col + interval];
                _puzzleBlocks[i, col + interval] = null;

                Vector2 move_position = new Vector2(col, i);
                _puzzleBlocks[i, col].Move(move_position);
                col++;
            }
        }
    }

    private PuzzleBlock GetBlockByIndex(BlockIndex index)
    {
        return _puzzleBlocks[index.row, index.col];
    }

    private void RefillBlocks()
    {
        for (int i = 0; i < Const.MAX_ROW; i++)
        {
            for (int j = 0; j < Const.MAX_COL; j++)
            {
                if (_puzzleBlocks[i, j] == null)
                {
                    int random = Random.Range(0, Const.PUZZLE_BLOCK_TYPE_COUNT);

                    PuzzleBlock block = _poppedPuzzleBlocks.Dequeue();
                    block.Init((Character_type)random);

                    _puzzleBlocks[i, j] = block;

                    block._transform.localPosition = new Vector2(j, i);
                    block.ChangeState(Block_state.Block_state_refill);
                }
            }
        }

        _poppedPuzzleBlocks.Clear();
    }

    private IEnumerator PopPuzzle()
    {
        PuzzleBlock block = GetBlockByIndex(_matchPuzzleBlocks[0]);

        PopMatchBlocks();
        ArrangeBlocks();

        yield return new WaitWhile(() => block.IsPlaying());

        RefillBlocks();

        yield return new WaitWhile(() => block.IsPlaying());
    }

    public void SetTurnFlag(bool flag)
    {
        if (_turnFlag == flag) return;
        _turnFlag = flag;
        _dimBox.SetActive(!_turnFlag);
    }

    public bool GetTurnFlag()
    {
        return _turnFlag;
    }
}
