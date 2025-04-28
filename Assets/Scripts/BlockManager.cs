using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    private BlockSpawner blockSpawner;

    // 配置済みのグリッドを管理
    private HashSet<Vector2Int> occupiedPositions = new();

    // 現在操作中のブロックグループ
    public BlockGroup currentBlockGroup;

    public int jamBlockCount = 0;
    public int jamBlockTriggerTurn = 0;
    public GameObject jamBlockPrefab; // 邪魔ブロのプレハブ

    public int turnCount = 0;

    private List<OjamaTicket> ojamaTickets = new();

    //カウントダウン
    public float baseTimeLimit = 30.0f;
    public float minTimeLimit = 10.0f;
    public float timeDecreasePerTurn = 0.2f;

    private float currentTimeLimit;
    private float countdownTime;

    public TMPro.TextMeshProUGUI countdownText; // ← TextMeshProのUI参照をInspectorで設定


    void Start()
    {
        blockSpawner = GetComponent<BlockSpawner>();
        currentTimeLimit = baseTimeLimit;
        countdownTime = currentTimeLimit;
        SpawnNewBlock(); // 最初のブロック生成
    }

    void Update()
    {
        if (currentBlockGroup == null) return; // 防御コード
        countdownTime -= Time.deltaTime;
        countdownTime = Mathf.Max(0, countdownTime);
        UpdateCountdownUI();

        if (countdownTime <= 0f)
        {
            // 時間切れ処理
            Debug.Log("時間切れ：ブロック消去 & お邪魔チケット追加");

            Destroy(currentBlockGroup.gameObject);
            currentBlockGroup = null; // これを追加！
            AddOjamaTicket(0, 3);
            CheckAndTriggerOjama();
            InGridManager.Instance.CheckGameOver();

            turnCount++;
            UpdateTimeLimit(); // ← 時間制限更新
            SpawnNewBlock();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E)) // エンターキーでブロック配置
        {
            if (CanPlace(currentBlockGroup))
            {
                PlaceBlock(currentBlockGroup);
                CheckAndTriggerOjama();
                InGridManager.Instance.CheckGameOver();
                turnCount++;
                Debug.Log("ターン数（配置後）: " + turnCount);
                UpdateTimeLimit();
                SpawnNewBlock();
            }
            else
            {
                Debug.Log("配置できません（すでにブロックがある）");
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
        BlockGroup group = blockSpawner.SpawnNowBlock(); // ← BlockSpawner側でBlockGroupを返すようにしてね
        currentBlockGroup = group;
        countdownTime = currentTimeLimit; // タイマーリセット
    }

    void SpawnJamBlocks(int count)
    {
        List<Vector2Int> emptyPositions = InGridManager.Instance.GetEmptyPositions();
        Debug.Log($"[JamBlock] 空きセル数: {emptyPositions.Count}");

        if (emptyPositions.Count == 0)
        {
            Debug.Log("[JamBlock] 空きセルがありません。");
            return;
        }

        for (int i = 0; i < count && emptyPositions.Count > 0; i++)
        {
            int randIndex = Random.Range(0, emptyPositions.Count);
            Vector2Int pos = emptyPositions[randIndex];
            emptyPositions.RemoveAt(randIndex);

            Debug.Log($"[JamBlock] 配置位置: {pos}");

            GameObject jamBlock = Instantiate(jamBlockPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            jamBlock.name = "JamBlock";

            BlockModel model = new BlockModel(pos, "", -1, jamBlock);
            model.isOjama = true; // ← これが重要！
            InGridManager.Instance.AddBlock(model);
            occupiedPositions.Add(pos);
        }

        Debug.Log($"[JamBlock] {count}個の邪魔ブロックを配置しました");
    }

    public void AddOjamaTicket(int afterTurns, int count)
    {
        int trigger = turnCount + afterTurns;
        ojamaTickets.Add(new OjamaTicket(trigger, count));
        Debug.Log($"[OjamaTicket] {trigger} ターン後に {count} 個の邪魔ブロ追加予定");
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

            // グリッド外 or スポーン位置（x:4~5, y:13~14）への配置は不可
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

            // block.transform から GameObject を取得
            GameObject blockObject = block.gameObject;

            // BlockModel の生成時に GameObject を渡す
            BlockModel model = new BlockModel(pos, block.character, block.blockId, blockObject);

            InGridManager.Instance.AddBlock(model);
            // 親子関係を解除（worldに直接ぶら下げる）
            block.transform.SetParent(null);
        }

        // BlockGroup自体を削除
        Destroy(group.gameObject);
        // emptyCellCountを再計算し、ログ出力
        InGridManager.Instance.RecalculateEmptyCellCount();
        Debug.Log("空きセル数: " + InGridManager.Instance.emptyCellCount);
        //turnCount++;
        //Debug.Log("ターン数（配置後）: " + turnCount);
        InGridManager.Instance.CheckGameOver();

    }

    // occupiedPositionsを外部から取得できるようにするプロパティ
    public HashSet<Vector2Int> OccupiedPositions
    {
        get { return occupiedPositions; }
    }
}
public class OjamaTicket
{
    public int triggerTurn; // 発動ターン
    public int count;       // 落とすお邪魔の数

    public OjamaTicket(int triggerTurn, int count)
    {
        this.triggerTurn = triggerTurn;
        this.count = count;
    }
}
