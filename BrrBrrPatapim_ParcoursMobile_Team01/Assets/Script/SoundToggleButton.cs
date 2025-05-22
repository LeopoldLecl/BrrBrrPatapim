using UnityEngine;

public class SoundToggleButton : MonoBehaviour
{
    public void Toggle()
    {
        if (SoundManager.instance != null)
        {
            SoundManager.instance?.ToggleSound();
        }
    }
}
