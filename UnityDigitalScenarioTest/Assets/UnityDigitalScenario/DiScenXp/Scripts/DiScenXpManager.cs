using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DiScenFw;

namespace UnityDigitalScenario
{
    public abstract class DiScenXpManager : MonoBehaviour
    {
        [Tooltip("Update the experience when an episode is completed.")]
        [SerializeField]
        protected bool updateExperience = true;

        [Tooltip("Update the scene while the AI agent is learning.")]
        [SerializeField]
        protected bool autoTrainingLiveUpdate = true;

        [Tooltip("Let the AI agent learn from actions.")]
        [SerializeField]
        protected bool agentLearning = true;

        [Tooltip("Enable deadlock detection when training the AI agent.")]
        [SerializeField]
        protected bool deadlockDetection = true;

        [Tooltip("Success conditions for the selected goal")]
        [SerializeField]
        protected EntityCondition[] successConditions;

        [Tooltip("Name of the current goal")]
        [SerializeField]
        protected string currentGoalName = "Undefined";

        protected Result lastResult = Result.InProgress;
        protected bool autoTraining = false;
        protected int autoTrainingSuccessCount = 0;
        protected int autoTrainingFailureCount = 0;
        protected int autoTrainingDeadlockCount = 0;
        //private ScenarioManager scenario;


        /// <summary>
        /// Start a new episode in the current experience.
        /// </summary>
        public virtual void NewEpisode()
        {
            //CancelSelection();
            //CleanUp();
            DiScenXpApi.NewEpisode();
            SyncSceneState();
        }


        /// <summary>
        /// Load an experience from the configured path.
        /// </summary>
        public virtual void LoadExperience(bool updateVE = true)
        {
            //CancelSelection();
            string filePath = GetExperienceFilePath();
            if (DiScenXpApi.LoadCurrentExperience(filePath))
            {
                Debug.Log("Experience loaded from " + filePath);
            }
            else
            {
                Debug.LogWarning("Failed to load experience from " + filePath);
            }
            if(updateVE)
            {
                OnExperienceLoaded();
            }
        }


        /// <summary>
        /// An experience was succesfully loaded from the configured path.
        /// </summary>
        public virtual void OnExperienceLoaded()
        {
            NewEpisode();
            //conditionApplyButton.interactable = false;
            //conditionCancelButton.interactable = false;
            //UpdateGoalUI();
        }

        /// <summary>
        /// Save the current experience to the configured path.
        /// </summary>
        public virtual void SaveExperience()
        {
            //CancelSelection();
            string filePath = GetExperienceFilePath();
            if (DiScenXpApi.SaveCurrentExperience(filePath))
            {
                Debug.Log("Experience saved to " + filePath);
            }
            else
            {
                Debug.LogWarning("Failed to save experience to " + filePath);
            }
        }


        /// <summary>
        /// Clear the current experience.
        /// </summary>
        public virtual void ClearCurrentExperience()
        {
            //CancelSelection();
            DiScenXpApi.ClearCurrentExperience();
            NewEpisode();
        }


        /// <summary>
        /// Start automatic training mode.
        /// </summary>
        public virtual void StartAutoTraining()
        {
            //CancelSelection();
            autoTraining = true;
            autoTrainingSuccessCount = 0;
            autoTrainingFailureCount = 0;
            autoTrainingDeadlockCount = 0;
            DiScenXpApi.SetDeadlockDetection(deadlockDetection);
        }


        /// <summary>
        /// Stop automatic training mode.
        /// </summary>
        public virtual void StopAutoTraining()
        {
            autoTraining = false;
            NewEpisode();
        }


        /// <summary>
        /// Switch to the given goal.
        /// </summary>
        /// <param name="goalName">Name of the goal to select.</param>
        /// <returns>true on success, false on error</returns>
        public virtual bool SetCurrentGoal(string goalName)
        {
            if (DiScenXpApi.SetCurrentGoal(goalName))
            {
                Debug.Log("Goal " + goalName + " selected");
                NewEpisode();
                currentGoalName = goalName;
                DiScenXpApi.SetDeadlockDetection(deadlockDetection);
                //UpdateGoalUI();
                return true;
            }
            return false;
        }


