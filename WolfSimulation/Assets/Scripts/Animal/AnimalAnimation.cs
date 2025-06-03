using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAnimation : MonoBehaviour
{
    [SerializeField] public Animator animator;

    readonly static int ISRUNNING = Animator.StringToHash("IsRunning");

    public void SetAnimationSpeed(float speed)
    {
        animator.speed = speed;
    }

    public void SetTrigger(int _id)
    {
        animator.SetTrigger(_id);
    }

    public void SetBool(int _id, bool isTrue)
    {
        animator.SetBool(_id, isTrue);
    }

    public void SetFloat(int _id, float value)
    {
        animator.SetFloat(_id, value);
    }

    public float GetAnimationTime()
    {
        return animator.GetNextAnimatorStateInfo(0).length;
    }
}
