using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectGame.UI
{
    public class UICostumePackageBanner : MonoBehaviour
    {
        // Base
        [SerializeField] private RawImage _imgIllust;
        [SerializeField] private UIText _txtName;
        [SerializeField] private UIText _txtTitle;
        [SerializeField] private UIText _txtDate;
        [SerializeField] private UIText _txtBuyCount;
        [SerializeField] private UIImage _imgBuyBtn;
        // Price
        [SerializeField] private UIImage _imgPrice;
        [SerializeField] private UIText _txtPrice;
        [SerializeField] private UIText _txtPriceCount;
        // Setting
        [SerializeField] private bool _isNativeSize = false;

        private MsgCostumeShopItem _shopData = null;
        private MsgPrice _priceData = null;
        private Action<uint> _callBuy = null;

        private bool _isBuyComplete = false;

        public MsgCostumeShopItem ShopData => _shopData;

        // 배너셋팅
        public void SetBanner(MsgCostumeShopItem data, Action<uint> callBuy = null)
        {
            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            _shopData = data;
            _callBuy = callBuy;

            // 일러스트 출력
            var illust = UIManager.Instance.GetRawTexture(UIRawImagePrefix.CostumeShopBanner, _shopData.banner_img);
            if (illust != null)
            {
                _imgIllust.texture = illust;
                
                if (_isNativeSize)
                {
                    _imgIllust.SetNativeSize();
                }
                
                _imgIllust.gameObject.SetActiveFast(true);
            }
            else
            {
                _imgIllust.gameObject.SetActiveFast(false);
            }

            var character = GetCharacter();
            if (character == null)
            {
                switch (data.gacha_type)
                {
                    case CostumeGachaType.CGT_GachaBeautyBasic:
                    case CostumeGachaType.CGT_GachaBeautyWishAll:
                    case CostumeGachaType.CGT_GachaBeautyWishPickup:
                        _txtName.gameObject.SetActiveFast(true);
                        _txtName.text = TextDataManager.Instance.GetText("str.costume.shop.banner.beauty.1");
                        break;
                    default:
                        _txtName.gameObject.SetActiveFast(false);
                        break;
                }
            }
            else
            {
                _txtName.gameObject.SetActiveFast(true);
                _txtName.text = TextDataManager.Instance.GetText(character.name);
            }
            
            _txtTitle.text = TextDataManager.Instance.GetText(_shopData.name);

            var openDatetime = GameTime.GetDateTimeFromUnixTime(_shopData.open_date.timestamp);
            var closeDatetime = GameTime.GetDateTimeFromUnixTime(_shopData.close_date.timestamp);
            _txtDate.text = string.Format(TextDataManager.Instance.GetText("str.gear.gacha.event.time"), openDatetime.Month, openDatetime.Day, openDatetime.Hour, openDatetime.Minute, closeDatetime.Month, closeDatetime.Day, closeDatetime.Hour, closeDatetime.Minute);
            
            SetRestrict();
            SetBuyButton();
        }

        // 제한갯수
        private void SetRestrict()
        {
            _isBuyComplete = false;
            var restrict = _shopData.restrict;
            if (restrict == null)
            {
                return;
            }
            
            var buyCount = UserDataManager.Instance.GetPurchaseCount(restrict, _shopData.uid, _shopData.open_date.timestamp, _shopData.close_date.timestamp);
            var maxCount = restrict.restrict_count;
            _isBuyComplete = buyCount >= maxCount;
            var buyFormat = TextDataManager.Instance.GetText("str.shop.period.limited");
            if (_txtBuyCount != null)
            {
                _txtBuyCount.text = _isBuyComplete ? TextDataManager.Instance.GetText("str.mystery.purchase.complete") : string.Format(buyFormat, buyCount, maxCount);
            }

            if (_imgBuyBtn != null)
            {
                _imgBuyBtn.color = _isBuyComplete ? Color.gray : Color.white;
            }
        }

        // 버튼 셋팅
        private void SetBuyButton()
        {
            if (_shopData?.price == null)
            {
                return;
            }

            _priceData = _shopData.price;
            
            if (UIDataHelper.IsCheckGoods(_priceData.type))
            {
                if (_priceData.type == GlobalDataType.GDT_FreeGem && _priceData.price == 0)
                {
                    _txtPrice.text = TextDataManager.Instance.GetText("str.gear.gacha.free.alchemy");
                }
                else
                {
                    _imgPrice.sprite = UIDataHelper.GetItemIcon(_priceData.type);
                    _txtPrice.text = Utils.ToCurrency(_priceData.price);
                }
            }
            else
            {
                var item = GameDataManager.Instance.GetData<MsgItem>(_priceData.uid);
                _txtPrice.text = string.Format(TextDataManager.Instance.GetText("str.gear.gacha.select.require"),
                    Utils.ToCurrency(_priceData.price));
                _imgPrice.sprite = UIManager.Instance.GetItemSprite(item);
            }
        }

        // 캐릭터 가져오기
        private MsgCharacter GetCharacter()
        {
            if (_shopData == null || _shopData.purchase_costume_uid == 0)
            {
                return null;
            }
            
            var costumeData = GameDataManager.Instance.GetData<MsgCostume>(_shopData.purchase_costume_uid);
            return UserDataManager.Instance.GetCharacterByCostumeData(costumeData);
        }

        // 구매 버튼
        public void OnBtnBuy()
        {
            if (_shopData == null || _priceData == null || _isBuyComplete)
            {
                return;
            }
            
            _callBuy?.Invoke(_shopData.uid);
        }
    }
}