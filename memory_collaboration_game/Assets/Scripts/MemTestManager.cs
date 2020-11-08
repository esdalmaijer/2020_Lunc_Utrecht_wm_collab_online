using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class MemTestManager : MonoBehaviour
{
    // GUI variables.
    [SerializeField] int nTrials;
    [SerializeField] int[] nStim = {2,4};
    [SerializeField] int minDist = 200;
    [SerializeField] int minOriDist = 10;
    [SerializeField] float stimDuration = 5.0f;
    [SerializeField] float maintenanceDuration = 3.0f;
    [SerializeField] float[] interTrialIntervalMinMax = {1.0f, 1.5f};
    [SerializeField] Gabor stimulusPrefab;
    [SerializeField] Database dataLogger;
    [SerializeField] string nextScene;

    // VARIABLES.
    // Participant number.
    private int participantID;
    // Maximum number of iterations while finding random numbers with
    // pre-defined conditions.
    private int maxIter = 1000;
    // Trial management.
    private int trialNr = 0;
    int[] trialSetSizes;
    private bool trialRandomisationPrepared = false;
    private bool trialRunning = false;
    private bool responsePhase = false;
    private bool responseRecorded = false;
    // Stimuli.
    private List<Gabor> stimList = new List<Gabor>();
    private int[] stimOri;
    private int[] stimX;
    private int[] stimY;
    int targetStimNr;
    // Timing.
    private float expStart;
    private string updateKey = "";
    private Dictionary<string, float> trialTiming = new
        Dictionary<string, float>();

    // Start is called before the first frame update
    void Start()
    {
        // Get the participant number.
        participantID = PlayerPrefs.GetInt("PPNR");
        Debug.Log(participantID);

        // Record starting time.
        expStart = Time.time;
        // Create a dict for storing trial timing, and one for recording when a
        // time should be recorded.
        string[] keyList = 
            {"trialStart", "stimOnset", "stimOffset", "respOnset",
                "respOffset"};
        foreach (string key in keyList)
        {
            trialTiming.Add(key, 0.0f);
        }

        // TRIAL RANDOMISATION
        // Compute how many trials there should be per stimulus number.
        int nTrialsPerSetSize = (int)Math.Floor((float)nTrials /
            (float)nStim.Length);
        // Create an array for all trials' set size.
        trialSetSizes = new int[nTrials];
        // Add the number for trials for each set size.
        for (int i = 0; i < nStim.Length; i++)
        {
            for (int j = 0; j < nTrialsPerSetSize; j++)
            {
                trialSetSizes[j + i*nTrialsPerSetSize] = nStim[i];
            }
        }
        // Fill the rest of the trials with randomly sampled set sizes.
        System.Random rand = new System.Random();
        for (int j = nTrialsPerSetSize * nStim.Length; j < nTrials; j++)
        {
            int i = rand.Next(nStim.Length);
            trialSetSizes[j] = nStim[i];
        }
        // Shuffle the array in place.
        for (int i = trialSetSizes.Length - 1; i > 0; i--)
        {
            int randomIndex = rand.Next(0, i + 1);

            int temp = trialSetSizes[i];
            trialSetSizes[i] = trialSetSizes[randomIndex];
            trialSetSizes[randomIndex] = temp;
        }
        // Log to the console to double-check randomisation. (Works!)
        //Debug.Log("Trial set sizes: " + String.Join(",", new
        //    List<int>(trialSetSizes).ConvertAll(i => i.ToString()).ToArray()));
        // Set the trial randomisation boolean.
        trialRandomisationPrepared = true;
    }

    // Update is called once per frame
    void Update()
    {
        // TRIAL PROGRESS
        // Run the next trial if one is not currently running.
        if (trialRunning == false & trialRandomisationPrepared == true)
        {
            // Check if the current trial exceeds the required number.
            if (trialNr >= nTrials)
            {
                // Load the next scene.
                StartNextScene();
            }
            // Start the next trial.
            else
            {
                //Debug.Log("Starting trial " + trialNr.ToString() +
                //    " with set size" + trialSetSizes[trialNr]);
                StartCoroutine(RunTrial(trialSetSizes[trialNr], stimDuration,
                    maintenanceDuration));
            }
        }
        // If we are currently in the response phase, check if a response has
        // been detected.
        if (responsePhase == true)
        {
            if (Input.GetKey(KeyCode.Space) &
                stimList[targetStimNr].GetMasked() == false)
            {
                trialTiming["respOffset"] = Time.time;
                RecordResponse();
                responsePhase = false;
            }
        }

        // TIMESTAMPS
        // Check if an event should be recorded.
        if (updateKey != "")
        {
            // Record the time.
            trialTiming[updateKey] = Time.time;
            updateKey = "";
        }
    }

    // TASK END
    private void StartNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    // TRIAL LOGIC
    private IEnumerator RunTrial(int nStimuli, float stimDuration,
        float maintenanceDuration)
    {
        // Set the boolean that indicates a trial is currently running.
        trialRunning = true;

        // Prepare the stimuli.
        PrepareStimuli(nStimuli);

        // Record the trial start.
        trialTiming["trialStart"] = Time.time;

        // Show the stimuli.
        ShowStimuli();
        // Wait for a the stimulus duration.
        yield return new WaitForSeconds(stimDuration);

        // Hide the stimuli.
        HideStimuli();
        // Wait for the delay interval.
        yield return new WaitForSeconds(maintenanceDuration);

        // Activate the response.
        StartResponsePhase();

        // Wait for response.
        responseRecorded = false;
        yield return new WaitUntil(() => responseRecorded);

        // Remove the stimuli.
        DeleteStimuli();

        // Wait for the inter-trial interval (randomly chosen between min/max).
        System.Random rand = new System.Random();
        int intervalMilliseconds = rand.Next(
            (int)interTrialIntervalMinMax[0]*1000,
            (int)interTrialIntervalMinMax[1]*1000);
        float intervalSeconds = (float)intervalMilliseconds / 1000.0f;
        yield return new WaitForSeconds(intervalSeconds);

        // Increment the trial number.
        trialNr++;

        // Unset trial running boolean.
        trialRunning = false;
    }

    // STIMULUS FUNCTIONS
    private void PrepareStimuli(int nStimuli)
    {
        // Compute the position ranges (10% buffer on all sides).
        int minX = (int)(Screen.width * 0.1f);
        int maxX = (int)(Screen.width * 0.9f);
        int minY = (int)(Screen.height * 0.1f);
        int maxY = (int)(Screen.height * 0.9f);

        // Generate new stimulus orientations. (Blessed be the tuple.)
        (stimX, stimY) = ProduceStimulusLocations(nStimuli, minX, maxX, minY,
            maxY, minDist);

        // Generate stimulus orientations.
        stimOri = ProduceStimulusOrientations(nStimuli, minOriDist);

        // Generate new stimuli.
        for (int i = 0; i < nStimuli; i++)
        {
            stimList.Add(Instantiate(stimulusPrefab));
            stimList[i].Init();
        }
        for (int i = 0; i < nStimuli; i++)
        {
            stimList[i].SetPosition(stimX[i], stimY[i]);
            stimList[i].SetOrientation(stimOri[i]);
        }

        // Make the stimuli invisible for now.
        foreach (Gabor stim in stimList)
        {
            stim.SetVisible(false);
        }
    }

    private void ShowStimuli()
    {
        // Set stimulus alphas to 1.
        foreach (Gabor stim in stimList)
        {
            stim.SetVisible(true);
        }
        // Record stimulus onset time.
        updateKey = "stimOnset";
    }

    private void HideStimuli()
    {
        // Turn on the mask.
        foreach (Gabor stim in stimList)
        {
            stim.SetMasked(true);
        }
        // Record stimulus onset time.
        updateKey = "stimOffset";
    }

    private void DeleteStimuli()
    {
        // Delete all stimuli.
        foreach (Gabor stim in stimList)
        {
            Destroy(stim.gameObject);
        }
        // Reset the stimulus list.
        stimList = new List<Gabor>();
    }

    // RESPONSE-RELATED FUNCTIONS
    private void StartResponsePhase()
    {
        // Randomly choose one stimulus to respond to.
        System.Random rand = new System.Random();
        targetStimNr = rand.Next(stimList.Count);
        // Allow the stimulus to be rotated. This will automatically unmask the
        // stimulus upon being clicked.
        stimList[targetStimNr].SetRotatable(true);
        // Show the selection ring around this stimulus.
        stimList[targetStimNr].SetSelection(true);
        // Record response window onset.
        updateKey = "respOnset";
        // Update the response phase boolean.
        responsePhase = true;
    }

    private void RecordResponse()
    {
        // Construct the stimulus orientation string.
        string stim_ori = "[" + String.Join(";", stimOri) + "]";
        string stim_x = "[" + String.Join(";", stimX) + "]";
        string stim_y = "[" + String.Join(";", stimY) + "]";
        // Construct the orientation string for all but the target.
        int[] nontargetOri = new int[trialSetSizes[trialNr]-1];
        int j = 0;
        for (int i = 0; i < stimOri.Length; i++)
        {
            if (i != targetStimNr)
            {
                nontargetOri[j] = stimOri[i];
                j++;
            }
        }
        string nontarget_ori = "[" + String.Join(";", nontargetOri) + "]";
        // Compute response time in milliseconds.
        int response_time = (int)(1000 * (trialTiming["respOffset"] -
            trialTiming["respOnset"]));
        // Tell the logger to upload the data to the database.
        dataLogger.LogMemoryTaskTrial(participantID, trialNr,
            trialSetSizes[trialNr],
            (int)(trialTiming["trialStart"] * 1000.0f),
            (int)(trialTiming["stimOnset"] * 1000.0f),
            (int)(trialTiming["stimOffset"] * 1000.0f),
            (int)(trialTiming["respOnset"] * 1000.0f),
            (int)(trialTiming["respOffset"] * 1000.0f),
            stim_x, stim_y, stim_ori, targetStimNr, nontarget_ori,
            stimOri[targetStimNr], (int)stimList[targetStimNr].GetOrientation(),
            response_time);
        // Flip the response recorded bool.
        responseRecorded = true;
    }

    // HELPER FUNCTIONS
    private (int[] x, int[] y) ProduceStimulusLocations(int nStimuli, int minX,
        int maxX, int minY, int maxY, int minDist)
    {
        // Compute the largest possible distance.
        double maxDist = Math.Sqrt(Math.Pow((double)minX - (double)maxX, 2) +
            Math.Pow((double)minY - (double)maxY, 2));

        // Create a new randomiser.
        System.Random rand = new System.Random();
        // Generate stimulus orientations.
        int[] stimX = new int[nStimuli];
        int[] stimY = new int[nStimuli];
        for (int i = 0; i < nStimuli; i++)
        {
            // Run until we find a new orientation that has enough distance
            // to all other orientations, or until we hit the iteration max.
            int j = 0;
            bool stimTooClose = true;
            while (stimTooClose & j < maxIter)
            {
                // Randomly choose a new location.
                int newX = rand.Next(minX, maxX);
                int newY = rand.Next(minY, maxY);
                // Compute the distance between new and existing locations.
                double lowestDist = maxDist;
                for (int k = 0; k < stimX.Length; k++)
                {
                    double xDist = (double)stimX[k] - (double)newX;
                    double yDist = (double)stimY[k] - (double)newY;
                    double dist = Math.Sqrt(Math.Pow(xDist,2) +
                        Math.Pow(yDist, 2));
                    if (dist < lowestDist)
                    {
                        lowestDist = dist;
                    }
                }
                // Save the new orientation if the orientation is sufficiently
                // different. Also set the Boolean to break the while loop.
                if (lowestDist > (double)minDist)
                {
                    stimX[i] = newX;
                    stimY[i] = newY;
                    stimTooClose = false;
                }
                // Increment the iteration counter.
                j++;
            }
        }
        return (stimX, stimY);
    }

    private int[] ProduceStimulusOrientations(int nStimuli, int minOriDist)
    {
        // Create a new randomiser.
        System.Random rand = new System.Random();
        // Generate stimulus orientations.
        int[] stimOri = new int[nStimuli];
        for (int i = 0; i < nStimuli; i++)
        {
            // Run until we find a new orientation that has enough distance
            // to all other orientations, or until we hit the iteration max.
            int j = 0;
            bool oriTooLow = true;
            while (oriTooLow & j < maxIter)
            {
                // Randomly choose a new orientation.
                int newOri = rand.Next(-180, 180);
                // Compute the distance between new and existing orientations.
                int lowestOriDist = 180;
                foreach (int ori in stimOri)
                {
                    if (Math.Abs(ori - newOri) < lowestOriDist)
                    {
                        lowestOriDist = Math.Abs(ori - newOri);
                    }
                }
                // Save the new orientation if the orientation is sufficiently
                // different. Also set the Boolean to break the while loop.
                if (lowestOriDist > minOriDist)
                {
                    stimOri[i] = newOri;
                    oriTooLow = false;
                }
                // Increment the iteration counter.
                j++;
            }
        }
        return stimOri;
    }
}
