using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARFoundation.Samples;
public class GlobalCtrl : MonoBehaviour
{
    private static GlobalCtrl instance;
    private static TrackManager trackManager;
    private static ActiManager actiManager;
    private static MeshManager meshManager;
    private static FaceVisualizer faceVisualizer;
    private static UIManager uiManager;

    public static GlobalCtrl M_Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GlobalCtrl>();
            return instance;
        }
    }
    public static TrackManager M_TrackManager
    {
        get
        {
            if (trackManager == null)
                trackManager = FindObjectOfType<TrackManager>();
            return trackManager;
        }
    }
    public static ActiManager M_ActiManager
    {
        get
        {
            if (actiManager == null)
                actiManager = FindObjectOfType<ActiManager>();
            return actiManager;
        }
    }
    public static MeshManager M_MeshManager
    {
        get
        {
            if (meshManager == null)
                meshManager = FindObjectOfType<MeshManager>();
            return meshManager;
        }
    }
    public static FaceVisualizer M_FaceVisualizer
    {
        get
        {
            if (faceVisualizer == null)
                faceVisualizer = FindObjectOfType<FaceVisualizer>();
            return faceVisualizer;
        }
    }
    public static UIManager M_UIManager
    {
        get
        {
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
            return uiManager;
        }
    }




    // Start is called before the first frame update.
    void Start()
    {
        M_UIManager.f_Init();//Initialize the UI manager first, then initialize the other functions  by interacting with the UI.


        //M_FaceVisualizer.rawArm.f_Init();
    }

    // Update is called once per frame.
    /// <summary>
    /// There is no Update functions in any other scripts.
    /// I collect all of the Update functions here, so that they will happen in the order I set.
    /// </summary>
    void FixedUpdate()
    {
        //input: elbow movement data
        //output: muscle activation level of 5 muscles
        M_ActiManager.UpdateActivation();
        //input: muscle activation level
        //output: the new distribution of vertices
        M_MeshManager.UpdateMesh();
        //input: the new distribution of vertices
        //output: new rendered visualization
        M_FaceVisualizer.UpdateVisualizer();
    }

}
