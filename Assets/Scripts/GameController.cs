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
        blockManager = FindObjectOfType<BlockManager>(); // BlockManager�̎Q�Ƃ��擾
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

                Debug.Log($"�`�F�C����: {chains.Count}");

                foreach (var group in chains)
                {
                    Debug.Log($"  �`�F�C���O���[�v: power={group.power}, �P�ꐔ={group.words.Count}");
                    totalPower += group.power;

                    foreach (var word in group.words)
                    {
                        Debug.Log($"    �P��: {string.Join(", ", word.blocks.Select(b => b.blockId))}");
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

                        Debug.Log($"    ���������P��: {wordStr}");
                    }
                }

                Debug.Log($"�����Ώۃu���b�N��: {blocksToRemove.Count}");

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

                Debug.Log($"�����Y���ŏ����邨�ז��u���b�N��: {ojamaCount}");

                foreach (var block in blocksToRemove)
                {
                    Debug.Log($"�폜: {block.character} at {block.gridPosition}");
                    InGridManager.Instance.RemoveBlock(block.gridPosition);
                    // ���������u���b�N�̈ʒu�����
                    blockManager.OccupiedPositions.Remove(block.gridPosition);  // �� �C��
                }

                int gainedScore = Mathf.RoundToInt(totalPower * 100);
                Debug.Log($"���p���[: {totalPower}");
                ScoreManager.Instance.AddScore(gainedScore);
                scoreUI.UpdateScoreText(ScoreManager.Instance.CurrentScore);
                Debug.Log($"�X�R�A���Z: {gainedScore} | ���v�X�R�A: {ScoreManager.Instance.CurrentScore}");

                //  �P�ꃊ�X�g�������i�Y�ꂸ�ɁI�j
                WordChecker.Instance.ClearAllWords();
                Debug.Log("�S�P�ꃊ�X�g�����������܂����B");
                // debugText���X�V
                StringBuilder sb = new StringBuilder();  // sb�̏�����
                if (WordChecker.Instance.debugText != null)  // WordChecker��debugText���Q��
                {
                    WordChecker.Instance.debugText.text = sb.ToString();  // sb�̓��e��\��
                }
            }
            else
            {
                Debug.Log("�����Ώۂ̃`�F�C���͑��݂��܂���B");
            }
        }
    }
}
