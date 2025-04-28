using UnityEngine;

public class BlockModel
{
    public Vector2Int gridPosition;
    public string character;
    public int blockId;
    public bool isOjama;
    public GameObject viewObject; // Å© í«â¡

    public BlockModel(Vector2Int gridPosition, string character, int blockId, GameObject viewObject)
    {
        this.gridPosition = gridPosition;
        this.character = character;
        this.blockId = blockId;
        this.viewObject = viewObject;
    }
}
