using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip[] musicTracks;
    public Vector2 delayRange = new Vector2(20f, 35f); // segundos

    private void Start() {
        StartCoroutine(DelayedStart(12.5f));
    }
    
    IEnumerator DelayedStart(float initialDelay) {
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(PlayMusicLoop());
    }

    IEnumerator PlayMusicLoop() {
        while (true) {
            AudioClip nextTrack = musicTracks[Random.Range(0, musicTracks.Length)];
            musicSource.clip = nextTrack;
            musicSource.Play();

            yield return new WaitForSeconds(nextTrack.length);

            float waitTime = Random.Range(delayRange.x, delayRange.y);
            yield return new WaitForSeconds(waitTime);
        }
    }
}