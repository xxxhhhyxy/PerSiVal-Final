using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System;
using System.Linq;

public class MeshCtrl : MonoBehaviour
{
    bool isFullModel = true;
    /// <summary>
    /// is this module activated?
    /// </summary>
    bool isInited = false;
    /// <summary>
    /// the NNModel, the core
    /// </summary>
    public NNModel onnxFile;
    /// <summary>
    /// the executable version of the NNModel
    /// </summary>
    private Model m_RuntimeModel;
    /// <summary>
    /// the worker processer for the model
    /// </summary>
    private IWorker m_Worker_compl;
    /// <summary>
    /// the given txt file, to know how David create a mesh surface
    /// </summary>
    public TextAsset Objfile;
    /// <summary>
    /// triangular faces, which is not used in your case, but keep it
    /// </summary>
    private int[] triangles;
    /// <summary>
    /// normal vectors from the file,which is not used in your case, but keep 
    /// </summary>
    private Vector3[] normals;
    /// <summary>
    /// vertices from the file, but later overwritten by the data from the NNModel
    /// </summary>
    private Vector3[] vertices;
    /// <summary>
    /// the output values of NNModel
    /// </summary>
    float[] outputCoords;
    List<Vector3> list_vertices;
    public int[] M_Triangles { get { return triangles; } }
    public Vector3[] M_Normals { get { return normals; } }
    public Vector3[] M_Vertices { get { return vertices; } }
    /// <summary>
    /// the activation NNModel, the core
    /// </summary>
    public NNModel actiOnnx;
    /// <summary>
    /// the NNModel, the core
    /// </summary>
    public NNModel interpOnnx;
    /// <summary>
    /// the executable version of the NNModel
    /// </summary>
    private Model m_ActiModel;
    /// <summary>
    /// the executable version of the NNModel
    /// </summary>
    private Model m_InterpModel;
    /// <summary>
    /// the worker processer for the model
    /// </summary>
    private IWorker m_Worker_Acti;
    /// <summary>
    /// the worker processer for the interpolation model
    /// </summary>
    private IWorker m_Worker_Interp;
    /// <summary>
    /// the output values of independent activation NNModel
    /// </summary>
    float[] ActiCoords;




    /// <summary>
    /// call by UIManager.cs
    /// </summary>
    public void f_Init(bool isOn)
    {
        isFullModel = isOn;
        ReadObjFile();

        if (isFullModel)
        {
            m_RuntimeModel = ModelLoader.Load(onnxFile);
            //m_Worker_compl = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, m_RuntimeModel);//for HoloLens
            m_Worker_compl = WorkerFactory.CreateWorker(WorkerFactory.GetBestTypeForDevice(WorkerFactory.Device.CPU), m_RuntimeModel);
        }
        else
        {
            m_ActiModel = ModelLoader.Load(actiOnnx);
            m_InterpModel = ModelLoader.Load(interpOnnx);
            m_Worker_Acti = WorkerFactory.CreateWorker(WorkerFactory.GetBestTypeForDevice(WorkerFactory.Device.CPU), m_ActiModel);
            m_Worker_Interp = WorkerFactory.CreateWorker(WorkerFactory.GetBestTypeForDevice(WorkerFactory.Device.CPU), m_InterpModel);

        }


        isInited = true;
    }

