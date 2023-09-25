using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.ParticleSystem;

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

    public enum RenderMethod
    {
        MeshFace,
        MeshPoint,
        ParticlePoint,
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
        /// colors for muscles
        /// </summary>
        public Color[] m_colors;
        /// <summary>
        /// the carrier of the mesh and visualization for this muscle
        /// </summary>
        public GameObject m_obj;

        /// <summary>
        /// Below belongs to render parts, we need to decouple it.
        /// </summary>
        private MeshFilter meshFilter;
        public Mesh mesh { get; set; }
        private MeshRenderer meshRender;

        private ParticleSystem particleSystem;
        private ParticleSystem.Particle[] m_particles;
        private ARPointCloudManager arPointcloud;
        //private ARParticleVisualizer arParticleVisualizer;
        /// <summary>
        /// above need to be decoupled
        /// </summary>


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

        private static Color SelectMuscleColor(MuscleEnum inputType)
        {
            switch (inputType)
            {
                case MuscleEnum.Biceps:
                    return Color.blue;
                case MuscleEnum.Brachialis:
                    return Color.red;
                case MuscleEnum.Brachiorad:
                    return Color.green;
                case MuscleEnum.Anoneus:
                    return Color.magenta;
                case MuscleEnum.Triceps:
                    return Color.yellow;
                default:
                    return Color.blue;
            }
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
            List<Color> tempColor = new List<Color>();
            List<int> tempIndecies = new List<int>();
            //put all data from distribution text asset into the list
            for (int i = 0; i < fileLines.Length; i++)
            {
                switch (fileLines[i].Split(' ')[0])
                {
                    case "v":
                        tempVertices.Add(0.001f * new Vector3(float.Parse(fileLines[i].Split(' ')[1]), float.Parse(fileLines[i].Split(' ')[2]), float.Parse(fileLines[i].Split(' ')[3])));
                        tempColor.Add(SelectMuscleColor(inputType));
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
            m_colors = tempColor.ToArray();
            numOfPoints = m_vertices.Length;

            Model runtimeModel = ModelLoader.Load(inputModel);
            m_worker = WorkerFactory.CreateWorker(WorkerFactory.GetBestTypeForDevice(WorkerFactory.Device.CPU), runtimeModel);

        }
        /// <summary>
        /// initialization before rendering
        /// </summary>
        public void InitCarrier(RenderMethod renderMethod = RenderMethod.MeshFace)
        {
            m_obj = new GameObject(m_muscleType.ToString());
            m_obj.transform.position = Vector3.zero;
            MeshUpperEnd = new GameObject("meshUpperEnd");
            MeshUpperEnd.transform.SetParent(m_obj.transform);
            MeshLowerEnd = new GameObject("meshLowerEnd");
            MeshLowerEnd.transform.SetParent(m_obj.transform);

            if (renderMethod.Equals(RenderMethod.ParticlePoint))
            {
                // set particle system
                particleSystem = m_obj.AddComponent<ParticleSystem>();
                //arPointcloud = m_obj.AddComponent<ARPointCloudManager>();
                //arParticleVisualizer = m_obj.AddComponent<ARParticleVisualizer>();
                var pmain = particleSystem.main;
                pmain.startSpeed = 0f;
                pmain.startLifetime = 99999f;
                pmain.maxParticles = 100000;
                pmain.startSize = 100f;
                pmain.startColor = SelectMuscleColor(m_muscleType);
                pmain.loop = false;

                // set particleRender
                var pRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                pRenderer.alignment = ParticleSystemRenderSpace.Local;
                pRenderer.renderMode = ParticleSystemRenderMode.Billboard;
                Material mat_Cloudpoints = new Material(Shader.Find("Custom/PointCloud"));
                pRenderer.material = mat_Cloudpoints;

                // no need to use emission
                var emission = particleSystem.emission;
                emission.enabled = false;
            }
            else
            {
                // set MeshRender and bind material
                meshRender = m_obj.AddComponent<MeshRenderer>();
                meshFilter = m_obj.AddComponent<MeshFilter>();
                meshRender.sharedMaterial = GlobalCtrl.M_FaceVisualizer.Mat_Muscle;
                mesh = meshFilter.mesh;
            }
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

        public void UpdateParticle()
        {
            #region initialize the posture of muscle object everytime
            m_obj.transform.position = Vector3.zero;
            m_obj.transform.rotation = Quaternion.identity;
            m_obj.transform.localScale = Vector3.one;
            m_obj.transform.SetParent(null);
            #endregion

        }

        public void UpdateMeshPointSetting()
        {
            /// Set Mesh
            mesh.vertices = m_vertices;
            mesh.colors = m_colors;
            mesh.SetIndices(Enumerable.Range(0, numOfPoints).ToArray(), MeshTopology.Points, 0);

        }

        public void UpdateMeshFaceSetting()
        {
            /// Set Mesh
            mesh.vertices = m_vertices;
            mesh.colors = m_colors;
            mesh.triangles = m_triangles;
            mesh.SetNormals(m_normals);

        }

        public void UpdateParticleSetting()
        {
            // Check if the array of particles need to be renewed
            if (m_particles == null || m_particles.Length < numOfPoints)
            {
                m_particles = new ParticleSystem.Particle[numOfPoints];
            }

            // Set each particle
            for (int i = 0; i < numOfPoints; i++)
            {
                m_particles[i].position = m_vertices[i];
                m_particles[i].startColor = SelectMuscleColor(m_muscleType);
                m_particles[i].startSize = 10f;
                m_particles[i].startLifetime = 50f;
                m_particles[i].remainingLifetime = 50f;
            }

            // Set particles into particle system
            particleSystem.SetParticles(m_particles, m_particles.Length);

            // For debug, check the generated particles
            var particles = m_particles;
            int particleCount = particleSystem.GetParticles(particles);
            Debug.Log(particleCount);

        }


        /// <summary>
        /// refresh the rendering
        /// </summary>
        public void UpdateVisualizer(RenderMethod renderMethod)
        {
            #region initialize the posture of muscle object everytime
            m_obj.transform.position = Vector3.zero;
            m_obj.transform.rotation = Quaternion.identity;
            m_obj.transform.localScale = Vector3.one;
            m_obj.transform.SetParent(null);
            #endregion

            /// Update Mesh settings
            if (renderMethod.Equals(RenderMethod.MeshFace))
            {
                UpdateMeshFaceSetting();
            }
            else if (renderMethod.Equals(RenderMethod.MeshPoint))
            {
                UpdateMeshPointSetting();
            }
            else if (renderMethod.Equals(RenderMethod.ParticlePoint))
            {
                UpdateParticleSetting();
            }
            else
            {
                UpdateMeshFaceSetting();
            }

            //GameObject trans = GlobalCtrl.M_FaceVisualizer.rawArm.dic_muscleTrans[m_muscleType];
            GameObject dataUpperEnd = GlobalCtrl.M_FaceVisualizer.rawArm.dic_UpperEnds[m_muscleType];
            GameObject dataLowerEnd = GlobalCtrl.M_FaceVisualizer.rawArm.dic_LowerEnds[m_muscleType];
            TendonInfo tendonInfo = GlobalCtrl.M_FaceVisualizer.rawArm.tendonInfo.SearchMuscle(m_muscleType);

            MeshUpperEnd.transform.position = m_vertices[tendonInfo.upperID];
            MeshLowerEnd.transform.position = m_vertices[tendonInfo.lowerID];


            float ratio = Vector3.Distance(dataUpperEnd.transform.position, dataLowerEnd.transform.position) / Vector3.Distance(MeshUpperEnd.transform.position, MeshLowerEnd.transform.position);
            m_obj.transform.localScale = ratio * Vector3.one;

            Vector3 targetVec = dataLowerEnd.transform.position - dataUpperEnd.transform.position;
            Quaternion q = Quaternion.FromToRotation(MeshLowerEnd.transform.position - MeshUpperEnd.transform.position, targetVec);
            m_obj.transform.rotation *= q;

            m_obj.transform.position = dataUpperEnd.transform.position - MeshUpperEnd.transform.position;
            Vector3 thisUp = Vector3.ProjectOnPlane(m_obj.transform.up, GlobalCtrl.M_TrackManager.LS2E).normalized;
            float tempAngle = Vector3.SignedAngle(GlobalCtrl.M_TrackManager.UpperNormal, thisUp, GlobalCtrl.M_TrackManager.LS2E);
            m_obj.transform.RotateAround(dataUpperEnd.transform.position, GlobalCtrl.M_TrackManager.LS2E, GlobalCtrl.M_FaceVisualizer.rawArm.dic_rot[m_muscleType] - tempAngle);


        }


    }

}
