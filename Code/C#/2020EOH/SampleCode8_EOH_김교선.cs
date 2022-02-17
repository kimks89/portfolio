using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 상점 관련UI 클래스(샘플소스코드)
public class UIDialogChargeShop : ManualSingletonUIDialog<UIDialogChargeShop>
{
    enum ItemListType
    {
        Normal, // 스테미나, 골드, 캐쉬, 포션
        Ticket, // 입장권
    }

    [SerializeField] private UIToggle[] _btnTaps;
    [SerializeField] private UIItemList[] _itemList; // 티켓이 아이템이 달라서 사용
    [SerializeField] private UIState _stateItemList;
    [SerializeField] private UILabel _labelStamina;
    [SerializeField] private UILabel _labelGold;
    [SerializeField] private UILabel _labelCash;
    [SerializeField] private UILabel _labelNationCoin;
    [SerializeField] private UILabel _labelEmpty;
    [SerializeField] private UILabel _labelRealCash;
    [SerializeField] private UILabel _labelWarning;

    private ShopHelper.ChargeType nowType = ShopHelper.ChargeType.None;
    private List<UIBaseShopItem> chargeItemList = new List<UIBaseShopItem>();
    private ItemListType itemListType = ItemListType.Normal;

    protected override void OnCreate()
    {
        base.OnCreate();
        Instance = this;
        type = UIDialogType.ChargeShop;
    }

    protected override void OnShow()
    {
        base.OnShow();
        if (GameHelper.IsLimitedContentsWithMessage(ENonBattleType.NB_ChargeShop))
        {
            OnClickBack();
            return;
        }

        SoundManager.Instance.PlayOneShot(SoundManager.Sound.UI, SoundNames.FX.COMMON_CONTENTS_OPEN_POPUP, Settings.Game.FxVolume);
    }

    private void OnEnable()
    {
        Messenger.AddListener("OnUpdateGoods", UpdateMyGoods);
        Messenger.AddListener("OnBuyShopItem", RefreshShopItems);
    }

    private void OnDisable()
    {
        nowType = ShopHelper.ChargeType.None;
        Messenger.Broadcast("UIDialogChargeShopHide", MessengerMode.DONT_REQUIRE_LISTENER);
        Messenger.RemoveListener("OnUpdateGoods", UpdateMyGoods);
        Messenger.RemoveListener("OnBuyShopItem", RefreshShopItems);

    }

    protected override void OnHide()
    {
        base.OnHide();

        switch (SceneManager.Instance.GetCurrentState())
        {
            case SceneManager.State.AIR_LOBBY:
                LogHelper.StartLogEvent("AirLobby");
                break;
            case SceneManager.State.NATION_MAP:
                LogHelper.StartLogEvent("countrymap");
                break;
            case SceneManager.State.TOWN:
                LogHelper.StartLogEvent("Town");
                break;
            default:
                LogHelper.StartLogEvent("charge_shope_Exit");
                break;
        }
    }

    // 충전소 셋팅
    public void SetChargeShopTap(ShopHelper.ChargeType type, EGoodsType goGoods = EGoodsType.None)
    {
        if (nowType == type)
        {
            return;
        }

        for (int i = 0; i < _btnTaps.Length; i++)
        {
            _btnTaps[i].Set(false);
        }

        nowType = type;
        _btnTaps[(int)nowType].Set(true);
        // 업데이트
        SetCharageShop();
        UpdateMyGoods();
        SetSelectItem(goGoods);
        // 문구
        string warningKey = "UI_LABEL_SHOP_LIST_BUY_WARNING";
        if (nowType == ShopHelper.ChargeType.RealCash)
        {
            warningKey = "UI_LABEL_SHOP_LIST_BUY_REALCASH_WARNING";
        }
        _labelWarning.text = Localization.Get(warningKey);
    }

    // 리셋
    void ResetClear()
    {
        for (int i = 0; i < _itemList.Length; i++)
        {
            _itemList[i].ClearItems();
        }
        chargeItemList.Clear();
    }

    // 셋팅
    void SetCharageShop()
    {
        ResetClear();
        SetCharageShopSlot(ShopHelper.GetRechargeShopList(nowType));
    }

    // 슬롯셋팅
    void SetCharageShopSlot(List<DataShop> _listShop)
    {
        int addCount = 0;
        itemListType = GetItemListType(nowType);
        for (int i = 0; i < _listShop.Count; i++)
        {
            if (itemListType == ItemListType.Ticket)
            {
                EGoodsType goods = (EGoodsType)_listShop[i].GetGOODS_TYPE();
                if (ItemHelper.IsOpenGoods(goods) == false) // 오픈상품 체크
                {
                    continue;
                }
            }
            addCount++;
            GameObject slot = _itemList[(int)itemListType].AddItem(i.ToString());
            UIBaseShopItem item = slot.GetComponent<UIBaseShopItem>();
            item.SettingShopItem(_listShop[i]);
            chargeItemList.Add(item);
        }
        _itemList[(int)itemListType].OnReposition();
        _stateItemList.SetActive(itemListType.ToString());
        _labelEmpty.cachedGameObject.SetActive(addCount <= 0);
    }

    // 아이템 정보리셋
    void RefreshShopItems()
    {
        for (int i = 0; i < chargeItemList.Count; i++)
        {
            chargeItemList[i].RefreshInfo();
        }
    }

    // 선택셋팅(티켓만)
    void SetSelectItem(EGoodsType type)
    {
        if (type == EGoodsType.None || itemListType == ItemListType.Normal)
        {
            return;
        }

        int selectCount = 0;
        for (int i = 0; i < chargeItemList.Count; i++)
        {
            if (chargeItemList[i].SetSelected(type))
            {
                selectCount = i;
            }
        }
        _itemList[(int)itemListType].MoveIndex(selectCount);
    }

    // 현재 탭에 따른 아이템리스트 타입
    ItemListType GetItemListType(ShopHelper.ChargeType type)
    {
        if (type == ShopHelper.ChargeType.Ticket)
        {
            return ItemListType.Ticket;
        }
        return ItemListType.Normal;
    }

    // 재화 갱신
    void UpdateMyGoods()
    {
        _labelStamina.text = GoodsHelper.GetMyCurrentAndMaxGoodsCount(EGoodsType.Stamina);
        _labelGold.text = GoodsHelper.GetMyCurrentAndMaxGoodsCount(EGoodsType.Gold);
        _labelCash.text = GoodsHelper.GetMyCurrentAndMaxGoodsCount(EGoodsType.Cash);
        _labelNationCoin.text = GoodsHelper.GetMyCurrentAndMaxGoodsCount(EGoodsType.NationCoin);
        _labelRealCash.text = GoodsHelper.GetMyCurrentAndMaxGoodsCount(EGoodsType.RealCash);
    }

    // 백버튼(닫기)
    public void OnClickBack()
    {
        Hide();
    }

    // 탭
    public void OnClickTap(GameObject go)
    {
        if (int.TryParse(go.name, out int index))
        {
            SetChargeShopTap((ShopHelper.ChargeType)index);
        }
    }

    // 패키지 상점 열기
    public void OnClickPackageShop()
    {
        ShopHelper.CallPackageShop(EPackageRecommandState.None, false);
    }
}
