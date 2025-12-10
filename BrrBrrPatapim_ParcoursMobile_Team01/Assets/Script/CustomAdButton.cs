using System;
using UnityEngine;

public class CustomAdButton : MonoBehaviour
{
    public void PlayRewardedAd()
    {
        AdService.Instance?.ShowRewardedAd();
    }
    
    public void PlayInterstitialAd()
    {
        AdService.Instance?.ShowInterstitialAd();
    }
    
    public void SetAdReward(string reward)
    {
        AdService.Instance?.SetCurrentReward(reward);
    }
}
