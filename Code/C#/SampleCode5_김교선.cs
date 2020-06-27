using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 배경음 타입
public enum EBgmSound
{
    None,
    Title,
    Loading,
    Lobby,
    Battle,
    Result,
}

[Serializable]
public class ManageBgmSound
{
    public EBgmSound SoundName;
    public AudioClip SoundClip;
}

public class SoundManager : MonoBehaviour
{
    public AudioSource BgmSpeaker;
    public List<ManageBgmSound> BgmSoundList = new List<ManageBgmSound>();

    public EBgmSound NowBgm { get; private set; }

    // 플레이 사운드
    public void PlaySound(EBgmSound bgm, bool isLoop = true)
    {
        // 같은 타입 체크
        if (bgm == NowBgm)
        {
            Debug.Log("Same Sound Type:" + NowBgm.ToString());
            return;
        }

        // Null 체크
        if (BgmSpeaker == null)
        {
            Debug.Log("Speaker Null");
            return;
        }

        // 사운드 리스트 체크
        if (BgmSoundList == null || BgmSoundList.Count <= 0)
        {
            Debug.Log("Empty SoundList");
            return;
        }

        // 셋팅
        StopSound();
        NowBgm = bgm;

        // 사운드 찾아서 Play
        for (int i = 0; i < BgmSoundList.Count; i++)
        {
            if (BgmSoundList[i].SoundName == NowBgm)
            {
                BgmSpeaker.clip = BgmSoundList[i].SoundClip;
                BgmSpeaker.Play();
                BgmSpeaker.loop = isLoop;
            }
        }
    }

    // 스탑 사운드(일시정지인지)
    public void StopSound(bool isPause = false)
    {
        // Null체크
        if (BgmSpeaker == null)
        {
            Debug.Log("Speaker Null");
            return;
        }

        // 플레이 체크
        if (BgmSpeaker.isPlaying == false)
        {
            Debug.Log("Speaker Null Or Not Play Sound");
            return;
        }

        // Clip체크
        if (BgmSpeaker.clip == null)
        {
            Debug.Log("Empty Sound Clip");
            return;
        }

        // 사운드멈춤
        NowBgm = EBgmSound.None;
        BgmSpeaker.Stop();

        // 일시정지가 아니면 null처리
        if (isPause == false)
        {
            BgmSpeaker.clip = null;
        }
    }

    // 볼륨조절
    public void SetVolume(float volume)
    {
        if (BgmSpeaker == null)
        {
            Debug.Log("Speaker Null");
            return 0f;
        }

        BgmSpeaker.volume = volume;
    }

    // 볼륨가져오기
    public float GetVolume()
    {
        if (BgmSpeaker == null)
        {
            Debug.Log("Speaker Null");
            return 0f;
        }
        else
        {
            return BgmSpeaker.volume;
        }
    }
}
