using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructClass;

public class FaceVisualizer : MonoBehaviour
{
    public RawArm rawArm;
    public Material Mat_Muscle;
    private bool isInited = false;
    public void f_Init()
    {
        rawArm.f_Init();
        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {
            a.Value.InitCarrier();
        }
        isInited = true;
    }

    public void UpdateVisualizer()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_UIManager.tg_tracked.isOn)
            return;
        rawArm.UpdateBone();
        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {
            a.Value.UpdateVisualizer();
        }
    }
}
