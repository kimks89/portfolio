using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Sapmple_UI1
public class UIBattleHeroInfo : UIBase
{
    // 스탯 기본정보
    public enum EAddStat
    {
        Attack,
        Defence,
        Health
    }

    // 스탯 상태정보
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
    private const string REFRESH_FUNC_NAME = "RefreshBattle";

    void OnEnable()
    {
        Messenger.AddListener(REFRESH_FUNC_NAME, Refresh);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(REFRESH_FUNC_NAME, Refresh);
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

    // 영웅 셋팅
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
            Refresh();
        }

    }

    // 스탯 갱신
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

    // 갱신
    void Refresh()
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
}

// Sample_UI2
public class UIAttendItem : UIBase
{
    [SerializeField]
    private UILabel _labelTitle;
    [SerializeField]
    private UIItemList _itemList;

    // 보상 아이템 셋팅
    public void SetAttendItem(DataAttendance data)
    {
        if (data == null)
        {
            Debug.Log("Data is Null");
            return;
        }
        _itemList.ClearItems();
        _labelTitle.text = data.GetName();
        List<RewardInfo> rewardList = GetAttendRewardList(data);
        for (int i = 0; i < rewardList.Count; i++)
        {
            GameObject slot = _itemList.AddItem(i.ToString());
            UIBaseRewardItem item = GlobalHelper.FindChildComponentByName<UIBaseRewardItem>(slot, "ThumbnailReward");
            item.SetReward(rewardList[i], true, true);
        }
        _itemList.OnRepositionDelay();
    }

    // 보상 리스트 가져오기
    List<RewardInfo> GetAttendRewardList(DataAttendance data)
    {
        List<RewardInfo> rewardList = new List<RewardInfo>();
        for (int i = 0; i < data.GetREWARD_TYPECount(); i++)
        {
            EGoodsType goods = (EGoodsType)data.GetREWARD_TYPE(i);
            if (goods != EGoodsType.None)
            {
                string rewardValue = data.GetREWARD_VALUE(i);
                RewardInfo reward = new RewardInfo(goods, rewardValue);
                rewardList.Add(reward);
            }
        }
        return rewardList;
    }
}

// Sample_UI3
public class UILogItem : UIBase
{
    [SerializeField]
    private UILabel _labelText;
    [SerializeField]
    private UILabel _labelDate;

    public void SetLogItem(GuildLog log)
    {
        if (log == null)
        {
            Debug.Log("Log is Null");
            return;
        }

        switch (log.Type) // 표시 형식이 달라지면 추가
        {
            case EGuildLogType.UpdateChangeName:
            case EGuildLogType.UpdateEmblem:
            case EGuildLogType.UpdateSignUpType:
                _labelText.text = string.Format(Localization.Get("UI_LABEL_GUILD_LOG_" + log.Type.ToString()));
                break;
            case EGuildLogType.UpdateLevel:
                _labelText.text = string.Format(Localization.Get("UI_LABEL_GUILD_LOG_UpdateLevel"), log.Name);
                break;
            default:
                _labelText.text = string.Format(Localization.Get("UI_LABEL_GUILD_LOG_TYPE_DESC"), log.Name, Localization.Get("UI_LABEL_GUILD_LOG_" + log.Type.ToString()));
                break;
        }

        _labelDate.text = string.Format(Localization.Get("UI_LABEL_GUILD_TIME_DESC"), TimeHelper.GetRemainTime(log.Date));

    }
}

// Sample_Model
// 운영툴로 부터 이벤트 미션들을 한 그룹으로 만드는 클래스
public class ContentsEventGroup
{
    public string Name { get; private set; }
    public uint EventKey { get; private set; }
    public EContentsEvent ContentsType { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int ExpirationDayCount { get; private set; }
    public DataDungeon OpenDungeon { get; private set; }

    // 미션 정보 리스트
    public List<ContentsEventInfo> EventInfoList { get; } = new List<ContentsEventInfo>();

    private const string PARSE_STRING_DATE = "yyyy-MM-dd HH:mm:ss";

    // 기본정보셋팅
    public ContentsEventGroup(Hashtable data)
    {
        EventInfoList.Clear();

        // 셋팅
        Name = data["name"].ToString();
        EventKey = uint.Parse(data["eventKey"].ToString());
        uint contentsId = uint.Parse(data["eventType"].ToString());
        ContentsType = (EContentsEvent)contentsId;
        ExpirationDayCount = int.Parse(data["expirationDay"].ToString());
        OpenDungeon = DataDungeon.GetByID(data["openDungeonID"].ToString());
        StartDate = DateTime.ParseExact((string)data["startDateTime"], PARSE_STRING_DATE, null);
        EndDate = DateTime.ParseExact((string)data["endDateTime"], PARSE_STRING_DATE, null);

        // 기간별 셋팅
        Hashtable dayTable = hash["dayTable"] as Hashtable;
        for (int day = (int)EDayCountOfWeek.Day1; day <= (int)EDayCountOfWeek.Day7; day++)
        {
            // 해당 요일 이벤트 체크
            if (dayTable.ContainsKey(day.ToString()) == false)
            {
                continue;
            }

            ContentsEventInfo eventInfo = new ContentsEventInfo(key, dayTable[day.ToString()]);
            EventInfoList.Add(eventInfo);
        }
    }
}

// Sample_Helper
public class TutorialHelper
{
    static Dictionary<ETutorialGroup, TutorialInfo> dicTutorialInfo = new Dictionary<ETutorialGroup, TutorialInfo>(); // 튜토리얼 그룹별 상태 저장

    // 튜토리얼 데이터 Dic으로 셋팅(key:그룹)
    static public void SetTutorialDatas()
    {
        dicTutorialInfo.Clear();
        Dictionary<uint, DataTutorial> dicDatas = DataTutorial.GetDicDataTutorial();
        foreach (DataTutorial data in dicDatas.Values)
        {
            ETutorialGroup groupType = (ETutorialGroup)data.GetGROUP_TYPE();
            if (groupType != ETutorialGroup.None)
            {
                if (dicTutorialInfo.ContainsKey(groupType)) // 같은 그룹이라면 데이터만 추가
                {
                    dicTutorialInfo[groupType].AddDataList(data);
                }
                else // 없는 그룹이면 생성
                {
                    dicTutorialInfo.Add(groupType, new TutorialInfo(data, false));
                }
            }
        }
    }

    // 클리어 그룹 체크
    static public bool IsClearByGroup(ETutorialGroup group)
    {
        if (dicTutorialInfo.ContainsKey(group))
        {
            return dicTutorialInfo[group].IsComplete;
        }
        else
        {
            Debug.Log("Not Tutorial Info Group:" + group.ToString());
        }
        return false;
    }

    // 아이템 sort
    static public void SortItemManager(ref List<ItemInfo> itemList, ItemInfo item)
    {
        itemList = itemList.OrderByDescending(x => x.GetID() == item.GetID()).ToList();
    }
}
