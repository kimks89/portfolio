using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHelper
{
    public enum EState
    {
        None, // 없음 혹은 종료
        Wait, // 대기
        Ready, // 준비
        Play, // 플레이
    }

    static EState nowState = EState.None;
    static EState beforeState = EState.None;

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

    // 현재 해당 튜토리얼이 플레이 중인지
    static public bool IsPlaying(ETutorialGroup group)
    {
        DataTutorial data = GetNowData();
        if (data != null)
        {
            ETutorialGroup dataGroup = (ETutorialGroup)data.GetGROUP_TYPE();
            return dataGroup == group;
        }
        return false;
    }

    // 리스트 정보 가져오기
    static public List<TutorialInfo> GetInfoList(ETutorialGroup group)
    {
        List<TutorialInfo> list = new List<TutorialInfo>();
        if (dicTutorialInfo.ContainsKey(group)) // 같은 그룹이라면 데이터만 추가
        {
            list = dicTutorialInfo[group];
        }
        return list;
    }

    // 상태 셋팅
    static public void SetState(EState state)
    {
        if (nowState == state)
        {
            Debug.Log("Same State Group:" + group.ToString());
            return;
        }

        beforeState = nowState;
        nowState = state;
    }

    // 상태 가져오기
    static public bool IsNowState(EState state)
    {
        return nowState == state;
    }

    // 이전 튜토리얼 체크
    static public bool CheckPrev(TutorialInfo info)
    {
        string condition = info.GetStartData().GetPREV_TUTORIAL();
        if (string.IsNullOrEmpty(condition) == false)
        {
            DataTutorial tutorialData = DataTutorial.GetByEnumID(condition);
            if (tutorialData != null)
            {
                ETutorialGroup group = (ETutorialGroup)tutorialData.GetGROUP_TYPE();
                if (dicTutorialInfo.ContainsKey(group))
                {
                    return dicTutorialInfo[group].IsComplete;
                }
            }
        }
        return true;
    }

    // 튜토리얼 선행 조건 체크
    static public bool CheckCondition(TutorialInfo info)
    {
        ETutorialCondition conditionType = (ETutorialCondition)info.GetStartData().GetCONDITION_TYPE();
        string conditionValue = info.GetStartData().GetCONDITION_VALUE();
        switch (conditionType)
        {
            case ETutorialCondition.EnterDungeon: // 던전 입장시
                DataDungeon enterData = DataDungeon.GetByEnumID(conditionValue);
                if (enterData != null && BattleInfo != null)
                {
                    return enterData == BattleInfo.DataDungeon;
                }
                break;
            case ETutorialCondition.ClearDungeon: // 던전 클리어
                DataDungeon clearData = DataDungeon.GetByEnumID(conditionValue);
                if (clearData != null)
                {
                    return DungeonHelper.GetDungeonRating(clearData) > 0;
                }
                break;
            case ETutorialCondition.ClearTutorial: // 튜토리얼 클리어
                DataTutorial tutorialData = DataTutorial.GetByEnumID(conditionValue);
                if (tutorialData != null)
                {
                    ETutorialGroup group = (ETutorialGroup)tutorialData.GetGROUP_TYPE();
                    return IsClearByGroup(group);
                }
                break;
            case ETutorialCondition.ClearCompletion: // 컴플리션 클리어
                DataCompletion completionData = DataCompletion.GetByEnumID(conditionValue);
                if (completionData != null)
                {
                    CompletionInfo completeInfo = CompletionHelper.GetMyCompletion(completionData.GetID());
                    if (completeInfo != null)
                    {
                        return CompletionHelper.IsComplete(completeInfo);
                    }
                }
                break;
            case ETutorialCondition.None:
                return true;
        }
        return false;
    }

    // 서버로 보낼 String ID값 가져오기
    static public string GetSendServerID(DataTutorial data)
    {
        string sendID = string.Empty;
        if (data != null)
        {
            // 같이 저장해야하는 특수한 경우
            if (string.IsNullOrEmpty(data.GetCONNECT_SAVE()) == false)
            {
                string sendValues = "";
                string[] saveArray = data.GetCONNECT_SAVE().Split(';');
                for (int i = 0; i < saveArray.Length; i++)
                {
                    string id = saveArray[i];
                    DataTutorial beforeData = DataTutorial.GetByEnumID(id);
                    if (sendValues.Length > 0)
                    {
                        sendValues += ",";
                    }
                    sendValues += beforeData.GetGROUP_TYPE();
                }
                sendID = sendValues + "," + data.GetGROUP_TYPE().ToString();
            }
            else
            {
                sendID = data.GetGROUP_TYPE().ToString();
            }
        }
        return sendID;
    }
}