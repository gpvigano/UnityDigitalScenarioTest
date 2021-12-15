using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiScenFw;
using GPVUDK;

namespace UnityDigitalScenario
{
    public class ScenarioElement : MonoBehaviour
    {
        public ElementData data = new ElementData();


        private ScenarioRoot scenarioRoot = null;
        private bool updatePending = false;


        public ScenarioRoot Root
        {
            get
            {
                if (scenarioRoot == null)
                {
                    scenarioRoot = GetComponentInParent<ScenarioRoot>();
                }
                return scenarioRoot;
            }
            set
            {
                if (scenarioRoot != null)
                {
                    scenarioRoot.RemoveElement(this);
                }
                scenarioRoot = value;
            }
        }


        public void TransformToData()
        {
            data.LocalTransform.Origin = DiScenApiUnity.ConvertVector3(transform.localPosition);
            Matrix4x4 rotMat = MathUtility.QuaternionToMatrix(transform.localRotation);
            //rotMat = MathUtility.MatrixToRotMat(rotMat);
            Vector3 fw = MathUtility.MatrixForward(rotMat);
            Vector3 up = MathUtility.MatrixUp(rotMat);
            Vector3 rt = MathUtility.MatrixRight(rotMat);
            data.LocalTransform.RightAxis = DiScenApiUnity.ConvertVector3(rt);
            data.LocalTransform.ForwardAxis = DiScenApiUnity.ConvertVector3(fw);
            data.LocalTransform.UpAxis = DiScenApiUnity.ConvertVector3(up);
            data.LocalTransform.Scale = DiScenApiUnity.ConvertVector3(transform.localScale);
            Transform parentTrasform = transform.parent;
            ScenarioElement elem = parentTrasform ? parentTrasform.GetComponentInParent<ScenarioElement>() : null;
            if (elem != null && !string.IsNullOrEmpty(data.LocalTransform.ParentId))
            {
                data.LocalTransform.ParentId = elem.data.Identifier;
            }
            else
            {
                data.LocalTransform.ParentId = "";
            }
            updatePending = false;
        }

        public void DataToTransform()
        {
            if (Root != null)
            {
                if (!string.IsNullOrEmpty(data.LocalTransform.ParentId))
                {
                    ScenarioElement parentElem = scenarioRoot.GetElement(data.LocalTransform.ParentId);
                    if (parentElem != null)
                    {
                        transform.parent = parentElem.transform;
                    }
                }
                else
                {
                    transform.parent = Root.transform;
                }
                Vector3 fw = DiScenApiUnity.ConvertVec(data.LocalTransform.ForwardAxis);
                Vector3 up = DiScenApiUnity.ConvertVec(data.LocalTransform.UpAxis);
                transform.localPosition = DiScenApiUnity.ConvertVec(data.LocalTransform.Origin) * DiScenApiUnity.unitScale;
                transform.localRotation.SetLookRotation(fw, up);
                transform.localScale = DiScenApiUnity.ConvertVec(data.LocalTransform.Scale);
                updatePending = false;
            }
            else
            {
                updatePending = true;
            }
        }


        protected virtual void OnDestroy()
        {
            if (Root) Root.RemoveElement(data.Identifier);
        }


        protected virtual void Awake()
        {
            if (Root) Root.AddElement(this);
        }


        protected virtual void Start()
        {
        }


        protected virtual void Update()
        {
            if (updatePending)
            {
                DataToTransform();
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(data.Identifier))
            {
                data.Identifier = gameObject.name;
                data.Type = "Element";
                TransformToData();
            }
            if (string.IsNullOrEmpty(data.Asset.Uri))
            {
                data.Asset.Uri = name;
            }

            if (gameObject.transform.root.gameObject.activeInHierarchy)
            {
                data.Asset.Source = AssetSourceType.Scene;
            }
            else
            {
                data.Asset.Source = AssetSourceType.Project;
            }
        }
#endif
    }
}