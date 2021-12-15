using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GPVUDK;
using UnityDigitalScenario;
using DiScenFw;

namespace ElectronicCircuit
{
    [RequireComponent(typeof(MouseDragAndDrop), typeof(ScenarioManager))]
    public class LedCircuitTest : DiScenXpManagerUI
    {

        private ScenarioManager scenario;
        private MouseDragAndDrop dragAndDrop;
        private bool updateAll = false;
        private CircuitComponent highlightedComponent = null;

        public override void NewEpisode()
        {
            CancelSelection();
            CleanUp();
            base.NewEpisode();
        }

        public override void LoadExperience(bool updateVE = true)
        {
            if (updateVE)
            {
                CancelSelection();
            }
            base.LoadExperience(updateVE);
        }

        public override void SaveExperience()
        {
            CancelSelection();
            base.SaveExperience();
        }

        public override void ClearCurrentExperience()
        {
            CancelSelection();
            base.ClearCurrentExperience();
        }

        public override void StartAutoTraining()
        {
            CancelSelection();
            base.StartAutoTraining();
        }

        //public override void StopAutoTraining()
        //{
        //    base.StopAutoTraining();
        //}


        public void LoadScenario()
        {
            scenario.LoadScenario();
            OnLoadScenario();
        }


        public void SaveScenario()
        {
            //foreach (CircuitComponent comp in electronicComponentArray)
            //{
            //    comp.TransformToData();
            //}
            scenario.SaveScenario();
        }


        public void CreateComponent(string catalogElementId)
        {
            if (scenario)
            {
                Debug.Log("CreateComponent " + catalogElementId);
                Dictionary<string, string> abbreviationMap = new Dictionary<string, string>();
                abbreviationMap.Add("Resistor", "R");
                abbreviationMap.Add("Switch", "SW");
                ScenarioElement elem = scenario.AddElementFromCatalog(catalogElementId, "", abbreviationMap);
                UpdateAll();

                // offset from the cursor position
                Vector3 offset = new Vector3(-1.5f, 0, -0.75f);
                // add a random offset to distinguish multiple instances
                offset.x += Random.Range(-0.5f, 0.5f);
                elem.transform.position = dragAndDrop.CursorTransform.position + offset;

                // TODO: directly drag soon after creation
                //Draggable draggable = elem.GetComponent<Draggable>();
                //if (draggable != null)
                //{
                //    if (dragAndDrop.CursorTransform != null)
                //    {
                //        dragAndDrop.CursorTransform.gameObject.SetActive(true);
                //    }
                //    dragAndDrop.Drag(draggable);
                //}

                //// delay update at the next frame
                //updateAll = true;
            }
        }


        protected override void Train()
        {
            base.Train();
            if (lastResult != Result.InProgress)
            {
                if (autoTrainingLiveUpdate)
                {
                    string msg = lastResult + ":  " + DiScenXpApi.GetInfo("CircuitSchema") + "\n";
                    Debug.Log(msg);
                    CleanUp();
                }
            }

        }


        protected override void InitializeScenario()
        {
            base.InitializeScenario();
            UpdateConfiguration();
        }

        [SerializeField]
        private GameObject cablePrefab;
        private Dictionary<string, CircuitComponent> electronicComponents = new Dictionary<string, CircuitComponent>();
        // TODO: visualized in Inspector for debugging, it can be hidden in future
        [SerializeField]
        private CircuitComponent[] electronicComponentArray;
        private List<GameObject> cableList = new List<GameObject>();
        private GameObject prevLeadObj = null;
        private GameObject tempCableObj = null;
        private List<CircuitComponentLead> connectedLeads = new List<CircuitComponentLead>();


        public void UpdateComponentsList()
        {
            electronicComponentArray = FindObjectsOfType<CircuitComponent>();
            electronicComponents.Clear();
            foreach (CircuitComponent comp in electronicComponentArray)
            {
                //if (electronicComponents.ContainsKey(comp.data.Identifier))
                //    Debug.LogWarning(comp.data.Identifier);
                //else
                electronicComponents.Add(comp.data.Identifier, comp);
            }
        }

        public void UpdateConfiguration()
        {
            string config = "";
            //                string config =
            //"PowerSupplyDC Battery 6000 50\n"+
            //"LED LED1 Red\n"+
            //"Resistor R1 2200 500\n"+
            //"Resistor R2 50 250\n"+
            //"Switch SW1 12000 40\n";
            //DiScenXpApi.RemoveAllComponents();

            foreach (CircuitComponent cmp in electronicComponentArray)
            {
                config += GetComponentConfig(cmp) + "\n";
            }

            DiScenXpApi.SetConfiguration(config);
        }

