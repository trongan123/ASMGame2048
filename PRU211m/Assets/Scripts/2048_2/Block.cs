using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;

    [SerializeField] private SpriteRenderer _render;
    [SerializeField] private TextMeshPro _text;
    public void Init(BlockType type)
    {
        Value = type.Value;
        _render.color = type.Color;
        _text.text = type.Value.ToString();
    }

    // Set block into next new block
    public void SetBlock(Node node)
    {
        if (Node != null) Node.OccupiedBlock = null;
        Node = node;
        Node.OccupiedBlock = this;
    }

    public void MergeBlock(Block blockToMergeWith)
    {
        // Set the block we are merging with
        MergingBlock = blockToMergeWith;

        // Set current node as unoccupied to allow other block to use its spot
        Node.OccupiedBlock = null;

        // Set the base block as mergin, so it does not get used twice
        blockToMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
