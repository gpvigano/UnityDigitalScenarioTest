using System.Collections;
using System.Collections.Generic;
//using System.Linq;
using UnityEngine;
using UnityDigitalScenario;

namespace UnityDigitalScenario
{
    public class ScenarioRoot : MonoBehaviour
    {

        private Dictionary<string, ScenarioElement> scenario = new Dictionary<string, ScenarioElement>();


        public int ElementsCount()
        {
            return scenario.Count;
        }


        public bool Empty()
        {
            return scenario.Count == 0;
        }


        public void AddElement(ScenarioElement element)
        {
            if (!scenario.ContainsValue(element))
            {
                scenario.Add(element.data.Identifier, element);
            }
        }


        public void RemoveElement(ScenarioElement element)
        {
            if (scenario.ContainsValue(element))
            {
                scenario.Remove(element.data.Identifier);
            }
        }


        public void RemoveElement(string elementId)
        {
            if (scenario.ContainsKey(elementId))
            {
                scenario.Remove(elementId);
            }
        }


        public bool ContainsElement(string id)
        {
            return scenario.ContainsKey(id);
        }


        public ScenarioElement GetElement(string id)
        {
            if (scenario.ContainsKey(id))
            {
                return scenario[id];
            }
            return null;
        }


        public Dictionary<string,ScenarioElement>.ValueCollection GetElements()
        {
            return scenario.Values;
        }


        public void Clear()
        {
            scenario.Clear();
            ScenarioElement[] elements = GetComponentsInChildren<ScenarioElement>();
            if (elements != null && elements.Length > 0)
            {
                foreach (ScenarioElement elem in elements)
                {
                    DestroyImmediate(elem.gameObject);
                }
            }
        }

        public void ScanHierarchy()
        {
            scenario.Clear();
            ScenarioElement[] elements = GetComponentsInChildren<ScenarioElement>();
            if (elements != null && elements.Length > 0)
            {
                foreach (ScenarioElement elem in elements)
                {
                    AddElement(elem);
                }
            }
        }


        private void Start()
        {
            //ScanHierarchy();
        }

        private void Update()
        {

        }

    }
}