        public void OnLoadScenario()
        {
            updateAll = true;
            //UpdateAll();
        }

        public void UpdateAll()
        {
            UpdateComponentsList();
            UpdateConfiguration();
            UpdateGoalUI();
            NewEpisode();
            tooltipText.text = "";
            updateAll = false;
        }


        public void DeleteComponent(CircuitComponent deletedComponent)
        {
            // delete selected component
            // do not delete power supply
            if (deletedComponent != null && deletedComponent.data.Type != "PowerSupplyDC")
            {
                string id = deletedComponent.data.Identifier;
                GameObject obj = deletedComponent.gameObject;
                OnObjectLeft(obj);
                ClearCurrentExperience();
                scenario.DeleteElement(id);
                updateAll = true;
            }

        }

        private void CleanUp()
        {
            foreach (CircuitComponent comp in electronicComponentArray)
            {
                SwitchObject[] icons = comp.GetComponentsInChildren<SwitchObject>();
                foreach (SwitchObject icon in icons)
                {
                    icon.SwitchOff();
                }
            }
            connectedLeads.Clear();
        }

        private void CancelSelection()
        {
            CircuitComponentLead lead = prevLeadObj ? prevLeadObj.GetComponent<CircuitComponentLead>() : null;
            prevLeadObj = null;
            if (lead != null)
            {
                ShowHints();
            }
            dragAndDrop.CancelDragging();
            if (dragAndDrop.CursorTransform != null)
            {
                dragAndDrop.CursorTransform.gameObject.SetActive(false);
            }
            if (tempCableObj != null)
            {
                tempCableObj.SetActive(false);
            }
        }

        private void OnCircuitcomponentClicked(CircuitComponent comp, GameObject leadObj)
        {
            SwitchComponent sw = comp as SwitchComponent;
            if (sw != null && leadObj == sw.SwitchCursor && prevLeadObj == null)
            {
                if (lastResult != Result.InProgress && prevLeadObj == null)
                {
                    NewEpisode();
                }
                else
                {
                    bool on = DiScenXpApi.CheckEntityProperty(comp.data.Identifier, "position", "1");
                    lastResult = DiScenXpApi.TakeAction("switch", new string[] { comp.data.Identifier, on ? "0" : "1" }, updateExperience);
                    SyncSceneState();
                }
                return;
            }
            CircuitComponentLead lead = leadObj ? leadObj.GetComponent<CircuitComponentLead>() : null;
            if (lead != null && !connectedLeads.Contains(lead) && lead.gameObject != prevLeadObj)
            {
                if (lastResult != Result.InProgress && prevLeadObj == null)
                {
                    NewEpisode();
                }
                if (prevLeadObj != null)
                {
                    if (dragAndDrop.CursorTransform != null)
                    {
                        dragAndDrop.CursorTransform.gameObject.SetActive(false);
                    }
                    if (tempCableObj != null)
                    {
                        tempCableObj.SetActive(false);
                    }
                    //GameObject cableObj = Instantiate(cablePrefab);
                    //Cable cable = cableObj.GetComponent<Cable>();
                    //cable.StartAnchor = prevLeadObj.transform;
                    //cable.EndAnchor = leadObj.transform;
                    //cableList.Add(cableObj);
                    CircuitComponent prevCircuitComponent = prevLeadObj.GetComponentInParent<CircuitComponent>();
                    CircuitComponentLead prevLead = prevLeadObj.GetComponent<CircuitComponentLead>();
                    //Debug.Log(prevCircuitComponent.componentId);
                    //Debug.Log(prevLead.leadName);
                    //Debug.Log(circuitComponent.componentId);
                    //Debug.Log(lead.leadName);
                    lastResult = DiScenXpApi.TakeAction("connect", new string[] {
                                prevCircuitComponent.data.Identifier, prevLead.leadName,
                                comp.data.Identifier, lead.leadName
                            }, updateExperience);
                    connectedLeads.Add(prevLead);
                    connectedLeads.Add(lead);
                    prevLeadObj = null;
                    SyncSceneState();
                }
                else
                {
                    if (dragAndDrop.CursorTransform != null)
                    {
                        dragAndDrop.CursorTransform.gameObject.SetActive(true);
                    }
                    if (tempCableObj == null)
                    {
                        tempCableObj = Instantiate(cablePrefab);
                    }
                    tempCableObj.SetActive(true);
                    SetUpCable(tempCableObj, leadObj.transform, dragAndDrop.CursorTransform);
                    //}
                    lead.hintIcon.SwitchOff();
                    prevLeadObj = leadObj;
                    ShowHints();
                }
                return;
            }
            Draggable draggable = comp.GetComponent<Draggable>();
            if (draggable != null)
            {
                dragAndDrop.Drag(draggable);
            }
        }

