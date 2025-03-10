using System.Collections;
using UnityEngine;
using Microlight.MicroBar;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int playerMaxHp = 100;
    [SerializeField] private int playerCurrentHp;
    [SerializeField] private MicroBar hpBar;
    //[SerializeField] private int testDamage = 10;
    //[SerializeField] private float testInterval = 3f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerCurrentHp = playerMaxHp;
        hpBar.Initialize(playerMaxHp);
        //StartCoroutine(TestDamageRoutine()); 
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

   /* private IEnumerator TestDamageRoutine()
    {
        while (playerCurrentHp > 0)
        {
            yield return new WaitForSeconds(testInterval);
            ReceiveDamage(testDamage);
            Debug.Log($"Player took {testDamage} damage. Current HP: {playerCurrentHp}");
        }
    }*/ 
}