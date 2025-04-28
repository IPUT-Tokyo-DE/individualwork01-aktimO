using System;
using System.Collections.Generic;
using UnityEngine;

public class InGridManager : MonoBehaviour
{
    public static InGridManager Instance { get; private set; }

    public const int GridWidth = 8;
    public const int GridHeight = 16;

    // �����O���b�h�inull�Ȃ炻�̈ʒu�Ƀu���b�N�Ȃ��j
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

    // �u���b�N���O���b�h�ɒǉ�
    public void AddBlock(BlockModel model)
    {
        if (IsInBounds(model.gridPosition))
        {
            internalGrid[model.gridPosition.x, model.gridPosition.y] = model;

            // �󂫃}�X�������炷�i�X�|�[���|�C���g�ȊO�Ȃ�j
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

            // �󂫃}�X���𑝂₷�i�X�|�[���|�C���g�ȊO�Ȃ�j
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


    // �w��ʒu�̃u���b�N�擾
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

    // �����O���b�h�̏�Ԃ��R���\�[���ɕ\��
    public void DebugGrid()
    {
        string gridRepresentation = "";
        for (int y = GridHeight - 1; y >= 0; y--)  // y�����ォ�牺�ɑ�������悤�ɕύX
        {
            for (int x = 0; x < GridWidth; x++)
            {
                BlockModel block = internalGrid[x, y];
                if (block == null)
                {
                    gridRepresentation += "[ ] "; // ��̃Z��
                }
                else
                {
                    gridRepresentation += "[" + block.character + "] "; // �u���b�N�̕�����\��
                }
            }
            gridRepresentation += "\n";
        }
        Debug.Log(gridRepresentation); // �R���\�[���ɃO���b�h��Ԃ��o��
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
            // �Q�[���I�[�o�[�̏���
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
        Debug.Log("�Q�[���I�[�o�[�I�󂫃Z�����Ȃ��Ȃ�܂����B");
        GameOverManager.Instance.ShowGameOver();
        // �����ŃQ�[���I�[�o�[�̃��W�b�N������
        // �Ⴆ�΁A�Q�[���I����ʂ�\��������A�Q�[�������Z�b�g�����肵�܂��B
        // �Q�[���I���̏����͈ȉ��̂悤�Ɏ����\�ł�:
        // Unity�̃V�[���������[�h������A�Q�[���I����UI��\�������肷�邱�Ƃ��ł��܂��B
        // UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
    }

}
