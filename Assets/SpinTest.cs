using UnityEngine;
using UnityEngine.UI;

public class SpinTest : MonoBehaviour
{
    public SlotMachineReel reel;
    public Button spinButton;

    private void Start()
    {
        spinButton.onClick.AddListener(Spin);
    }

    public void Spin()
    {
        reel.SpinWithRandomResult();
    }
}