        private void OnObjectClicked(GameObject obj)
        {
            //if (leadObj.name == "battery")
            //{
            //    NewEpisode();
            //    return;
            //}
            CircuitComponentLead lead = obj.GetComponentInParent<CircuitComponentLead>();
            GameObject leadObj = lead ? lead.gameObject : null;
            //GameObject rootObj = leadObj ? leadObj.transform.root.gameObject : null;
            CircuitComponent comp = obj.GetComponentInParent<CircuitComponent>();
            //GameObject obj = leadObj?leadObj.transform.parent.gameObject:null;
            //if(obj==null && leadObj!=null) obj = leadObj.transform.root.gameObject;
            if (comp != null)
            {
                //Debug.LogFormat("Selected: {0}/{1}", comp.data.Identifier, leadObj ? leadObj.name : obj.name);
                OnCircuitcomponentClicked(comp, leadObj ?? obj);
            }
        }


        protected override void Start()
        {
            base.Start();

            if (tooltipText != null)
            {
                tooltipText.transform.root.gameObject.SetActive(true);
            }

            Debug.Log("Digital system configuration:\n" + DiScenXpApi.GetConfiguration());

            OnExperienceLoaded();

            // TODO: LoadScenario does not work here
            //LoadScenario();
        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            scenario = FindObjectOfType<ScenarioManager>();
            dragAndDrop = GetComponent<MouseDragAndDrop>();
            UpdateComponentsList();
        }
#endif

        // Update is called once per frame
        protected override void Update()
        {
            base.Update();

            if (updateAll)
            {
                UpdateAll();
            }
            //if (Input.GetKey(KeyCode.Space))
            //if ((Input.GetKeyDown(KeyCode.Space) && lastResult != Result.InProgress)
            //    || (Input.GetKey(KeyCode.Space) && lastResult == Result.InProgress))
            //{
            //    Train();
            //    return;
            //}
            if (Input.GetKey(KeyCode.Delete))
            {
                DeleteComponent(highlightedComponent);
                return;
            }
            if (Input.GetButtonDown("Fire2"))
            {
                CancelSelection();
                if (lastResult != Result.InProgress)
                {
                    NewEpisode();
                }
                return;
            }
            tooltipText.transform.root.position = dragAndDrop.CursorTransform.position - Camera.main.transform.forward;
            if (prevLeadObj != null)
            {
                CircuitComponentLead lead = null;
                if (dragAndDrop.Hit)
                {
                    lead = dragAndDrop.HitObject.GetComponent<CircuitComponentLead>();
                }
                SwitchObject icon = dragAndDrop.CursorTransform.GetComponentInChildren<SwitchObject>();
                if (icon != null)
                {
                    SwitchObject leadIcon = null;
                    if (lead != null)
                    {
                        leadIcon = lead.GetComponentInChildren<SwitchObject>(true);
                    }
                    if (leadIcon != null && prevLeadObj != null && prevLeadObj != lead.gameObject)
                    {
                        lead.GetComponentInChildren<SwitchObject>();
                        icon.Switch(leadIcon.switchedObjects[leadIcon.CurrIndex].name);
                    }
                    else
                    {
                        icon.SwitchOff();
                    }
                }
                dragAndDrop.SnapToObject = (lead != null);
            }
            else
            {
                dragAndDrop.SnapToObject = false;
            }
        }

        private void OnObjectHit(GameObject hitObject)
        {
            tooltipText.transform.root.position = dragAndDrop.HitPoint - Camera.main.transform.forward;
            CircuitComponent hitCircuitComponent = hitObject.GetComponentInParent<CircuitComponent>();
            if (hitCircuitComponent != null)
            {
                tooltipText.text = hitCircuitComponent.data.Identifier;
                highlightedComponent = hitCircuitComponent;
            }
            CircuitComponentLead hitCircuitComponentLead = hitObject.GetComponentInParent<CircuitComponentLead>();
            if (hitCircuitComponentLead != null)
            {
                tooltipText.text = hitCircuitComponentLead.leadName;
            }

        }

