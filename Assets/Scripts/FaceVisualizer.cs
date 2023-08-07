using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructClass;

public class FaceVisualizer : MonoBehaviour
{
    public RawArm rawArm;
    public Material Mat_Muscle;
    public bool usingPointcloud = false;
    private bool isInited = false;
    public void f_Init(bool isPointcloud)
    {
        usingPointcloud = isPointcloud;
        if (usingPointcloud) {
            Material mat_Cloudpoints = new Material(Shader.Find("Custom/PointCloud"));
            Mat_Muscle = mat_Cloudpoints;
        }
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
            a.Value.UpdateVisualizer(usingPointcloud);
        }
    }

}
