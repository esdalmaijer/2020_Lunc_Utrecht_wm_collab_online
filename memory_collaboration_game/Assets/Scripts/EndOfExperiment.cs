using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndOfExperiment : MonoBehaviour
{
    [SerializeField] string prolificCompletionCode;
    [SerializeField] float waitUntilRedirectSeconds = 5.0f;
    [SerializeField] TextMesh thankYouText;

    // Variables.
    private bool isProlificParticipant;
    private string prolificCompletionURL = "";

    // Start is called before the first frame update
    void Start()
    {
        // Load the Boolean that indicates whether the participant came from
        // Prolific, to determine whether they should be redirected.
        if (PlayerPrefs.HasKey("isProlificParticipant"))
        {
            isProlificParticipant =
                PlayerPrefs.GetInt("isProlificParticipant") == 1;
        }
        else
        {
            isProlificParticipant = false;
        }

        // Update the Thank You text.
        if (isProlificParticipant)
        {
            thankYouText.text = "Thank you for participating!\n\n" +
                "You should be redirected to Prolific within " +
                System.Math.Round(waitUntilRedirectSeconds).ToString() +
                " seconds.\n\n" +
                "If you are not redirected, your completion code is: " +
                prolificCompletionCode + "\n\n" +
                "Please write this down, and quote it with the researcher.";
        }
        else
        {
            thankYouText.text = "Thank you for participating!";
        }

        // Launch the redirection to the Prolific completion page.
        if (isProlificParticipant)
        {
            StartCoroutine(WaitThenRedirect(waitUntilRedirectSeconds,
                prolificCompletionCode));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Open the Prolific completion page.
    private IEnumerator WaitThenRedirect(float waitSeconds,
        string completionCode)
    {
        Debug.Log("Loading URL in " + waitSeconds.ToString() + " seconds: " +
            prolificCompletionURL + "?cc=" + completionCode);

        // Wait for the required time.
        yield return new WaitForSeconds(waitSeconds);
        // Open the URL.
        Application.OpenURL(prolificCompletionURL + "?cc=" + completionCode);
    }
}