        private void OnObjectLeft(GameObject prevObject)
        {
            tooltipText.text = "";
            highlightedComponent = null;
        }


        private void SetUpCable(GameObject cableObj, Transform startTransform, Transform endTransform)
        {
            Cable cable = cableObj.GetComponent<Cable>();
            cable.StartAnchor = startTransform;
            cable.EndAnchor = endTransform;
            cable.Build();
        }


        protected override void SyncSceneState()
        {
            int cablesCount = 0;
            //if (lastResult != Result.InProgress)
            //{
            //    Debug.Log(lastResult);
            //}
            string[] entities = DiScenXpApi.GetScenarioEntities();
            //for (int i = 0; i < cableList.Count; i++)
            //{
            //    Destroy(cableList[i]);
            //}
            //cableList.Clear();
            foreach (string entityId in entities)
            {
                //Debug.Log(entityId + "changed");
                if (!electronicComponents.ContainsKey(entityId))
                {
                    Debug.LogError("Missing component " + entityId);
                    continue;
                }
                CircuitComponent comp = electronicComponents[entityId];
                RelationshipData[] entityRels = DiScenXpApi.GetEntityRelationships(entityId);
                //Debug.Log(entityId + " rel: " + entityRels.Length);
                foreach (RelationshipData rel in entityRels)
                {
                    //string msg = entityId + "/" + rel.RelationshipId + " : " + rel.RelatedEntityId + "/" + rel.RelatedEndPoint;
                    //Debug.Log(msg);
                    CircuitComponent targetObj = electronicComponents[rel.RelatedEntityId];
                    GameObject cableObj;
                    if (cablesCount < cableList.Count)
                    {
                        // reuse existing cables
                        cableObj = cableList[cablesCount];
                    }
                    else
                    {
                        // create new cables if needed
                        cableObj = Instantiate(cablePrefab);
                        cableList.Add(cableObj);
                    }
                    cablesCount++;
                    SetUpCable(cableObj, comp.Lead(rel.RelationshipId).transform, targetObj.Lead(rel.RelatedEndPoint).transform);
                    connectedLeads.Add(comp.Lead(rel.RelationshipId));
                    connectedLeads.Add(targetObj.Lead(rel.RelatedEndPoint));
                }
                bool burntOut = DiScenXpApi.CheckEntityProperty(entityId, "burnt out", "true");
                comp.SetBurntOut(burntOut);
                LedComponent led = comp as LedComponent;

                if (led != null)
                {
                    string litUp = DiScenXpApi.GetEntityProperty(entityId, "lit up");
                    led.LightUp(litUp == "true");
                }

                SwitchComponent sw = comp as SwitchComponent;
                if (sw != null)
                {
                    bool on = DiScenXpApi.CheckEntityProperty(entityId, "position", "1");
                    sw.Switch(on);
                }
            }

            // some cables were reused, the others must be destroyed
            if (cableList.Count > cablesCount)
            {
                for (int i = cablesCount; i < cableList.Count; i++)
                {
                    Destroy(cableList[i]);
                }
                cableList.RemoveRange(cablesCount, cableList.Count - cablesCount);
            }

            ShowHints();
        }

        protected string GetComponentConfig(CircuitComponent cmp)
        {
            string config = cmp.data.Type + " " + cmp.data.Identifier;
            switch (cmp.data.Type)
            {
                case "PowerSupplyDC":
                    PowerSupplyComponent powerSupply = cmp as PowerSupplyComponent;
                    if (powerSupply != null)
                    {
                        config += " " + powerSupply.milliVolt.ToString()
                            + " " + powerSupply.milliAmpere.ToString();
                        ////Debug.LogFormat("{0} {1} {2}",
                        //    powerSupply.data.Identifier,
                        //    powerSupply.milliVolt,
                        //    powerSupply.milliAmpere);
                    }
                    break;

                case "Switch":
                    SwitchComponent sw = cmp as SwitchComponent;
                    if (sw != null)
                    {
                        config += " " + sw.milliVoltMax.ToString()
                            + " " + sw.milliAmpereMax.ToString();
                        ////Debug.LogFormat("{0} {1} {2}",
                        //    sw.data.Identifier,
                        //    sw.milliVoltMax,
                        //    sw.milliAmpereMax);
                    }
                    break;

                case "Resistor":
                    ResistorComponent res = cmp as ResistorComponent;
                    if (res != null)
                    {
                        config += " " + res.ohm.ToString()
                            + " " + res.milliWattMax.ToString();
                        ////Debug.LogFormat("{0} {1} {2}",
                        //    res.data.Identifier,
                        //    res.ohm,
                        //    res.milliWattMax);
                    }
                    break;

                case "LED":
                    LedComponent led = cmp as LedComponent;
                    if (led != null)
                    {
                        config += " " + led.ledModel;
                        ////Debug.LogFormat("{0} {1}",
                        //    led.data.Identifier,
                        //    led.ledModel);
                    }
                    break;
            }

            return config;
        }


