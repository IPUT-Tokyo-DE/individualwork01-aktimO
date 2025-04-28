using System.Collections.Generic;
using UnityEngine;

class BlockPreviewData
{
    public bool isWild;
    public List<string> characters = new(); // 1個または2個
}
public class BlockSpawner : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject wildBlockPrefab;
    public GameObject blockGroupPrefab;
    public int spawnX = 3;
    public int spawnY = 15;
    public WordDictionary wordDictionary;
    public int currentTopicIndex = 0;
    private int blockIdCounter = 1;

    // BlockSpawnerに
    BlockPreviewData now;
    BlockPreviewData next1;
    BlockPreviewData next2;

    public Transform next1Position;
    public Transform next2Position;

    // ★ プレビュー用（本物とは完全に別物）
    List<GameObject> next1PreviewBlocks = new();
    List<GameObject> next2PreviewBlocks = new();

    void Start()
    {
        // 最初のセットアップ
        now = CreateRandomBlockPreview();
        next1 = CreateRandomBlockPreview();
        next2 = CreateRandomBlockPreview();

        ShowPreview(next1, next1PreviewBlocks, next1Position);
        ShowPreview(next2, next2PreviewBlocks, next2Position);
    }

    // ランダムに次のブロックを決める（データだけ作る）
    BlockPreviewData CreateRandomBlockPreview()
    {
        var data = new BlockPreviewData();
        int rand = Random.Range(0, 100);

        if (rand < 20)
        {
            data.isWild = true;
            data.characters.Add(GetWildCharacter());
        }
        else if (rand < 50)
        {
            data.isWild = false;
            data.characters.Add(GetRandomCharacter());
        }
        else
        {
            data.isWild = false;
            data.characters.Add(GetRandomCharacter());
            data.characters.Add(GetRandomCharacter());
        }
        return data;
    }

    // プレビューを表示する（空オブジェクトの位置を基準に並べる）
    void ShowPreview(BlockPreviewData data, List<GameObject> previewList, Transform basePosition)
    {
        // まず前のプレビューを消す
        foreach (var obj in previewList)
        {
            Destroy(obj);
        }
        previewList.Clear();

        for (int i = 0; i < data.characters.Count; i++)
        {
            GameObject prefab = data.isWild ? wildBlockPrefab : blockPrefab;
            GameObject block = Instantiate(prefab, Vector3.zero, Quaternion.identity); // ← ここでは位置は仮置き
            var controller = block.GetComponent<BlockController>();

            controller.isWild = data.isWild;
            controller.SetCharacter(data.characters[i]);
            controller.SetPreviewPosition(basePosition.position + new Vector3(i, 0, 0));
            controller.isPreview = true;

            previewList.Add(block);
        }
    }


    // 実際にフィールドにnowを出す
    public BlockGroup SpawnNowBlock()
    {
        // 先に now をずらす
        now = next1;
        next1 = next2;
        next2 = CreateRandomBlockPreview();

        ShowPreview(next1, next1PreviewBlocks, next1Position);
        ShowPreview(next2, next2PreviewBlocks, next2Position);

        // 本物を作る
        GameObject groupObj = new GameObject("BlockGroup");
        BlockGroup group = groupObj.AddComponent<BlockGroup>();

        int x = spawnX;
        int y = spawnY;

        for (int i = 0; i < now.characters.Count; i++)
        {
            GameObject prefab = now.isWild ? wildBlockPrefab : blockPrefab;
            GameObject block = Instantiate(prefab, new Vector3(x + i, y, 0), Quaternion.identity, groupObj.transform);
            var controller = block.GetComponent<BlockController>();
            controller.gridPosition = new Vector2Int(x + i, y);
            controller.blockId = blockIdCounter++;
            controller.character = now.characters[i];
            controller.isWild = now.isWild;
            group.blocks.Add(controller);
        }

        return group;
    }

    string GetWildCharacter()
    {
        var chars = wordDictionary.topics[currentTopicIndex].availableCharacters;
        return chars.Count > 0 ? chars[0] : "?";
    }

    string GetRandomCharacter()
    {
        if (wordDictionary != null && wordDictionary.topics.Count > currentTopicIndex)
        {
            var chars = wordDictionary.topics[currentTopicIndex].availableCharacters;
            if (chars.Count > 0)
            {
                int index = Random.Range(0, chars.Count);
                return chars[index];
            }
        }
        return "?";
    }
}
