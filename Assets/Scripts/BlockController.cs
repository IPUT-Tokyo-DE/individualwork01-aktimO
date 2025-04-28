using UnityEngine;
using TMPro;

public class BlockController : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string character; // 文字情報
    public int blockId;      // ブロック番号
    public TextMeshProUGUI textMesh; // 事前にアタッチされたText
    public bool isWild = false;
    public int wildIndex = 0; // 文字インデックスを追加（必要に応じて初期化）

    // ★プレビュー用
    public bool isPreview = false;
    private Vector3 previewPosition; // プレビュー用の表示位置

    void Start()
    {
        if (isPreview)
        {
            transform.position = previewPosition;
        }
        else
        {
            transform.position = new Vector3(gridPosition.x, gridPosition.y, 0);
        }

        if (textMesh == null)
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
        }

        UpdateText();
    }

    void UpdateText()
    {
        if (textMesh != null)
        {
            textMesh.text = character;
        }
    }

    void Update()
    {
        // 文字は常に正面向きに固定
        if (textMesh != null)
        {
            textMesh.transform.rotation = Quaternion.identity;
        }
    }

    public void SetCharacter(string c)
    {
        character = c;
        UpdateText();
    }

    // ★プレビュー専用 初期化
    public void SetPreviewPosition(Vector3 pos)
    {
        isPreview = true;
        previewPosition = pos;
    }
}
