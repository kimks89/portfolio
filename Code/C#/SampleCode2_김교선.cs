using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGuildItem : UIBase
{
    [SerializeField]
    private UILabel _labelTitle;
    [SerializeField]
    private UIItemList _itemList;
    [SerializeField]
    private UILabel _labelText;
    [SerializeField]
    private UILabel _labelDate;

    // 보상 아이템 셋팅
    public void SetRewardItem(DataGuildReward data)
    {
        // null체크
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
            RewardItem item = UIHelper.FindChildComponentByName<RewardItem>(slot, "ThumbnailReward");
            item.SetReward(rewardList[i], true, true);
        }
        _itemList.OnRepositionDelay();
    }

    // 기록 셋팅
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

    // 보상 리스트 가져오기
    List<RewardInfo> GetAttendRewardList(DataGuildReward data)
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