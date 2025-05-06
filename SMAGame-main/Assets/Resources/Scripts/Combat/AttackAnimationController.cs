using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationController : MonoBehaviour, IAttackListener
{
    [SerializeField] private Animator uiAnimator;
    
    void Start()
    {
        FindObjectOfType<AttackEventListener>().RegisterListener(this);
    }
    
    public void OnAttackStart(int attackType)
    {
        switch (attackType)
        {
            case PlayerCombat.ATTACK_BASIC:
                uiAnimator.SetTrigger("ShowBasicAttackEffect");
                break;
            case PlayerCombat.ATTACK_SPECIAL:
                uiAnimator.SetTrigger("ShowSpecialAttackEffect");
                break;
        }
    }
    
    public void OnAttackPerform(int attackType) { }
    
    public void OnAttackEnd(int attackType) { }
}