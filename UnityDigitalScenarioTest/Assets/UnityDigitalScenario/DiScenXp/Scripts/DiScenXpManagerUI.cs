using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DiScenFw;

namespace UnityDigitalScenario
{
    public abstract class DiScenXpManagerUI : DiScenXpManager
    {
        [Tooltip("Text for goal name displaying")]
        [SerializeField]
        protected Text goalNameText;

        [Tooltip("Dropdown for goal selection")]
        [SerializeField]
        protected Dropdown goalNameDropdown;

        [Tooltip("Viewport content panel for conditions list")]
        [SerializeField]
        protected RectTransform conditionContentPanel;

        [Tooltip("Text for statistics displaying")]
        [SerializeField]
        protected Text statisticsText;

        [Tooltip("Text for displaying a tooltip under the cursor")]
        [SerializeField]
        protected Text tooltipText;

        [Tooltip("Button for applying the changes to conditions")]
        [SerializeField]
        protected Button conditionApplyButton;

        [Tooltip("Button for removing the current goal.")]
        [SerializeField]
        protected Button goalRemoveButton;

        [Tooltip("Button for cancelling the changes to conditions")]
        [SerializeField]
        protected Button conditionCancelButton;

        [Tooltip("Button for saving a new goal in the new goal panel")]
        [SerializeField]
        protected Button goalSaveButton;

        protected string goalInputBuffer;
        protected bool updatingUI = false;
        protected GameObject conditionItemRemovedObj = null;
        protected GameObject conditionItemAddedObj = null;
        protected GameObject conditionItemTemplate = null;


        /// <summary>
        /// An experience was succesfully loaded from the configured path.
        /// </summary>
        public override void OnExperienceLoaded()
        {
            conditionApplyButton.interactable = false;
            conditionCancelButton.interactable = false;
            UpdateGoalUI();
        }


