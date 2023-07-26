using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation.Samples;

public class TrackManager : MonoBehaviour
{
    [HideInInspector]
    public Vector3 LShoulder;
    [HideInInspector]
    public Vector3 LElbow;
    [HideInInspector]
    public Vector3 LWrist;
    [HideInInspector]
    public Vector3 RShoulder;
    [HideInInspector]
    public Vector3 RElbow;
    [HideInInspector]
    public Vector3 RWrist;

    public HumanBodyTracker m_HumanBodyTracker;

    public Vector3 LS2E { get => LElbow - LShoulder; }
    public Vector3 LE2W { get => LWrist - LElbow; }
    public Vector3 RS2E { get => RElbow - RShoulder; }
    public Vector3 RE2W { get => RWrist - RElbow; }
    public Vector3 LowerNormal { get => Vector3.ProjectOnPlane(-LS2E, LE2W).normalized; }
    public Vector3 UpperNormal { get => Vector3.ProjectOnPlane(LE2W, LS2E).normalized; }
    //#region body angles
    ///// <summary>
    ///// the angle between upper arm and torso, its subscale on sagittal body plane, similar for the others
    ///// </summary>
    //public float Sagittal { get => Vector3.Angle(Vector3.ProjectOnPlane(LS2E, BodyLeft), Vector3.down); }
    //public float Frontal { get => Vector3.Angle(Vector3.ProjectOnPlane(LS2E, BodyForward), Vector3.down); }
    //public float Transverse { get => Vector3.Angle(Vector3.ProjectOnPlane(LS2E, Vector3.up), BodyForward); }
    //#endregion
    private void FixedUpdate()
    {
        GlobalCtrl.M_UIManager.tg_tracked.isOn = m_HumanBodyTracker.IsTracked;

        //if (!GlobalCtrl.M_UIManager.tg_tracked.isOn)
        //    return;

        //LShoulder = m_HumanBodyTracker.LShoulder;
        //LElbow = m_HumanBodyTracker.LElbow;
        //LWrist = m_HumanBodyTracker.LWrist;
        //RShoulder = m_HumanBodyTracker.RShoulder;
        //LElbow = m_HumanBodyTracker.RElbow;
        //RWrist = m_HumanBodyTracker.RWrist;

       
    }
}
