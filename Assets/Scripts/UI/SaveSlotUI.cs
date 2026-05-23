using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text slotLabel;
    [SerializeField] private TMP_Text timestampLabel;
    [SerializeField] private Button   loadButton;
    [SerializeField] private Button   deleteButton;

    private int                _slot;
    private LoadMenuController _controller;

    public void Setup(int slot, SaveData data, LoadMenuController controller)
    {
        _slot       = slot;
        _controller = controller;

        bool hasData = data != null;
        loadButton.interactable   = hasData;
        deleteButton.interactable = hasData;

        slotLabel.text      = $"Ranura {slot + 1}";
        timestampLabel.text = hasData ? data.timestamp : "— vacío —";
    }

    public void OnLoad()   => _controller.LoadSlot(_slot);
    public void OnDelete() => _controller.DeleteSlot(_slot);
}