        /// <summary>
        /// Switch to the given goal and update the UI.
        /// </summary>
        /// <param name="goalName">Name of the goal to select.</param>
        /// <returns>true on success, false on error</returns>
        public override bool SetCurrentGoal(string goalName)
        {
            if (base.SetCurrentGoal(goalName))
            {
                UpdateGoalUI();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Add a new goal and related experience and update the UI.
        /// </summary>
        /// <param name="goalName">Name of the new goal.</param>
        /// <returns>true on success, false on error</returns>
        public override bool AddNewGoal(string goalName)
        {
            if (!base.AddNewGoal(goalName))
            {
                return false;
            }
            UpdateGoalUI();
            return true;
        }


        /// <summary>
        /// Switch to the goal with the given index in the dropdown list of the UI.
        /// </summary>
        /// <param name="goalIndex">Index of the goal in the dropdown list of the UI.</param>
        public void SelectGoal(int goalIndex)
        {
            string[] goals = DiScenXpApi.GetGoals();
            if (goals.Length > goalIndex)
            {
                string goalName = goals[goalIndex];
                SetCurrentGoal(goalName);
            }
        }


        /// <summary>
        /// Create a new goal from the name entered in the UI input field.
        /// </summary>
        public void NewGoal()
        {
            string goalName = goalInputBuffer;
            AddNewGoal(goalName);
        }


        /// <summary>
        /// Remove the current goal and update the UI.
        /// </summary>
        public void RemoveGoal()
        {
            string goalToRemove = currentGoalName;
            List<string> goals = new List<string>( DiScenXpApi.GetGoals());
            if (goals.Count > 1 && goals.Contains(goalToRemove))
            {
                int idx = goals.IndexOf(goalToRemove);
                idx = idx < 1 ? idx + 1 : idx - 1;
                base.SetCurrentGoal(goals[idx]);
                DiScenXpApi.RemoveGoal(goalToRemove);
                UpdateGoalUI();
            }
        }


        /// <summary>
        /// Set the experience update on/off (UI callback).
        /// </summary>
        /// <param name="updateXp">true=update on, false=update off</param>
        public void SetExperienceUpdate(bool updateXp)
        {
            updateExperience = updateXp;
        }


        /// <summary>
        /// Set the auto training live update on/off (UI callback).
        /// </summary>
        /// <param name="update">true=update on, false=update off</param>
        public void SetAutoTrainingLiveUpdate(bool update)
        {
            autoTrainingLiveUpdate = update;
        }


        /// <summary>
        /// Set the AI agent learning on/off (UI callback).
        /// </summary>
        /// <param name="learning">true=learning on, false=learning off</param>
        public void SetAgentLearning(bool learning)
        {
            agentLearning = learning;
        }


        /// <summary>
        /// Set the AI agent deadlock detection on/off (UI callback).
        /// </summary>
        /// <param name="on">true=enabled, false=disabled</param>
        public void SetDeadlockDetection(bool on)
        {
            deadlockDetection = on;
            DiScenXpApi.SetDeadlockDetection(deadlockDetection);
        }


        /// <summary>
        /// Store the goal name from input field (UI callback).
        /// </summary>
        /// <param name="inputText">Edited goal name</param>
        public void SetGoalInputBuffer(string inputText)
        {
            goalInputBuffer = inputText;
            if (goalSaveButton)
            {
                if (goalInputBuffer == currentGoalName || string.IsNullOrEmpty(goalInputBuffer))
                {
                    goalSaveButton.interactable = false;
                }
                else
                {
                    goalSaveButton.interactable = true;
                }
            }
        }


        /// <summary>
        /// Update the UI according to the condition configuration (UI callback).
        /// </summary>
        /// <param name="dropdown"></param>
        public void OnConditionEdit(Dropdown dropdown)
        {
            if (!updatingUI)
            {
                conditionApplyButton.interactable = true;
                conditionCancelButton.interactable = true;
                Transform conditionItemPanel = dropdown.transform.parent;
                Dropdown entityDropdown = conditionItemPanel.GetChild(0).GetComponent<Dropdown>();
                Dropdown propertyDropdown = conditionItemPanel.GetChild(1).GetComponent<Dropdown>();
                string entityId = entityDropdown.captionText.text;
                string propertyId = propertyDropdown.captionText.text;
                if (dropdown == entityDropdown)
                {
                    UpdateConditionPropertyUI(entityId, entityDropdown);
                    propertyId = propertyDropdown.captionText.text;
                    UpdateConditionValueUI(entityId, propertyId, propertyDropdown);
                }
                else if (dropdown == propertyDropdown)
                {
                    UpdateConditionValueUI(entityId, propertyId, propertyDropdown);
                }
            }
        }


        /// <summary>
        /// Apply changes to the conditions changes in the UI (UI callback).
        /// </summary>
        public void OnConditionEditApply()
        {
            DiScenXpApi.ResetSuccessCondition();
            successConditions = new EntityCondition[conditionContentPanel.childCount - 1];
            for (int i = 1; i < conditionContentPanel.childCount; i++)
            {
                Transform conditionItem = conditionContentPanel.GetChild(i).GetChild(0);
                Dropdown entityDropdown = conditionItem.GetChild(0).GetComponent<Dropdown>();
                Dropdown propertyDropdown = conditionItem.GetChild(1).GetComponent<Dropdown>();
                Dropdown valueDropdown = conditionItem.GetChild(2).GetComponent<Dropdown>();
                EntityCondition cond = new EntityCondition();
                cond.EntityId = entityDropdown.options[entityDropdown.value].text;
                cond.PropertyId = propertyDropdown.options[propertyDropdown.value].text;
                cond.PropertyValue = valueDropdown.options[valueDropdown.value].text;
                successConditions[i - 1] = cond;
                DiScenXpApi.AddSuccessCondition(cond.EntityId, cond.PropertyId, cond.PropertyValue);
            }

            conditionApplyButton.interactable = false;
            conditionCancelButton.interactable = false;
            UpdateGoalUI();
            ClearCurrentExperience();
        }


        /// <summary>
        /// Discard changes to the conditions changes in the UI (UI callback).
        /// </summary>
        public void OnConditionEditCancel()
        {
            conditionApplyButton.interactable = false;
            conditionCancelButton.interactable = false;
            UpdateGoalUI();
        }


        /// <summary>
        /// Store the added element in the conditions list (UI callback).
        /// </summary>
        /// <param name="itemObject">New element in the conditions list</param>
        public void OnConditionAdd(GameObject itemObject)
        {
            conditionItemAddedObj = itemObject;
        }


        /// <summary>
        /// Store the removed element in the conditions list (UI callback).
        /// </summary>
        /// <param name="itemObject">New element in the conditions list</param>
        public void OnConditionRemove(GameObject itemObject)
        {
            conditionItemRemovedObj = itemObject;
        }


        /// <summary>
        /// Compute statistics and update UI.
        /// </summary>
        protected void RefreshStatistics()
        {
            if (statisticsText != null)
            {
                int totalTrials = autoTrainingFailureCount + autoTrainingSuccessCount + autoTrainingDeadlockCount;
                int successRate = totalTrials > 0 ? autoTrainingSuccessCount * 100 / totalTrials : 0;
                statisticsText.text = "Auto training statistics:\n"
                    + "episodes = " + totalTrials.ToString()
                    + "\n  succeeded = " + autoTrainingSuccessCount.ToString()
                    + "\n  failed = " + autoTrainingFailureCount.ToString()
                    + "\n  deadlock = " + autoTrainingDeadlockCount.ToString()
                    + "\n  success rate = " + successRate.ToString() + " %";
            }
        }


        /// <summary>
        /// Update the UI related to the current goal.
        /// </summary>
        protected void UpdateGoalUI()
        {
            updatingUI = true;
            string goalName = DiScenXpApi.GetCurrentGoal();
            successConditions = DiScenXpApi.GetSuccessConditions();
            List<string> goals = new List<string>(DiScenXpApi.GetGoals());
            if (goalRemoveButton != null)
            {
                goalRemoveButton.interactable = goals.Count > 1;
            }
            if (goalNameText != null)
            {
                goalNameText.text = goalName;
            }
            if (goalNameDropdown != null)
            {
                goalNameDropdown.options.Clear();
                if (!goals.Contains(goalName))
                {
                    goals.Add(goalName);
                }
                goalNameDropdown.AddOptions(goals);
                goalNameDropdown.value = goals.IndexOf(goalName);

                goalNameDropdown.captionText.text = goalName;
            }
            if (conditionContentPanel != null)
            {
                List<GameObject> items = new List<GameObject>();
                for (int i = 0; i < conditionContentPanel.childCount; i++)
                {
                    GameObject itemPanel = conditionContentPanel.GetChild(i).gameObject;
                    if (itemPanel != conditionItemTemplate)
                    {
                        items.Add(itemPanel);
                    }
                }
                foreach (GameObject item in items)
                {
                    Destroy(item);
                }

                if (successConditions.Length > 0)
                {
                    conditionItemTemplate.SetActive(false);

                    int count = 0;
                    foreach (EntityCondition cond in successConditions)
                    {
                        count++;
                        GameObject lastItemObj = Instantiate(conditionItemTemplate, conditionContentPanel);
                        lastItemObj.SetActive(true);
                        lastItemObj.name = "Item" + count;
                        UpdateConditionUI(lastItemObj, cond.EntityId, cond.PropertyId, cond.PropertyValue);
                    }
                }
                else
                {
                    // no success condition defined
                    conditionItemTemplate.SetActive(true);
                }
            }
            updatingUI = false;
        }



        /// <summary>
        /// Update the UI related to the success conditions.
        /// </summary>
        protected void UpdateConditionUI(GameObject condItemObj,
            string entityId = null, string propertyId = null, string propertyValue = null)
        {
            List<string> compDropOptions = new List<string>();
            Dictionary<string, int> compIndex = new Dictionary<string, int>();
            string[] scenarioEntities = DiScenXpApi.GetScenarioEntities();
            for (int i = 0; i < scenarioEntities.Length; i++)
            {
                compDropOptions.Add(scenarioEntities[i]);
                compIndex.Add(scenarioEntities[i], i);
            }

            Transform condItemTransform = condItemObj.transform;
            Transform conditionItemPanel = condItemTransform.GetChild(0);
            Dropdown lastComponentDropdown = conditionItemPanel.GetChild(0).GetComponent<Dropdown>();
            Dropdown propertyDropdown = conditionItemPanel.GetChild(1).GetComponent<Dropdown>();
            Dropdown valueDropdown = conditionItemPanel.GetChild(2).GetComponent<Dropdown>();
            lastComponentDropdown.interactable = true;
            propertyDropdown.interactable = true;
            valueDropdown.interactable = true;
            Button removeButton = condItemTransform.GetChild(2).GetComponent<Button>();
            removeButton.interactable = true;
            lastComponentDropdown.options.Clear();
            lastComponentDropdown.AddOptions(compDropOptions);
            if (entityId != null && !compIndex.ContainsKey(entityId))
            {
                entityId = null;
            }
            if (entityId != null)
            {
                lastComponentDropdown.value = compIndex[entityId];
                lastComponentDropdown.captionText.text = entityId;
            }
            if (entityId == null && scenarioEntities.Length > 0)
            {
                entityId = scenarioEntities[0];
            }
            if (entityId != null)
            {
                UpdateConditionPropertyUI(entityId, lastComponentDropdown, propertyId);
                List<string> propOptions = new List<string>(DiScenXpApi.GetPossibleEntityProperties(entityId));
                if (propertyId == null && propOptions.Count > 0)
                {
                    propertyId = propOptions[0];
                }
                if (propertyId != null)
                {
                    UpdateConditionValueUI(entityId, propertyId, propertyDropdown, propertyValue);
                }
            }
        }


        /// <summary>
        /// Update the UI related to a success condition configuration.
        /// </summary>
        protected void UpdateConditionValueUI(string entityId, string propertyId, Dropdown propertyDropdown, string propertyValue = null)
        {
            Transform conditionItemPanel = propertyDropdown.transform.parent;
            List<string> valOptions = new List<string>(DiScenXpApi.GetPossibleEntityPropertyValues(entityId, propertyId));
            Dropdown valueDropdown = conditionItemPanel.GetChild(2).GetComponent<Dropdown>();
            valueDropdown.options.Clear();
            valueDropdown.AddOptions(valOptions);
            if (propertyValue != null)
            {
                valueDropdown.value = valOptions.IndexOf(propertyValue);
                valueDropdown.captionText.text = propertyValue;
            }
        }


        /// <summary>
        /// Perform a complete training step and update the UI.
        /// </summary>
        protected override void Train()
        {
            base.Train();
            if (lastResult != Result.InProgress)
            {
                RefreshStatistics();
            }
        }


        /// <summary>
        /// Load the cyber system, initialize, start a new episode and update the UI.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            InitConditionItemTemplate();
            UpdateGoalUI();
        }


        /// <summary>
        /// Manage automatic training and pending actions.
        /// </summary>
        protected override void Update()
        {
            base.Update();
            if (conditionItemRemovedObj)
            {
                int count = conditionItemRemovedObj.transform.parent.childCount;
                Destroy(conditionItemRemovedObj);
                conditionApplyButton.interactable = true;
                conditionCancelButton.interactable = true;
                conditionItemRemovedObj = null;
                if (count == 2)
                {
                    conditionItemTemplate.SetActive(true);
                }
            }
            if (conditionItemAddedObj)
            {
                updatingUI = true;
                GameObject lastItemObj = Instantiate(conditionItemAddedObj, conditionContentPanel);
                bool isTemplateItem = (conditionItemAddedObj == conditionItemTemplate);
                conditionItemAddedObj = null;
                if (isTemplateItem)
                {
                    UpdateConditionUI(lastItemObj);
                    lastItemObj.SetActive(true);
                    conditionItemTemplate.SetActive(false);
                }
                conditionApplyButton.interactable = true;
                conditionCancelButton.interactable = true;
                updatingUI = false;
            }
        }


        private void InitConditionItemTemplate()
        {
            if (conditionItemTemplate == null)
            {
                Transform templateItemTransform = conditionContentPanel.GetChild(0);
                conditionItemTemplate = templateItemTransform.gameObject;
                conditionItemTemplate.SetActive(false);

                Dropdown entityDropdown = templateItemTransform.GetChild(0).GetChild(0).GetComponent<Dropdown>();
                Dropdown propertyDropdown = templateItemTransform.GetChild(0).GetChild(1).GetComponent<Dropdown>();
                Dropdown valueDropdown = templateItemTransform.GetChild(0).GetChild(2).GetComponent<Dropdown>();
                entityDropdown.options.Clear();
                entityDropdown.AddOptions(new List<string> { "(entity)" });
                propertyDropdown.options.Clear();
                propertyDropdown.AddOptions(new List<string> { "(property)" });
                valueDropdown.options.Clear();
                valueDropdown.AddOptions(new List<string> { "(value)" });
                entityDropdown.interactable = false;
                propertyDropdown.interactable = false;
                valueDropdown.interactable = false;
                Button removeButton = templateItemTransform.GetChild(2).GetComponent<Button>();
                removeButton.interactable = false;
            }
        }


        private void UpdateConditionPropertyUI(
            string entityId, Dropdown entityDropdown, string propertyId = null)
        {
            Transform conditionItemPanel = entityDropdown.transform.parent;
            Dropdown propertyDropdown = conditionItemPanel.GetChild(1).GetComponent<Dropdown>();
            List<string> propOptions = new List<string>( DiScenXpApi.GetPossibleEntityProperties(entityId));
            propertyDropdown.options.Clear();
            propertyDropdown.AddOptions(propOptions);
            if (propertyId != null)
            {
                propertyDropdown.value = propOptions.IndexOf(propertyId);
                propertyDropdown.captionText.text = propertyId;
            }
        }
    }
}
