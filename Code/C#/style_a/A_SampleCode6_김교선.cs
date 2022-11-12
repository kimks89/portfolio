using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI슬라이드 연출관련(샘플소스코드)
public class UISliderAnimate : MonoBehaviour
{
    [SerializeField] private float SpeedTime; // 속도조절

    private UISlider uiSlider; // slider컴퍼넌트
    private bool isEnable = false; // 작동셋팅

    private float startValue = 0f; // 시작값
    private float targetValue = 0f; // 목표값
    private float elaspedTime = 0f; // 경과시간

    void Awake()
    {
        // slider컴퍼넌트 가져오기
        if (uiSlider == null)
        {
            uiSlider = GetComponent<UISlider>();
        }
    }

    void Update()
    {
        // 예외처리
        if (isEnable == false || uiSlider == null)
        {
            return;
        }

        // slider 증가
        if (elaspedTime < SpeedTime)
        {
            elaspedTime += Time.deltaTime;
            uiSlider.value = Mathf.Lerp(startValue, targetValue, elaspedTime / SpeedTime);
        }

        // slider 멈춤
        if (uiSlider.value >= 1.0f || elaspedTime >= SpeedTime)
        {
            StopSlider();
        }
    }

    // slider셋팅(시작,최대,목표값)
    void SetValue(float start, float max, float target)
    {
        float startPointValue = start / max;

        uiSlider.value = startPointValue;

        // 타겟이상이면 Max치로 변경
        if (target > max)
        {
            target = max;
        }

        startValue = startPointValue;
        targetValue = target / max;
    }

    // slider 시작
    public void PlaySlider()
    {
        isEnable = true;
        elaspedTime = 0f;
    }

    // slider 멈출
    public void StopSlider()
    {
        isEnable = false;
        elaspedTime = 0f;
    }
}
