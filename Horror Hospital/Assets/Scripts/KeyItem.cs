using UnityEngine;

public class KeyItem : MonoBehaviour
{
    public bool collected { get; private set; } = false;

    public void Pickup()
    {
        collected = true;
        gameObject.SetActive(false);
    }
}