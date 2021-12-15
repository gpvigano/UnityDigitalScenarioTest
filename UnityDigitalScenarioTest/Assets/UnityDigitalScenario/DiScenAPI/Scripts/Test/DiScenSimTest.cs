using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityDigitalScenario;

public class DiScenSimTest : SimulationManager
{
    [SerializeField]
    private Slider progressSlider;

    [SerializeField]
    private Text progressText;

    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button pauseButton;

    [SerializeField]
    private Button stopButton;

    private bool firstFrame = true;
    private bool loaded = false;
    private bool started = false;
    private bool updatingUI = false;
    private bool progressModified = false;
    private float progressChange = 0;

    public void OnSliderChange(float val)
    {
        if (!updatingUI && progressSlider && DiScenApiUnity.SimulationStarted())
        {
            progressModified = true;
            progressChange = val;
        }
    }

    protected void UpdateSlider()
    {
        if (DiScenApiUnity.SimulationStarted())
        {
            updatingUI = true;
            progressSlider.value = (float)DiScenApiUnity.ComputeSimulationProgress();
            if (progressText)
            {
                progressText.text = DiScenApiUnity.GetSimulationDateTimeAsString();
            }
            updatingUI = false;
        }
    }

    protected override void Update()
    {
        if (progressModified)
        {
            DiScenApiUnity.SetSimulationProgress(progressChange);
            if (progressText)
            {
                progressText.text = DiScenApiUnity.GetSimulationDateTimeAsString();
            }
            progressModified = false;
        }
        base.Update();
        if (firstFrame)
        {
            scenarioManager.LoadScenario();
            firstFrame = false;
        }
        if (!loaded && ScenarioLoaded)
        {
            LoadHistory();
            loaded = true;
        }
        if (!started && DiScenApiUnity.ValidSimulation())
        {
            started = true;
            PlaySimulation();
        }
        if (started)
        {
            UpdateSlider();
        }
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        DiScenApiUnity.SimulationLoadedEvent += OnSimulationLoaded;
        DiScenApiUnity.SimulationPlayEvent += OnSimulationPlay;
        DiScenApiUnity.SimulationPauseEvent += OnSimulationPause;
        DiScenApiUnity.SimulationStopEvent += OnSimulationStop;
    }


    protected override void OnDisable()
    {
        DiScenApiUnity.SimulationLoadedEvent -= OnSimulationLoaded;
        DiScenApiUnity.SimulationPlayEvent -= OnSimulationPlay;
        DiScenApiUnity.SimulationPauseEvent -= OnSimulationPause;
        DiScenApiUnity.SimulationStopEvent -= OnSimulationStop;
        base.OnDisable();
    }

    private void OnSimulationLoaded()
    {
        if(pauseButton)
        {
            pauseButton.interactable = false;
        }
        if(playButton)
        {
            playButton.interactable = true;
        }
        if(stopButton)
        {
            stopButton.interactable = false;
        }
        if (progressSlider)
        {
            progressSlider.interactable = false;
        }
    }


    private void OnSimulationUpdated()
    {
    }


    private void OnSimulationPlay()
    {
        if(pauseButton)
        {
            pauseButton.interactable = true;
        }
        if(playButton)
        {
            playButton.interactable = false;
        }
        if(stopButton)
        {
            stopButton.interactable = true;
        }
        if(progressSlider)
        {
            progressSlider.interactable = true;
        }
    }


    private void OnSimulationPause()
    {
        if(playButton)
        {
            playButton.interactable = true;
        }
        if(pauseButton)
        {
            pauseButton.interactable = false;
        }
        if(stopButton)
        {
            stopButton.interactable = true;
        }
    }


    private void OnSimulationStop()
    {
        if(playButton)
        {
            playButton.interactable = true;
        }
        if(pauseButton)
        {
            pauseButton.interactable = false;
        }
        if(stopButton)
        {
            stopButton.interactable = false;
        }
        if(progressSlider)
        {
            updatingUI = true;
            progressSlider.value = 0;
            progressSlider.interactable = false;
            updatingUI = false;
        }
        if (progressText)
        {
            progressText.text = "";
        }
    }


    private void OnSimulationTimeChanged(float progress)
    {
    }
}
