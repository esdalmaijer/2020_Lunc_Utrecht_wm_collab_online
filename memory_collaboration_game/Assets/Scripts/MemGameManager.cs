using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class MemGameManager : MonoBehaviour
{
    // GUI variables.
    [SerializeField] int nTrials;
    [SerializeField] int nMultiplayerRounds;
    [SerializeField] int[] nStim = {2,4};
    [SerializeField] int minDist = 100;
    [SerializeField] int minOriDist = 10;
    [SerializeField] float stimDuration = 5.0f;
    [SerializeField] float maintenanceDuration = 3.0f;
    [SerializeField] float[] interTrialIntervalMinMax = {1.0f, 1.5f};
    [SerializeField] float[] interClaimTimeMinMax = { 3.0f, 6.5f };
    [SerializeField] float[] opponentWaitUnderHalfMinMax = { 3.0f, 8.0f };
    [SerializeField] float[] opponentWaitOverHalfMinMax = { 20.0f, 40.0f };
    [SerializeField] float[] loadingTimeMinMax = { 2.5f, 10.0f };
    [SerializeField] Gabor stimulusPrefab;
    [SerializeField] Database dataLogger;
    [SerializeField] CapacityLookup capacityComputer;
    [SerializeField] SpriteRenderer loadingSpinner;
    [SerializeField] TextMesh loadingText;
    [SerializeField] GameObject ratingConstellation;
    [SerializeField] string nextScene;
    [SerializeField] bool multiPlayer = false;

    [SerializeField] Coin feedbackPrefab;
    [SerializeField] Treasure_Box Treasure_BoxPrefab;
    [SerializeField] Treasure_Box_front Treasure_Box_frontPrefab;
    [SerializeField] TextMesh coinTotalText;
    [SerializeField] Font feedbackFont;


    // VARIABLES.
    // Participant number.
    private int participantID;
    private List<int> participantErrors = new List<int>();
    private float playerCapacity;
    // Opponent and multiplayer.
    private float opponentCapacity;
    private string opponentStrategy;
    private Color opponentColour = new Color(0.0f, 0.0f, 0.0f, 0.3f);
    private bool allStimuliClaimed;
    // Maximum number of iterations while finding random numbers with
    // pre-defined conditions.
    private int maxIter = 10000;
    // Loading.
    private bool loadingFinished = false;
    private bool loadSpinnerSpinning = false;
    private float loadSpinnerFreq = 0.3f;
    // Trial management.
    private int trialNr = 0;
    int[] trialSetSizes;
    private bool trialRandomisationPrepared = false;
    private bool trialRunning = false;
    private bool responsePhase = false;
    private bool responseRecorded = false;
    private bool endingExperiment = false;
    private int respError = -999;
    private int opponentRespError = -999;
    // Feedback 
    private Treasure_Box treasureChest;
    private Treasure_Box_front treasureChestFront;
    private List<Coin> coinList = new List<Coin>();
    private List<Vector3> coinStartPosList = new List<Vector3>();
    private Vector3 boxPosition = new Vector3(0.05f, -2.0f, -10);
    private Vector3 boxFrontPosition = new Vector3(0.0f, -3.77f, -5);
    private GameObject coinText;
    private float coinTextX = -5.45f;
    private float coinTextY = 1.0f;
    private float[] coinX = {
        -6.5f, -5.8f, -5.1f, -4.4f,
        -6.5f, -5.8f, -5.1f, -4.4f,
        -6.5f, -5.8f, -5.1f, -4.4f,
        };
    private float[] coinY = {
        4.0f, 4.0f, 4.0f, 4.0f,
        3.0f, 3.0f, 3.0f, 3.0f,
        2.0f, 2.0f, 2.0f, 2.0f,
        };
    private float coinZ = -7.5f;
    private bool coinsMoving = false;
    private float coinMoveStartTime;
    private float coinMoveDuration = 1.0f;
    private float coinMoveLag = 0.5f;
    private int nCoins = 0;
    private int maxCoins = 12;
    private int minCoins = 1;
    private double rewardSD = 17.0;
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
    private float mostRecentOpponentClaim;
    // Ratings.
    private bool oneToggleSelected = false;
    private bool ratingConfirmed = false;
    private bool ratingsCompleted = false;
    private TextMesh ratingQuestion;
    private Toggle[] ratingToggles;
    private string[] ratingQuestionTexts = {
        "How good was the collaboration between you and your crewmate?", 
        "How likable did you find your crewmate?",
        "How fair did you think your crewmate was?",
        "How good did you think your crewmate's memory was?"};

    // Start is called before the first frame update
    void Start()
    {
        // Create a new randomiser. (We'll need it later.)
        System.Random rand = new System.Random();

        // Get the participant number.
        participantID = PlayerPrefs.GetInt("PPNR");
        //Debug.Log("Player number: " + participantID.ToString());

        // Get the player's and opponent's stats.
        if (multiPlayer)
        {
            // Get the rating constellation's most important components:
            // The rating question, and the tickboxes.
            TextMesh[] ratingTextMeshes =
                ratingConstellation.GetComponentsInChildren<TextMesh>();
            for (int i = 0; i < ratingTextMeshes.Length; i++)
            {
                if (ratingTextMeshes[i].name == "Likert Question")
                {
                    ratingQuestion = ratingTextMeshes[i];
                }
            }
            // Get the rating toggles.
            ratingToggles = ratingConstellation.GetComponentsInChildren<Toggle>();
            // Deactivate ALL the rating things for now.
            ratingConstellation.SetActive(false);

            // Get the current game round.
            int round = PlayerPrefs.GetInt("gameRoundNr");
            //Debug.Log("Round: " + round.ToString());

            // Get player visual short-term memory capacity.
            playerCapacity = PlayerPrefs.GetFloat("playerCapacity");
            //Debug.Log("Player capacity: " + playerCapacity.ToString());

            // Get the opponent's short-term memory capacity.
            string capacityOrder = PlayerPrefs.GetString("gameCapacityOrder");
            string[] capacityOrderArray = capacityOrder.Split(';');
            float opponentCapacityMultiplier = Convert.ToSingle(capacityOrderArray[round]);
            opponentCapacity = playerCapacity * opponentCapacityMultiplier;
            //Debug.Log("Round Capacities: " + capacityOrder);
            //Debug.Log("Opponent Capacity: " + opponentCapacity.ToString());

            // Get the opponent's strategy.
            string strategyOrder = PlayerPrefs.GetString("gameStrategyOrder");
            string[] strategyOrderArray = strategyOrder.Split(';');
            opponentStrategy = strategyOrderArray[round];
            //Debug.Log("Round Strategies: " + strategyOrder);
            //Debug.Log("Opponent strategy: " + opponentStrategy.ToString());

            // Randomly choose an opponent number.
            int opponentID = rand.Next(participantID + 1 + round * 7,
                participantID + (round+1) * 7);
            // Randomly choose loading duration.
            float loadingDuration = loadingTimeMinMax[0] +
                (float)rand.NextDouble() *
                (loadingTimeMinMax[1]-loadingTimeMinMax[0]);
            // Start loading animation.
            StartCoroutine(Loading(opponentID, loadingDuration));
        }

        // In non-multiplayer, things are a bit simpler.
        else
        {
            // In non-multiplayer, we don't to the loading.
            LoadingStop();
        }

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
        // LOADING
        // Spin the loader.
        if (loadSpinnerSpinning)
        {
            //// Apply the rotation.
            Vector3 rotation = new Vector3(0.0f, 0.0f,
                Time.deltaTime * 360.0f * loadSpinnerFreq);
            loadingSpinner.gameObject.transform.Rotate(-1 * rotation);
        }

        // TRIAL PROGRESS
        // Run the next trial if one is not currently running.
        if (trialRunning == false & trialRandomisationPrepared == true &
            loadingFinished == true)
        {
            // Check if the current trial exceeds the required number.
            if (trialNr >= nTrials)
            {
                // End the experiment (but only once).
                if (!endingExperiment)
                {
                    StartCoroutine(EndExperiment());
                }
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

        // FEEDBACK
        if (coinsMoving)
        {
            // Check the current time-in-movement.
            float timeInMovement = Time.time - coinMoveStartTime;
            // Continue if we're past the lag phase.
            if (timeInMovement > coinMoveLag)
            {
                // Compute the current position in the movement towards
                // the treasure chest.
                float movementProp = (timeInMovement - coinMoveLag) /
                    coinMoveDuration;
                // Check if the movement is at its end.
                if (movementProp >= 1.0f)
                {
                    coinsMoving = false;
                }
                // Loop through all coins to move each.
                for (int i = 0; i < coinList.Count; i++)
                {
                    // Interpolate the new position from the current
                    // position and the time since the movement started.
                    Vector3 endPos = boxFrontPosition;
                    endPos.z = coinList[i].transform.position.z;
                    Vector3 newPos = Vector3.Lerp(coinStartPosList[i],
                        endPos, movementProp);
                    // Set the new position.
                    coinList[i].transform.position = newPos;
                }
            }
        }
    }

    // TASK START
    private IEnumerator Loading(int opponentNumber, float durationSeconds)
    {
        // Start the spinning.
        loadSpinnerSpinning = true;

        // Wait for the spinning time.
        yield return new WaitForSeconds(durationSeconds);

        // Stop the spinning.
        loadSpinnerSpinning = false;

        // Wait for a wee bit longer.
        yield return new WaitForSeconds(0.3f);

        // Choose the right text.
        string compareText = "";
        if (playerCapacity == opponentCapacity)
        {
            compareText = "the same as yours";
        }
        else if (playerCapacity < opponentCapacity)
        {
            compareText = "better than yours";
        }
        else if (playerCapacity > opponentCapacity)
        {
            compareText = "worse than yours";
        }
        // Change the text.
        loadingText.color = new Color(0.0f, 1.0f, 0.0f);
        loadingText.text = "Player " + opponentNumber +
            " is your new crewmate!\n\n\n" +
            "Their memory is " + compareText + ".";

        // Wait for the a bit more time, so people can read.
        yield return new WaitForSeconds(5.0f);

        // Stop the loading.
        LoadingStop();
    }

    private void LoadingStop()
    {
        // Destroy the loading spinner and text.
        Destroy(loadingSpinner.gameObject);
        Destroy(loadingText.gameObject);
        // Toggle the loading finished bool.
        loadingFinished = true;
    }

    // TASK END
    private IEnumerator EndExperiment()
    {
        // Set the ending experiment bool.
        endingExperiment = true;

        // Compute and store the player's average and standard deviation.
        // This is only necessary if this is the individual test.
        if (!multiPlayer)
        {
            // Go through all set sizes, and compute the standard deviation
            // of errors for each.
            double estimatedCapacities = 0.0;
            foreach (int setSize in nStim)
            {
                // Select the errors from the current set size.
                List<int> errorsForThisSetSize = new List<int>();
                for (int i = 0; i < trialSetSizes.Length; i++)
                {
                    if (trialSetSizes[i] == setSize)
                    {
                        errorsForThisSetSize.Add(participantErrors[i]);
                    }
                }
                // Compute the standard deviation or errors at this set size.
                // Convert to radians for the DKL conversion.
                double std = ComputeStandardDeviation(errorsForThisSetSize) *
                    Mathf.Deg2Rad;
                // Look up the DKL associated with this error SD.
                double dkl = capacityComputer.MatchCapacity(std);
                //Debug.Log("SD=" + std.ToString() + ", DKL=" + dkl.ToString());
                // Multiply the DKL by the number of stimuli to compute the
                // full capacity.
                estimatedCapacities += (dkl * setSize);
            }
            // Compute the average estimates capacity.
            estimatedCapacities /= nStim.Length;
            //Debug.Log("capacity=" + estimatedCapacities.ToString());
            // Save the player's capacity.
            PlayerPrefs.SetFloat("playerCapacity", (float)estimatedCapacities);
            PlayerPrefs.Save();

            // Log the capacity to the database, and then load the next scene.
            //Debug.Log("Adding capacity to the database.");
            bool uploadCompleted = false;
            StartCoroutine(dataLogger.PostParticipantCapacity(participantID,
                PlayerPrefs.GetFloat("playerCapacity"),
                success => { uploadCompleted = true; }));

            // Wait until the upload is completed.
            yield return new WaitUntil(() => uploadCompleted);

            // Load the next scene.
            LoadNextScene(nextScene);
        }

        // The following is for the multiplayer mode.
        else
        {
            // Start rating.
            StartCoroutine(RunRatings());
            // Wait until the ratings are completed.
            yield return new WaitUntil(() => ratingsCompleted);

            // Increment the game round.
            int round = PlayerPrefs.GetInt("gameRoundNr");
            PlayerPrefs.SetInt("gameRoundNr", round + 1);
            PlayerPrefs.Save();

            // Choose the current scene to reload if there are more rounds to play.
            string nextSceneName;
            if (PlayerPrefs.GetInt("gameRoundNr") < nMultiplayerRounds)
            {
                nextSceneName =
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            }
            // Choose the next scene if all rounds are done.
            else
            {
                nextSceneName = nextScene;
            }
            //Debug.Log("Round=" + PlayerPrefs.GetInt("gameRoundNr").ToString() +
            //    ", n_rounds=" + nMultiplayerRounds.ToString() +
            //    ", next_scene=" + nextSceneName);

            // Load the selected scene.
            LoadNextScene(nextSceneName);
        }
    }

    private void LoadNextScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
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
        // In multiplayer, wait until all stimuli are claimed.
        if (multiPlayer)
        {
            // Reset the claim Boolean.
            allStimuliClaimed = false;
            // Allow the stimuli to be claimed.
            MakeStimuliClaimable();
            // Start the ClaiMonitor.
            StartCoroutine(ClaiMonitor());
            // Wait until all stimuli are claimed.
            yield return new WaitUntil(() => allStimuliClaimed);
            // Wait for a second so the last stimulus can be memorised.
            yield return new WaitForSeconds(1.0f);
        }
        // In a non-multiplayer game, wait for a the stimulus duration.
        else
        {
            yield return new WaitForSeconds(stimDuration);
        }

        // Hide the stimuli.
        HideStimuli();
        // Wait for the delay interval.
        yield return new WaitForSeconds(maintenanceDuration);

        // Activate the response.
        StartResponsePhase();

        // Wait for response.
        respError = -999;
        responseRecorded = false;
        yield return new WaitUntil(() => responseRecorded);

        // Remove the stimuli.
        DeleteStimuli();

        // Show feedback for the player.
        ShowFeedback(false);
        // Wait until the coins have stopped moving.
        yield return new WaitUntil(() => coinsMoving == false);

        // Show feedback for the opponent.
        if (multiPlayer)
        {
            // Delete previous feedback.
            DeleteFeedback();
            // Show opponent feedback.
            ShowFeedback(true);
            // Wait until the coins have stopped moving.
            yield return new WaitUntil(() => coinsMoving == false);
        }

        // Wait for the inter-trial interval (randomly chosen between min/max).
        System.Random rand = new System.Random();
        int intervalMilliseconds = rand.Next(
            (int)interTrialIntervalMinMax[0]*1000,
            (int)interTrialIntervalMinMax[1]*1000);
        float intervalSeconds = (float)intervalMilliseconds / 1000.0f;
        yield return new WaitForSeconds(intervalSeconds);

        // Delete Feedback from previous trial. 
        DeleteFeedback();

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
        int maxY = (int)(Screen.height * 0.8f);

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

    private void MakeStimuliClaimable()
    {
        // Set stimulus claimability to true.
        foreach (Gabor stim in stimList)
        {
            stim.SetClaimable(true);
        }
    }

    private IEnumerator ClaiMonitor()
    {
        // Set update frequency.
        float loopFreq = 5.0f;
        // Create a new randomiser.
        System.Random rand = new System.Random();

        // Reset most recent claim time.
        mostRecentOpponentClaim = Time.time - expStart;

        // Run until all stimuli are claimed.
        while (!allStimuliClaimed)
        {
            // Check how many stimuli are claimed.
            int claimCount = 0;
            int playerCount = 0;
            int opponentCount = 0;
            foreach (Gabor stim in stimList)
            {
                if (stim.GetClaimed())
                {
                    claimCount ++;
                    if (stim.GetOwner() == 0)
                    {
                        playerCount++;
                    }
                    else
                    {
                        opponentCount++;
                    }
                }
            }

            // Check if all stimuli are claimed.
            allStimuliClaimed = claimCount >= trialSetSizes[trialNr];

            // If the number of claimed stimuli has increased, a new stimulus
            // was claimed by the participant.
            if (playerCount > opponentCount)
            {
                // Wait for a variable delay.
                int intervalMilliseconds = rand.Next(
                    (int)interClaimTimeMinMax[0] * 1000,
                    (int)interClaimTimeMinMax[1] * 1000);
                float intervalSeconds = (float)intervalMilliseconds / 1000.0f;
                yield return new WaitForSeconds(intervalSeconds);
                // Claim a new stimulus for the computer.
                OpponentClaim();
                // Update last opponent claim time.
                mostRecentOpponentClaim = Time.time - expStart;
            }
            // Also claim after a number of seconds have passed without
            // the player claiming anything. This happens more quickly
            // if fewer than half of the stimuli have been claimed by the
            // player so far. The final stimulus will never be claimed if the
            // player has not yet claimed an item.
            if (opponentCount < trialSetSizes[trialNr] - 1)
            {
                // The probability of choosing the next stimulus should rise
                // between min and max wait seconds. The actual probability curve
                // depends on the frequency of looping.
                float passedTimeSinceLastClaim = (Time.time - expStart) -
                    mostRecentOpponentClaim;
                float p;
                // Choose the correct set of waiting min/max times.
                float[] waitMinMax;
                if (opponentCount < trialSetSizes[trialNr] / 2)
                {
                    waitMinMax = opponentWaitUnderHalfMinMax;
                }
                else
                {
                    waitMinMax = opponentWaitOverHalfMinMax;
                }
                // Compute the probability of stopping the wait at the
                // current time since last claim.
                if (passedTimeSinceLastClaim <= waitMinMax[0])
                {
                    p = 0.0f;
                }
                else if (passedTimeSinceLastClaim > waitMinMax[0] &
                    passedTimeSinceLastClaim < waitMinMax[1])
                {
                    p = (passedTimeSinceLastClaim - waitMinMax[0])
                        / (waitMinMax[1] - waitMinMax[0]);
                }
                else
                {
                    p = 1.0f;
                }
                // Correct for cumulative chance. Note that this is a very poor
                // way of doing it; it's just intended to spread the range a
                // bit better. I'm a sleep-deprived new parent; don't have the
                // mental capacity to implement the proper way.
                p /= loopFreq;
                // Randomly determine whether a new item is claimed.
                if (rand.NextDouble() < p)
                {
                    // Claim a new stimulus for the computer.
                    OpponentClaim();
                    // Update last opponent claim time.
                    mostRecentOpponentClaim = Time.time - expStart;
                }
            }
        // Wait for a wee bit.
        yield return new WaitForSeconds(1.0f / loopFreq);
        }
    }

    private void OpponentClaim()
    {
        // Check how many stimuli are claimed, and keep lists of the
        // available stimuli, but also the (x,y) positions of the stimuli
        // that are claimed by the computer.
        List<int> availableIndices = new List<int>();
        List<int> xPos = new List<int>();
        List<int> yPos = new List<int>();
        for (int i = 0; i < stimList.Count; i++)
        {
            Gabor stim = stimList[i];

            // Check if the stimulus is available.
            if (stim.GetClaimed())
            {
                // Check if the stimulus is owned by the player (==0) or
                // the computer opponent (==1).
                if (stim.GetOwner() == 1)
                {
                    xPos.Add(stimX[i]);
                    yPos.Add(stimY[i]);
                }
            }
            else
            {
                availableIndices.Add(i);
            }
        }

        // If no available stimuli are available, stop here.
        if (availableIndices.Count == 0)
        {
            return;
        }

        // Choose one of the available stimuli.
        int chosen;
        // If the opponent's strategy is to get the closest stimulus, find
        // and chose the closest available stimulus. Proximity is computed
        // as the distance between an available stimulus, and the average
        // position of all stimuli that were already claimed by the computer.
        if ((opponentStrategy == "closest") & (xPos.Count > 0))
        {
            // Compute the average X and Y of currently chosen stimuli.
            int sumX = 0;
            int sumY = 0;
            for (int i = 0; i < xPos.Count; i++)
            {
                sumX += xPos[i];
                sumY += yPos[i];
            }
            int avgX = sumX / xPos.Count;
            int avgY = sumY / yPos.Count;

            // Find the stimulus closest to the available stimuli.
            int closestIndex = availableIndices[0];
            double lowestDistance = System.Math.Sqrt(
                System.Math.Pow((double)stimX[closestIndex] - (double)avgX, 2) +
                System.Math.Pow((double)stimY[closestIndex] - (double)avgY, 2));
            foreach (int i in availableIndices)
            {
                double xDist = (double)stimX[i] - (double)avgX;
                double yDist = (double)stimY[i] - (double)avgY;
                double dist = Math.Sqrt(Math.Pow(xDist, 2) +
                    Math.Pow(yDist, 2));
                if (dist < lowestDistance)
                {
                    lowestDistance = dist;
                    closestIndex = i;
                }
            }
            // The closest stimulus is the Chosen One.
            chosen = closestIndex;
        }
        // In case of random strategy, just pick a random unclaimed stimulus.
        else
        {
            System.Random rand = new System.Random();
            chosen = availableIndices[rand.Next(0, availableIndices.Count)];
        }

        // Claim the available stimulus.
        stimList[chosen].Claim(1, opponentColour);
    }

    private void HideStimuli()
    {
        // Turn on the mask, and set all stimulus orientations to 0, so that
        // the mask orientation does not give any information away.
        foreach (Gabor stim in stimList)
        {
            stim.SetMasked(true);
            stim.SetOrientation(0.0f);
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

    private void ShowFeedback(bool forOpponent)
    {
        // Show the treasure chest.
        treasureChest = Instantiate(Treasure_BoxPrefab, boxPosition,
            transform.rotation);
        treasureChestFront = Instantiate(Treasure_Box_frontPrefab,
            boxFrontPosition, transform.rotation);

        // Compute the ratio so that 0 error matches with the maximum
        // number of coins.
        double ratio = (double)(maxCoins-minCoins) /
            NormalPDF(0.0, 0.0, rewardSD);
        // Compute the number of coins that should be awarded here.
        float relevantRespError;
        if (forOpponent)
        {
            relevantRespError = opponentRespError;
        }
        else
        {
            relevantRespError = respError;
        }
        int nCoin = minCoins + (int)Math.Round(ratio *
            NormalPDF(Math.Abs(relevantRespError), 0.0, rewardSD));

        // Initialise the win text.
        coinText = new GameObject();
        coinText.name = "Coin Text";
        Vector3 coinTextPos = new Vector3(coinTextX, coinTextY, coinZ);
        if (forOpponent)
        {
            coinTextPos.x *= -1.0f;
        }
        coinText.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        coinText.transform.position = coinTextPos;
        TextMesh coinTextMesh = coinText.AddComponent<TextMesh>();
        //MeshRenderer coinTextMeshRenderer = coinText.AddComponent<MeshRenderer>();
        // For some reason, setting the font does not work.
        //coinTextMesh.font = feedbackFont;
        coinTextMesh.fontStyle = FontStyle.Normal;
        coinTextMesh.fontSize = 50;
        coinTextMesh.color = new Color(1.0f, 1.0f, 1.0f);
        coinTextMesh.anchor = TextAnchor.MiddleCenter;
        coinTextMesh.alignment = TextAlignment.Center;
        if (forOpponent)
        {
            coinTextMesh.text = "Your crewmate contributed \n" + nCoin.ToString() + " coins!";
        }
        else
        {
            if (multiPlayer)
            {
                coinTextMesh.text = "You contributed \n" + nCoin.ToString() + " coins!";
            }
            else
            {
                coinTextMesh.text = "You earned \n" + nCoin.ToString() + " coins!";
            }
        }

        // Initialise the coins.
        // original position of the coin prefab
        // Create all the coins.
        for (int i = 0; i < nCoin; i++)
        {
            Vector3 coinPosition;
            if (forOpponent)
            {
                coinPosition = new Vector3(-1.0f*coinX[i], coinY[i], coinZ);
            }
            else
            {
                coinPosition = new Vector3(coinX[i], coinY[i], coinZ);
            }
            coinList.Add(Instantiate(feedbackPrefab, coinPosition,
                transform.rotation));
            coinStartPosList.Add(coinPosition);
        }
        // Note the start time.
        coinMoveStartTime = Time.time;

        // Allow the coins to move to the chest.
        coinsMoving = true;

        // Add coins to total in PlayerPrefs.
        nCoins += nCoin;

        // Update the visible coin total.
        coinTotalText.text = nCoins.ToString();
    }

    private void DeleteFeedback()
    {
        // Stop the movement animation.
        coinsMoving = false;

        // Remove the coins.
        foreach (Coin coinInstance in coinList)
        {
            Destroy(coinInstance.gameObject);
        }
        // Clear our the list of coins.
        coinList = new List<Coin>();
        coinStartPosList = new List<Vector3>();

        // Remove the text.
        Destroy(coinText);

        // Remove the chest.
        Destroy(treasureChestFront.gameObject);
        Destroy(treasureChest.gameObject);
    }

    // RESPONSE-RELATED FUNCTIONS
    private void StartResponsePhase()
    {
        // Create a new randomiser.
        System.Random rand = new System.Random();

        // In multiplayer, only some stimuli are claimed by the participant.
        // A random stimulus should be chosen from these.
        if (multiPlayer)
        {
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < stimList.Count; i++)
            {
                if (stimList[i].GetOwner() == 0)
                {
                    availableIndices.Add(i);
                }
            }
            // In the impossible situation that no stimuli were claimed,
            // simply set 0.
            if (availableIndices.Count == 0)
            {
                targetStimNr = 0;
            }
            else
            {
                // Choose a random number from the claimed stimuli.
                targetStimNr = availableIndices[rand.Next(availableIndices.Count)];
            }
        }
        // In single-player, all stimuli are claimed by the participant.
        else
        {
            // Randomly choose one stimulus to respond to.
            targetStimNr = rand.Next(stimList.Count);
        }
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

    private int OpponentResponse()
    {
        // Count the number of stimuli claimed by the opponent.
        int opponentClaimed = 0;
        for (int i = 0; i < stimList.Count; i++)
        {
            if (stimList[i].GetOwner() != 0)
            {
                opponentClaimed += 1;
            }
        }

        // If the opponent claimed none, return the maximum possible value.
        if (opponentClaimed == 0)
        {
            return 90;
        }

        // Divide the opponent's short-term memory capacity over the number
        // of items.
        double dklPerStim = (double)opponentCapacity / (double)opponentClaimed;

        // Look up the associated standard deviation.
        double sd = capacityComputer.MatchSD(dklPerStim);

        // Generate a random value from a normal distribution with mean 0
        // and standard deviation as per computed above.
        double respRad = RandN(0.0, sd);

        // Convert to degrees.
        int resp = (int)Math.Round(respRad * Mathf.Rad2Deg);

        //Debug.Log("DKL=" + dklPerStim.ToString() + ", SD=" + sd.ToString() +
        //    ", resp_rad=" + respRad.ToString() + ", resp_deg" + resp.ToString());

        return resp;
    }

    private void RecordResponse()
    {
        // In multiplayer, a few more variables have to be computed.
        string playerClaimedString = "";
        string opponentClaimedString = "";
        if (multiPlayer)
        {
            // Simulate a response from the opponent.
            opponentRespError = OpponentResponse();

            // Check who claimed which stimuli.
            List<int> playerClaimed = new List<int>();
            List<int> opponentClaimed = new List<int>();
            for (int i = 0; i < stimList.Count; i++)
            {
                if (stimList[i].GetOwner() == 0)
                {
                    playerClaimed.Add(i);
                }
                else
                {
                    opponentClaimed.Add(i);
                }
            }
            playerClaimedString = "[" + String.Join(";", playerClaimed) + "]";
            opponentClaimedString = "[" + String.Join(";", opponentClaimed) + "]";
        }

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
        // Get the orientation of the user-rotated stimulus.
        int responseOri = (int)stimList[targetStimNr].GetOrientation();
        // Compute circular error. We add 180 here to transfor the space from
        // range (-180, 180) to (0, 360). Bit easier.
        respError = ComputeCircularDifference(stimOri[targetStimNr]+180,
            responseOri+180, 180);
        // Add response error to the internal list.
        participantErrors.Add(respError);
        // Compute response time in milliseconds.
        int response_time = (int)(1000 * (trialTiming["respOffset"] -
            trialTiming["respOnset"]));
        // Tell the logger to upload the data to the database.
        if (multiPlayer)
        {
            dataLogger.LogMemoryGameTrial(participantID,
                PlayerPrefs.GetInt("gameRoundNr"), opponentCapacity,
                opponentStrategy,  trialNr, trialSetSizes[trialNr],
                (int)(trialTiming["trialStart"] * 1000.0f),
                (int)(trialTiming["stimOnset"] * 1000.0f),
                (int)(trialTiming["stimOffset"] * 1000.0f),
                (int)(trialTiming["respOnset"] * 1000.0f),
                (int)(trialTiming["respOffset"] * 1000.0f),
                stim_x, stim_y, stim_ori, targetStimNr, nontarget_ori,
                stimOri[targetStimNr], responseOri, respError, response_time,
                playerClaimedString, opponentClaimedString, opponentRespError);
        }
        else
        {
            dataLogger.LogMemoryTaskTrial(participantID, trialNr,
                trialSetSizes[trialNr],
                (int)(trialTiming["trialStart"] * 1000.0f),
                (int)(trialTiming["stimOnset"] * 1000.0f),
                (int)(trialTiming["stimOffset"] * 1000.0f),
                (int)(trialTiming["respOnset"] * 1000.0f),
                (int)(trialTiming["respOffset"] * 1000.0f),
                stim_x, stim_y, stim_ori, targetStimNr, nontarget_ori,
                stimOri[targetStimNr], responseOri, respError, response_time);
        }
        // Flip the response recorded bool.
        responseRecorded = true;
    }

    // RATING FUNCTIONS
    public void RatingConfirmation()
    {
        // Check if at least one of the Toggles is on.
        int nSelected = 0;
        foreach (Toggle donToggleone in ratingToggles)
        {
            if (donToggleone.isOn)
            {
                nSelected++;
            }
        }
        // Set the confirmation to True or False.
        if (nSelected > 0)
        {
            ratingConfirmed = true;
        }
        else
        {
            ratingConfirmed = false;
        }
    }

    private IEnumerator RunRatings()
    {
        // Activate the rating constellation.
        ratingConstellation.SetActive(true);

        // Run through all questions.
        for (int i = 0; i < ratingQuestionTexts.Length; i++)
        {
            // Set the new question.
            ratingQuestion.text = ratingQuestionTexts[i];
            // Reset the Toggles.
            foreach (Toggle togTogToggle in ratingToggles)
            {
                togTogToggle.isOn = false;
            }

            // Wait for at least one toggle to be clicked.
            StartCoroutine(WaitForToggleSelection());
            yield return new WaitUntil(() => oneToggleSelected);

            // Set the confirmation to False.
            ratingConfirmed = false;

            // Wait for confirmation button to be pressed.
            yield return new WaitUntil(() => ratingConfirmed);

            // Check which toggle was activated.
            string selectedRating = "0";
            foreach (Toggle togTogToggle in ratingToggles)
            {
                if (togTogToggle.isOn)
                {
                    selectedRating = togTogToggle.GetComponentInChildren<Text>().text;
                }
            }
            // Submit rating to database.
            dataLogger.LogRating(participantID,
                PlayerPrefs.GetInt("gameRoundNr"), i+1,
                Int32.Parse(selectedRating));
        }

        // Toggle the ratingsCompleted bool.
        ratingsCompleted = true;
    }

    private IEnumerator WaitForToggleSelection()
    {
        oneToggleSelected = false;

        while (!oneToggleSelected)
        {
            // Check if at least one Toggle is selected.
            int nSelected = 0;
            foreach (Toggle toggleMcToggleFace in ratingToggles)
            {
                if (toggleMcToggleFace.isOn)
                {
                    nSelected++;
                }
            }
            if (nSelected > 0)
            {
                oneToggleSelected = true;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }    

    // HELPER FUNCTIONS
    private int ComputeCircularDifference(int ori1, int ori2, int zero)
    {
        float error = (float)Math.Abs(ori1 - ori2) % zero;

        if (error > zero / 2)
            error = zero - error;

        return (int)error;
    }

    private double ComputeMean(List<int> values)
    {
        // Go through all values, add each to the total.
        int tot = 0;
        foreach (int value in values)
        {
            tot += value;
        }
        // Divide total by number of elements to get average.
        double avg = (double)tot / (double)values.Count;

        return avg;
    }

    private double ComputeStandardDeviation(List<int> values)
    {
        // Compute the average.
        double avg = ComputeMean(values);

        // Go through all values, add the squared difference to the total.
        double tot = 0;
        foreach (int value in values)
        {
            tot += Math.Pow(value - avg, 2);
        }
        // Divide sum of squared differences by number of elements, and then
        // take the square root to obtain SD.
        double std = Math.Sqrt(tot / (double)values.Count);

        return std;
    }

    private (int[] x, int[] y) ProduceStimulusLocations(int nStimuli, int minX,
        int maxX, int minY, int maxY, int minDist)
    {
        //Debug.Log("Finding " + nStimuli.ToString() + " locations with " +
        //    "Xmin=" + minX.ToString() +
        //    "Xmax=" + maxX.ToString() +
        //    "Ymin=" + minY.ToString() +
        //    "Ymax=" + maxY.ToString()
        //    );

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
            while (stimTooClose & (j < maxIter))
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
                    double dist = Math.Sqrt(Math.Pow(xDist, 2) +
                        Math.Pow(yDist, 2));
                    if (dist < lowestDist)
                    {
                        lowestDist = dist;
                    }
                }
                // Save the new orientation if the orientation is sufficiently
                // different. Also set the Boolean to break the while loop.
                // Also stop if this is the last iteration.
                if ((lowestDist > (double)minDist) | (j == maxIter-1))
                {
                    stimX[i] = newX;
                    stimY[i] = newY;
                    stimTooClose = false;
                    Debug.Log("Location " + i.ToString() + " found in " + j.ToString() + " attempts");
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

    private double RandN(double m, double std)
    {
        // Create a new randomiser.
        System.Random rand = new System.Random();
        // Generate random doubles from a uniform distribution.
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        // Convert to random value from a standard normal distribution
        // (M=0, SD=1)
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                     Math.Sin(2.0 * Math.PI * u2);
        // Convert to random value from a normal distribution with specified
        // mean and standard deviation.
        double randNormal = m + std * randStdNormal;

        return randNormal;
    }

    private double NormalCDF(double x)
    {
        // Implementation from:
        // https://www.johndcook.com/blog/csharp_phi/

        // constants
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        // Save the sign of x
        int sign = 1;
        if (x < 0)
            sign = -1;
        x = Math.Abs(x) / Math.Sqrt(2.0);

        // A&S formula 7.1.26
        double t = 1.0 / (1.0 + p * x);
        double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) *
            t * Math.Exp(-x * x);

        return 0.5 * (1.0 + sign * y);
    }

    private double NormalPDF(double x, double m, double sd)
    {
        double y = (1.0 / (sd * Math.Sqrt(2 * Math.PI))) *
            Math.Exp(-0.5 * Math.Pow(((x-m)/sd),2));

        return y;
    }
}
