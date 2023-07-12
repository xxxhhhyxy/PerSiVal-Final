using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Barracuda;
using System.Linq;

namespace StructClass
{

    /// <summary>
    /// the enumeration of different muscles
    /// </summary>
    public enum MuscleEnum
    {
        Biceps = 0,
        Brachialis = 1,
        Brachiorad = 2,
        Anoneus = 3,
        Triceps = 4
    }
    /// <summary>
    /// the generic structure
    /// after defining the T, then a structure with 5 sub-variables will be generated
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public struct MuscleStruct<T>
    {
        public T Biceps;
        public T Brachialis;
        public T Brachiorad;
        public T Anoneus;
        public T Triceps;
        /// <summary>
        /// find the corresponding muscle stuff with the input muslce type
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public T SearchMuscle(MuscleEnum input)
        {
            switch (input)
            {
                case MuscleEnum.Biceps:
                    return Biceps;
                case MuscleEnum.Brachialis:
                    return Brachialis;
                case MuscleEnum.Brachiorad:
                    return Brachiorad;
                case MuscleEnum.Anoneus:
                    return Anoneus;
                case MuscleEnum.Triceps:
                    return Triceps;
                default:
                    return Biceps;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public struct TendonInfo
    {
        public int upperID;
        public Vector3 upperPos;
        public int lowerID;
        public Vector3 lowerPos;
    }
    /// <summary>
    /// the core, the class of a muscle
    /// </summary>
    [Serializable]
    public class MuscleClass
    {
        /// <summary>
        /// the type of the muscle
        /// </summary>
        public MuscleEnum m_muscleType { get; private set; }
        private NNModel m_model;
        public IWorker m_worker;
        /// <summary>
        /// triangular faces, which is not used in your case, but keep it
        /// </summary>
        public int[] m_triangles;
        /// <summary>
        /// normal vectors from the file,which is not used in your case, but keep 
        /// </summary>
        public Vector3[] m_normals;
        /// <summary>
        /// vertices from the file, but later overwritten by the data from the NNModel
        /// </summary>
        public Vector3[] m_vertices;
        /// <summary>
        /// the carrier of the mesh and visualization for this muscle
        /// </summary>
        public GameObject m_obj;
        private MeshFilter meshFilter;
        private Mesh mesh;
        private MeshRenderer meshRender;


        private GameObject MeshUpperEnd;
        private GameObject MeshLowerEnd;
        private bool debugTab = false;
        private int numOfPoints;
        /// <summary>
        /// the output values of NNModel
        /// </summary>
        float[] outputCoords;
        List<Vector3> list_vertices;

  

        public static Vector3 ReflectX(Vector3 input)
        {
            return new Vector3(-input.x, input.y, input.z);
        }

        /// <summary>
        /// the data processor of muscle
        /// </summary>
        /// <param name="inputType">the type of this muscle (of the 5 predined)</param>
        /// <param name="inputText">the predefined distribution of muslce object</param>
        /// <param name="inputModel">the NNModel of this muscle</param>
        public MuscleClass(MuscleEnum inputType, TextAsset inputText, NNModel inputModel)
        {
            m_muscleType = inputType;
            //each line of the distribution text asset
            string[] fileLines = inputText.text.Split('\n');
            List<Vector3> tempVertices = new List<Vector3>();
            List<int> tempTriangle = new List<int>();
            List<Vector3> tempVN = new List<Vector3>();
            //put all data from distribution text asset into the list
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
            m_vertices = tempVertices.ToArray();
            m_triangles = tempTriangle.ToArray();
            m_normals = tempVN.ToArray();
            numOfPoints = m_vertices.Length;

            Model runtimeModel = ModelLoader.Load(inputModel);
            m_worker = WorkerFactory.CreateWorker(WorkerFactory.GetBestTypeForDevice(WorkerFactory.Device.CPU), runtimeModel);

        }
        /// <summary>
        /// initialization before rendering
        /// </summary>
        public void InitCarrier()
        {
            m_obj = new GameObject(m_muscleType.ToString());
            m_obj.transform.position = Vector3.zero;
            MeshUpperEnd = new GameObject("meshUpperEnd");
            MeshUpperEnd.transform.SetParent(m_obj.transform);
            MeshLowerEnd = new GameObject("meshLowerEnd");
            MeshLowerEnd.transform.SetParent(m_obj.transform);
            meshFilter = m_obj.AddComponent<MeshFilter>();
            meshRender = m_obj.AddComponent<MeshRenderer>();
            meshRender.sharedMaterial = GlobalCtrl.M_FaceVisualizer.Mat_Muscle;
            mesh = meshFilter.mesh;
            GlobalCtrl.M_UIManager.f_txt_debug(m_muscleType.ToString() + " mesh");


        }
        /// <summary>
        /// update the mesh data of this muscle, but for now just the data
        /// calculating the mesh is ok, how to render them is a critical problem, due to the lack of rendering power
        /// </summary>
        /// <param name="activations"></param>
        public void UpdateMesh(float[] activations)
        {
            //Debug.Log(activations[0] + "/" + activations[1] + "/" + activations[2]);
            int[] shape_Full = { 1, 1, 1, 5 };
            outputCoords = Predict_Reduced(activations, shape_Full, m_worker).ToArray();
            if (list_vertices == null)
                list_vertices = new List<Vector3>();
            list_vertices.Clear();
            for (int i = 0; i < numOfPoints; i++)
            {
                Vector3 output = new Vector3(outputCoords[i], outputCoords[i + numOfPoints], outputCoords[i + 2 * numOfPoints]);
                //list_vertices.Add(0.001f * output);
                list_vertices.Add(MuscleClass.ReflectX(output));
            }
            m_vertices = list_vertices.ToArray();
            //Debug.Log(m_vertices[0].ToString("F3"));
        }

        /// <summary>
        /// calculate the distribution of vertices via NNModel
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<float> Predict_Reduced(float[] input, int[] shape, IWorker _worker)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //int[] shape = { 1, 1, 1, 5 };
            Tensor input_acts = new Tensor(shape, input);
            watch.Restart();
            Tensor output = _worker.Execute(input_acts).PeekOutput();//only work in main thread
            new WaitForCompletion(output);
            input_acts.Dispose();
            watch.Stop();
            return output.AsFloats().ToList();
        }


        /// <summary>
        /// refresh the rendering
        /// </summary>
        public void UpdateVisualizer()
        {
            m_obj.transform.position = Vector3.zero;
            m_obj.transform.rotation = Quaternion.identity;
            m_obj.transform.localScale = Vector3.one;
            m_obj.transform.SetParent(null);

            mesh.vertices = m_vertices;
            mesh.triangles = m_triangles;
            mesh.SetNormals(m_normals);


            //GameObject trans = GlobalCtrl.M_FaceVisualizer.rawArm.dic_muscleTrans[m_muscleType];
            GameObject dataUpperEnd = GlobalCtrl.M_FaceVisualizer.rawArm.dic_UpperEnds[m_muscleType];
            GameObject dataLowerEnd = GlobalCtrl.M_FaceVisualizer.rawArm.dic_LowerEnds[m_muscleType];
            TendonInfo tendonInfo = GlobalCtrl.M_FaceVisualizer.rawArm.tendonInfo.SearchMuscle(m_muscleType);

            MeshUpperEnd.transform.position =mesh.vertices[tendonInfo.upperID];
            MeshLowerEnd.transform.position =mesh.vertices[tendonInfo.lowerID];


            float ratio = Vector3.Distance(dataUpperEnd.transform.position, dataLowerEnd.transform.position) / Vector3.Distance(MeshUpperEnd.transform.position, MeshLowerEnd.transform.position);
            m_obj.transform.localScale = ratio * Vector3.one;

            Vector3 targetVec = dataLowerEnd.transform.position - dataUpperEnd.transform.position;
            Quaternion q = Quaternion.FromToRotation(MeshLowerEnd.transform.position - MeshUpperEnd.transform.position,targetVec);
            m_obj.transform.rotation*=q;

            m_obj.transform.position = dataUpperEnd.transform.position - MeshUpperEnd.transform.position;
            Vector3 thisUp = Vector3.ProjectOnPlane(m_obj.transform.up,GlobalCtrl.M_TrackManager.LS2E).normalized;
            float tempAngle=Vector3.SignedAngle(GlobalCtrl.M_TrackManager.UpperNormal ,thisUp, GlobalCtrl.M_TrackManager.LS2E);
            m_obj.transform.RotateAround(dataUpperEnd.transform.position, GlobalCtrl.M_TrackManager.LS2E, GlobalCtrl.M_FaceVisualizer.rawArm.dic_rot[m_muscleType] - tempAngle);

            /*
             
            //Debug.Log(Vector3.Distance(MeshUpperEnd.transform.position, MeshLowerEnd.transform.position) +"/"+
            //    Vector3.Distance(dataUpperEnd.transform.position, dataLowerEnd.transform.position));
            //float ratio = Vector3.Distance(dataUpperEnd.transform.position, dataLowerEnd.transform.position)/Vector3.Distance(MeshUpperEnd.transform.position, MeshLowerEnd.transform.position) ;
            //m_obj.transform.localScale = ratio * Vector3.one;
            //trans.transform.position = (MeshUpperEnd.transform.position + MeshLowerEnd.transform.position) / 2;
            //m_obj.transform.SetParent(trans.transform);
            //trans.transform.position = (dataUpperEnd.transform.position + dataLowerEnd.transform.position) / 2;
            //trans.transform.LookAt(MeshUpperEnd.transform.position, GlobalCtrl.M_TrackManager.LowerNormal);
            */





            //float ratio = Vector3.Distance((GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsUpper[m_muscleType].transform.position),
            //    (GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsLower[m_muscleType].transform.position))
            //    / Vector3.Distance(upperEnds, lowerEnds);

            //Vector3 curVec = lowerEnds - upperEnds;
            //Vector3 tarVec = GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsLower[m_muscleType].transform.position - GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsUpper[m_muscleType].transform.position;
            //Quaternion rot = Quaternion.FromToRotation(curVec, tarVec);

            //Vector3 upperOffset = upperEnds - m_obj.transform.position;
            //m_obj.transform.localScale = Vector3.one * ratio;
            //Debug.Log(ratio * upperOffset);
            //Debug.Log(GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsUpper[m_muscleType].transform.position);
            //m_obj.transform.position = GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsUpper[m_muscleType].transform.position + ratio * upperOffset;
            //m_obj.transform.rotation = rot * m_obj.transform.rotation;
            //if (!debugTab)
            //{
            //    GlobalCtrl.M_UIManager.f_txt_debug(upperOffset.ToString() + "\n" +
            //        "tar " + GlobalCtrl.M_FaceVisualizer.rawArm.tendonSlotsUpper[m_muscleType].transform.position.ToString() + "\n"
            //        + "cur " + upperEnds.ToString() + "\n"
            //        + "obj " + m_obj.transform.position.ToString() + "\n");

            //}
        }
    }

}
