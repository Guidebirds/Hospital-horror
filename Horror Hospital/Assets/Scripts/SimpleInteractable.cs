using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SimpleInteractable : MonoBehaviour
{
    public string animationBool = "Open";
    public AudioSource audioSource;
    public AudioClip interactSound;

    private Animator animator;
    private bool open = false;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public bool IsOpen => open;

    public void Toggle()
    {
        open = !open;
        if (animator != null)
        {
            animator.SetBool(animationBool, open);
        }
        if (audioSource != null && interactSound != null)
            audioSource.PlayOneShot(interactSound);
    }
}