    /// <summary>
    /// get the basic distribution of the arm rendering model
    /// </summary>
    private void ReadObjFile()
    {
        string[] fileLines = Objfile.text.Split('\n');
        List<Vector3> tempVertices = new List<Vector3>();
        List<int> tempTriangle = new List<int>();
        List<Vector3> tempVN = new List<Vector3>();
        for (int i = 0; i < fileLines.Length; i++)
        {
            switch (fileLines[i].Split(' ')[0])
            {
                case "v":
                    tempVertices.Add(0.001f * new Vector3(float.Parse(fileLines[i].Split(' ')[1]), float.Parse(fileLines[i].Split(' ')[2]), float.Parse(fileLines[i].Split(' ')[3])));
                    break;
                case "vn"://vertical normal, don't need to care
                    tempVN.Add(new Vector3(float.Parse(fileLines[i].Split(' ')[1]), float.Parse(fileLines[i].Split(' ')[2]), float.Parse(fileLines[i].Split(' ')[3])));
                    break;
                case "f":
                    {
                        string[] words = fileLines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string msg = String.Join(" ", words).Replace("//", "/");
                        int a = int.Parse(msg.Split(' ')[1].Split('/')[0]);
                        int b = int.Parse(msg.Split(' ')[2].Split('/')[0]);
                        int c = int.Parse(msg.Split(' ')[3].Split('/')[0]);
                        tempTriangle.Add(a - 1);
                        tempTriangle.Add(b - 1);
                        tempTriangle.Add(c - 1);
                    }
                    break;
                default:
                    break;
            }
        }
        //Debug.Log(tempVertices[tempVertices.Count - 1].ToString("F2"));

        //mesh.vertices = tempVertices.ToArray();
        //mesh.uv = uvs;
        //mesh.uv2 = uvs;
        vertices = tempVertices.ToArray();
        triangles = tempTriangle.ToArray();
        normals = tempVN.ToArray();
        //mesh.triangles = triangles;
        //mesh.RecalculateNormals();
        //meshRender.sharedMaterial = mat_Biceps;
    }
    /// <summary>
    /// this is called by GlobalCtrl.cs
    /// </summary>
    public void UpdateMesh()
    {
        if (!isInited)
            return;
        if (!GlobalCtrl.M_Instance.M_HumanBodyTracker.isTracked)
            return;
        if (isFullModel)
        {
            int[] shape_Full = { 1, 1, 1, 5 };
            outputCoords = Predict_Reduced(GlobalCtrl.M_MuscleActiCtrl.activations,shape_Full,m_Worker_compl).ToArray();
        }
        else
        {
            int[] shape_Acti = { 1, 1, 1, 5 };
            int[] shape_Interp = { 1, 1, 1, 30 };
            ActiCoords = Predict_Reduced(GlobalCtrl.M_MuscleActiCtrl.activations, shape_Acti, m_Worker_Acti).ToArray();
            outputCoords = Predict_Reduced(ActiCoords, shape_Interp, m_Worker_Interp).ToArray();

        }



        ReadVertices();
        GlobalCtrl.M_UIManager.f_Txt_Debug(GlobalCtrl.M_Instance.LShoulder.ToString("F2")+
            vertices[0].ToString("F2")+"NNN"+ vertices[10].ToString("F2"));
    }

    /// <summary>
    /// calculate the distribution of vertices via NNModel
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public List<float> Predict_Reduced(float[] input,int[] shape,IWorker _worker)
    {
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        //int[] shape = { 1, 1, 1, 5 };
        Tensor input_acts = new Tensor(shape, input);
        watch.Restart();
        Tensor output = _worker.Execute(input_acts).PeekOutput();//only work in main thread
        new WaitForCompletion(output);
        input_acts.Dispose();
        watch.Stop();
        //Debug.Log(output_x[0, 0, 0, 0]);
        //Debug.Log(output_x[1, 0, 0, 0]);
        //Debug.Log(output_x[2, 0, 0, 0]);
        //Tensor output = new Tensor(output_x.shape.Squeeze(),output_x.ToReadOnlyArray());
        //output_x.Print();
        //output.Print();
        //Debug.Log(output_x.AsFloats()[3]+"/"+ output_x.AsFloats().ToList()[4] + "/"+ output_x.AsFloats().ToList()[5] + "/");        
        //Debug.Log(output_x.AsFloats()[2812] + "/" + output_x.AsFloats().ToList()[2813] + "/" + output_x.AsFloats().ToList()[2814] + "/");
        //Debug.Log(output_x.AsFloats()[5618] + "/" + output_x.AsFloats().ToList()[5619] + "/" + output_x.AsFloats().ToList()[5620] + "/");
        //output_x[0, 0, 0,:].AsFloats().ToList()
        return output.AsFloats().ToList();
    }
    private void ReadVertices()
    {

        if (list_vertices == null)
            list_vertices = new List<Vector3>();
        list_vertices.Clear();
        for (int i = 0; i < 2809; i++)
        {
            list_vertices.Add(new Vector3(0.001f * outputCoords[i], 0.001f * outputCoords[i + 2809], 0.001f * outputCoords[i + 2 * 2809]));
        }
        vertices = list_vertices.ToArray();
    }
}
