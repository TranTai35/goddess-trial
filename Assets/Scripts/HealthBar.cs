using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Image healthBar;

    public void SetHealthBar(float maxHealth, float curHealth)
    {
        healthBar.fillAmount = curHealth / maxHealth;
    }
}
