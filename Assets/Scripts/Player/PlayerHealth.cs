using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Microlight.MicroBar;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int playerMaxHp = 100;
    [SerializeField] private int playerCurrentHp;
    [SerializeField] private MicroBar hpBar;
    [SerializeField] private Image lowHpImage;

    private Color color;
    private Coroutine pulseCoroutine;

    private void Awake()
    {
        Instance = this;
        color = lowHpImage.color;
        color.a = 0f; // Start fully transparent
        lowHpImage.color = color;
    }

    private void Start()
    {
        playerCurrentHp = playerMaxHp;
        hpBar.Initialize(playerMaxHp);
        StartPulsing();
    }

    private void Update()
    {
        if (playerCurrentHp <= 20)
        {
            if (pulseCoroutine == null)
                StartPulsing();
        }
        else
        {
            if (pulseCoroutine != null)
            {
                StopCoroutine(pulseCoroutine); 
                pulseCoroutine = null;
                color.a = 0f; 
                lowHpImage.color = color;
            }
        }
    }

    private void StartPulsing()
    {
        pulseCoroutine = StartCoroutine(PulseCoroutine());
    }

    private IEnumerator PulseCoroutine()
    {
        float pulseDuration = 1f; 
        float maxAlpha = 0.5f; 

        while (true)
        {
            
            float pulseSpeedFactor = Mathf.Clamp01((21 - playerCurrentHp) / 20f); 
            pulseDuration = Mathf.Lerp(1f, 0.3f, pulseSpeedFactor); 

            
            if (playerCurrentHp <= 5)
            {
                pulseDuration = Mathf.Max(pulseDuration, 0.5f); 
            }
            
            for (float t = 0f; t < 1f; t += Time.deltaTime / pulseDuration)
            {
                color.a = Mathf.Lerp(0f, maxAlpha, t);
                lowHpImage.color = color;
                yield return null;
            }

            
            for (float t = 0f; t < 1f; t += Time.deltaTime / pulseDuration)
            {
                color.a = Mathf.Lerp(maxAlpha, 0f, t);
                lowHpImage.color = color;
                yield return null;
            }

            
            if (playerCurrentHp > 20)
            {
                break; 
            }
        }
    }

    public void ReceiveDamage(int damage)
    {
        playerCurrentHp = Mathf.Max(playerCurrentHp - damage, 0);
        hpBar.UpdateBar(playerCurrentHp);
    }

    public void Heal(int hpHealed)
    {
        playerCurrentHp = Mathf.Min(playerCurrentHp + hpHealed, playerMaxHp);
        hpBar.UpdateBar(playerCurrentHp);
    }
}
