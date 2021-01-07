using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExplanationController : MonoBehaviour
{
    // GUI editable variables.
    [SerializeField] float minPageTime = 5.0f;
    [SerializeField] float stimDuration = 5.0f;
    [SerializeField] float maintenanceDuration = 1.5f;
    [SerializeField] int respErrorColourCutoffPercentage = 75;
    [SerializeField] TextMesh explanationText;
    [SerializeField] TextMesh prevButtonText;
    [SerializeField] TextMesh nextButtonText;
    [SerializeField] Gabor stimulusPrefab;
    [SerializeField] string nextScene;

    // Variables.
    private int currentPage = 0;
    private float lastPageFlip;
    private List<Gabor> stimList = new List<Gabor>();
    private float[] stimOris = {42, 69};
    private float[] stimX = {0.4f, 0.6f};
    private float[] stimY = {0.3f, 0.4f};
    private bool flippingPhase = false;
    private bool selectionPhase = false;
    private bool responsePhase = false;
    private Color playerColour = new Color(1.0f, 1.0f, 1.0f, 0.3f);
    private Color opponentColour = new Color(0.0f, 0.0f, 0.0f, 0.3f);

    // Explanation texts.
    private string[] pages = {
        "Well done, matey! Now let's find\n" +
        "you a crew mate to play with.",

        "In the next part, your job is still\n" +
        "to remember as well as you can.\n\n" +
        "But now, you will have a crew mate who\n" +
        "can help you. You earn coins together!",

        "Each of you can claim items. Once\n" +
        "you claim an item, it is yours to\n" +
        "remember.",

        "To claim an item, simply click it\n" +
        "with your mouse. Try it below:",

        "Items claimed by you appear in white,\n" +
        "and items by your crew mate in black.\n" +
        "You will only be asked about your\n" +
        "own items.",

        "Now click to claim and remember your item:",

        "And again:",

        "One more time:",

        "The better your crew memorised the\n" +
        "items, the more coins you both win!" +
        "\n\n\nHit the Next button to start."};

    // Start is called before the first frame update
    void Start()
    {
        // Start with the first page.
        LoadPage(currentPage);
    }

    // Update is called once per frame
    void Update()
    {
        // Compute how much time has passed since last page flip.
        float timePassed = Time.time - lastPageFlip;

        // Update the text on the Next button.
        if (selectionPhase)
        {
            nextButtonText.text = "Click items";
            if (nextButtonText.color.r != 0.7f)
            {
                nextButtonText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
        else if (responsePhase)
        {
            nextButtonText.text = "Press Space";
            if (nextButtonText.color.r != 0.7f)
            {
                nextButtonText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
        else if (timePassed < minPageTime)
        {
            nextButtonText.text = "Next (" +
                ((int)System.Math.Ceiling(minPageTime-timePassed)).ToString() +
                ")";
            if (nextButtonText.color.r != 0.7f)
            {
                nextButtonText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
        else
        {
            nextButtonText.text = "Next";
            if (nextButtonText.color.r != 1.0f)
            {
                nextButtonText.color = new Color(1.0f, 1.0f, 1.0f);
            }
        }

        // Check if the selection phase is still ongoing.
        if (selectionPhase)
        {
            int nClaimed = 0;
            foreach (Gabor stim in stimList)
            {
                if (stim.GetClaimed())
                {
                    nClaimed++;
                }
            }
            if (nClaimed >= stimList.Count)
            {
                selectionPhase = false;
            }
        }

        // Check if the Space bar is pressed in the response phase.
        if (responsePhase)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                // Only acknowledge the keypress if at least one
                // stimulus is no longer masked.
                foreach (Gabor stim in stimList)
                {
                    if (!stim.GetMasked())
                    {
                        responsePhase = false;
                    }
                }
            }
        }
    }

    // Function to advance to the next part of the instructions.
    // Will be coupled to the Next button.
    public void NextPage()
    {
        // Only advance if enough time has passed.
        if (Time.time - lastPageFlip < minPageTime)
        {
            return;
        }

        // Clean the current page.
        ClearPage();

        // Update the page number.
        currentPage += 1;

        // If there is a next page, load it.
        if (currentPage < pages.Length)
        {
            LoadPage(currentPage);
        }
        else
        {
            StartNextScene();
        }
    }

    // Function to return to the previous part of the instructions.
    // Will be coupled to the Previous button.
    public void PreviousPage()
    {
        // Clean the current page.
        ClearPage();

        // Update the page number.
        currentPage -= 1;
        // Don't go below 0.
        if (currentPage < 0)
        {
            currentPage = 0;
        }

        // Load the previous page.
        LoadPage(currentPage);
    }

    // Functions to add and remove elements.
    private void LoadPage(int pageNumber)
    {
        // Update the button text.
        if (pageNumber == 0)
        {
            prevButtonText.color = new Color(0.7f, 0.7f, 0.7f);
        }
        else
        {
            prevButtonText.color = new Color(1.0f, 1.0f, 1.0f);
        }

        // Update the text.
        explanationText.text = pages[pageNumber];

        // Initialise the Gabors.
        for (int i = 0; i < stimOris.Length; i++)
        {
            Gabor stim = Instantiate(stimulusPrefab);
            stim.Init();
            stim.SetOrientation(stimOris[i]);
            stim.SetPosition(
                (int)System.Math.Round(stimX[i] * Screen.width),
                (int)System.Math.Round(stimY[i] * Screen.height));
            stim.SetClaimable(false);
            stim.SetMasked(false);
            stim.SetRotatable(false);
            stim.SetSelection(false);
            stim.SetVisible(false);
            stimList.Add(stim);
        }

        // Add the page-specific objects.
        switch (pageNumber)
        {
            // Nothing on the first page.
            case 0:
                break;
            // Nothing on the second page.
            case 1:
                break;
            // Nothing on the third page.
            case 2:
                break;
            // All claimable Gabors.
            case 3:
                for (int i = 0; i < stimList.Count; i++)
                {
                    stimList[i].SetMasked(false);
                    stimList[i].SetRotatable(false);
                    stimList[i].SetSelection(false);
                    stimList[i].SetClaimable(true);
                    stimList[i].SetVisible(true);
                }
                selectionPhase = true;
                break;
            // Two player-claimed Gabors, and two opponent.
            case 4:
                for (int i = 0; i < stimList.Count; i++)
                {
                    stimList[i].SetMasked(false);
                    stimList[i].SetRotatable(false);
                    stimList[i].SetSelection(false);
                    stimList[i].SetClaimable(false);
                    stimList[i].SetVisible(true);
                    if (i < stimList.Count / 2)
                    {
                        stimList[i].Claim(0, playerColour);
                    }
                    else
                    {
                        stimList[i].Claim(1, opponentColour);
                    }
                }
                break;
            // Fake trial 1
            case 5:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 2.
            case 6:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 4.
            case 7:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // End (nothing happens).
            case 9:
                break;
            // Default case.
            default:
                break;
        }

        // Update the latest page flip.
        lastPageFlip = Time.time;
    }

    private void ClearPage()
    {
        // Clear the text.
        explanationText.text = "";
        explanationText.color = new Color(1.0f, 1.0f, 1.0f);

        // Destroy all Gabors.
        foreach (Gabor stim in stimList)
        {
            Destroy(stim.gameObject);
        }
        stimList = new List<Gabor>();

        // Stop the flipping phase.
        if (flippingPhase)
        {
            flippingPhase = false;
        }
        // Stop the selection phase.
        if (selectionPhase)
        {
            selectionPhase = false;
        }
        // Stop the response phase.
        if (responsePhase)
        {
            responsePhase = false;
        }
    }

    private void StartNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    private IEnumerator FlickStimulusMask(int nFlicks, float flickSeconds)
    {
        for (int i = 0; i < nFlicks; i++)
        {
            // Only do this in the flipping phase.
            if (flippingPhase)
            {
                // Toggle each stimulus to the opposite of what they currently are.
                foreach (Gabor stim in stimList)
                {
                    stim.SetMasked(!stim.GetMasked());
                }
                // Wait for a few seconds.
                yield return new WaitForSeconds(flickSeconds);
            }
        }
    }

    private IEnumerator PracticeTrial(float stimDuration,
        float maintenanceDuration)
    {
        // Create random stimulus orientations.
        System.Random rand = new System.Random();
        float[] stimAngles = new float[stimList.Count];
        for (int i = 0; i < stimList.Count; i++)
        {
            stimAngles[i] = (float)rand.Next(180);
        }

        // Randomly decide which stimuli to take.
        bool chooseLower = rand.Next(2) == 1;

        // Set orientations and make sure stimuli are visible.
        for (int i = 0; i < stimList.Count; i++)
        {
            stimList[i].SetOrientation(stimAngles[i]);
            stimList[i].SetClaimable(true);
            stimList[i].SetMasked(false);
            stimList[i].SetRotatable(false);
            stimList[i].SetSelection(false);
            stimList[i].SetVisible(true);
            // Claim some of the stimuli.
            if (((!chooseLower) & (i >= stimList.Count/2)) |
                ((chooseLower) & (i < stimList.Count / 2)))
            {
                stimList[i].Claim(1, opponentColour);
            }
        }

        // Start the selection phase, and wait for it
        // to end.
        float selectionPhaseStart = Time.time;
        selectionPhase = true;
        yield return new WaitUntil(() => selectionPhase == false);
        // Wait for at least one second.
        float remainingTime = stimDuration - (Time.time - selectionPhaseStart);
        if (remainingTime < 1)
        {
            remainingTime = 1.0f;
        }
        yield return new WaitForSeconds(remainingTime);

        // Mask all stimuli.
        foreach (Gabor stim in stimList)
        {
            stim.SetOrientation(0.0f);
            stim.SetMasked(true);
        }
        // Wait for a few seconds.
        yield return new WaitForSeconds(maintenanceDuration);

        // Probe one of the claimed stimuli.
        List<int> claimedStimuli = new List<int>();
        for (int i = 0; i < stimList.Count; i++)
        {
            if (stimList[i].GetOwner() == 0)
            {
                claimedStimuli.Add(i);
            }
        }
        int probeNr = claimedStimuli[rand.Next(claimedStimuli.Count)];
        stimList[probeNr].SetRotatable(true);
        stimList[probeNr].SetSelection(true);

        // Start the response phase.
        responsePhase = true;
        // Wait until a response is recorded.
        yield return new WaitUntil(() => responsePhase==false);

        // Compute and present the error.
        int respError = ComputeCircularDifference((int)stimAngles[probeNr],
            (int)stimList[probeNr].GetOrientation(), 180);
        int respErrorPercentage = (int)System.Math.Round(100.0f -
            100.0f * ((float)respError / 90.0f));
        explanationText.text = "You were " +
            respErrorPercentage.ToString() + "% correct!";
        if (respErrorPercentage >= respErrorColourCutoffPercentage)
        {
            explanationText.color = new Color(0.0f, 1.0f, 0.0f);
        }
        else
        {
            explanationText.color = new Color(1.0f, 0.0f, 0.0f);
        }
    }

    private int ComputeCircularDifference(int ori1, int ori2, int zero)
    {
        float error = (float)System.Math.Abs(ori1 - ori2) % zero;

        if (error > zero / 2)
            error = zero - error;

        return (int)error;
    }
}
