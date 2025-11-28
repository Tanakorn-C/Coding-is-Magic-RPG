using UnityEngine;

// This script ensures that the animation prefab (like your Fire Ball)
// destroys itself after its animation has finished playing.
public class DestroyAfterAnimation : MonoBehaviour
{
    void Start()
    {
        // Get the Animator component attached to this GameObject
        Animator animator = GetComponent<Animator>();

        if (animator != null)
        {
            // Get the length of the currently playing animation clip
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            
            // Call the 'DestroySelf' method after the animation is finished
            Destroy(gameObject, animationLength);
        }
        else
        {
            Debug.LogError("DestroyAfterAnimation script needs an Animator component to work!");
            // Fallback: Destroy after 2 seconds if no animator is found
            Destroy(gameObject, 2f);
        }
    }
}