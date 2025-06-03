using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WolfAnimation
{
    public readonly static int IsChasing = Animator.StringToHash("IsRunning");
    public readonly static int Death = Animator.StringToHash("Death");
    public readonly static int IsWalking = Animator.StringToHash("IsWalking");
    public readonly static int IsMating = Animator.StringToHash("IsMating");
    public readonly static int Damaged = Animator.StringToHash("Damaged");
    public readonly static int IdleRandom1 = Animator.StringToHash("IdleRandom1");
    public readonly static int IdleRandom2 = Animator.StringToHash("IdleRandom2");
    public readonly static int IsEating = Animator.StringToHash("IsEating");
    public readonly static int IsHealing = Animator.StringToHash("IsHealing");
    public readonly static int Attack = Animator.StringToHash("Attack");
}
