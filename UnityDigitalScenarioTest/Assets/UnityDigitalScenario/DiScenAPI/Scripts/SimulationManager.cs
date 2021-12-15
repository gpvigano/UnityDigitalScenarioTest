using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiScenFw;
using GPVUDK;

namespace UnityDigitalScenario
{
    [RequireComponent(typeof(ScenarioManager))]
    public class SimulationManager : MonoBehaviour
    {
        [SerializeField]
        protected string historyFileName = "history";

        [SerializeField]
        protected ScenarioManager scenarioManager = null;

        public bool ScenarioLoaded
        {
            get
            {
                return scenarioManager.ScenarioLoaded;
            }
        }


        public void LoadHistory()
        {
            DiScenApiUnity.Initialize();
            DiScenApiUnity.ClearSimulation();
            string filePath = GetHistoryFilePath();
            if (!DiScenApiUnity.LoadSimulation(filePath))
            {
                Debug.LogWarning("Failed to load " + filePath);
            }
        }


        public void SaveHistory()
        {
            string filePath = GetHistoryFilePath();
            if (!DiScenApiUnity.SaveSimulation(filePath))
            {
                Debug.LogWarning("Failed to save " + filePath);
            }
        }


        public void PlaySimulation()
        {
            DiScenApiUnity.PlaySimulation();
        }

        public void StopSimulation()
        {
            DiScenApiUnity.StopSimulation();
        }

        public void PauseSimulation()
        {
            DiScenApiUnity.PauseSimulation();
        }


        protected virtual string GetHistoryFilePath()
        {
            string filePath = Application.streamingAssetsPath + "/" + historyFileName.Replace(' ', '_') + ".json";
            return filePath;
        }


        protected virtual void Awake()
        {
            if (scenarioManager == null)
            {
                scenarioManager = GetComponent<ScenarioManager>();
            }
        }


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (scenarioManager == null)
            {
                scenarioManager = GetComponent<ScenarioManager>();
            }
            //UpdateCatalog();
        }
#endif


        protected virtual void Update()
        {
            DiScenApiUnity.UpdateSimulation(Time.deltaTime);
        }


        protected virtual void Start()
        {
            DiScenApiUnity.Initialize();
        }


        protected virtual void OnEnable()
        {
            DiScenApiUnity.SyncSceneObjectTransformEvent += SyncSceneObjectTransformEvent;
            DiScenApiUnity.SyncElementTransformEvent += SyncElementTransformEvent;
            DiScenApiUnity.LerpElementTransformEvent += LerpElementTransformEvent;
        }


        protected virtual void OnDisable()
        {
            DiScenApiUnity.SyncSceneObjectTransformEvent -= SyncSceneObjectTransformEvent;
            DiScenApiUnity.SyncElementTransformEvent -= SyncElementTransformEvent;
            DiScenApiUnity.LerpElementTransformEvent -= LerpElementTransformEvent;
        }


        protected virtual void SyncSceneObjectTransformEvent(
            string elemId,
            ref LocalTransformData scenarioTransform)
        {
            ScenarioElement elem = scenarioManager.Root.GetElement(elemId);
            if (elem)
            {
                elem.data.LocalTransform = scenarioTransform;
                elem.DataToTransform();
            }
            else
            {
                DiScenApiUnity.Log("Element " + elemId + " not found.", LogLevel.Error, "Simulation", false, true, "MissingElement");
            }
        }


        protected virtual void SyncElementTransformEvent(
            string elemId,
            ref LocalTransformData sceneTransform)
        {
            ScenarioElement elem = scenarioManager.Root.GetElement(elemId);
            if (elem)
            {
                elem.TransformToData();
                sceneTransform = elem.data.LocalTransform;
            }
            else
            {
                DiScenApiUnity.Log("Element " + elemId + " not found.", LogLevel.Error, "Simulation", false, true, "MissingElement");
            }
        }


        protected virtual void LerpElementTransformEvent(string elemId,
            ref LocalTransformData transform1,
            ref LocalTransformData transform2,
            float trim)
        {
            ScenarioElement elem = scenarioManager.Root.GetElement(elemId);
            if (elem)
            {
                Vector3 fw1 = DiScenApiUnity.ConvertVec(transform1.ForwardAxis);
                Vector3 up1 = DiScenApiUnity.ConvertVec(transform1.UpAxis);
                Quaternion rot1 = Quaternion.LookRotation(fw1, up1);
                Vector3 pos1 = DiScenApiUnity.ConvertVec(transform1.Origin) * DiScenApiUnity.unitScale;
                Vector3 scale1 = DiScenApiUnity.ConvertVec(transform1.Scale);

                Vector3 fw2 = DiScenApiUnity.ConvertVec(transform2.ForwardAxis);
                Vector3 up2 = DiScenApiUnity.ConvertVec(transform2.UpAxis);
                Quaternion rot2 = MathUtility.ForwardUpToQuaternion(fw2, up2);
                Vector3 pos2 = DiScenApiUnity.ConvertVec(transform2.Origin) * DiScenApiUnity.unitScale;
                Vector3 scale2 = DiScenApiUnity.ConvertVec(transform2.Scale);

                Transform elemTr1 = scenarioManager.Root.transform;
                // transform according to parent change
                if (transform1.ParentId != transform2.ParentId)
                {
                    bool parent1Defined = !string.IsNullOrEmpty(transform1.ParentId);
                    bool parent2Defined = !string.IsNullOrEmpty(transform2.ParentId);
                    ScenarioElement elem1 = parent1Defined ?
                        scenarioManager.Root.GetElement(transform1.ParentId) : null;
                    ScenarioElement elem2 = parent2Defined ?
                        scenarioManager.Root.GetElement(transform2.ParentId) : null;
                    bool parent1ok = true;
                    bool parent2ok = true;
                    if (parent1Defined && elem1 == null)
                    {
                        parent1ok = false;
                        DiScenApiUnity.Log("Parent element " + transform1.ParentId + " not found.", LogLevel.Error, "Simulation", false, true, "ParentNotFound");
                    }
                    if (parent2Defined && elem2 == null)
                    {
                        parent2ok = false;
                        DiScenApiUnity.Log("Parent element " + transform2.ParentId + " not found.", LogLevel.Error, "Simulation", false, true, "ParentNotFound");
                    }

                    if (parent1ok && parent2ok)
                    {
                        elemTr1 = parent1Defined ?
                            elem1.transform : scenarioManager.Root.transform;
                        Transform elemTr2 = parent2Defined ?
                            elem2.transform : scenarioManager.Root.transform;
                        MathUtility.SetRelativeToOtherTransform(
                            elemTr1,
                            elemTr2,
                            ref pos2,
                            ref rot2,
                            ref scale2
                            );
                    }
                }

                elem.data.LocalTransform.ParentId = transform1.ParentId;
                elem.transform.parent = elemTr1;
                elem.transform.localPosition = Vector3.Lerp(pos1, pos2, trim);
                elem.transform.localRotation = Quaternion.Slerp(rot1, rot2, trim);
                elem.transform.localScale = Vector3.Lerp(scale1, scale2, trim);
                //elem.TransformToData();
            }
            else
            {
                DiScenApiUnity.Log("Element " + elemId + " not found.", LogLevel.Error, "Simulation", false, true, "MissingElement");
            }
        }

    }
}