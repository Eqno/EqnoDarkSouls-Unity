using UnityEngine;

public class VirtualInput : MonoBehaviour
{
    bool jump = false, leftSlash = false, rightSlash = false;
    public bool GetRun()
    {
        return false;
    }
    public bool GetJumpDown()
    {
        if (jump)
        {
            jump = false;
            return true;
        }
        return false;
    }
    public bool GetRollDown()
    {
        return false;
    }
    public bool GetJabDown()
    {
        return false;
    }
    public bool GetCrouchDown()
    {
        return false;
    }
    public bool GetLeftSlashDown()
    {
        return false;
    }
    public bool GetRightSlashDown()
    {
        if (rightSlash)
        {
            rightSlash = false;
            return true;
        }
        return false;
    }
    public bool GetDefense()
    {
        return false;
    }

    public void SetJumpDown()
    {
        jump = true;
    }
    public void SetLeftSlashDown()
    {
        leftSlash = true;
    }
    public void SetRightSlashDown()
    {
        rightSlash = true;
    }
}
