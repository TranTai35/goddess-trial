using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Định nghĩa 2 loại trang phép thuật
public enum SpellType { Utility, Attack }

public class SpellItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;
    public GameObject costUIGroup;
    public TextMeshProUGUI costText;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color equippedColor = new Color(0.5f, 1f, 0.5f);

    private int myIndex;
    private SpellType myType; // Lưu loại phép của nút này
    private MagicianNPC npcManager;

    // Cập nhật hàm Setup để nhận thêm loại SpellType
    public void Setup(int index, SpellType type, SpellLearnData data, MagicianNPC manager)
    {
        myIndex = index;
        myType = type;
        npcManager = manager;
        costText.text = data.diamondCost.ToString();
    }

    public void UpdateVisuals(bool isBought, bool isEquipped)
    {
        if (isEquipped)
        {
            backgroundImage.color = equippedColor;
            costUIGroup.SetActive(false);
        }
        else if (isBought)
        {
            backgroundImage.color = normalColor;
            costUIGroup.SetActive(false);
        }
        else
        {
            backgroundImage.color = normalColor;
            costUIGroup.SetActive(true);
        }
    }

    public void OnItemClicked()
    {
        // Truyền thêm loại phép qua cho NPC xử lý đúng danh sách
        npcManager.OnSpellButtonClicked(myIndex, myType);
    }
}