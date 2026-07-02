using UnityEngine;

public class BGMTrigger : MonoBehaviour
{
    [Header("이 창이 열릴 때 틀 BGM 이름")]
    public string bgmName;

    // 1. 씬(Scene) 단위로 완전히 넘어갈 때를 대비 (씬 시작 시 실행)
    void Start()
    {
        PlayMusic();
    }

    // 2. 한 씬 안에서 UI 창(GameObject)만 껐다 켤 때를 대비 (창이 켜질 때 실행)
    void OnEnable()
    {
        PlayMusic();
    }

    private void PlayMusic()
    {
        // SoundManager가 씬에 존재하고, bgmName이 비어있지 않을 때만 실행
        if (SoundManager.instance != null && !string.IsNullOrEmpty(bgmName))
        {
            SoundManager.instance.PlayBGM(bgmName);
        }
    }
}