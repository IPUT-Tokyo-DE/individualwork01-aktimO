using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Text;
public class WordChecker : MonoBehaviour
{
    public static WordChecker Instance { get; private set; }
    private int nextWordId = 0;
    private List<WordMatch> allWords = new List<WordMatch>();
    public TextMeshProUGUI debugText; // Inspectorから設定する

    public List<WordGroupData> ConsumeWordChains()
    {
        List<WordGroupData> result = new();

        // スコア計算済みのグループを再利用
        result.AddRange(lastScoredGroups);

        // ログ出力を追加して内容を確認
        Debug.Log($"ConsumeWordChains called. lastScoredGroups count: {lastScoredGroups.Count}");
        for (int i = 0; i < lastScoredGroups.Count; i++)
        {
            var group = lastScoredGroups[i];
            string wordList = string.Join(", ", group.words.Select(w => w.word));
            Debug.Log($"  グループ {i}: words = {wordList}, power = {group.power}");
        }

        // 必ず処理後にクリア
        lastScoredGroups.Clear();
        allWords.Clear(); // 一度使った単語は消す（リセット）

        return result;
    }


    private List<WordGroupData> lastScoredGroups = new();

    public bool HasActiveWordChains()
    {
        return lastScoredGroups != null && lastScoredGroups.Count > 0;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CheckForWords(BlockModel addedBlock)
    {

        Vector2Int pos = addedBlock.gridPosition;

        List<BlockModel> horizontalRange = GetConnectedBlocksInDirection(pos, Vector2Int.right);
        List<BlockModel> verticalRange = GetConnectedBlocksInDirection(pos, Vector2Int.down);

        if (horizontalRange.Count >= 3)
        {
            CheckWordCombinations(horizontalRange, Vector2Int.right);
        }

        if (verticalRange.Count >= 3)
        {
            CheckWordCombinations(verticalRange, Vector2Int.down);
        }
    }

    private List<BlockModel> GetConnectedBlocksInDirection(Vector2Int startPos, Vector2Int direction)
    {
        List<BlockModel> connectedBlocks = new List<BlockModel>();
        Vector2Int currentPos = startPos;

        while (InGridManager.Instance.IsInBounds(currentPos))
        {
            BlockModel block = InGridManager.Instance.GetBlock(currentPos);
            if (block != null)
            {
                connectedBlocks.Add(block);
                currentPos += direction;
            }
            else break;
        }

        currentPos = startPos - direction;
        while (InGridManager.Instance.IsInBounds(currentPos))
        {
            BlockModel block = InGridManager.Instance.GetBlock(currentPos);
            if (block != null)
            {
                connectedBlocks.Insert(0, block);
                currentPos -= direction;
            }
            else break;
        }

        return connectedBlocks;
    }

    private void CheckWordCombinations(List<BlockModel> blocks, Vector2Int direction)
    {
        List<WordMatch> foundWords = new List<WordMatch>();

        for (int i = 0; i < blocks.Count; i++)
        {
            for (int j = i + 2; j < blocks.Count; j++)
            {
                string word = string.Empty;
                List<BlockInfo> wordBlocks = new List<BlockInfo>();

                for (int k = i; k <= j; k++)
                {
                    word += blocks[k].character;
                    wordBlocks.Add(new BlockInfo
                    {
                        character = blocks[k].character,
                        gridPosition = blocks[k].gridPosition,
                        blockId = blocks[k].blockId
                    });
                }

                if (IsValidWord(word))
                {
                    WordMatch newMatch = new WordMatch
                    {
                        id = nextWordId++,
                        word = word,
                        blocks = new List<BlockInfo>(wordBlocks),
                        direction = direction
                    };

                    foundWords.Add(newMatch);
                    Debug.Log($"一致する単語: {word}");
                }
            }
        }

        // ↓ ここから全体管理用リストに登録し、マージ処理を行う

        foreach (var newWord in foundWords)
        {
            List<WordMatch> mergedSources = new List<WordMatch>();

            foreach (var existing in allWords)
            {
                if (existing.direction == newWord.direction && AreWordsConnected(existing.blocks, newWord.blocks, newWord.direction))
                {
                    mergedSources.Add(existing);
                }
            }

            if (mergedSources.Count > 0)
            {
                WordMatch merged = new WordMatch
                {
                    blocks = new List<BlockInfo>(newWord.blocks),
                    direction = newWord.direction
                };

                foreach (var source in mergedSources)
                {
                    foreach (var b in source.blocks)
                    {
                        if (!merged.ContainsBlock(b.gridPosition))
                            merged.blocks.Add(b);
                    }
                }

                // 並び順を揃えて文字列構築
                merged.blocks.Sort((a, b) =>
                    direction == Vector2Int.right ?
                        a.gridPosition.x.CompareTo(b.gridPosition.x) :
                        b.gridPosition.y.CompareTo(a.gridPosition.y));

                string combinedWord = string.Empty;
                foreach (var b in merged.blocks)
                {
                    combinedWord += b.character;
                }

                merged.word = combinedWord;

                allWords.RemoveAll(w => mergedSources.Contains(w));
                allWords.Add(merged);

                Debug.Log($"マージ後の単語: {merged.word}");
            }
            else
            {
                allWords.Add(newWord);
            }
        }

        // デバッグ表示：すべての現在の単語
        foreach (var match in allWords)
        {
            string blockPositions = string.Join(", ", match.blocks.ConvertAll(b => $"[{b.character}({b.gridPosition.x},{b.gridPosition.y})]"));
            Debug.Log($"登録単語: {match.word} | ブロック: {blockPositions}");
        }
        // 単語同士のすべての交差を検出
        Debug.Log("=== 単語交差判定 ===");
        Dictionary<WordMatch, List<WordMatch>> wordCrossMap = new();

        for (int i = 0; i < allWords.Count; i++)
        {
            for (int j = 0; j < allWords.Count; j++)
            {
                if (i == j) continue;

                WordMatch w1 = allWords[i];
                WordMatch w2 = allWords[j];

                if (w1.blocks.Any(b1 => w2.blocks.Any(b2 => b1.blockId == b2.blockId)))
                {
                    if (!wordCrossMap.ContainsKey(w1))
                        wordCrossMap[w1] = new List<WordMatch>();

                    if (!wordCrossMap[w1].Contains(w2))
                        wordCrossMap[w1].Add(w2);
                }
            }
        }

        // DFS でグループ化
        List<List<WordMatch>> wordGroups = new();
        HashSet<WordMatch> visited = new();

        foreach (var word in allWords)
        {
            if (visited.Contains(word)) continue;

            List<WordMatch> group = new();
            Stack<WordMatch> stack = new();
            stack.Push(word);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current)) continue;

                visited.Add(current);
                group.Add(current);

                if (wordCrossMap.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                            stack.Push(neighbor);
                    }
                }
            }

            wordGroups.Add(group);
        }

        // 孤立単語の処理（交差なし）
        foreach (var word in allWords)
        {
            if (!visited.Contains(word))
            {
                wordGroups.Add(new List<WordMatch> { word });
            }
        }
        // 同じ単語が複数あってもすべて保持できるようにリスト化
        Dictionary<string, List<WordMatch>> wordMap = new();

        foreach (var w in allWords)
        {
            if (!wordMap.ContainsKey(w.word))
                wordMap[w.word] = new List<WordMatch>();

            wordMap[w.word].Add(w);
        }

        List<WordGroupData> scoredGroups = new();

        foreach (var group in wordGroups)
        {
            WordGroupData data = new();
            foreach (var wordMatch in group)
            {
                data.words.Add(wordMatch);
                data.totalChars += wordMatch.word.Length;
            }

            int chainCount = data.words.Count;
            float baseScore = data.totalChars;
            float bonus = data.totalChars * (chainCount - 1) * 0.2f;

            // グランドクロス
            bool hasGrandCross = data.words.Count(w => w.word.Length >= 6) >= 2;
            data.isGrandCross = hasGrandCross;
            float grandCrossBonus = hasGrandCross ? 5f : 0f;

            bool HasCycle(Dictionary<WordMatch, HashSet<WordMatch>> graph)
            {
                var visited = new HashSet<WordMatch>();

                bool Dfs(WordMatch current, WordMatch parent)
                {
                    visited.Add(current);
                    foreach (var neighbor in graph[current])
                    {
                        if (!visited.Contains(neighbor))
                        {
                            if (Dfs(neighbor, current)) return true;
                        }
                        else if (neighbor != parent)
                        {
                            // 訪問済みで親ではない → サイクル発見
                            return true;
                        }
                    }
                    return false;
                }

                foreach (var node in graph.Keys)
                {
                    if (!visited.Contains(node))
                    {
                        if (Dfs(node, null)) return true;
                    }
                }
                return false;
            }

            // メビウス判定（インスタンスベース）
            bool hasMobius = false;
            Dictionary<WordMatch, HashSet<WordMatch>> crossings = new();

            foreach (var w in data.words)
                crossings[w] = new HashSet<WordMatch>();

            for (int i = 0; i < data.words.Count; i++)
            {
                for (int j = 0; j < data.words.Count; j++)
                {
                    if (i == j) continue;
                    var w1 = data.words[i];
                    var w2 = data.words[j];
                    if (w1.blocks.Any(b1 => w2.blocks.Any(b2 => b1.blockId == b2.blockId)))
                    {
                        crossings[w1].Add(w2);
                        crossings[w2].Add(w1); // 無向グラフにするため両方向
                    }
                }
            }

            hasMobius = HasCycle(crossings); // ← ここでさっきのDFS関数を呼ぶ

            float mobiusBonusMultiplier = hasMobius ? 1.5f : 1f;
            data.isMobius = hasMobius;

            data.power = (baseScore + bonus + grandCrossBonus) * mobiusBonusMultiplier;
            scoredGroups.Add(data);
        }
        lastScoredGroups = scoredGroups;


            Debug.Log(" パワー付き単語連鎖グループ ");
        for (int i = 0; i < scoredGroups.Count; i++)
        {
            var g = scoredGroups[i];
            string wordList = string.Join(", ", g.words.Select(w => w.word));
            string tag = (g.isMobius ? "[Möbius] " : "") + (g.isGrandCross ? "[GrandCross] " : "");
            Debug.Log($"グループ {i}: {tag}{wordList} | 文字数: {g.totalChars} | 連鎖数: {g.words.Count} | パワー: {g.power}");
        }
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < scoredGroups.Count; i++)
        {
            var g = scoredGroups[i];
            string wordList = string.Join(", ", g.words.Select(w => w.word));
            // タグ生成（両方対応）
            string tag = "";
            if (g.isGrandCross && g.isMobius)
                tag = "【グランドクロス＆メビウス】";
            else if (g.isGrandCross)
                tag = "【グランドクロス】";
            else if (g.isMobius)
                tag = "【メビウス】";

            // タグをメインメッセージの中に埋め込む
            sb.AppendLine($"グループ {i} {tag}: {wordList} | 文字数: {g.totalChars} | 連鎖数: {g.words.Count} | パワー: {g.power}");
        }
        // 表示用に設定
        if (debugText != null)
        {
            debugText.text = sb.ToString();
        }
        //lastScoredGroups.Clear(); ここのせいでリストが消えていた
        //lastScoredGroups.AddRange(scoredGroups);
    }


    private bool AreWordsConnected(List<BlockInfo> existing, List<BlockInfo> candidate, Vector2Int direction)
    {
        foreach (var b1 in existing)
        {
            foreach (var b2 in candidate)
            {
                if (b1.gridPosition == b2.gridPosition)
                {
                    return true;
                }
            }
        }

        foreach (var b1 in existing)
        {
            foreach (var b2 in candidate)
            {
                if (IsAdjacentInDirection(b1.gridPosition, b2.gridPosition, direction))
                    return true;
            }
        }

        return false;
    }

    private bool IsAdjacentInDirection(Vector2Int a, Vector2Int b, Vector2Int dir)
    {
        return a + dir == b || b + dir == a;
    }

    private bool IsValidWord(string word)
    {
        foreach (TopicEntry topic in WordDictionary.Instance.topics)
        {
            if (topic.wordList.Contains(word))
                return true;
        }
        return false;
    }
    public void ClearAllWords()
    {
        allWords.Clear();
        lastScoredGroups.Clear();
    }

}

public class BlockInfo
{
    public string character;
    public Vector2Int gridPosition;
    public int blockId;
}

public class WordMatch
{
    public string word;
    public int id; // <- これを追加
    public List<BlockInfo> blocks;
    public Vector2Int direction;
    public int Length => word.Length;

    public bool ContainsBlock(Vector2Int pos)
    {
        foreach (var b in blocks)
        {
            if (b.gridPosition == pos)
                return true;
        }
        return false;
    }
}
public class WordGroupData
{
    public List<WordMatch> words = new();
    public int totalChars;
    public float power;
    public bool isMobius; // 仮置き
    public bool isGrandCross; // 仮置き
}