        /// <summary>
        /// Add a new goal and related experience.
        /// </summary>
        /// <param name="goalName">Name of the new goal.</param>
        /// <returns>true on success, false on error</returns>
        public virtual bool AddNewGoal(string goalName)
        {
            if (DiScenXpApi.AddNewGoal(goalName))
            {
                DiScenXpApi.SetCurrentGoal(goalName);
                //CancelSelection();
                //CleanUp();
                currentGoalName = goalName;
                InitializeScenario();
                NewEpisode();
            }
            else
            {
                Debug.LogError("Failed to add new goal " + goalName);
                return false;
            }
            DiScenXpApi.SetDeadlockDetection(deadlockDetection);
            return true;
            //UpdateGoalUI();
        }


        /// <summary>
        /// Method implementing the sychronization from the framework scenario state to the Unity scene.
        /// </summary>
        protected abstract void SyncSceneState();


        /// <summary>
        /// Name of the connected cyber system (DLL name without extension).
        /// </summary>
        protected string cyberSystemName = "";


        /// <summary>
        /// Get the file path for the experience serialization.
        /// </summary>
        protected string GetExperienceFilePath()
        {
            string filePath = Application.streamingAssetsPath + "/" + currentGoalName.Replace(' ', '_') + ".json";
            return filePath;
        }


        /// <summary>
        /// Perform a complete training step, also updating experience (if allowed).
        /// </summary>
        protected virtual void Train()
        {
            lastResult = DiScenXpApi.TrainAssistant(updateExperience, agentLearning);
            if (autoTrainingLiveUpdate)
            {
                SyncSceneState();
            }
            if (lastResult != Result.InProgress)
            {
                if (lastResult == Result.Succeeded)
                {
                    autoTrainingSuccessCount++;
                }
                if (lastResult == Result.Failed)
                {
                    autoTrainingFailureCount++;
                }
                if (lastResult == Result.Deadlock)
                {
                    autoTrainingDeadlockCount++;
                }
                //if (autoTrainingLiveUpdate)
                //{
                //    string msg = lastResult + ":  " + DiScenXpApi.GetInfo("CircuitSchema") + "\n";
                //    Debug.Log(msg);
                //    CleanUp();
                //}
                //RefreshStatistics();
            }

        }


        /// <summary>
        /// Initialize the scenario, creating a new goal if none is defined.
        /// </summary>
        protected virtual void InitializeScenario()
        {
            if (string.IsNullOrEmpty(currentGoalName))
            {
                currentGoalName = DiScenXpApi.GetCurrentGoal();
            }
            if (string.IsNullOrEmpty(currentGoalName))
            {
                string[] goals = DiScenXpApi.GetGoals();
                if (goals != null && goals.Length > 0)
                    currentGoalName = goals[0];
            }
            if (!string.IsNullOrEmpty(currentGoalName))
            {
                if (!DiScenXpApi.SetCurrentGoal(currentGoalName))
                {
                    DiScenXpApi.ResetSuccessCondition();

                    foreach (EntityCondition cond in successConditions)
                    {
                        DiScenXpApi.AddSuccessCondition(
                            cond.EntityId,
                            cond.PropertyId,
                            cond.PropertyValue);
                    }
                }

                DiScenXpApi.SetDeadlockDetection(deadlockDetection);
                //UpdateConfiguration();
            }
        }


        /// <summary>
        /// Load the configured cyber system, initialize the scenario and start a new episode.
        /// </summary>
        protected virtual void Start()
        {
            string cybDysPath = Application.streamingAssetsPath + "/" + cyberSystemName;
            Debug.Log("Loading cyber system: " + cybDysPath);
            if (DiScenXpApi.LoadCyberSystem(cybDysPath))
            {
                Debug.Log("Cyber system loaded.");
            }
            else
            {
                Debug.LogError("Failed to load cyber system.");
                throw new MissingComponentException("Failed to load cyber system.");
            }
            LoadExperience(false); // delay update after initialization
            InitializeScenario();
            NewEpisode();
        }


        /// <summary>
        /// Perform a complete training step if automatic training is set.
        /// </summary>
        protected virtual void Update()
        {
            if (autoTraining)
            {
                Train();
                //if (lastResult == Result.Succeeded)
                //{
                //    autoTraining = false;
                //}
                return;
            }

        }


        /// <summary>
        /// Deinitialize the framwork at application exit.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            Debug.Log("DiScenXpApi.Deinitialize()");
            DiScenXpApi.Deinitialize();
        }
    }
}