using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDialogBuyPackage : SingletonUIDialog<UIDialogBuyPackage>
{
    // Common
    [SerializeField]
    private UILabel _labelTitle;
    [SerializeField]
    private UILabel _labelDesc;
    [SerializeField]
    private UIItemList _itemList;
    [SerializeField]
    private UIEventMainSlotItemDetail slotDetail;

    protected override void OnCreate()
    {
        base.OnCreate();
        Instance = this;
        type = UIDialogType.BuyPackage;
    }

	// 외부에서 값 받기
    public void SetInfo(DataPackage package, DataPackageShop shop, UIEventMainSlotItemDetail mainSlot, Action<int, GameObject> callBack)
    {
		_labelTitle.text = Localization.Get("UI_LABEL_TITLE_BUY_PACKAGE");
        _labelDesc.text = shop.GetNAME() + Localization.Get("UI_LABEL_QUESTION_AT_BUY_TYPE1");
        slotDetail.SetData(shop, mainSlot.EndDay, mainSlot.SpanRebuyTime, mainSlot.DateRebuyTime, mainSlot.ImageName, mainSlot.LimitType, mainSlot.BuyCount);
        slotDetail.OnClickCallback = callBack;
        SetReward(package, shop);
    }

	// 보상 셋팅
    public void SetReward(DataPackage package, DataPackageShop shop)
    {
        if (package == null || shop == null)
        {
            return;
        }

        _itemList.ClearItems();
        int start = (int)EPackageRewardType.Main;
        int end = (int)EPackageRewardType.Bonus;
        for (int i = start; i <= end; i++)
        {
            List<RewardInfo> infos = new List<RewardInfo>();
            EPackageRewardType rewardType = (EPackageRewardType)i;
            infos.AddRange(GetPackageInfos(package.GetID(), rewardType, shop));

            if (infos.Count > 0)
            {
                GameObject slot = _itemList.AddItem(rewardType.ToString());
                UIBaseRewardGroupItem item = slot.GetComponent<UIBaseRewardGroupItem>();
                if (item != null)
                {
                    item.SetRewardGroupItem(infos, rewardType, shop);
                }
            }
        }
        _itemList.OnRepositionDelay();
    }

    // Get RewardList
    List<RewardInfo> GetPackageInfos(uint id, EPackageRewardType selectType = EPackageRewardType.None, DataPackageShop shop = null)
    {
        List<RewardInfo> infos = new List<RewardInfo>();
        DataPackage package = DataPackage.GetByID(id);
        for (int i = 0; i < package.GetREWARD_TYPECount(); i++)
        {
            if (selectType != EPackageRewardType.None && selectType != (EPackageRewardType)package.GetPRICE_TYPE(i))
            {
                continue;
            }

            RewardInfo info = new RewardInfo();
            info.type = (EGoodsType)package.GetREWARD_TYPE(i);
            info.value = package.GetREWARD_VALUE(i);

            infos.Add(info);
        }

        if (shop != null && selectType == EPackageRewardType.Bonus)
        {
            RewardInfo bonus = ShopHelper.GetMonthlyBonus(shop);
            if (bonus != null)
            {
                RewardInfo searchInfo = infos.Find(x => x.type == bonus.type);
                if (searchInfo != null) // 같은 타입이면 갯수증가
                {
                    searchInfo.AddCount(bonus.GetCount());
                }
                else // 다른 타입이면 리스트 추가
                {
                    infos.Add(bonus);
                }
            }
        }

        return infos;
    }

	// 창 닫기
    public void OnClickClose()
    {
        Hide();
    }
}
