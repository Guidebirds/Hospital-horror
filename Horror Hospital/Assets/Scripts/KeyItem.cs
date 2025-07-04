using UnityEngine;

public class KeyItem : MonoBehaviour, IInteractable
{
    public bool collected { get; private set; } = false;

    public void Pickup()
    {
        collected = true;
        gameObject.SetActive(false);
    }
}