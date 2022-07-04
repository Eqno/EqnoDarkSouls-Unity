using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIntelligence : MonoBehaviour
{
    private VirtualInput VirtualInput;
    private EnemyMotionController MotionController;

    void Awake()
    {
        VirtualInput = GetComponent<VirtualInput>();
        MotionController = GetComponent<EnemyMotionController>();
    }

    public void INPUT()
    {
        MotionController.MoveDirection = (MotionController.LockTarget.transform.position - transform.position).normalized;
        MotionController.unitMagnitude = MotionController.forceFore = 1;

        if (Vector3.Distance(MotionController.LockTarget.transform.position, transform.position) < 3)
        {
            VirtualInput.SetRightSlashDown();
        }
    }
}
