using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeLookControl : MonoBehaviour
{
    public Cinemachine.CinemachineFreeLook freeLookCam;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            freeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
        } else
        {
            freeLookCam.m_XAxis.m_InputAxisName = "";
            freeLookCam.m_XAxis.m_InputAxisValue = 0;
        }
    }
}
