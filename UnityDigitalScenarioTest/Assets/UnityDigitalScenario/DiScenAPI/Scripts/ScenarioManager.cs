using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiScenFw;

namespace UnityDigitalScenario
{
    public class ScenarioManager : MonoBehaviour
    {
        [SerializeField]
        protected LogText logText = null;

        [Tooltip("Name of the scenario file (without extension).")]
        [SerializeField]
        protected string scenarioFileName = "scenario";

        [Tooltip("Unit scale for distances in scenario data.")]
        [SerializeField]
        protected float unitScale = 1.0f;

        [Tooltip("Root object for the scenario.\nIf not set it will be automatically created.")]
        [SerializeField]
        protected ScenarioRoot scenarioRoot = null;

        [Tooltip("List of game objects used to create elements.\nDrop here objects from prefabs or scene objects.")]
        [SerializeField]
        protected List<GameObject> catalogObjects = new List<GameObject>();

        protected Dictionary<string, GameObject> catalog = new Dictionary<string, GameObject>();


        public ScenarioRoot Root
        {
            get
            {
                if (scenarioRoot == null)
                {
                    GameObject rootObj = new GameObject(scenarioFileName);
                    scenarioRoot = rootObj.AddComponent<ScenarioRoot>();
                }
                return scenarioRoot;
            }
        }


        public bool ScenarioLoaded
        {
            get
            {
                if (scenarioRoot == null)
                {
                    return false;
                }
                return !scenarioRoot.Empty();
            }
        }


        public ScenarioElement GetCatalogElement(string elementId)
        {
            if (catalog.ContainsKey(elementId))
            {
                return catalog[elementId].GetComponent<ScenarioElement>();
            }
            return null;
        }


        public bool DeleteElement(string elementId)
        {
            if(!Root.ContainsElement(elementId))
            {
                return false;
            }
            ScenarioElement element = Root.GetElement(elementId);
            if(element == null)
            {
                return false;
            }
            Root.RemoveElement(elementId);
            Destroy(element.gameObject);
            // TODO: check if the scenario is already synchronized, then delete only the element
            OnSyncScenario();
            //DiScenApiUnity.DeleteElement(elementId);
            return true;
        }


        public ScenarioElement AddElementFromCatalog(string catalogElementId, string elementId = "", Dictionary<string,string> abbreviationMap=null)
        {
            if (!catalog.ContainsKey(catalogElementId))
            {
                return null;
            }
            GameObject obj = Instantiate(catalog[catalogElementId]);
            obj.SetActive(true);
            ScenarioElement elem = obj.GetComponent<ScenarioElement>();
            // if the identifier is empty build a unique identifier
            if (string.IsNullOrEmpty(elementId))
            {
                string prefix = elem.data.Type;
                if(abbreviationMap!=null && abbreviationMap.ContainsKey(prefix))
                {
                    prefix = abbreviationMap[prefix];
                }
                int num = 1;
                do
                {
                    elementId = prefix + num.ToString();
                    num++;
                } while (Root.ContainsElement(elementId));

            }
            elem.Root = Root;
            obj.name = elementId;
            elem.data.Identifier = elementId;
            elem.DataToTransform();
            Root.AddElement(elem);
            // TODO: check if the scenario is already synchronized, then delete only the element
            OnSyncScenario();
            //DiScenApiUnity.AddElement(elem.data);
            return elem;
        }

        public void LoadScenario()
        {
            DiScenApiUnity.Initialize();
            //DiScenApiUnity.ClearScenario();
            OnSyncScene();
            string filePath = GetScenarioFilePath();
            if (!DiScenApiUnity.LoadScenario(filePath))
            {
                Debug.LogWarning("Failed to load " + filePath);
            }
        }


        public void SaveScenario()
        {
            OnSyncScenario();
            string filePath = GetScenarioFilePath();
            if (!DiScenApiUnity.SaveScenario(filePath))
            {
                Debug.LogWarning("Failed to save " + filePath);
            }
        }


        protected virtual string GetScenarioFilePath()
        {
            string filePath = Application.streamingAssetsPath + "/" + scenarioFileName.Replace(' ', '_') + ".json";
            return filePath;
        }


        protected virtual void UpdateCatalog()
        {
            catalog.Clear();
            if (catalogObjects.Count > 0)
            {
                foreach (GameObject catalogObj in catalogObjects)
                {
                    ScenarioElement elem = catalogObj.GetComponent<ScenarioElement>();
                    if (elem != null)
                    {
                        catalog.Add(elem.data.Identifier, catalogObj);
                    }
                }
            }
        }


        protected virtual void Awake()
        {
            DiScenApiUnity.Initialize();
        }


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (scenarioRoot == null)
            {
                scenarioRoot = FindObjectOfType<ScenarioRoot>();
            }
            if (logText == null)
            {
                logText = FindObjectOfType<LogText>();
            }
            //UpdateCatalog();
        }
#endif


        protected virtual void Start()
        {
            UpdateCatalog();
        }


        protected virtual void Update()
        {
            DiScenApiUnity.UpdateScenario(Time.deltaTime);
        }


        protected virtual void OnEnable()
        {
            DiScenApiUnity.SyncScenarioEvent += OnSyncScenario;
            DiScenApiUnity.SyncSceneEvent += OnSyncScene;
            DiScenApiUnity.ScreenDisplayMessageEvent += OnScreenDisplayMessage;
        }


        protected virtual void OnDisable()
        {
            DiScenApiUnity.SyncScenarioEvent -= OnSyncScenario;
            DiScenApiUnity.SyncSceneEvent -= OnSyncScene;
            DiScenApiUnity.ScreenDisplayMessageEvent -= OnScreenDisplayMessage;
        }


        protected virtual void OnSyncScenario()
        {
            Debug.Log("SyncScenarioEvent");
            int count = 0;
            Dictionary<string, ScenarioElement>.ValueCollection scenarioElements = Root.GetElements();
            ElementData[] elements = new ElementData[scenarioElements.Count];
            foreach (ScenarioElement elem in scenarioElements)
            {
                elem.TransformToData();
                elements[count] = elem.data;
                count++;
            }
            DiScenApiUnity.ClearScenario();
            DiScenApiUnity.AddElements(elements);
        }


        protected virtual void OnSyncScene()
        {
            ElementData[] elements = DiScenApiUnity.GetElements();
            Root.Clear();
            DiScenApiUnity.unitScale = unitScale;
            foreach (ElementData element in elements)
            {
                string uri = element.Asset.Uri;
                if (catalog.ContainsKey(uri))
                {
                    GameObject obj = Instantiate(catalog[uri]);
                    obj.SetActive(true);
                    obj.name = element.Identifier;
                    ScenarioElement elem = obj.GetComponent<ScenarioElement>();
                    elem.Root = Root;
                    elem.data = element;
                    elem.DataToTransform();
                    Root.AddElement(elem);
                }
            }
        }


        protected void OnScreenDisplayMessage(LogLevel severity, string message, string msgTag)
        {
            if (logText != null && logText.isActiveAndEnabled)
            {
                logText.AddMessage(severity, message, msgTag);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            Debug.Log(" DiScenApiUnity.Deinitialize()");
            DiScenApiUnity.Deinitialize();
        }
    }
}
