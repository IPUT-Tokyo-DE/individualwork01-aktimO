using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab; // マス目を表すPrefab（四角形のSpriteなど）
    public int width = 10; // 盤面の横幅（10）
    public int height = 15; // 盤面の縦幅（15）
    public float cellSize = 1f; // 各マスのサイズ（1x1）

    void Start()
    {
        CreateGrid(); // 盤面を作成
    }

    void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0); // 各マスの位置を計算
                Instantiate(cellPrefab, position, Quaternion.identity, transform); // セルを生成
            }
        }
    }
}
