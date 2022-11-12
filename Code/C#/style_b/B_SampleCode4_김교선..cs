using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace ProjectGame.FullScene
{
    public class FullSceneArchiveLobby : FullSceneBase
    {
        private const float ShowUITime = 3.4f;
        
#if UNITY_EDITOR
        [Header("Test Mode For Art Team")]
        [SerializeField] private bool _isShowUI = false;
        [SerializeField] private MasterGenderType _genderType = MasterGenderType.MGT_Male;
        private bool _isTestMode = false;
#endif
        [Header("3D Component")] 
        [SerializeField] private PlayableDirector _directorIntro;
        [SerializeField] private PlayableDirector _directorLoop;
        [SerializeField] private GameObject _goMaster;
        [SerializeField] private ThreeDPreviewPlace _bgPlace;
        [SerializeField] private GameObject _goCrystalParent;
        [Header("UI Component")]
        [SerializeField] private UIArchiveLobby _uiArchiveLobby;
        
        private BgSetting _bgSetting = null;
        private List<string> _effectSoundNames = new List<string>();

        protected override void Awake()
        {
            base.Awake();
            
#if UNITY_EDITOR
            _isTestMode = UserDataManager.Instance.IsValidUser() == false;
            
            if (_isTestMode)
            {
                ResourceManager.Instance.Init();
                UserPreference.Instance.UserMasterGender = _genderType.ToString();
            }
#endif  
            OneAudioListener.Instance.Refresh();

            LoadBgSetting();
            ResetCrystalImages();
            ResetDirector();

            ShowUI(false);
        }

        protected override void Start()
        {
            base.Start();
            Utils.SetDefaultShaderColor();
            
            SceneRoot.PushBackBtnHandler(OnBtnClose);
            _uiArchiveLobby.CallBack = OnBtnClose;
            _uiArchiveLobby.CallDialog = SetPlayDirector;
            
            ShowUI(true);

            ThreeDPreviewStack.Instance.BGPlace = _bgPlace;
            
            LoadMaster();
            
            _directorIntro.gameObject.SetActiveFast(true);
            _directorIntro.Play();
            _directorIntro.stopped += DirectorOnStopped;
            _directorLoop.stopped += DirectorOnStopped;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (SingletonBase.ApplicationIsQuitting)
            {
                return;
            }
            
            if (_bgSetting != null)
                _bgSetting.PopShader();

            _directorIntro.stopped -= DirectorOnStopped;
            _directorLoop.stopped -= DirectorOnStopped;

            StopSoundClip(_directorIntro);
            StopSoundClip(_directorLoop);

            SceneRoot.PopBackBtnHandler(OnBtnClose);
        }

        protected override void Update()
        {
            base.Update();
            
            // BackKey 예외처리
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (LoadingIndicator.IsShowing)
                {
                    return;
                }

                if (AutoSequenceManager.Instance.IsPlayingAutoSequence)
                {
                    return;
                }

                if (SceneFadeInOut.Instance.IsFadeOngoing)
                {
                    return;
                }
                
                if (SceneRoot.BackBtnHandlers.Count > 0)
                {
                    SceneRoot.BackBtnHandlers[SceneRoot.BackBtnHandlers.Count - 1]();
                }
            }
        }

        // UI호출
        public void ShowUI(bool isShow)
        {
#if UNITY_EDITOR
            if (_isTestMode && isShow)
            {
                isShow = _isShowUI;
            }
#endif  
            
            _uiArchiveLobby.ShowUI(isShow, isShow ? ShowUITime : 0f);
        }

        // BG배경셋팅 호출
        private void LoadBgSetting()
        {
            var bgSettingPath = $"StageBg/ArchiveLobby/ArchiveLobby.setting";

            var bgSettingObj = ResourceManager.Instance.Load<GameObject>(bgSettingPath, false);
            if (bgSettingObj != null)
            {
                _bgSetting = bgSettingObj.GetComponent<BgSetting>();
                if (_bgSetting != null)
                {
                    _bgSetting.Apply();	
                }
            }
        }

        // 연출용 크리스탈 이미지 리셋
        private void ResetCrystalImages()
        {
            var imageList = _goCrystalParent.GetComponentsInChildren<ArchiveLobbyImageChanger>(true);
            if (imageList == null || imageList.Length <= 0)
            {
                Debug.Log("Null is ImageChanger");
                return;
            }

            foreach (var image in imageList)
            {
                if (image == null)
                {
                    continue;
                }
                
                image.SetImage();
            }
        }

        // Direcotr 리셋
        private void ResetDirector()
        {
            _directorIntro.Stop();
            _directorIntro.time = 0;
            _directorIntro.gameObject.SetActiveFast(false);

            _directorLoop.Stop();
            _directorLoop.time = 0;
            _directorLoop.gameObject.SetActiveFast(false);
        }

        // 디렉터 플레이
        private void SetPlayDirector(bool isPlay)
        {
            var nowDirector = _directorIntro;
            if (_directorLoop.gameObject.activeSelf)
            {
                nowDirector = _directorLoop;
            }
            
            if (isPlay)
            {
                nowDirector.Play();
            }
            else
            {
                nowDirector.Pause();
            }

            SetPlayAudioSound(isPlay);
        }

        // 오디오 플레이
        private void SetPlayAudioSound(bool isPlay)
        {
            if (UserPreference.Instance.IsMuteSound() || _effectSoundNames.Count <= 0)
            {
                return;
            }
            
            var audioSources = SoundManager.Instance.GetComponentsInChildren<AudioSource>();
            if (audioSources == null || audioSources.Length <= 0)
            {
                return;
            }

            foreach (var audioSource in audioSources)
            {
                if (audioSource == null || _effectSoundNames.Contains(audioSource.name) == false)
                {
                    continue;
                }

                if (isPlay)
                {
                    audioSource.Play();
                }
                else
                {
                    audioSource.Pause();
                }
            }
        }

        // 디렉터 멈출시 콜백
        private void DirectorOnStopped(PlayableDirector director)
        {
            _directorIntro.gameObject.SetActiveFast(false);

            _directorLoop.gameObject.SetActiveFast(true);
            _directorLoop.Play();
        }
        
        // 사운드 클립 멈춤
        private void StopSoundClip(PlayableDirector director)
        {
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
                return;
            
            var tracks = timelineAsset.GetOutputTracks();
            foreach (var track in tracks)
            {
                if (track.GetType() != typeof(PlaySoundTack))
                {
                    continue;
                }

                var clips = track.GetClips();
                foreach (var clip in clips)
                {
                    var clipAsset = clip.asset as PlaySoundClip;
                    if (clipAsset == null)
                    {
                        continue;
                    }

                    clipAsset.template.StopSound(true);
                }
            }
        }

        // 유저 마스터 불러오기
        // 씬에서 작업된 모델에 위치, 애니메이션, 트랙정보 등 저장 -> 사본(유저별 마스터)에 적용후 원본 삭제
        private void LoadMaster()
        {
            var master = UserDataManager.Instance.UserMasterCharacter;
            if (master == null || _goMaster == null)
            {
                return;
            }

            var originAnimator = _goMaster.GetComponent<Animator>();
            Animator originHairAnimator = null;
            var animators = _goMaster.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (animator != null && animator.gameObject.name.ToLower().Contains("hair"))
                {
                    originHairAnimator = animator;
                    break;
                }
            }

            var loader = UnitLoader.GetLoader(master);
            if (loader == null)
            {
                return;
            }

            var goModel = loader.LoadCharacter(UnitLoader.LoadType.None, UnitLoader.CharMatType.Hd, _goMaster.transform.parent);
            if (goModel == null)
            {
                return;
            }
            
            goModel.SetLayerRecursively(_goMaster.layer);

            var trCurrent = _goMaster.transform;
            var trModel = goModel.transform;

            trModel.localPosition = trCurrent.localPosition;
            trModel.localRotation = trCurrent.localRotation;
            trModel.localScale = trCurrent.localScale;

            var uma = goModel.GetComponent<UnitMasterActor>();
            if (uma != null)
            {
                uma.SetLoadType(UnitLoader.LoadType.None, UnitLoader.CharMatType.Hd);
            }

            var newAnimator = goModel.GetComponent<Animator>();
            var newAnimators = goModel.GetComponentsInChildren<Animator>();
            Animator newHairAnimator = null;
            if (originHairAnimator != null)
            {
                foreach (var animator in newAnimators)
                {
                    if (animator != null && animator.gameObject.name.ToLower().Contains("hair"))
                    {
                        newHairAnimator = animator;
                    }
                }
            }
            
            if (newAnimator != null && _directorIntro != null)
            {
                var timelineAsset = _directorIntro.playableAsset as TimelineAsset;
                if (timelineAsset != null)
                {
                    var tracks = timelineAsset.GetOutputTracks();
                    foreach (var track in tracks)
                    {
                        var bindingObject = _directorIntro.GetGenericBinding(track);
                        if (bindingObject == originAnimator)
                        {
                            _directorIntro.SetGenericBinding(track, newAnimator);
                        }
                        else if (newHairAnimator != null && originHairAnimator != null && bindingObject == originHairAnimator)
                        {
                            _directorIntro.SetGenericBinding(track, newHairAnimator);
                        }

                        AddEffectSound(track);
                    }
                }

                if (_directorLoop != null)
                {
                    var timelineAsset2 = _directorLoop.playableAsset as TimelineAsset;
                    if (timelineAsset2 != null)
                    {
                        var tracks = timelineAsset2.GetOutputTracks();
                        foreach (var track in tracks)
                        {
                            var bindingObject = _directorLoop.GetGenericBinding(track);
                            if (bindingObject == originAnimator)
                            {
                                _directorLoop.SetGenericBinding(track, newAnimator);
                            }
                            else if (newHairAnimator != null && originHairAnimator != null && bindingObject == originHairAnimator)
                            {
                                _directorLoop.SetGenericBinding(track, newHairAnimator);
                            }
                            
                            AddEffectSound(track);
                        }
                    }
                }
            }
            
            DestroyImmediate(_goMaster);
            _goMaster = goModel;
        }

        // 이펙트 사운드 추가
        private void AddEffectSound(TrackAsset track)
        {
            if (track.GetType() != typeof(PlaySoundTack))
            {
                return;
            }

            var clips = track.GetClips();
            if (clips == null)
            {
                return;
            }
            
            foreach (var clip in clips)
            {
                var clipAsset = clip.asset as PlaySoundClip;
                if (clipAsset == null || clipAsset.template?.SoundDatas == null)
                {
                    continue;
                }

                var soundDataList = clipAsset.template.SoundDatas;
                foreach (var soundData in soundDataList)
                {
                    if (soundData == null || soundData.SoundName.IsNullOrEmpty())
                    {
                        continue;
                    }
                    var data = GameDataManager.Instance.GetData<MsgSound>(soundData.SoundName);
                    if (data == null || data.file_name.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var fileName = data.file_name;
                    if (_effectSoundNames.Contains(fileName))
                    {
                        continue;
                    }
                
                    _effectSoundNames.Add(data.file_name);
                }
            }
        }

        #region Button Interaction
        // Back
        public void OnBtnClose()
        {
#if UNITY_EDITOR
            if (_isTestMode)
            {
                return;
            }
#endif
            
            _uiArchiveLobby.ShowUI(false);
            var message = TextDataManager.Instance.GetText("str.archive.exit.msg");
            var txtNo = TextDataManager.Instance.GetText("str.archive.exit.no");
            var txtYes = TextDataManager.Instance.GetText("str.archive.exit.yes");
            
            var box = MessageBox.Show3DCharacterBox(message, Constants.EventMuStrUid);
            box.SetCustomButton(MessageBox.MessageBoxCustomButton.No, txtNo);
            box.SetCustomButton(MessageBox.MessageBoxCustomButton.Yes, txtYes);
            box.OnClose = cb =>
            {
                if (cb == MessageBox.MessageCallBack.CB_YES)
                {
                    StartCoroutine(CoClose());
                }
                else
                {
                    _uiArchiveLobby.ShowUI(true);
                }
            };
        }

        IEnumerator CoClose()
        {
            // Close시 메세지 Dialog가 제대로 Pop될때 까지 대기
            while (Dialog.DialogStackCount > 0)
            {
                yield return null;
            }
            
            UnloadFullScene();
        }
        #endregion
    }
}