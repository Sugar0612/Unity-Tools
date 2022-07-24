using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class ParticleData 
{
    [SerializeField]public ParticleSystem component;
    [SerializeField]public string name;
    [SerializeField]public float maxArea = 0.0f;
    [SerializeField]public float totalArea = 0.0f;
    [SerializeField]public float aveArea = 0.0f;

    [SerializeField]public int totalParticleCount = 0;
    [SerializeField]public int maxParticleCount = 0;
    [SerializeField]public float aveParticleCount = 0;
    public ParticleData() { }

    public ParticleData(ParticleSystem _Component)
    {
        component = _Component;
    }

    public ParticleData(string name, float Area = 0.0f, int ParticleCount = 0)
    {
        this.name = name;

        Update_Data(Area, ParticleCount);
    }

    public void Update_Data(float Area, int ParticlesCount)
    {
        maxArea = Mathf.Max(Area, maxArea);
        totalArea += Area;

        maxParticleCount = Mathf.Max(ParticlesCount, maxParticleCount);
        totalParticleCount += ParticlesCount;
    }

    public void Clone(ParticleData _p, int framCount)
    {
        _p.name = name;
        _p.maxArea = maxArea;
        _p.aveArea = totalArea / framCount * 1.0f;
        _p.maxParticleCount = maxParticleCount;
        _p.aveParticleCount = (float)totalParticleCount / (float)framCount;

        Debug.Log("ParticleSystem name is: " + _p.name +
            " | overDraw Max is: " + _p.maxArea + 
            " | ave overDraw is: " + _p.aveArea + 
            " | particle Max Count is: " + _p.maxParticleCount + 
            " | particle Ave Count is: " + _p.aveParticleCount);

    }
};

public interface IParticleCore
{
   void StartRecord(List<GameObject> gameObject);

   void StopRecord(List<ParticleData> data);

}

