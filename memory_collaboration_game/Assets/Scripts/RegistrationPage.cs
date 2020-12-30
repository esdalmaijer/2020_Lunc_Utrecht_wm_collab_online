using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationPage : MonoBehaviour
{
    // GUI variables.
    [SerializeField] TextMesh welcomeText;
    [SerializeField] InputField ageInputField;
    [SerializeField] Toggle shareConsentToggle;
    [SerializeField] Toggle genderManToggle;
    [SerializeField] Toggle genderWomanToggle;
    [SerializeField] Toggle genderNonbinaryToggle;
    [SerializeField] Toggle genderNotSayingToggle;
    [SerializeField] Database dataLogger;
    [SerializeField] string nextScene;

    // Properties.
    GameObject confirmationButton;
    SpriteRenderer confirmationButtonSprite;
    private bool databaseError = false;
    private bool newParticipantUploaded = false;
    // Participant details.
    private bool newParticipant;
    private string participantGender;
    private int participantAge;
    private bool participantAgeInputSet;
    private bool participantGenderSelected;
    private int participantDataShareConsent;
    private string participantGUID;

    // Start is called before the first frame update
    void Start()
    {
        // Add the listener to the input field.
        ageInputField.onEndEdit.AddListener(UpdateAge);
        // Add the listener to the consent toggle.
        shareConsentToggle.onValueChanged.AddListener(UpdateShareConsent);
        // Add the listener to all gender toggles.
        genderManToggle.onValueChanged.AddListener(UpdateGender);
        genderWomanToggle.onValueChanged.AddListener(UpdateGender);
        genderNonbinaryToggle.onValueChanged.AddListener(UpdateGender);
        genderNotSayingToggle.onValueChanged.AddListener(UpdateGender);

        // Find the confirmation button.
        confirmationButton = transform.Find("Confirmation Button").gameObject;
        confirmationButtonSprite = confirmationButton.GetComponent<SpriteRenderer>();
        // Add this registration as the parent to the confirmation button.
        confirmationButton.AddComponent<RegistrationConfirmButton>().parent = this;
        // Deactive the button for now.
        confirmationButton.SetActive(false);

        // CHECK OR CREATE UNIQUE ID
        // Check if a GUID is in the PlayerPrefs.
        if (PlayerPrefs.HasKey("GUID"))
        {
            // Grab the GUID from the PlayerPrefs.
            participantGUID = PlayerPrefs.GetString("GUID");
        }
        // Create a new GUID and add it to the PlayerPrefs.
        else
        {
            participantGUID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString("GUID", participantGUID);
            PlayerPrefs.Save();
        }

        // ROUND ORDER
        // Start the rounds at 0.
        PlayerPrefs.SetInt("gameRoundNr", 0);

        // Set opponent memory capacity. This is defined in standard deviations
        // from the player's performance during the memory test.
        List<float> capacityOptions = new List<float>() { 1.25f, 1.0f, 0.75f };
        List<string> capacityOptionsRandomised = new List<string>();
        System.Random rand = new System.Random();
        int iterations = capacityOptions.Count;
        for (int i = 0; i < iterations; i++)
        {
            // Randomly choose an index from the options.
            int index = rand.Next(capacityOptions.Count);
            // Set the capacity.
            capacityOptionsRandomised.Add(capacityOptions[index].ToString());
            // Remove the chosen option.
            capacityOptions.RemoveAt(index);
        }
        string capacityOrderString = string.Join(";", capacityOptionsRandomised);
        PlayerPrefs.SetString("gameCapacityOrder", capacityOrderString);

        // Set the opponent strategy. This refers to what items they will
        // choose, and can be set to "closest" to make the opponent always
        // choose the closest unclaimed stimulus, or "random" to make them
        // choose one of the unclaimed stimuli at random.
        string[] strategyOptions = {"closest", "random"};
        List<string> strategyOptionsRandomised = new List<string>();
        for (int i = 0; i < capacityOptionsRandomised.Count; i++)
        {
            strategyOptionsRandomised.Add(
                strategyOptions[rand.Next(strategyOptions.Length)]);
        }
        string strategyOrderString = string.Join(";", strategyOptionsRandomised);
        PlayerPrefs.SetString("gameStrategyOrder", strategyOrderString);
        

        // GET PARTICIPANT NUMBER FROM DATABASE
        // Check if the GUID exists in the database.
        StartCoroutine(GetParticipantNumber(participantGUID, ppNr =>
        {
            // The dataLogger will return a participant number if the GUID
            // was already in the database.
            if (ppNr > 0)
            {
                newParticipant = false;
                PlayerPrefs.SetInt("PPNR", ppNr);
                PlayerPrefs.Save();
                welcomeText.text = "Welcome back, participant " +
                    PlayerPrefs.GetInt("PPNR").ToString();
                welcomeText.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            }
            // The dataLogger will return -1 on an error, and 0 if the
            // participant number was not in the database.
            else
            {
                if (ppNr == 0)
                {
                    newParticipant = true;
                }
                else
                {
                    OhNoDatabaseError("Could not load participant number");
                }
            }
        }));
    }

    // Update is called once per frame
    void Update()
    {
        // If all info is available, activate the confirmation button.
        if (participantGenderSelected & participantAgeInputSet)
        {
            confirmationButton.SetActive(true);
        }
        else
        {
            confirmationButton.SetActive(false);
        }
    }

    // This is called on an edit to the age input field.
    private void UpdateAge(string text)
    {
        // Set the age input bool.
        participantAgeInputSet = true;
        // This should be safe to parse as integer, because the input field
        // only allows integer input.
        participantAge = Int32.Parse(ageInputField.text);
        // Stop people from taking the piss.
        if (participantAge < 18)
        {
            participantAge = 18;
            ageInputField.text = participantAge.ToString();
        }
        else if (participantAge > 120)
        {
            participantAge = 120;
            ageInputField.text = participantAge.ToString();
        }
    }

    private void UpdateGender(bool isOn)
    {
        // Check if any toggle is on now.
        if (genderManToggle.isOn == false &
            genderWomanToggle.isOn == false &
            genderNonbinaryToggle.isOn == false &
            genderNotSayingToggle.isOn == false)
        {
            participantGenderSelected = false;
            participantGender = "";
        }
        // Check which of the toggles is on. (A Toggle Group prevents more than
        // one being selected at any time.)
        else
        {
            participantGenderSelected = true;
            if (genderManToggle.isOn)
            {
                participantGender = "m";
            }
            else if (genderWomanToggle.isOn)
            {
                participantGender = "f";
            }
            else if (genderNonbinaryToggle.isOn)
            {
                participantGender = "nb";
            }
            else if (genderNotSayingToggle.isOn)
            {
                participantGender = "no";
            }
        }
        Debug.Log(participantGender);
    }

    private void UpdateShareConsent(bool isOn)
    {
        participantDataShareConsent = Convert.ToInt32(shareConsentToggle.isOn);
    }

    public void ConfirmationClick()
    {
        // Check if all the necessary info is available.
        if (participantGenderSelected & participantAgeInputSet)
        {
            // Set the colour of the button to more yellowish. This highlight
            // will signal that the button click was registered.
            confirmationButtonSprite.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
            // Start the confirmation sequence.
            StartCoroutine(ConfirmationSequence());
        }
    }

    private IEnumerator ConfirmationSequence()
    {
        // Register the new participant.
        if (newParticipant)
        {
            yield return StartCoroutine(dataLogger.PostParticipant(
                participantGender, participantAge, participantDataShareConsent,
                participantGUID, result =>
                {
                    // Upon a successful return, process the participant number.
                    if (result == "error")
                        {
                            OhNoDatabaseError("Could not connect to database!");
                        }
                    else if (result == "fart noise")
                    {
                        OhNoDatabaseError("Error registering to the database.");
                    }
                    else if (result == "biscuit" | result == "debug")
                    {
                        newParticipantUploaded = true;
                        Debug.Log("Success!");
                    }
                    else
                    {
                        OhNoDatabaseError("What just happened? Database returned: " + result);
                    }
                }));
        }
        // Wait until the new participant number has been uploaded.
        yield return new WaitUntil(() => newParticipantUploaded | databaseError);
        // Proceed if the new participant was successfully uploaded.
        if (newParticipantUploaded)
        {
            yield return StartCoroutine(GetParticipantNumber(participantGUID,
                newNumber =>
            {
                // Include the participant number in the welcome text.
                welcomeText.text = "Welcome, participant " + newNumber.ToString();
                welcomeText.color = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            }));
            // Wait for a bit.
            yield return new WaitForSeconds(5.0f);
            // Advance to the next Scene.
            StartNextScene();
        }
    }

    private void OhNoDatabaseError(string errorMessage)
    {
        databaseError = true;
        Debug.Log("ERROR: " + errorMessage);
        // Include the error in the welcome text.
        welcomeText.text = "DATABASE ERROR: " + errorMessage;
        welcomeText.color = new Color(1.0f, 0.0f, 0.5f, 1.0f);
    }

    private IEnumerator GetParticipantNumber(string guid,
        System.Action<int> callback)
    {
        // Get the participant number.
        yield return StartCoroutine(dataLogger.GetParticipant(participantGUID,
            ppNr =>
            {
                // The dataLogger will return a participant number if the GUID
                // was already in the database.
                if (ppNr >= 0)
                {
                    PlayerPrefs.SetInt("PPNR", ppNr);
                    PlayerPrefs.Save();
                    newParticipantUploaded = true;
                    callback(ppNr);
                }
                // The dataLogger will return -1 on an error, and 0 if the
                // participant number was not in the database.
                else
                {
                    callback(-1);
                    OhNoDatabaseError("Could not load participant number");
                }
            }));
    }

    private void StartNextScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }
}
