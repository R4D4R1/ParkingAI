using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FoodButton : MonoBehaviour
{
    public event EventHandler OnUsed;

    private bool canUseButton;
    private void Awake()
    {
        canUseButton = true;
    }

    private void Start()
    {
        ResetButton();
    }

    public bool CanUseButton()
    {
        return canUseButton;
    }

    public void UseButton()
    {
        if (canUseButton)
        {
            canUseButton = false;
            OnUsed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ResetButton()
    {
        canUseButton = true;
    }
}
