using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplanationController : MonoBehaviour
{
    // GUI editable variables.
    [SerializeField] float minPageTime = 7.0f;
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
    private float[] stimX = {0.3f, 0.6f};
    private float[] stimY = {0.3f, 0.4f};
    private bool flippingPhase = false;
    private bool responsePhase = false;

    // Explanation texts.
    private string[] pages = {
        "Yarrrr, matey! Welcome to this memory test!\n\n" +
        "In this test, you will be asked to remember the\n" +
        "orientations of lines. The better you remember,\n" +
        "the more doubloons you score!",

        "You will see several circles with lines each time.\n" +
        "They look like the ones you see below.\n\n" +
        "Your job is to remember all of them!",

        "You have a few seconds to memorise the lines.\n" +
        "Then, they will disappear (like the example\n" +
        "below). Try to keep them in mind!",

        "After a few seconds, a circle will appear\n" +
        "around one of the circles. You have to reproduce\n" +
        "its orientation by clicking in the circle, and\n" +
        "moving your mouse while holding its left button." +
        "\n\nWhen you're done, press Space to confirm.",

        "Now try to memorise and then reproduce these:",

        "Now please remember these:",

        "Remember and recall:",

        "And again:",

        "One more time:",

        "The better you memorise the orientations,\n" +
        "the more coins you'll win!" +
        "\n\n\nHit the Next button to start."};

    // Start is called before the first frame update
    void Start()
    {
        // Start with the first page.
        LoadPage(currentPage);

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
    }

    // Update is called once per frame
    void Update()
    {
        // Compute how much time has passed since last page flip.
        float timePassed = Time.time - lastPageFlip;

        // Update the text on the Next button.
        if (responsePhase)
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

        // Add the page-specific objects.
        switch (pageNumber)
        {
            // Nothing on the first page.
            case 0:
                break;
            // Two Gabors with orientations.
            case 1:
                foreach (Gabor stim in stimList)
                {
                    stim.SetClaimable(false);
                    stim.SetMasked(false);
                    stim.SetRotatable(false);
                    stim.SetSelection(false);
                    stim.SetVisible(true);
                }
                break;
            // Two Gabors with orientations, masked, unmasked, etc.
            case 2:
                foreach (Gabor stim in stimList)
                {
                    stim.SetClaimable(false);
                    stim.SetMasked(true);
                    stim.SetRotatable(false);
                    stim.SetSelection(false);
                    stim.SetVisible(true);
                }
                // Flick five times, every two seconds.
                flippingPhase = true;
                StartCoroutine(FlickStimulusMask(6, 2.0f));
                break;
            // Two Gabors, both masked, one probed.
            case 3:
                foreach (Gabor stim in stimList)
                {
                    stim.SetClaimable(false);
                    stim.SetMasked(true);
                    stim.SetRotatable(false);
                    stim.SetSelection(false);
                    stim.SetVisible(true);
                }
                // Probe the first stimulus.
                responsePhase = true;
                stimList[0].SetRotatable(true);
                stimList[0].SetSelection(true);
                break;
            // Fake trial 1.
            case 4:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 2
            case 5:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 3.
            case 6:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 4.
            case 7:
                StartCoroutine(PracticeTrial(stimDuration,
                    maintenanceDuration));
                break;
            // Fake trial 5.
            case 8:
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
        // Make the Gabors invisible.
        foreach (Gabor stim in stimList)
        {
            stim.SetClaimable(false);
            stim.SetMasked(false);
            stim.SetRotatable(false);
            stim.SetSelection(false);
            stim.SetVisible(false);
        }
        // Stop the flipping phase.
        if (flippingPhase)
        {
            flippingPhase = false;
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

        // Set orientations and make sure stimuli are visible.
        for (int i = 0; i < stimList.Count; i++)
        {
            stimList[i].SetOrientation(stimAngles[i]);
            stimList[i].SetClaimable(false);
            stimList[i].SetMasked(false);
            stimList[i].SetRotatable(false);
            stimList[i].SetSelection(false);
            stimList[i].SetVisible(true);
        }
        // Wait for a few seconds.
        yield return new WaitForSeconds(stimDuration);

        // Mask all stimuli.
        foreach (Gabor stim in stimList)
        {
            stim.SetOrientation(0.0f);
            stim.SetMasked(true);
        }
        // Wait for a few seconds.
        yield return new WaitForSeconds(maintenanceDuration);

        // Probe one of the stimuli.
        int probeNr = rand.Next(stimList.Count);
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
