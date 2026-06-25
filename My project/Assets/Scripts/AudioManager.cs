using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip[] audioClips;
    private static float volume = 1;

    private void Awake()
    {
        instance = this;
    }
    public void PlayAudio(int id)
    {
        audioSource.PlayOneShot(audioClips[id]);
    }
}
