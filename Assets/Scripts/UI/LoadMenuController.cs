using UnityEngine;

public class LoadMenuController : MonoBehaviour
{
    [SerializeField] private SaveSlotUI[] slots;

    private MainMenuController _mainMenu;

    void OnEnable()
    {
        _mainMenu = FindFirstObjectByType<MainMenuController>();
        RefreshSlots();
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].Setup(i, SaveManager.Load(i), this);
    }

    public void LoadSlot(int slot)   => _mainMenu.CargarSlot(slot);
    public void DeleteSlot(int slot)
    {
        SaveManager.Delete(slot);
        RefreshSlots();
    }
}
