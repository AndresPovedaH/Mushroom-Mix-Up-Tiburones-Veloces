using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationS : MonoBehaviour
{
    Animator animator;
    int isRunHash;
    int isJumpHash;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        isRunHash = Animator.StringToHash("isRun");
        isJumpHash = Animator.StringToHash("isJump");
    }

    // Update is called once per frame
    void Update()
    {
        bool isjump = animator.GetBool(isJumpHash);
        bool isRun = animator.GetBool(isRunHash);
        bool forwardPressed = Input.GetKey("w");
        bool jumpPressed = Input.GetKey("space");

        if (!isRun && forwardPressed)
        {
            animator.SetBool(isRunHash, true);
        }

        if (isRun && !forwardPressed)
        {
            animator.SetBool(isRunHash, false);
        }

        if (!isjump && (forwardPressed && jumpPressed))
        {
            animator.SetBool(isJumpHash, true);
        }
        if (isjump && (!forwardPressed || !jumpPressed))
        {
            animator.SetBool(isJumpHash, false);
        }
    }
}
