using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBattleHeroInfo : UIBase
{
    // 스탯enum
    public enum EAddStat
    {
        Attack,
        Defence,
        Health
    }

    // 상태enum
    public enum EGetState
    {
        Positive,
        Negative,
    }

    // UI
    [SerializeField]
    private UISlider _sliderHP;
    [SerializeField]
    private UILabel _labelHP;
    [SerializeField]
    private UIBattleElementPanel _elementPanel;
    [SerializeField]
    private UIBattleStonePanel _stonePanel;
    [SerializeField]
    private UIBattleActivityPanel _activityPanel;
    [SerializeField]
    private GameObject _activityMax;
    [SerializeField]
    private UILabel[] _labelsAddStat;
    [SerializeField]
    private UIState[] _statesAddStat;

    // Info
    private UnitBattleActor nowUnit;

    // const
    private const string RESET_INFO_FUNC_NAME = "ResetInfo";

    void OnEnable()
    {
        Messenger.AddListener(RESET_INFO_FUNC_NAME, ResetInfo);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(RESET_INFO_FUNC_NAME, ResetInfo);
    }

    // UI초기화
    void ResetUI()
    {
        for (int i = 0; i < _labelsAddStat.Length; i++)
        {
            _labelsAddStat[i].cachedGameObject.SetActive(false);
        }

        UISprite followImg = GlobalHelper.FindChildComponentByName<UISprite>(_sliderHP.gameObject, "FollowBar");
        if (followImg != null)
        {
            followImg.fillAmount = 0f;
        }
    }

    // 정보 갱신
    void ResetInfo()
    {
        if (nowUnit == null)
        {
            Debug.Log("Unit is Null");
        }

        _elementPanel.Refresh();
        _stonePanel.Refresh();
        _activityPanel.RefreshSort();
        _activityMax.SetActive(nowUnit.ActivityManager.UIs.Count >= ACTIVITY_MAX);
        RefreshStat();
    }

    // 셋팅
    public void Set(UnitBattleActor actor)
    {
        ResetUI();
        if (nowUnit == null)
        {
            gameObject.SetActive(false);
            Debug.Log("Unit is Null");
        }
        else
        {
            gameObject.SetActive(true);
            nowUnit = actor;
            _elementPanel.Initialize(nowUnit);
            _stonePanel.Initialize(nowUnit, CommonDefine.LAYER_POPUP_3D_UI);
            _activityPanel.Initialize(nowUnit, true, activityPos);
            ResetInfo();
        }

    }

    // 스탯갱신
    void RefreshStat()
    {
        // 공격력
        int addAttack = Mathf.RoundToInt(nowUnit.BattleActorInfo.GetAdd(EAppliableStat.AS_AverDamage));
        string attack = addAttack.ToString();
        EGetState attackState = EGetState.Negative;
        if (addAttack > 0)
        {
            attack = "+" + addAttack.ToString();
            attackState = EGetState.Positive;
        }
        _labelsAddStat[(int)EAddStat.Attack].text = "(" + attack + ")";
        _labelsAddStat[(int)EAddStat.Attack].cachedGameObject.SetActive(addAttack != 0);
        _statesAddStat[(int)EAddStat.Attack].Reset();
        _statesAddStat[(int)EAddStat.Attack].SetActive(attackState.ToString());
        // 방어력
        int addDefence = Mathf.RoundToInt(nowUnit.BattleActorInfo.GetAdd(EAppliableStat.AS_Defence));
        string defence = addDefence.ToString();
        EGetState defenceState = EGetState.Negative;
        if (addDefence > 0)
        {
            defence = "+" + addDefence.ToString();
            defenceState = EGetState.Positive;
        }
        _labelsAddStat[(int)EAddStat.Defence].text = "(" + defence + ")";
        _labelsAddStat[(int)EAddStat.Defence].cachedGameObject.SetActive(addDefence != 0);
        _statesAddStat[(int)EAddStat.Defence].Reset();
        _statesAddStat[(int)EAddStat.Defence].SetActive(defenceState.ToString());
        // 체력
        int addHealth = Mathf.RoundToInt(nowUnit.BattleActorInfo.GetAdd(EAppliableStat.AS_MaxHp));
        string health = addHealth.ToString();
        EGetState healthState = EGetState.Negative;
        if (addHealth > 0)
        {
            health = "+" + addHealth.ToString();
            healthState = EGetState.Positive;
        }
        _labelsAddStat[(int)EAddStat.Health].text = "(" + health + ")";
        _labelsAddStat[(int)EAddStat.Health].cachedGameObject.SetActive(addHealth != 0);
        _statesAddStat[(int)EAddStat.Health].Reset();
        _statesAddStat[(int)EAddStat.Health].SetActive(healthState.ToString());
    }
}