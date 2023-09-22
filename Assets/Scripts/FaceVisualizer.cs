using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StructClass;
using UnityEngine.XR.ARFoundation;

public class FaceVisualizer : MonoBehaviour
{
    public RawArm rawArm;
    public Material Mat_Muscle;
    private bool isInited = false;
    public RenderMethod renderMethod = RenderMethod.MeshFace;
    private Mesh mesh;

    public void f_Init(RenderMethod renderOpt)
    {
        renderMethod = renderOpt;
        if (renderMethod.Equals(RenderMethod.MeshPoint)) {
            Material mat_Cloudpoints = new Material(Shader.Find("Custom/PointCloud"));
            Mat_Muscle = mat_Cloudpoints;
        }
        rawArm.f_Init();
        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {
            a.Value.InitCarrier(renderMethod);

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
            a.Value.UpdateVisualizer(renderMethod);
        }
    }

}
