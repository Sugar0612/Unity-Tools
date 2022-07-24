using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if true

namespace ParticleTools
{
    public class ParticleToolWindow : EditorWindow
    {
        private int FrameCount;

        Vector3 windowScrollPos = Vector2.zero;
        Vector3 scrollPos = Vector2.zero;
        Vector3 scrollPos2 = Vector2.zero;
        public List<GameObject> ParticleList = new List<GameObject>();

        [SerializeField] public List<ParticleData> ParticleDatas = new List<ParticleData>();

        private SerializedObject serObj;

        /// <summary>
        /// SerializedProperty - ParticleList
        /// </summary>
        private SerializedProperty gosPty;

        /// <summary>
        /// SerializedProperty - ParticleDatas
        /// </summary>
        private SerializedProperty pdatasPty;

        public ParticleToolWindow()
        {
            this.position = new Rect(50f, 50f, 300f, 150f);
            this.minSize = new Vector2(400f, 200f);
        }

        [MenuItem("Tools/ParticleTool")]
        public static ParticleToolWindow ShowWindow()
        {
            ParticleToolWindow window = EditorWindow.GetWindow(typeof(ParticleToolWindow)) as ParticleToolWindow;
            if ((UnityEngine.Object) window != (UnityEngine.Object) null)
            {
                window.titleContent =
                    EditorGUIUtility.TrTextContent("Particle Tool Window", (string) null, (Texture) null);
                window.initialize();
            }

            return window;
        }

        private void initialize()
        {
            //ParticleList = ParticleTool.ParticleList;
        }

        private void OnEnable()
        {
            //系列化对象的初始化
            serObj = new SerializedObject(this);
            gosPty = serObj.FindProperty("ParticleList");
            pdatasPty = serObj.FindProperty("ParticleDatas");
        }

        private void OnDisable()
        {
            Particle.Instance.Close();
        }


        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("StartRecord", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                Debug.Log(ParticleList.Count);
                foreach (var itr in ParticleList)
                {
                    Debug.Log(itr.name + "|" + itr.GetComponent<ParticleSystem>());
                }

                //ParticleSystem SSS;
                Particle.Instance.StartRecord(ParticleList);
            }

            if (GUILayout.Button("StopRecord", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                ParticleDatas = new List<ParticleData>();
                Particle.Instance.StopRecord(ParticleDatas);
                foreach (var _p in ParticleDatas)
                {
                    Debug.LogWarning("ParticleSystem name is: " + _p.name +
                                     " | overDraw Max is: " + _p.maxArea +
                                     " | ave overDraw is: " + _p.aveArea +
                                     " | particle Max Count is: " + _p.maxParticleCount +
                                     " | particle Ave Count is: " + _p.aveParticleCount);
                }
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(gosPty, true);
            GUILayout.BeginHorizontal();
            GUILayout.Label("排序方式:");
            if (GUILayout.Button("最大面积", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                ParticleDatas.Sort((x, y) => { return x.maxArea < y.maxArea ? 1 : -1; });
            }

            if (GUILayout.Button("平均面积", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                ParticleDatas.Sort((x, y) => { return x.aveArea < y.aveArea ? 1 : -1; });
            }

            if (GUILayout.Button("最大粒子数", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                ParticleDatas.Sort((x, y) => { return x.maxParticleCount < y.maxParticleCount ? 1 : -1; });
            }

            if (GUILayout.Button("最大粒子数", EditorStyles.toolbarButton, GUILayout.MinWidth(80f)))
            {
                ParticleDatas.Sort((x, y) => { return x.aveParticleCount < y.aveParticleCount ? 1 : -1; });
            }

            GUILayout.EndHorizontal();
            //GUILayout.Space(10); // 10 像素宽的空格
            GUILayout.Label("结果:");
            scrollPos =
                EditorGUILayout.BeginScrollView(scrollPos,GUILayout.Width(position.width-20), GUILayout.Height(200));
            GUILayout.BeginHorizontal();
            if (pdatasPty.isArray)
            {
                for (int i = 0; i < pdatasPty.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(pdatasPty.GetArrayElementAtIndex(i), true,GUILayout.Width(300));
                    //
                    //ParticleInfo
                }
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            scrollPos2 =
                EditorGUILayout.BeginScrollView(scrollPos2,GUILayout.Width(position.width-20), GUILayout.Height(position.height-500));
            //ParticleInfo
            EditorGUILayout.PropertyField(pdatasPty, true);
            EditorGUILayout.EndScrollView();
            

            serObj.Update();
            EditorGUI.BeginChangeCheck();
            if (EditorGUI.EndChangeCheck())
            {
                serObj.ApplyModifiedProperties();
            }
        }
    }
}
#endif