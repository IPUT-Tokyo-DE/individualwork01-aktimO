using UnityEngine;
using TMPro;

public class BlockController : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string character; // �������
    public int blockId;      // �u���b�N�ԍ�
    public TextMeshProUGUI textMesh; // ���O�ɃA�^�b�`���ꂽText
    public bool isWild = false;
    public int wildIndex = 0; // �����C���f�b�N�X��ǉ��i�K�v�ɉ����ď������j

    // ���v���r���[�p
    public bool isPreview = false;
    private Vector3 previewPosition; // �v���r���[�p�̕\���ʒu

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
        // �����͏�ɐ��ʌ����ɌŒ�
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

    // ���v���r���[��p ������
    public void SetPreviewPosition(Vector3 pos)
    {
        isPreview = true;
        previewPosition = pos;
    }
}