public class Particle : IParticleCore
{
    private static Particle _instance;
    private Dictionary<ParticleSystem, ParticleData> dic = new Dictionary<ParticleSystem, ParticleData>();
    ParticleSystem_Tools tool;
    public static Particle Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Particle();
                _instance.Initalize();
            }
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Initalize()
    {
        tool = new ParticleSystem_Tools();
    }

    //// Update is called once per frame
    void Update()
    {
        dic = tool.Run();
    }

    public void Close()
    {
        EditorApplication.update -= Update;
    }

    public void StartRecord(List<GameObject> gameObject)
    {
        tool.SetList(gameObject);
        tool.Set_can_use(true);
        EditorApplication.update += _instance.Update;
    }

    public void StopRecord(List<ParticleData> data)
    {
        //Debug.Log(dic.Count);

        tool.Set_can_use(false);
        foreach (var p in dic)
        {
            var key = p.Key;
            var val = p.Value;
            ParticleData buf_data = new ParticleData(key);
            p.Value.Clone(buf_data, tool.GetFramCount());

            data.Add(buf_data);
            //Debug.Log("ParticleSystem name is: " + key.name + " overDraw Max is: " + val.max_Area + " ave overDraw is: " + val.ave_Area);
        }
        EditorApplication.update -= _instance.Update;
    }

    private class ParticleSystem_Tools
    {
        bool can_use = false;
        List<GameObject> objs = new List<GameObject>();
        List<ParticleSystem> root_Ptcsyses = new List<ParticleSystem>();
        
        Dictionary<ParticleSystem, ParticleData> PtcsysAreaDic = new Dictionary<ParticleSystem, ParticleData>();
        List<ParticleSystem> list_Ptcsyses = new List<ParticleSystem>();
        ParticleSystem[] Ptcsyses;

        private ParticleSystemRenderMode mode = new ParticleSystemRenderMode();
        private ParticleSystemRenderer sysRender = new ParticleSystemRenderer();
        private List<Vector3> verices_ptc = new List<Vector3>();

        private int frames_count = 0;

        public ParticleSystem_Tools()
        {
            objs = new List<GameObject>();
        }

        public void SetList(List<GameObject> gameObjs)
        {
            objs = gameObjs;
        }

        private void mount()
        {
            for (int i = 0; i < objs.Count; ++i)
            {
                var Ptcsys = objs[i].GetComponent<ParticleSystem>();
                if (Ptcsys != null && root_Ptcsyses.Contains(Ptcsys) == false)
                {
                    //Debug.Log(Ptcsys.name);
                    root_Ptcsyses.Add(Ptcsys);
                    list_Ptcsyses.Add(Ptcsys);
                }
            }

            foreach (var p in root_Ptcsyses)
            {
                SearchParticleSystemInChildren(p);
            }
        }

        private void SearchParticleSystemInChildren(ParticleSystem PtcS)
        {
            var PtcSysArray = PtcS.GetComponentsInChildren<ParticleSystem>();
            if (PtcSysArray.Length == 0) return;

            foreach(var p in PtcSysArray)
            {
                if (p != null && list_Ptcsyses.Contains(p) == false)
                {
                    //Debug.Log(p.name);
                    list_Ptcsyses.Add(p);
                    SearchParticleSystemInChildren(p);
                }
            }
        }

        private void GetParticleSys()
        {
            if (Resources.FindObjectsOfTypeAll(typeof(ParticleSystem)) is ParticleSystem[])
            {
                Ptcsyses = Resources.FindObjectsOfTypeAll(typeof(ParticleSystem)) as ParticleSystem[];
            }
        }

        private void FramCountAdd()
        {
            frames_count += 1;
        }

        public int GetFramCount()
        {
            return frames_count;
        }

        public void Set_can_use(bool flag)
        {
            if (can_use == flag) return;

            can_use = flag;
        }

        public bool Get_can_use()
        {
            return can_use;
        }

        private void ParticleSysAreas()
        {
            //PtcsysAreaDic = new Dictionary<ParticleSystem, ParticleData>();
            for (int i = 0; i < list_Ptcsyses.Count; ++i)
            {
                float area = 0.0f;
                int particle_count = 0;
                var p = list_Ptcsyses[i];
                //Debug.Log(p.name);
                var render = p.GetComponent<Renderer>();
                sysRender = render.GetComponent<ParticleSystemRenderer>();
                mode = sysRender.renderMode;

                if (mode.ToString() == "Billboard")
                {
                    ParticleSys_Billboard particleSystem = new ParticleSys_Billboard(p);
                    area = particleSystem.getArea();
                    particle_count = particleSystem.GetParticlesCount();
                }
                else if(mode.ToString() == "Mesh")
                {
                    ParticleSys_Mesh particleSystem = new ParticleSys_Mesh(p);
                    area = particleSystem.GetAreaOfParticleSystem();
                    particle_count = particleSystem.GetParticlesCount();
                }


                if(PtcsysAreaDic.ContainsKey(p) == false)
                {
                    ParticleData Ptc_data = new ParticleData(p.name, area);
                    PtcsysAreaDic.Add(p, Ptc_data);
                    //Debug.Log(PtcsysAreaDic.Count);
                }
                else
                {
                    PtcsysAreaDic[p].Update_Data(area, particle_count);
                }
            }
        }

        private Dictionary<ParticleSystem, ParticleData> getPtcsysDic()
        {
            return PtcsysAreaDic;
        }

        public Dictionary<ParticleSystem, ParticleData> Run()
        {
            if (can_use == false) return new Dictionary<ParticleSystem, ParticleData>();

            mount();
            //GetParticleSys();
            ParticleSysAreas();
            var dic = getPtcsysDic();
            FramCountAdd();

            return dic;
        }

        private class ParticleSys_Billboard
        {
            private ParticleSystem Ptcsys;
            private float[,] dir = new float[,] { { 1.0f, 1.0f }, { 1.0f, -1.0f }, { -1.0f, -1.0f }, { -1.0f, 1.0f } };
            private ParticleSystem.Particle[] particles;
            private Dictionary<int, List<Vector3>> vertices = new Dictionary<int, List<Vector3>>();

            private float Area = 0.0f;
            public ParticleSys_Billboard(ParticleSystem _ptcsys)
            {
                Ptcsys = _ptcsys;
                Run();
            }

            private void GetParticles()
            {
                int length = Ptcsys.main.maxParticles;
                particles = new ParticleSystem.Particle[length];
                Ptcsys.GetParticles(particles);
            }

            public int GetParticlesCount()
            {
                return Ptcsys.particleCount;
            }

            private void GetParticlesVertices()
            {
                vertices = new Dictionary<int, List<Vector3>>();
                for (int i = 0; i < particles.Length; ++i)
                {
                    var pos = particles[i].position;
                    if (pos == new Vector3(0.0f, 0.0f, 0.0f)) continue;

                    var smltSpType = Ptcsys.main.simulationSpace;
                    if (smltSpType.ToString() == "Local")
                    {
                        pos = Ptcsys.transform.TransformPoint(pos);
                    }

                    var size3d = particles[i].GetCurrentSize3D(Ptcsys);
                    float width = size3d.x / 2.0f;
                    float height = size3d.y / 2.0f;

                    List<Vector3> verBuf = new List<Vector3>();
                    for (int j = 0; j < dir.GetLength(0); ++j)
                    {
                        float dir_x = dir[j, 0];
                        float dir_y = dir[j, 1];

                        float w = dir_x * width;
                        float h = dir_y * height;

                        verBuf.Add(new Vector3(pos.x + w, pos.y + h, pos.z));
                    }
                    vertices.Add(i, verBuf);
                }
            }

            private float calculateArea()
            {
                float area = 0.0f;
                if (vertices.Count <= 0) return 0.0f;

                int cube_cnt = vertices.Count;
                var point = vertices[0][0];
                var point_ = vertices[0][3];

                var p = Camera.main.WorldToScreenPoint(point);
                var p_ = Camera.main.WorldToScreenPoint(point_);
                float x = p.x;
                float x_ = p_.x;
                var dis = Mathf.Abs(x - x_);
                area = dis * dis * 0.1f;

                return area * cube_cnt;
            }

            private void DrawParticle()
            {
                // draw.
                foreach (var pair in vertices)
                {
                    var list = pair.Value;
                    for (int k = 1; k <= list.Count; ++k)
                    {
                        int idx = -1, idx_ = -1;
                        idx = k;
                        idx_ = k - 1;

                        if (k == list.Count)
                        {
                            idx = k - 1;
                            idx_ = 0;
                        }
                        Debug.DrawLine(list[idx], list[idx_], Color.black);
                    }
                }
            }

            public float getArea()
            {
                return Area;
            } 

            private void Run()
            {
                GetParticles();
                GetParticlesVertices();
                Area = calculateArea();
                DrawParticle();
            }
        };

        private class ParticleSys_Mesh {

            private ParticleSystem Ptcsys;
            private ParticleSystemRenderer sysRender = new ParticleSystemRenderer();
            private List<Vector3> verices_ptc = new List<Vector3>();
            private ParticleSystem.Particle[] particles;

            private List<Particle_ConvexHull> particles_hull = new List<Particle_ConvexHull>();

            private float AreaTotal = 0.0f;

            public ParticleSys_Mesh(ParticleSystem _ptcsys)
            {
                Ptcsys = _ptcsys;
                var render = Ptcsys.GetComponent<Renderer>();
                sysRender = render.GetComponent<ParticleSystemRenderer>();
                var local_scale = Ptcsys.transform.localScale;
                for (int i = 0; i < sysRender.mesh.vertices.Length; ++i)
                {
                    var mesh_vertices = sysRender.mesh.vertices[i];
                    verices_ptc.Add(mesh_vertices);
                }

                Run();
            }

            private void getParticles()
            {
                int length = Ptcsys.main.maxParticles;
                particles = new ParticleSystem.Particle[length];
                Ptcsys.GetParticles(particles);
            }

            public int GetParticlesCount()
            {
                return Ptcsys.particleCount;
            }

            private void calculateParticle()
            {
                for(int i = 0; i < particles.Length; ++i)
                {
                    if (particles[i].position != new Vector3(0.0f, 0.0f, 0.0f))
                    {

                        Particle_ConvexHull particle_cvh = new Particle_ConvexHull(particles[i],
                            verices_ptc, Ptcsys);
                        particles_hull.Add(particle_cvh);
                    }
                }
            }

            private void TotalArea_PtcSys()
            {
                AreaTotal = 0.0f;
                foreach(var p in particles_hull)
                {
                    AreaTotal += p.getArea();
                }
            }

            private void DreaMeshParticle()
            {
                for(int i = 0; i < particles_hull.Count; ++i)
                {
                    var parthull = particles_hull[i];
                    if (parthull.Get_edge_particle() == false)
                    {
                        parthull.Draw();
                    }
                    else
                    {
                        parthull.BdDraw();
                    }
                }
            }

            public float GetAreaOfParticleSystem()
            {
                return AreaTotal;
            }

            private void Run()
            {
                getParticles();
                calculateParticle();
                TotalArea_PtcSys();
                DreaMeshParticle();
            }

            private class Particle_ConvexHull
            {
                private ParticleSystem.Particle Ptc;
                private ParticleSystem PtcSys;
                private List<Vector3> vertices_mesh = new List<Vector3>();
                private List<Vector3> ScVertices = new List<Vector3>();
                private List<Vector3> ScVertices_bd = new List<Vector3>();
                private List<Vector3> HullRes = new List<Vector3>();
                private List<Vector3> HullRes_bd = new List<Vector3>();
                private List<Vector3> buf_ScVertices = new List<Vector3>();
                private List<Vector3> buf_ScVertices_bd = new List<Vector3>();
                private List<Vector2> ScCoordes = new List<Vector2>();

                private Vector2 P0 = new Vector2();
                private int top = -1;
                private bool edge_particle = false;

                private float Area = 0.0f;
                private float Area_bd = 0.0f;

                public Particle_ConvexHull(ParticleSystem.Particle ptc_, List<Vector3> vertices, 
                    ParticleSystem ptcsys_)
                {
                    Ptc = ptc_;
                    PtcSys = ptcsys_;
                    vertices_mesh = vertices;

                    int width = UnityEngine.Screen.width;
                    int height = UnityEngine.Screen.height;

                    for (float i = 0.0f; i <= width; i += 20.0f)
                    {
                        for (float j = 0.0f; j <= height; j += 20.0f)
                        {
                            ScCoordes.Add(new Vector2(i, j));
                        }
                    }

                    Run();
                }

                private void GetParticleVertices()
                {
                    Vector3 PtcPos = Ptc.position;
                    if(PtcSys.main.simulationSpace.ToString() == "Local")
                    {
                        PtcPos = PtcSys.transform.TransformPoint(PtcPos);
                    }

                    if (PtcPos == new Vector3(0.0f, 0.0f, 0.0f)) return;

                    for(int i = 0; i < vertices_mesh.Count; ++i)
                    {
                        Vector3 currentSize_3d = Ptc.GetCurrentSize3D(PtcSys);
                        var buf_vertices_mesh = vertices_mesh[i];
                        
                        buf_vertices_mesh.x *= currentSize_3d.x;
                        buf_vertices_mesh.y *= currentSize_3d.y;
                        buf_vertices_mesh.z *= currentSize_3d.z;

                        Vector3 scPos = Camera.main.WorldToScreenPoint(buf_vertices_mesh + PtcPos);
                        buf_ScVertices.Add(scPos);
                        ScVertices.Add(new Vector3(scPos.x, scPos.y, i));


                        float ScWidth = UnityEngine.Screen.width;
                        float ScHeight = UnityEngine.Screen.height;
                        foreach (var val in buf_ScVertices)
                        {
                            if ((val.x < 0.0f) || (val.x > ScWidth) || (val.y < 0.0f) || (val.y > ScHeight))
                            {
                                edge_particle = true;
                                break;
                            }
                        }
                    }
                }

                private int cmp(Vector2 x, Vector2 y)
                {
                    Vector2 a = new Vector2(x.x, x.y);
                    Vector2 b = new Vector2(y.x, y.y);

                    float result = cross(P0, a, b);
                    if (result > 0 || (0 == result && dis(P0, a) - dis(P0, b) < 0)) return -1;
                    else return 1;
                }

                private float cross(Vector2 _a, Vector2 _b, Vector2 _c)
                {
                    Vector2 a = new Vector2(_a.x, _a.y);
                    Vector2 b = new Vector2(_b.x, _b.y);
                    Vector2 c = new Vector2(_c.x, _c.y);
                    return (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
                }

                private double dis(Vector2 a, Vector2 b)
                {
                    return Mathf.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
                }

                private void ConvexHull(string type = "SC")
                {
                    List<Vector3> vert = new List<Vector3>();
                    List<Vector3> result = new List<Vector3>();

                    if (type == "SC")
                    {
                        vert = ScVertices;
                    }
                    else if(type == "bdSC")
                    {
                        vert = ScVertices_bd;
                    }

                    if (vert.Count <= 3) return;

                    int k = 0;
                    for (int i = 1; i < vert.Count; ++i)
                    {
                        if ((vert[i].y < vert[k].y) || (vert[i].y == vert[k].y && vert[i].x < vert[i].x)) k = i;
                    } // ���������µĵ�.

                    Vector3 tem = vert[0];
                    vert[0] = vert[k];
                    vert[k] = tem;
                    P0 = new Vector2(vert[0].x, vert[0].y);

                    vert.Sort((x, y) => cmp(x, y));

                    for (int i = 0; i < vert.Count; ++i)
                    {
                        result.Add(new Vector3());
                    }

                    top = -1;
                    result[++top] = vert[0];
                    result[++top] = vert[1];
                    result[++top] = vert[2];

                    for (int i = 3; i < vert.Count; ++i)
                    {
                        while ((top - 1 >= 0) && ((cross(result[top - 1], vert[i], result[top]) > 0) ||
                               (cross(result[top - 1], vert[i], result[top]) == 0 &&
                               dis(result[top - 1], vert[i]) > dis(result[top - 1], result[top]))))
                        {
                            top--;
                        }
                        result[++top] = vert[i];
                    }

                    for (int i = 0; i <= top; ++i)
                    {
                        if(type == "SC")
                        {
                           float idx = result[i].z;
                           HullRes.Add(new Vector3(result[i].x, result[i].y, idx));
                        }
                        else if(type == "bdSC")
                        {
                           HullRes_bd.Add(new Vector2(result[i].x, result[i].y));
                        }
                    }
                }
                    
                // �жϵ� �Ƿ��� �߶���..
                private bool is_point_onLine(Vector2 p, Vector2 p1, Vector2 p2)
                {
                    bool is_online = false;
                    float s = (p1.x - p.x) * (p2.y - p.y) - (p2.x - p.x) * (p1.y - p.y);
                    if ((Mathf.Abs(s) < float.Epsilon) && ((p.x - p1.x) * (p.x - p2.x) <= 0.0f) && ((p.y - p1.y) * (p.y - p2.y) <= 0.0f))
                    {
                        is_online = true;
                    }
                    return is_online;

                }

                //�ж������߶��Ƿ��ཻ..
                private bool two_lines_intersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
                {
                    bool is_intersect = false;

                    float s = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);
                    if (s != 0.0f)
                    {
                        float r = ((p1.y - p3.y) * (p4.x - p3.x) - (p1.x - p3.x) * (p4.y - p3.y)) / s;
                        float l = ((p1.y - p3.y) * (p2.x - p1.x) - (p1.x - p3.x) * (p2.y - p1.y)) / s;

                        if ((r >= 0.0f) && (r <= 1.0f) && (l >= 0.0f) && (l <= 1.0f))
                        {
                            is_intersect = true;
                        }
                    }
                    return is_intersect;
                }

                private bool point_in_polygon_2d(Vector2 point)
                {
                    bool is_inPolygon = false;
                    int count = 0;

                    float minX = float.MaxValue;
                    for (int i = 0; i < HullRes.Count; ++i)
                    {
                        minX = Mathf.Min(minX, HullRes[i].x);
                    }

                    float x = point.x;
                    float y = point.y;
                    Vector2 p1 = new Vector2(x, y);
                    Vector2 p2 = new Vector2(minX - 10, y);

                    for (int i = 1; i <= HullRes.Count; ++i)
                    {
                        int idx = i, idx_ = i - 1;
                        if (idx == HullRes.Count)
                        {
                            idx = i - 1;
                            idx_ = 0;
                        }

                        float cx1 = HullRes[idx].x;
                        float cy1 = HullRes[idx].y;
                        float cx2 = HullRes[idx_].x;
                        float cy2 = HullRes[idx_].y;

                        if (is_point_onLine(point, new Vector2(cx1, cy1), new Vector2(cx2, cy2)))
                        {
                            return true;
                        }
                        else if (Mathf.Abs(cy1 - cy2) < float.Epsilon)  // ƽ��.
                        {
                            continue;
                        }
                        else if (is_point_onLine(new Vector2(cx1, cy1), p1, p2))
                        {
                            if (cy1 > cy2)
                            {
                                count++;
                            }
                        }
                        else if (is_point_onLine(new Vector2(cx2, cy2), p1, p2))
                        {
                            if (cy2 > cy1)
                            {
                                count++;
                            }
                        }
                        else if (two_lines_intersect(new Vector2(cx1, cy1), new Vector2(cx2, cy2), p1, p2))
                        {
                            count++;
                        }
                    }

                    if (count % 2 == 1)
                    {
                        is_inPolygon = true;
                    }

                    return is_inPolygon;
                }

                private void addPoint()
                {
                    for (int i = 0; i < ScCoordes.Count; ++i)
                    {
                        var point = ScCoordes[i];                       
                        var x = point.x;
                        var y = point.y;
                        if (point_in_polygon_2d(point) == true)
                        {
                            // Debug.Log("[" + x + ", " + y + "]");
                            int index = ScVertices.Count;
                            ScVertices.Add(new Vector3(x, y, index));
                        }
                    }

                    float ScWidth = UnityEngine.Screen.width;
                    float ScHeight = UnityEngine.Screen.height;

                    ScVertices_bd.Clear();
                    foreach (var val in ScVertices)
                    {
                        if ((val.x >= 0.0f && val.x <= ScWidth) && (val.y >= 0.0f && val.y <= ScHeight))
                        {
                            ScVertices_bd.Add(val);
                            //Debug.Log("vertexSc: [" + i + "].");
                        }
                    }
                }

                public void Draw(string type = "SC")
                {


                    for (int i = 1; i <= top; ++i)
                    {
                        int j = ((int)HullRes[i].z), j_ = ((int)HullRes[i - 1].z),
                            k = i, k_ = i - 1;

                        if (i == top)
                        {
                            j_ = ((int)HullRes[0].z);
                            k_ = 0;
                        }
                        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(HullRes[k].x, HullRes[k].y,
                            buf_ScVertices[j].z));
                        Vector3 prev_point = Camera.main.ScreenToWorldPoint(new Vector3(HullRes[k_].x, HullRes[k_].y,
                            buf_ScVertices[j_].z));
                        Debug.DrawLine(point, prev_point, Color.black);
                    }
                }


                public void BdDraw()
                {
                    for (int i = 1; i <= top; ++i)
                    {
                        if (i >= HullRes_bd.Count) break;

                        int k = i, k_ = i - 1;

                        if (i == top)
                        {
                            k_ = 0;
                        }

                        float camera_z = -Camera.main.transform.position.z == 0.0f ? -10.0f :
                            -Camera.main.transform.position.z;
                        //Debug.Log(camera_z);
                        Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(HullRes_bd[k].x, HullRes_bd[k].y, camera_z));
                        Vector3 prev_point = Camera.main.ScreenToWorldPoint(new Vector3(HullRes_bd[k_].x, HullRes_bd[k_].y, camera_z));
                        Debug.DrawLine(point, prev_point, Color.red);
                    }
                }


                private float TriangleArea(string type = "SC")
                {
                    List<Vector3> hull = new List<Vector3>();
                    if (type == "SC")
                    {
                        hull = HullRes;
                    }
                    else if (type == "bdSC")
                    {
                        hull = HullRes_bd;
                    }

                    if (hull.Count < 2) return -1.0f;

                    float area = 0.0f;

                    Vector3 p = hull[0];
                    Vector3 targetP = hull[1];

                    for (int i = 2; i < hull.Count; ++i)
                    {
                        Vector2 point = hull[i];
                        float buf = (p.x * targetP.y + targetP.x * point.y + point.x * p.y - p.x * point.y - targetP.x * p.y - point.x * targetP.y);
                        targetP = point;
                        //Debug.Log("buf is: [" + buf + "].");
                        area += buf;
                    }
                    return area / 2.0f;
                }

                public float getArea()
                {
                    if(edge_particle == true)
                    {
                        return Area_bd;
                    }
                    return Area;
                }

                public bool Get_edge_particle()
                {
                    return edge_particle;
                }

                private void Run()
                {
                    GetParticleVertices();
                    ConvexHull();
                    Area = TriangleArea();

                    if(edge_particle == true)
                    {
                        addPoint();
                        ConvexHull("bdSC");
                        Area_bd = TriangleArea("bdSC");
                    }
                }
            };
        };
    };
}
