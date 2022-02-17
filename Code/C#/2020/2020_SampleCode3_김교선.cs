using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 운영툴로 부터 이벤트 미션들을 한 그룹으로 만드는 클래스(샘플소스코드)
public class ContentsEventGroup
{
    public string Name { get; private set; }                    // 이벤트 이름
    public uint EventKey { get; private set; }                  // 이벤트 고유키
    public EContentsEvent ContentsType { get; private set; }    // 이벤트 타입
    public DateTime StartDate { get; private set; }             // 이벤트 시작날짜
    public DateTime EndDate { get; private set; }               // 이벤트 종료날짜
    public DataDungeon OpenDungeon { get; private set; }        // 이벤트 던전체크

    // 미션 정보 리스트
    public List<ContentsEventInfo> EventInfoList { get; } = new List<ContentsEventInfo>();

    private const string PARSE_STRING_DATE = "yyyy-MM-dd HH:mm:ss";

    // 초기화
    private void Initialize()
    {
        EventInfoList.Clear();
        StartDate = DateTime.MinValue;
        EndDate = DateTime.MinValue;
    }

    // 기본정보셋팅
    public ContentsEventGroup(Hashtable data)
    {
        EventInfoList.Clear();

        // 셋팅
        Name = data["name"].ToString();
        EventKey = uint.Parse(data["eventKey"].ToString());
        uint contentsId = uint.Parse(data["eventType"].ToString());
        ContentsType = (EContentsEvent)contentsId;
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

    // 기본값 셋팅
    private void ConstructDefault(Hashtable hashTable)
    {
        Hashtable hash = hashTable["completions"] as Hashtable;

        if (hash == null)
        {
            return;
        }

        foreach (EContentsEventReward eventRewardType in Enum.GetValues(typeof(EContentsEventReward)))
        {
            if (eventRewardType == EContentsEventReward.None)
            {
                continue;
            }

            string key = eventRewardType.ToString();
            string index = ((int)eventRewardType).ToString();

            if (string.IsNullOrEmpty(key) || hash.ContainsKey(index) == false)
            {
                continue;
            }

            ArrayList arrayData = (ArrayList)hash[index];
            AddEventInfo(arrayData, eventRewardType, EDayCountOfWeek.None);
        }
    }

    // 이번테 정보 가져오기
    public List<ContentsEventInfo> GetEventListByDetail(EContentsEventReward detail)
    {
        List<ContentsEventInfo> allList = new List<ContentsEventInfo>();

        for (int i = 0; i < EventInfoList.Count; i++)
        {
            if (EventInfoList[i].DetailType == detail)
            {
                allList.Add(EventInfoList[i]);
            }
        }
        return allList;
    }

    // 이용가능 체크
    public bool IsAvailiable()
    {
        if (DungeonHelper.IsCleared(OpenDungeon) || TimeHelper.IsInCurrent(StartDate, EndDate))
        {
            return true;
        }

        return false;
    }
}

// 컨텐츠 이벤트 Helper
static public class ContentsEventHelper
{
    static private Dictionary<EContentsEvent, ContentsEventGroup> dicEventGroup = new Dictionary<EContentsEvent, ContentsEventGroup>();

    // Set Complete User Info(유저 완료정보)
    static public void SetUserComplete(Hashtable hashTable)
    {
        ArrayList arrList = (ArrayList)hashTable["completionList"];
        Hashtable eventInfo = hashTable["eventInfo"] as Hashtable;

        if (arrList != null)
        {
            completeList.Clear();
            for (int i = 0; i < arrList.Count; i++)
            {
                Hashtable data = (Hashtable)arrList[i];
                ContentsEventComplete userInfo = new ContentsEventComplete(data);
                completeList.Add(userInfo);
            }
        }

        if (eventInfo != null)
        {
            foreach (ContentsEventGroup group in MissionAttendanceHelper.MissionAttendanceEvents)
            {
                string key = group.EventKey.ToString();
                if (eventInfo.ContainsKey(key))
                {
                    Hashtable infoHash = eventInfo[key] as Hashtable;
                    DateTime startTime = DateTime.MinValue;
                    DateTime endTime = DateTime.MinValue;
                    int dayCount = 0;

                    if (infoHash.ContainsKey("startTime"))
                    {
                        startTime = DateTime.Parse(infoHash["startTime"] as string);
                    }
                    if (infoHash.ContainsKey("endTime"))
                    {
                        endTime = DateTime.Parse(infoHash["endTime"] as string);
                    }
                    if (infoHash.ContainsKey("day"))
                    {
                        dayCount = int.Parse(infoHash["day"].ToString());
                    }

                    group.SetUserEventStartTime(startTime);
                    group.SetUserEventEndTime(endTime);
                    group.SetDayCount(dayCount);
                }
            }
        }
    }

    // Set Contents Group(그룹 및 미션키로 갯수만 셋팅)
    static public void SetGroup(ArrayList arrList, Hashtable origin)
    {
        if (arrList == null)
        {
            return;
        }

        dicEventGroup.Clear();

        for (int i = 0; i < arrList.Count; i++)
        {
            Hashtable headData = (Hashtable)arrList[i];
            uint contentsID = uint.Parse(headData["eventType"].ToString());
            EContentsEvent contentsType = (EContentsEvent)contentsID;
            ContentsEventGroup group = new ContentsEventGroup(headData, origin);

            switch (contentsType) // 타입 형태가 다르면 추가하여 셋팅할것
            {
                case EContentsEvent.Attendance:
                    AttendanceHelper.AddGroup(group);
                    break;

                default:
                    if (dicEventGroup.ContainsKey(contentsType))
                    {
                        Debug.Log("ContentsEvent Exist Key:" + contentsType.ToString(), Color.magenta);
                        continue;
                    }
                    dicEventGroup.Add(contentsType, group);
                    break;
            }
        }
    }

    // Set Event Mission(미션키로 안에 실질적인 내용물 셋팅, 다른 컨텐츠 같은 미션키 존재)
    static public void SetMission(Hashtable data)
    {
        if (data == null)
        {
            return;
        }

        for (int i = 0; i < AllEventGroupList.Count; i++)
        {
            for (int j = 0; j < AllEventGroupList[i].EventInfoList.Count; j++)
            {
                ContentsEventInfo info = AllEventGroupList[i].EventInfoList[j];
                string key = info.MissionKey.ToString();
                if (data.ContainsKey(key))
                {
                    Hashtable missionData = (Hashtable)data[key];
                    info.UpdateMission(missionData);
                    info.UpdateCompleteInfo();
                }
            }
        }
    }
}