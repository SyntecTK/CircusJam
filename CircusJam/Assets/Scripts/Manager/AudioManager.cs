using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioClip cardShuffleSound;
    [SerializeField] private AudioSource audioSource;

    private void OnEnable()
    {
        EventManager.OnTurnEnded += PlayCardShuffleSound;
    }

    private void OnDisable()
    {
        EventManager.OnTurnEnded -= PlayCardShuffleSound;
    }

    private void PlayCardShuffleSound()
    {
        if (cardShuffleSound != null && audioSource != null)
        {
            audioSource.clip = cardShuffleSound;
            audioSource.Play();
        }
    }
}
