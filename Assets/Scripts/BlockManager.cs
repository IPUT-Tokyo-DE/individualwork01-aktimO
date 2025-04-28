using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    private BlockSpawner blockSpawner;

    // �z�u�ς݂̃O���b�h���Ǘ�
    private HashSet<Vector2Int> occupiedPositions = new();

    // ���ݑ��쒆�̃u���b�N�O���[�v
    public BlockGroup currentBlockGroup;

    public int jamBlockCount = 0;
    public int jamBlockTriggerTurn = 0;
    public GameObject jamBlockPrefab; // �ז��u���̃v���n�u

    public int turnCount = 0;

    private List<OjamaTicket> ojamaTickets = new();

    //�J�E���g�_�E��
    public float baseTimeLimit = 30.0f;
    public float minTimeLimit = 10.0f;
    public float timeDecreasePerTurn = 0.2f;

    private float currentTimeLimit;
    private float countdownTime;

    public TMPro.TextMeshProUGUI countdownText; // �� TextMeshPro��UI�Q�Ƃ�Inspector�Őݒ�


    void Start()
    {
        blockSpawner = GetComponent<BlockSpawner>();
        currentTimeLimit = baseTimeLimit;
        countdownTime = currentTimeLimit;
        SpawnNewBlock(); // �ŏ��̃u���b�N����
    }

    void Update()
    {
        if (currentBlockGroup == null) return; // �h��R�[�h
        countdownTime -= Time.deltaTime;
        countdownTime = Mathf.Max(0, countdownTime);
        UpdateCountdownUI();

        if (countdownTime <= 0f)
        {
            // ���Ԑ؂ꏈ��
            Debug.Log("���Ԑ؂�F�u���b�N���� & ���ז��`�P�b�g�ǉ�");

            Destroy(currentBlockGroup.gameObject);
            currentBlockGroup = null; // �����ǉ��I
            AddOjamaTicket(0, 3);
            CheckAndTriggerOjama();
            InGridManager.Instance.CheckGameOver();

            turnCount++;
            UpdateTimeLimit(); // �� ���Ԑ����X�V
            SpawnNewBlock();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E)) // �G���^�[�L�[�Ńu���b�N�z�u
        {
            if (CanPlace(currentBlockGroup))
            {
                PlaceBlock(currentBlockGroup);
                CheckAndTriggerOjama();
                InGridManager.Instance.CheckGameOver();
                turnCount++;
                Debug.Log("�^�[�����i�z�u��j: " + turnCount);
                UpdateTimeLimit();
                SpawnNewBlock();
            }
            else
            {
                Debug.Log("�z�u�ł��܂���i���łɃu���b�N������j");
            }
        }
    }
    void UpdateTimeLimit()
    {
        currentTimeLimit = Mathf.Max(minTimeLimit, baseTimeLimit - turnCount * timeDecreasePerTurn);
        countdownTime = currentTimeLimit;
    }
    void UpdateCountdownUI()
    {
        if (countdownText != null)
        {
            countdownText.text = countdownTime.ToString("F1") + "s";
        }
    }
    void SpawnNewBlock()
    {
        BlockGroup group = blockSpawner.SpawnNowBlock(); // �� BlockSpawner����BlockGroup��Ԃ��悤�ɂ��Ă�
        currentBlockGroup = group;
        countdownTime = currentTimeLimit; // �^�C�}�[���Z�b�g
    }

    void SpawnJamBlocks(int count)
    {
        List<Vector2Int> emptyPositions = InGridManager.Instance.GetEmptyPositions();
        Debug.Log($"[JamBlock] �󂫃Z����: {emptyPositions.Count}");

        if (emptyPositions.Count == 0)
        {
            Debug.Log("[JamBlock] �󂫃Z��������܂���B");
            return;
        }

        for (int i = 0; i < count && emptyPositions.Count > 0; i++)
        {
            int randIndex = Random.Range(0, emptyPositions.Count);
            Vector2Int pos = emptyPositions[randIndex];
            emptyPositions.RemoveAt(randIndex);

            Debug.Log($"[JamBlock] �z�u�ʒu: {pos}");

            GameObject jamBlock = Instantiate(jamBlockPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            jamBlock.name = "JamBlock";

            BlockModel model = new BlockModel(pos, "", -1, jamBlock);
            model.isOjama = true; // �� ���ꂪ�d�v�I
            InGridManager.Instance.AddBlock(model);
            occupiedPositions.Add(pos);
        }

        Debug.Log($"[JamBlock] {count}�̎ז��u���b�N��z�u���܂���");
    }

    public void AddOjamaTicket(int afterTurns, int count)
    {
        int trigger = turnCount + afterTurns;
        ojamaTickets.Add(new OjamaTicket(trigger, count));
        Debug.Log($"[OjamaTicket] {trigger} �^�[����� {count} �̎ז��u���ǉ��\��");
    }

    void CheckAndTriggerOjama()
    {
        List<OjamaTicket> triggered = new();

        foreach (var ticket in ojamaTickets)
        {
            if (ticket.triggerTurn == turnCount)
            {
                SpawnJamBlocks(ticket.count);
                triggered.Add(ticket);
            }
        }

        foreach (var t in triggered)
        {
            ojamaTickets.Remove(t);
        }
    }


    bool CanPlace(BlockGroup group)
    {
        foreach (var block in group.blocks)
        {
            Vector2Int pos = block.gridPosition;

            // �O���b�h�O or �X�|�[���ʒu�ix:4~5, y:13~14�j�ւ̔z�u�͕s��
            if (occupiedPositions.Contains(pos) ||
                (pos.x >= 3 && pos.x <= 4 && pos.y >= 14 && pos.y <= 15))
            {
                return false;
            }
        }
        return true;
    }

    void PlaceBlock(BlockGroup group)
    {
        foreach (var block in group.blocks)
        {
            Vector2Int pos = block.gridPosition;
            occupiedPositions.Add(pos);

            // block.transform ���� GameObject ���擾
            GameObject blockObject = block.gameObject;

            // BlockModel �̐������� GameObject ��n��
            BlockModel model = new BlockModel(pos, block.character, block.blockId, blockObject);

            InGridManager.Instance.AddBlock(model);
            // �e�q�֌W�������iworld�ɒ��ڂԂ牺����j
            block.transform.SetParent(null);
        }

        // BlockGroup���̂��폜
        Destroy(group.gameObject);
        // emptyCellCount���Čv�Z���A���O�o��
        InGridManager.Instance.RecalculateEmptyCellCount();
        Debug.Log("�󂫃Z����: " + InGridManager.Instance.emptyCellCount);
        //turnCount++;
        //Debug.Log("�^�[�����i�z�u��j: " + turnCount);
        InGridManager.Instance.CheckGameOver();

    }

    // occupiedPositions���O������擾�ł���悤�ɂ���v���p�e�B
    public HashSet<Vector2Int> OccupiedPositions
    {
        get { return occupiedPositions; }
    }
}
public class OjamaTicket
{
    public int triggerTurn; // �����^�[��
    public int count;       // ���Ƃ����ז��̐�

    public OjamaTicket(int triggerTurn, int count)
    {
        this.triggerTurn = triggerTurn;
        this.count = count;
    }
}
