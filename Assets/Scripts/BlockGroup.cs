using UnityEngine;
using System.Collections.Generic;

public class BlockGroup : MonoBehaviour
{
    public List<BlockController> blocks = new();
    private Vector2Int groupOffset = Vector2Int.zero;

    private float repeatDelay = 0.4f;    // 初回移動からリピートが始まるまでの時間
    private float repeatRate = 0.1f;     // リピート間隔
    private float nextMoveTime = 0f;

    private Vector2Int heldDirection = Vector2Int.zero;

    void Update()
    {
        Vector2Int inputDir = GetInputDirection();

        // 押し始め
        if (inputDir != Vector2Int.zero && heldDirection != inputDir)
        {
            heldDirection = inputDir;
            TryMove(heldDirection);
            nextMoveTime = Time.time + repeatDelay;
        }
        // 長押し
        else if (inputDir == heldDirection && inputDir != Vector2Int.zero && Time.time >= nextMoveTime)
        {
            TryMove(heldDirection);
            nextMoveTime = Time.time + repeatRate;
        }
        // キー離したとき
        else if (inputDir == Vector2Int.zero)
        {
            heldDirection = Vector2Int.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (blocks.Count == 1 && blocks[0].isWild)
                CycleWildCharacters(); // ワイルド切り替え
            else
                Rotate(); // 通常回転
        }
    }

    Vector2Int GetInputDirection()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) return Vector2Int.left;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) return Vector2Int.right;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) return Vector2Int.up;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) return Vector2Int.down;
        return Vector2Int.zero;
    }

    void CycleWildCharacters()
    {
        var wordDict = FindObjectOfType<BlockSpawner>().wordDictionary;
        var topic = wordDict.topics[FindObjectOfType<BlockSpawner>().currentTopicIndex];
        var availableChars = topic.availableCharacters;

        if (availableChars.Count == 0) return;

        foreach (var block in blocks)
        {
            if (!block.isWild) continue;

            block.wildIndex = (block.wildIndex + 1) % availableChars.Count;
            string nextChar = availableChars[block.wildIndex];
            block.SetCharacter(nextChar);
        }
    }

    void TryMove(Vector2Int dir)
    {
        foreach (var block in blocks)
        {
            Vector2Int newPos = block.gridPosition + dir;
            if (newPos.x < 0 || newPos.x > 7 || newPos.y < 0 || newPos.y > 15)
                return;
        }

        foreach (var block in blocks)
        {
            block.gridPosition += dir;
            block.transform.position = new Vector3(block.gridPosition.x, block.gridPosition.y, 0);
        }
    }

    void Rotate()
    {
        if (blocks.Count != 2) return;

        BlockController pivot = blocks[0];
        BlockController other = blocks[1];

        Vector2Int delta = other.gridPosition - pivot.gridPosition;
        Vector2Int rotatedDelta = new Vector2Int(-delta.y, delta.x);

        Vector2Int newPos = pivot.gridPosition + rotatedDelta;

        // グリッド範囲外なら回転キャンセル
        if (!IsInsideGrid(pivot.gridPosition) || !IsInsideGrid(newPos)) return;

        // 衝突するブロックがないか（必要ならここにチェックを追加）
        other.gridPosition = newPos;
        other.transform.position = new Vector3(newPos.x, newPos.y, 0);
    }

    bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < 8 && pos.y >= 0 && pos.y < 16;
    }

}
