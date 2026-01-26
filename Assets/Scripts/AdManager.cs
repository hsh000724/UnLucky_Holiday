using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    // [1] 광고 단위 ID 설정 (실제 AdMob ID로 교체 필수!)
    // 테스트 ID 사용 권장: Android 기준
    //private const string InterstitialAdUnitId = "ca-app-pub-1273595572389184/2006811303"; //전면
    //private const string RewardedAdUnitId = "ca-app-pub-1273595572389184/7799684725"; //보상형 
    private const string InterstitialAdUnitId = "ca-app-pub-3940256099942544/1033173712"; //테스트전면
    private const string RewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; //테스트보상형 

    private InterstitialAd _interstitialAd;
    private RewardedAd _rewardedAd;

    // 싱글톤 패턴 (어디서든 쉽게 접근 가능)
    public static AdManager Instance { get; private set; }

    // 보상형 광고 시청 완료 시 외부로 보낼 이벤트 (부활 로직 연결용)
    public event Action<bool> OnRewardedAdCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Google Mobile Ads SDK 초기화
        MobileAds.Initialize(initializationStatus =>
        {
            Debug.Log("AdMob SDK 초기화 완료.");
            // 초기화 완료 후 광고 로드
            LoadInterstitialAd();
            LoadRewardedAd();
        });
    }

    //
    // 1. 전면 광고 (Interstitial Ad) - 게임 종료 시
    //

    public void LoadInterstitialAd()
    {
        // 이미 로드된 광고가 있다면 정리
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        // 광고 요청 생성
        var adRequest = new AdRequest();

        // 광고 로드
        InterstitialAd.Load(InterstitialAdUnitId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"[Interstitial] 광고 로드 실패: {error.GetMessage()}");
                    return;
                }

                _interstitialAd = ad;
                Debug.Log("[Interstitial] 광고 로드 성공");

                // 광고 시청 종료 시 다음 광고 로드를 예약하는 이벤트 등록
                _interstitialAd.OnAdFullScreenContentClosed += HandleInterstitialClosed;
            });
    }

    public void ShowInterstitialAd()
    {
        if (_interstitialAd != null && _interstitialAd.CanShowAd())
        {
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogError("[Interstitial] 로드된 광고가 없거나 표시할 수 없는 상태입니다. 다시 로드합니다.");
            LoadInterstitialAd();
        }
    }

    // 광고가 닫힌 후 처리 (다음 광고 로드를 위한 재귀 호출)
    private void HandleInterstitialClosed()
    {
        Debug.Log("[Interstitial] 광고 시청 종료됨.");
        LoadInterstitialAd();
    }


    //
    // 2. 보상형 광고 (Rewarded Ad) - 플레이어 부활 시
    //

    public void LoadRewardedAd()
    {
        // 이미 로드된 광고가 있다면 정리
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        // 광고 요청 생성
        var adRequest = new AdRequest();

        // 광고 로드
        RewardedAd.Load(RewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null)
                {
                    Debug.LogError($"[Rewarded] 광고 로드 실패: {error.GetMessage()}");
                    return;
                }

                _rewardedAd = ad;
                Debug.Log("[Rewarded] 광고 로드 성공");
            });
    }

    public void ShowRewardedAd()
    {
        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            // 광고 표시 및 보상 처리 콜백 함수
            _rewardedAd.Show((Reward reward) =>
            {
                // [부활 로직] - 광고 시청 후 보상 지급 시 실행
                Debug.Log($"[Rewarded] 보상 지급 완료: {reward.Type}, {reward.Amount}");
                OnRewardedAdCompleted?.Invoke(true); // 외부 (GameManager)에 부활 신호 전달
            });

            // 광고 닫힘 이벤트도 연결하여 다음 광고를 바로 로드
            _rewardedAd.OnAdFullScreenContentClosed += HandleRewardedClosed;

        }
        else
        {
            Debug.LogError("[Rewarded] 로드된 광고가 없거나 표시할 수 없는 상태입니다.");
            OnRewardedAdCompleted?.Invoke(false); // 보상 실패 신호 전달
            LoadRewardedAd();
        }
    }

    // 광고가 닫힌 후 처리 (다음 광고 로드를 위한 재귀 호출)
    private void HandleRewardedClosed()
    {
        Debug.Log("[Rewarded] 광고 시청 종료됨.");
        LoadRewardedAd();
    }
}