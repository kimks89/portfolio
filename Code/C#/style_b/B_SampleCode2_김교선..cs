using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
using Random = System.Random;

namespace ProjectGame.UI
{
    public class DlgBoxOpenAction : Dialog
    {
        [Serializable]
        public class ActionInfo
        {
            public GameObject Item;
            public ParticleSystem ParticleEffect;
            public float DelayTime = 0f;
            public int TouchCount = 1;
            public string SoundName = string.Empty;

            private Tweener[] _tweeners;
            private int _touchedCount = 0;
            
            public void OnInit()
            {
                if (Item == null)
                {
                    return;
                }

                _tweeners = Item.GetComponentsInChildren<Tweener>(true);
                _touchedCount = 0;
            }

            public void OnPlay(bool isPlay)
            {
                Item.SetActiveFast(isPlay);
                
                if (ParticleEffect != null)
                {
                    ParticleEffect.gameObject.SetActiveFast(isPlay);
                    ParticleEffect.Stop();
                    if (isPlay)
                    {
                        ParticleEffect.Play();
                    }
                }

                if (_tweeners != null && _tweeners.Length > 0)
                {
                    foreach (var tween in _tweeners)
                    {
                        if (tween == null)
                        {
                            continue;
                        }

                        tween.enabled = isPlay;
                        tween.ResetToBeginning();
                        if (isPlay)
                        {
                            tween.PlayForward();
                        }
                    }
                }

                if (isPlay)
                {
                    if (SoundName.IsNullOrEmpty() == false)
                    {
                        SoundManager.Instance.PlaySound(SoundName);
                    }
                    
                    _touchedCount++;
                }
            }

            public bool IsCompleteAction()
            {
                return TouchCount <= _touchedCount;
            }
        }
        
        private static string PrefabToPath = "UI/Common/BoxOpen";

        [SerializeField] private ActionInfo[] _actionInfos;
        [SerializeField] private GameObject _goTouchImg;
        [SerializeField] private Tweener _tweenRandomMessage;
        [SerializeField] private UIText _txtRandomMessage;
        [SerializeField] private GameObject _goNotify;
        [SerializeField] private string _completedSound;
        [SerializeField] private float _randomMessageDelayTime = 0f;

        private Action _closeAction = null;
        private MsgBoxRandom _boxRandom = null;
        private ActionInfo _nowActionInfo = null;
        private int _actionCount = 0;
        private bool _isTouch = false;

        protected override bool IsUseBackPanel => true;
        protected override bool IsFullScene => false;
        protected override bool IsBlockClose => true;

        // 원하는 템플릿의 박스 프리팹 가져오기
        public static DlgBoxOpenAction OpenShow(string path, uint uid, Action closeAction = null)
        {
            var dlg = Create<DlgBoxOpenAction>(null, $"{PrefabToPath}/{path}");
            if (dlg != null)
            {
                dlg.SetInfo(closeAction, uid);
            }

            return dlg;
        }

        protected override void OnDialogInit()
        {
            base.OnDialogInit();

            if (_tweenRandomMessage != null)
            {
                _tweenRandomMessage.enabled = false;
                _tweenRandomMessage.gameObject.SetActiveFast(false);
                _tweenRandomMessage.ResetToBeginning();
            }

            if (_actionInfos != null && _actionInfos.Length > 0)
            {
                foreach (var info in _actionInfos)
                {
                    if (info == null)
                    {
                        continue;
                    }
                
                    info.OnInit();
                    info.OnPlay(false);
                }
            }
            
            _goNotify.SetActiveFast(true);
            
            _actionCount = 0;
        }

        protected override void OnDialogShow()
        {
            base.OnDialogShow();

            StopCoroutine(CoIdle());
            StartCoroutine(CoIdle());
        }

        protected override void OnDialogHide(bool hideBySystem = false)
        {
            base.OnDialogHide(hideBySystem);
            _closeAction?.Invoke();
        }

        public void SetInfo(Action closeAction, uint uid)
        {
            _closeAction = closeAction;
            _boxRandom = GameDataManager.Instance.GetData<MsgBoxRandom>(uid);
        }

        // 기본상태
        private IEnumerator CoIdle()
        {
            var message = string.Empty;
            
            // 랜덤 메세지 뽑기(중복x)
            if (_boxRandom != null)
            {
                var rndRangeCount = (int)_boxRandom.random_msg_range;

                var rndList = new List<int>();
                for (var i = 1; i <= rndRangeCount; i++)
                {
                    rndList.Add(i);
                }

                var random = new Random();

                while (0 < rndList.Count)
                {
                    var index = random.Next(rndList.Count);
                    var rndValue = rndList[index].ToString("00");
                    
                    message = TextDataManager.Instance.GetText($"{_boxRandom.random_msg}.{rndValue}");
                    if (message.IsNullOrEmpty() == false)
                    {
                        break;
                    }

                    rndList.RemoveAt(index);

                    yield return null;
                }

                _txtRandomMessage.text = message;
            }

            yield return null;

            StopCoroutine(CoPlay());
            StartCoroutine(CoPlay());
        }

        // 연출 플레이
        private IEnumerator CoPlay()
        {
            // 0. null check
            if (_actionInfos == null || _actionInfos.Length <= 0)
            {
                yield break;
            }

            // 1. Next Action
            for (var i = 0; i < _actionInfos.Length; i++)
            {
                var info = _actionInfos[i];

                if (info == null)
                {
                    continue;
                }
                
                if (i == _actionCount)
                {
                    _nowActionInfo = info;
                    info.OnPlay(true);
                }
                else
                {
                    info.OnPlay(false);
                }
            }

            var isLastAction = _nowActionInfo != null && _nowActionInfo.IsCompleteAction() && _actionInfos.Length - 1 == _actionCount;
            
            _goNotify.SetActiveFast(isLastAction == false);
            
            yield return new WaitForSeconds(_nowActionInfo?.DelayTime ?? 0f);

            // 2. Finish Check
            if (isLastAction)
            {
                StopCoroutine(CoFinish());
                StartCoroutine(CoFinish());
            }
            else
            {
                _isTouch = true;
            }

            yield return null;
        }

        // 연출 종료
        private IEnumerator CoFinish()
        {
            var isWait = false;
            if (_tweenRandomMessage != null)
            {
                isWait = true;

                yield return new WaitForSeconds(_randomMessageDelayTime);
                    
                _tweenRandomMessage.enabled = true;
                _tweenRandomMessage.gameObject.SetActiveFast(true);
                _tweenRandomMessage.PlayForward();
                    
                _tweenRandomMessage.onFinished.AddListener(delegate
                {
                    isWait = false;
                });
            }

            if (_completedSound.IsNullOrEmpty() == false)
            {
                SoundManager.Instance.PlaySound(_completedSound);
            }

            while (isWait)
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
            
            _isTouch = true;

            yield return null;
        }

        public void OnBtnAction()
        {
            if (_isTouch == false)
            {
                return;
            }

            if (_nowActionInfo == null || _nowActionInfo.IsCompleteAction())
            {
                _actionCount++;
            }

            _isTouch = false;

            if (_actionCount < _actionInfos.Length)
            {
                StopCoroutine(CoPlay());
                StartCoroutine(CoPlay());
                return;
            }

            CloseDialogWithMsg();
        }
    }
}