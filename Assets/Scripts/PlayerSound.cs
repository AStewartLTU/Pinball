using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip jumpSound;

    public AudioClip deathSound;

    public void PlayerJump()
    {
        audioSource.PlayOneShot(jumpSound);

    }

    public void PlayDie()
    {
        audioSource.PlayOneShot(deathSound);
        
    }

    private void Awake()
    {
        audioSource = this.gameObject.GetComponent<AudioSource>();
    }


}
