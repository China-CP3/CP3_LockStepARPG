using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    private Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isWPressed = Input.GetKey(KeyCode.W);
        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        if (isWPressed && isShiftPressed)
        {
            // W + Shift 按下，播放Run动画
            animator.SetBool("Walk", false);
            animator.SetBool("Run", true);
        }
        else if (isWPressed)
        {
            // 只按下W，播放Walk动画
            animator.SetBool("Walk", true);
            animator.SetBool("Run", false);
        }
        else
        {
            // 没有按下W，播放Idle动画
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
        }
    }
}