        private void ShowActionHint(ActionData action, string iconName)
        {
            switch (action.ActionId)
            {
                case "connect":
                    CircuitComponentLead lead1 = electronicComponents[action.Params[0]].Lead(action.Params[1]);
                    CircuitComponentLead lead2 = electronicComponents[action.Params[2]].Lead(action.Params[3]);
                    if (prevLeadObj != null)
                    {
                        if (prevLeadObj == lead1.gameObject)
                        {
                            if (!connectedLeads.Contains(lead2))
                            {
                                lead2.hintIcon.Switch(iconName);
                            }
                        }
                        if (prevLeadObj == lead2.gameObject)
                        {
                            if (!connectedLeads.Contains(lead1))
                            {
                                lead1.hintIcon.Switch(iconName);
                            }
                        }
                    }
                    else
                    {
                        if (!connectedLeads.Contains(lead1))
                        {
                            lead1.hintIcon.Switch(iconName);
                        }
                        if (!connectedLeads.Contains(lead2))
                        {
                            lead2.hintIcon.Switch(iconName);
                        }
                    }
                    break;
                case "switch":
                    if (prevLeadObj == null)
                    {
                        CircuitComponent comp = electronicComponents[action.Params[0]];
                        SwitchObject swObj = comp.transform.Find("IconSwitch").GetComponent<SwitchObject>();
                        swObj.Switch(iconName);
                    }
                    break;
            }
        }

        private void ShowHints()
        {
            foreach (CircuitComponent comp in electronicComponentArray)
            {
                foreach (CircuitComponentLead lead in comp.Leads)
                {
                    lead.hintIcon.SwitchOff();
                }
                Transform iconSwitch = comp.transform.Find("IconSwitch");
                if (iconSwitch != null)
                {
                    iconSwitch.GetComponent<SwitchObject>().SwitchOff();
                }
            }

            if (prevLeadObj != null)
            {
                ActionData[] availableActions = DiScenXpApi.GetAvailableActions();
                foreach (ActionData action in availableActions)
                {
                    ShowActionHint(action, "Unknown");
                }
                ActionData[] forbiddenActions = DiScenXpApi.GetForbiddenActions();
                foreach (ActionData action in forbiddenActions)
                {
                    ShowActionHint(action, "Stop");
                }
            }
            else
            {
                ActionData[] forbiddenActions = DiScenXpApi.GetForbiddenActions();
                foreach (ActionData action in forbiddenActions)
                {
                    if (action.ActionId == "switch")
                    {
                        ShowActionHint(action, "Stop");
                    }
                }
            }
            ActionData[] suggestedActions = DiScenXpApi.GetSuggestedActions();
            foreach (ActionData action in suggestedActions)
            {
                ShowActionHint(action, "Suggested");
            }
            //Debug.Log(DiScenXpApi.GetCurrentMessage());
        }

        private void OnEnable()
        {
            dragAndDrop.BeginMouseOverObject += OnObjectHit;
            dragAndDrop.EndMouseOverObject += OnObjectLeft;
            dragAndDrop.ObjectSelected += OnObjectClicked;
            tooltipText.text = "";
        }

        private void OnDisable()
        {
            dragAndDrop.BeginMouseOverObject -= OnObjectHit;
            dragAndDrop.EndMouseOverObject -= OnObjectLeft;
            dragAndDrop.ObjectSelected -= OnObjectClicked;
        }

        private void Awake()
        {
            cyberSystemName = "SimplECircuitCybSys";
            if (dragAndDrop == null)
            {
                dragAndDrop = GetComponent<MouseDragAndDrop>();
            }
            if (scenario == null)
            {
                scenario = FindObjectOfType<ScenarioManager>();
            }
            if (dragAndDrop == null)
            {
                dragAndDrop = GetComponent<MouseDragAndDrop>();
            }
            UpdateComponentsList();
        }
    }
}
