using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject cellPrefab; // �}�X�ڂ�\��Prefab�i�l�p�`��Sprite�Ȃǁj
    public int width = 10; // �Ֆʂ̉����i10�j
    public int height = 15; // �Ֆʂ̏c���i15�j
    public float cellSize = 1f; // �e�}�X�̃T�C�Y�i1x1�j

    void Start()
    {
        CreateGrid(); // �Ֆʂ��쐬
    }

    void CreateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0); // �e�}�X�̈ʒu���v�Z
                Instantiate(cellPrefab, position, Quaternion.identity, transform); // �Z���𐶐�
            }
        }
    }
}
