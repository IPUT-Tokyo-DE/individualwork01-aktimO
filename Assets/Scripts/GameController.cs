using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GameController : MonoBehaviour
{
    public ScoreUI scoreUI;
    private BlockManager blockManager;

    void Start()
    {
        blockManager = FindObjectOfType<BlockManager>(); // BlockManagerの参照を取得
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (WordChecker.Instance.HasActiveWordChains())
            {
                var chains = WordChecker.Instance.ConsumeWordChains();
                float totalPower = 0f;
                HashSet<BlockModel> blocksToRemove = new();
                HashSet<Vector2Int> positionsToCheckForOjama = new();

                Debug.Log($"チェイン数: {chains.Count}");

                foreach (var group in chains)
                {
                    Debug.Log($"  チェイングループ: power={group.power}, 単語数={group.words.Count}");
                    totalPower += group.power;

                    foreach (var word in group.words)
                    {
                        Debug.Log($"    単語: {string.Join(", ", word.blocks.Select(b => b.blockId))}");
                        string wordStr = "";

                        foreach (var info in word.blocks)
                        {
                            var model = InGridManager.Instance.GetBlockById(info.blockId);
                            if (model != null)
                            {
                                blocksToRemove.Add(model);
                                positionsToCheckForOjama.Add(model.gridPosition);
                                wordStr += model.character;
                            }
                        }

                        Debug.Log($"    完成した単語: {wordStr}");
                    }
                }

                Debug.Log($"消去対象ブロック数: {blocksToRemove.Count}");

                Vector2Int[] directions = new Vector2Int[]
                {
                    Vector2Int.up,
                    Vector2Int.down,
                    Vector2Int.left,
                    Vector2Int.right
                };

                int ojamaCount = 0;

                foreach (var pos in positionsToCheckForOjama)
                {
                    foreach (var dir in directions)
                    {
                        Vector2Int neighborPos = pos + dir;
                        var neighborBlock = InGridManager.Instance.GetBlock(neighborPos);

                        if (neighborBlock != null && neighborBlock.isOjama)
                        {
                            if (blocksToRemove.Add(neighborBlock))
                                ojamaCount++;
                        }
                    }
                }

                Debug.Log($"巻き添えで消えるお邪魔ブロック数: {ojamaCount}");

                foreach (var block in blocksToRemove)
                {
                    Debug.Log($"削除: {block.character} at {block.gridPosition}");
                    InGridManager.Instance.RemoveBlock(block.gridPosition);
                    // 消去したブロックの位置を解放
                    blockManager.OccupiedPositions.Remove(block.gridPosition);  // ← 修正
                }

                int gainedScore = Mathf.RoundToInt(totalPower * 100);
                Debug.Log($"総パワー: {totalPower}");
                ScoreManager.Instance.AddScore(gainedScore);
                scoreUI.UpdateScoreText(ScoreManager.Instance.CurrentScore);
                Debug.Log($"スコア加算: {gainedScore} | 合計スコア: {ScoreManager.Instance.CurrentScore}");

                //  単語リスト初期化（忘れずに！）
                WordChecker.Instance.ClearAllWords();
                Debug.Log("全単語リストを初期化しました。");
                // debugTextを更新
                StringBuilder sb = new StringBuilder();  // sbの初期化
                if (WordChecker.Instance.debugText != null)  // WordCheckerのdebugTextを参照
                {
                    WordChecker.Instance.debugText.text = sb.ToString();  // sbの内容を表示
                }
            }
            else
            {
                Debug.Log("消去対象のチェインは存在しません。");
            }
        }
    }
}
