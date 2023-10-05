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
    private bool isArmReady = false;
    public RenderMethod renderMethod = RenderMethod.MeshFace;
    private Mesh mesh;
    public Shader shader_PointCloud;
    public void f_Init(RenderMethod renderOpt)
    {
        if (!isArmReady)
        {
            rawArm.f_Init();
            isArmReady = true;
        }
        InitMuscles(renderOpt);
    }
    public void InitMuscles(RenderMethod renderOpt)
    {
        renderMethod = renderOpt;
        if (renderMethod.Equals(RenderMethod.MeshPoint))
        {
            //Material mat_Cloudpoints = new Material(Shader.Find("Custom/PointCloud"));
            Material mat_Cloudpoints = new Material(shader_PointCloud);
            Mat_Muscle = mat_Cloudpoints;
        }
        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {
            a.Value.InitCarrier(renderMethod);

        }

        isInited = true;
    }
    public void ClearRender()
    {
        foreach (var a in GlobalCtrl.M_MeshManager.Dic_trackedMuscle)
        {
            a.Value.ClearRender();
        }
        isInited = false;
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
