using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Block _blockPrefab;
    [SerializeField] private SpriteRenderer _boardPrefab;
    [SerializeField] private List<BlockType> _types;
    [SerializeField] private float _travelTime = 0.2f;
    [SerializeField] private int _winCondition = 2048;
    [SerializeField] GameObject gamePanel;

    [SerializeField] private GameObject _winScreen, _loseScreen;

    // If the touch is longer than MAX_SWIPE_TIME, we dont consider it a swipe
    public const float MAX_SWIPE_TIME = 0.5f;

    // Factor of the screen width that we consider a swipe
    // 0.17 works well for portrait mode 16:9 phone
    public const float MIN_SWIPE_DISTANCE = 0.17f;

    private List<Node> _nodes;
    private List<Block> _blocks;
    private GameState _state;
    private int _round;
    Vector2 startPos;
    float startTime;

    private BlockType GetBlockTypeByValue(int value) => _types.First(t => t.Value == value);

    // Start is called before the first frame update
    void Start()
    {
        ChangeState(GameState.GenerateLevel);
    }

    void Update()
    {
        if (_state != GameState.WaitingInput) return;

        if (Input.touches.Length > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                startPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);
                startTime = Time.time;
            }
            if (t.phase == TouchPhase.Ended)
            {
                if (Time.time - startTime > MAX_SWIPE_TIME) // Press too long
                    return;

                Vector2 endPos = new Vector2(t.position.x / (float)Screen.width, t.position.y / (float)Screen.width);

                Vector2 swipe = new Vector2(endPos.x - startPos.x, endPos.y - startPos.y);

                if (swipe.magnitude < MIN_SWIPE_DISTANCE) // Too short swipe
                    return;

                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                { // Horizontal swipe
                    if (swipe.x > 0)
                    {
                        Shift(Vector2.right);
                    }
                    else
                    {
                        Shift(Vector2.left);
                    }
                }
                else
                { // Vertical swipe
                    if (swipe.y > 0)
                    {
                        Shift(Vector2.up);
                    }
                    else
                    {
                        Shift(Vector2.down);
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) Shift(Vector2.left);
        if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Shift(Vector2.right);
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) Shift(Vector2.up);
        if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) Shift(Vector2.down);

    }

    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                SpawnBlocks(_round++ == 0 ? 2 : 1);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
                break;
            case GameState.Win:
                _winScreen.SetActive(true);
                break;
            case GameState.Lose:
                _loseScreen.SetActive(true);
                break;
        }
    }


    void GenerateGrid()
    {
        _round = 0;

        // Spawn grid
        _nodes = new List<Node>();
        _blocks = new List<Block>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity);
                _nodes.Add(node);
            }
        }

        // Center our board
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        
        board.size = new Vector2(_width, _height);

        // Center our camera
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

        ChangeState(GameState.SpawningBlocks);
    }

    void SpawnBlocks(int amount)
    {
        // Find all the grid squares available
        var freeNodes = _nodes.Where(n => n.OccupiedBlock == null).OrderBy(b => Random.value).ToList();

        // Spawn blocks as the amount
        foreach(var node in freeNodes.Take(amount))
        {
            SpawnBlock(node, Random.value > 0.8f ? 4 : 2);
        }

        if (freeNodes.Count() == 1)
        {
            ChangeState(GameState.Lose);
            return;
        }

        ChangeState(_blocks.Any(b => b.Value == _winCondition) ? GameState.Win : GameState.WaitingInput);
    }

    void SpawnBlock(Node node, int value)
    {
        var block = Instantiate(_blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeByValue(value));
        block.SetBlock(node);
        _blocks.Add(block);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //_winScreen.SetActive(false);
    }

    void Shift(Vector2 dir)
    {
        ChangeState(GameState.Moving);

        var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if(dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();
        
        foreach(var block in orderedBlocks)
        {
            var next = block.Node;  // Check itself first
            // Check every block at least once
            do
            {
                block.SetBlock(next);

                // Next block
                var posibleNode = GetNodeAtPosition(next.Pos + dir);
                if(posibleNode != null)
                {
                    // We know a node is present
                    // Change block into next block

                    // If so, this block can merge into this new block
                    if(posibleNode.OccupiedBlock != null && posibleNode.OccupiedBlock.CanMerge(block.Value))
                    {
                        block.MergeBlock(posibleNode.OccupiedBlock);
                    }
                    // Otherwise, can we move to this spot?
                    else if (posibleNode.OccupiedBlock == null) next = posibleNode;

                    // None hit? Just end loop
                }
            } while (next != block.Node);

            // Move block
            
        }

        var sequence = DOTween.Sequence();

        foreach(var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, _travelTime));
        }

        sequence.OnComplete(() =>
        {
            foreach(var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                MergeBlocks(block.MergingBlock, block);
            }
            ChangeState(GameState.SpawningBlocks);
        });

    }

    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlock(baseBlock.Node, baseBlock.Value * 2);
        Sound2048.MergeSound("merge");
        RemoveBlock(baseBlock);
        RemoveBlock(mergingBlock);
    }

    void RemoveBlock(Block block)
    {
        _blocks.Remove(block);
        Destroy(block.gameObject);
    }

    Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

}

[Serializable]
public struct BlockType
{
    public int Value;
    public Color Color;
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
