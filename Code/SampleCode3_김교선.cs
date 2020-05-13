using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 운영툴로 부터 이벤트 미션들을 한 그룹으로 만드는 클래스
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