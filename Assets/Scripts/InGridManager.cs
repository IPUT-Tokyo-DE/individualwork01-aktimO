using System;
using System.Collections.Generic;
using UnityEngine;

public class InGridManager : MonoBehaviour
{
    public static InGridManager Instance { get; private set; }

    public const int GridWidth = 8;
    public const int GridHeight = 16;

    // 内部グリッド（nullならその位置にブロックなし）
    private BlockModel[,] internalGrid = new BlockModel[GridWidth, GridHeight];

    private readonly HashSet<Vector2Int> spawnPoints = new()
    {
        new Vector2Int(3, 15),
        new Vector2Int(4, 15),
        new Vector2Int(3, 14),
        new Vector2Int(4, 14),
    };

    public int emptyCellCount { get; private set; }


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        RecalculateEmptyCellCount();
    }

    // ブロックをグリッドに追加
    public void AddBlock(BlockModel model)
    {
        if (IsInBounds(model.gridPosition))
        {
            internalGrid[model.gridPosition.x, model.gridPosition.y] = model;

            // 空きマス数を減らす（スポーンポイント以外なら）
            if (!spawnPoints.Contains(model.gridPosition))
                emptyCellCount--;
        }
        WordChecker.Instance.CheckForWords(model);
    }

    public void RemoveBlock(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            var block = internalGrid[position.x, position.y];
            if (block != null && block.viewObject != null)
            {
                Destroy(block.viewObject);
            }

            // 空きマス数を増やす（スポーンポイント以外なら）
            if (!spawnPoints.Contains(position))
                emptyCellCount++;

            internalGrid[position.x, position.y] = null;
        }
    }

    public void RecalculateEmptyCellCount()
    {
        emptyCellCount = 0;
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector2Int pos = new(x, y);
                if (!spawnPoints.Contains(pos) && internalGrid[x, y] == null)
                {
                    emptyCellCount++;
                }
            }
        }
    }


    // 指定位置のブロック取得
    public BlockModel GetBlock(Vector2Int position)
    {
        if (IsInBounds(position))
        {
            return internalGrid[position.x, position.y];
        }
        return null;
    }

    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GridWidth && pos.y >= 0 && pos.y < GridHeight;
    }

    // 内部グリッドの状態をコンソールに表示
    public void DebugGrid()
    {
        string gridRepresentation = "";
        for (int y = GridHeight - 1; y >= 0; y--)  // y軸が上から下に増加するように変更
        {
            for (int x = 0; x < GridWidth; x++)
            {
                BlockModel block = internalGrid[x, y];
                if (block == null)
                {
                    gridRepresentation += "[ ] "; // 空のセル
                }
                else
                {
                    gridRepresentation += "[" + block.character + "] "; // ブロックの文字を表示
                }
            }
            gridRepresentation += "\n";
        }
        Debug.Log(gridRepresentation); // コンソールにグリッド状態を出力
    }

    public BlockModel GetBlockById(int id)
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                var block = internalGrid[x, y];
                if (block != null && block.blockId == id)
                {
                    return block;
                }
            }
        }
        return null;
    }


    public void CheckGameOver()
    {
        if (emptyCellCount == 0)
        {
            // ゲームオーバーの処理
            GameOver();
        }
    }

    public List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new();

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                Vector2Int pos = new(x, y);
                if (!spawnPoints.Contains(pos) && internalGrid[x, y] == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }


    private void GameOver()
    {
        Debug.Log("ゲームオーバー！空きセルがなくなりました。");
        GameOverManager.Instance.ShowGameOver();
        // ここでゲームオーバーのロジックを実装
        // 例えば、ゲーム終了画面を表示したり、ゲームをリセットしたりします。
        // ゲーム終了の処理は以下のように実装可能です:
        // Unityのシーンをリロードしたり、ゲーム終了のUIを表示したりすることができます。
        // UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }

}
