using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSoundManager : MonoBehaviour, IAttackListener
{
    [SerializeField] private AudioClip[] attackStartSounds;
    [SerializeField] private AudioClip[] attackHitSounds;
    [SerializeField] private AudioClip[] attackEndSounds;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    void Start()
    {
        FindObjectOfType<AttackEventListener>().RegisterListener(this);
    }
    
    public void OnAttackStart(int attackType)
    {
        // El índice del array corresponde al tipo de ataque - 1
        if (attackStartSounds.Length >= attackType)
            audioSource.PlayOneShot(attackStartSounds[attackType - 1]);
    }
    
    public void OnAttackPerform(int attackType)
    {
        if (attackHitSounds.Length >= attackType)
            audioSource.PlayOneShot(attackHitSounds[attackType - 1]);
    }
    
    public void OnAttackEnd(int attackType)
    {
        if (attackEndSounds.Length >= attackType)
            audioSource.PlayOneShot(attackEndSounds[attackType - 1]);
    }
}