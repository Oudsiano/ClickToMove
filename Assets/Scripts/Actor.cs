using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Actor : MonoBehaviour
{
    public int maxHealth;

    public int currentHealth { get; private set; }

    public Slider helthBar;

    public void Update()
    {
        helthBar.value = currentHealth;
    }

    void Awake()
    { currentHealth = maxHealth; }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if(currentHealth <= 0)
        {  Death(); }
    }

    void Death()
    {
        Destroy(gameObject);
    }
}
