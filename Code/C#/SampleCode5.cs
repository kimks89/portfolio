using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using uTools;

// 트윈 컴퍼넌트를 시퀀스 별로 실행시 사용
public class UITweenSequence : MonoBehaviour
{
    [System.Serializable]
    public class TweenGroupInfo
    {
        public GameObject GoTarget;
        public bool IsApplyReverse = false;
        public bool IsInChildren = false;
        public float AddDelayTime = 0f;

        private Tweener[] _tweeners;
        public Tweener[] Tweeners => _tweeners;
        public bool IsPossible => Tweeners != null && Tweeners.Length > 0;

        public void OnReset()
        {
            if (GoTarget == null)
            {
                return;
            }

            _tweeners = IsInChildren ? GoTarget.GetComponentsInChildren<Tweener>(true) : GoTarget.GetComponents<Tweener>();
            
            if (IsPossible == false)
            {
                return;
            }
            
            foreach (var tweener in Tweeners)
            {
                if (tweener == null)
                {
                    continue;
                }

                tweener.ResetToBeginning();
                tweener.enabled = false;
            }
        }
        
        public void OnPlaying(bool isReverse = false)
        {
            if (IsPossible == false)
            {
                return;
            }

            foreach (var tweener in Tweeners)
            {
                if (tweener == null)
                {
                    continue;
                }

                if (isReverse)
                {
                    tweener.PlayReverse();
                }
                else
                {
                    tweener.ResetToBeginning();
                    tweener.PlayForward();
                }
            }
        }

        public float GetDelayTime(bool isAddDelay)
        {
            var delayTime = 0f;
            
            if (IsPossible)
            {
                foreach (var tweener in Tweeners)
                {
                    delayTime += tweener.delay + tweener.duration;
                }

                if (isAddDelay)
                {
                    delayTime += AddDelayTime;
                }
            }

            return delayTime;
        }
    }

    public TweenGroupInfo[] TweenInfos;
    
    private int _groupIndex = 0;
    private TweenGroupInfo _nowGroup;
    private bool _isPossible => TweenInfos != null && TweenInfos.Length > 0;

    private void OnEnable()
    {
        ResetTweener();
        PlayTweener();
    }

    // 트위너 리셋
    private void ResetTweener()
    {
        if (_isPossible == false)
        {
            return;
        }

        foreach (var info in TweenInfos)
        {
            if (info == null)
            {
                continue;
            }
            
            info.OnReset();
        }
    }

    // 트위너 플레이
    private void PlayTweener()
    {
        if (_isPossible == false)
        {
            return;
        }

        if (_groupIndex >= TweenInfos.Length)
        {
            _groupIndex = 0;
        }
        
        StopCoroutine(CoGroupTweener());
        StartCoroutine(CoGroupTweener());
    }

    // 그룹 트위너
    private IEnumerator CoGroupTweener()
    {
        _nowGroup = TweenInfos[_groupIndex];

        if (_nowGroup == null)
        {
            Debug.Log($"#TweenSeqIndex:{_groupIndex} is Null#");
            yield break;
        }
        
        _nowGroup.OnPlaying();
        yield return new WaitForSeconds(_nowGroup.GetDelayTime(_nowGroup.IsApplyReverse == false));
        
        if (_nowGroup.IsApplyReverse)
        {
            _nowGroup.OnPlaying(true);
            yield return new WaitForSeconds(_nowGroup.GetDelayTime(true));
        }
        
        _groupIndex++;
        PlayTweener();
        
        yield return null;
    }
}
