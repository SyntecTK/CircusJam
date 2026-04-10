using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip cardShuffleSound;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    

    private void OnEnable()
    {
        EventManager.OnTurnEnded += PlayCardShuffleSound;
    }

    private void OnDisable()
    {
        EventManager.OnTurnEnded -= PlayCardShuffleSound;
    }

    public void PlayCardShuffleSound()
    {
        if (cardShuffleSound != null && audioSource != null)
        {
            audioSource.clip = cardShuffleSound;
            audioSource.Play();
        }
    }
}
