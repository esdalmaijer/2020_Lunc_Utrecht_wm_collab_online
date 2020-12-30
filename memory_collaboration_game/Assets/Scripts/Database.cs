using UnityEngine;
using UnityEngine.Networking;

using System;
using System.Collections;

public class Database : MonoBehaviour
{
    // DEBUG MODE
    // This is somewhat necessitated by the WWW weirdness on webGL.
    private bool DEBUG = true;

    // Database settings.
    private string secretKey = "";
    private string getParticipantURL = "";
    private string addParticipantURL = "";
    private string updateParticipantURL = "";
    private string addMemoryTrialURL = "";
    private string addMemoryGameTrialURL = "";
    private string addRatingURL = "";

    // Variables.
    public int participantNr = -1;
    private string latestReturnValue;

    // Functions.
    void Start()
    {
    }

    // Matched PHP md5 function.
    // See: http://wiki.unity3d.com/index.php?title=MD5
    private string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);
        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new
            System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);
        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }
        return hashString.PadLeft(32, '0');
    }

    public IEnumerator GetParticipant(string guid, System.Action<int> callback)
    {
        // Reset participant number.
        participantNr = -1;

        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(guid + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = getParticipantURL +
            "guid=" + WWW.EscapeURL(guid) +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result =>
        {
            // Parse the output.
            if (result == "error")
            {
                participantNr = -1;
            }
            else
            {
                if (result == "")
                {
                    participantNr = 0;
                }
                else
                {
                    try
                    {
                        participantNr = Int32.Parse(result);
                    }
                    catch (FormatException e)
                    {
                        participantNr = 0;
                    }
                }
            }
            // Let the main function know that we are done here.
            callback(participantNr);
        }));
    }

    // Convenience function for hitting and quiting the participant
    // coroutine, without waiting for the callback.
    public void LogParticipant(string gender, int age, int consent_share,
        string guid)
    {
        StartCoroutine(PostParticipant(gender, age, consent_share, guid,
            callback => {}));
    }

    public IEnumerator PostParticipant(string gender, int age, int consent_share,
        string guid, System.Action<string> callback)
    {
        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(gender + age.ToString() +
            consent_share.ToString() + guid + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = addParticipantURL +
            "gender=" + WWW.EscapeURL(gender) +
            "&age=" + age +
            "&consent_share=" + consent_share +
            "&guid=" + WWW.EscapeURL(guid) +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result => {
            callback(result);
        }));
    }

    // Convenience function for hitting and quiting the participant
    // coroutine, without waiting for the callback.
    public void AddParticipantCapacity(int participant_nr, float capacity)
    {
        StartCoroutine(PostParticipantCapacity(participant_nr, capacity,
            callback => { }));
    }

    public IEnumerator PostParticipantCapacity(int participant_nr,
        float capacity, System.Action<string> callback)
    {
        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(participant_nr.ToString() + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = updateParticipantURL +
            "participant_nr=" + participant_nr +
            "&capacity=" + capacity +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result => {
            callback(result);
        }));
    }

    // Convenience function for hitting and quiting the trial coroutine.
    public void LogMemoryTaskTrial(int participant_id, int trialnr, int nstim,
        int trial_onset, int stimulus_onset, int stimulus_offset,
        int resp_onset, int resp_offset, string stim_x, string stim_y,
        string stim_ori, int target_nr, string nontarget_ori, int target_ori,
        int resp_ori, int error, int rt)
    {
        // Start the coroutine, and ignore the callback.
        StartCoroutine(PostMemoryTaskTrial(participant_id, trialnr, nstim,
            trial_onset, stimulus_onset, stimulus_offset, resp_onset,
            resp_offset, stim_x, stim_y, stim_ori, target_nr, nontarget_ori,
            target_ori, resp_ori, error, rt, result => {}));
    }

    public IEnumerator PostMemoryTaskTrial(int participant_id, int trialnr,
        int nstim, int trial_onset, int stimulus_onset, int stimulus_offset,
        int resp_onset, int resp_offset, string stim_x, string stim_y,
        string stim_ori, int target_nr, string nontarget_ori, int target_ori,
        int resp_ori, int error, int rt, System.Action<string> callback)
    {
        //This connects to a server side PHP script that will add the
        // participant info to a MySQL database.

        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(participant_id.ToString() + trialnr.ToString() +
            trial_onset.ToString() + stim_ori + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = addMemoryTrialURL +
            "participant_id=" + participant_id +
            "&trialnr=" + trialnr +
            "&nstim=" + nstim +
            "&trial_onset=" + trial_onset +
            "&stimulus_onset=" + stimulus_onset +
            "&stimulus_offset=" + stimulus_offset +
            "&resp_onset=" + resp_onset +
            "&resp_offset=" + resp_offset +
            "&stim_x=" + WWW.EscapeURL(stim_x) +
            "&stim_y=" + WWW.EscapeURL(stim_y) +
            "&stim_ori=" + WWW.EscapeURL(stim_ori) +
            "&target_nr=" + target_nr +
            "&nontarget_ori=" + WWW.EscapeURL(nontarget_ori) +
            "&target_ori=" + target_ori +
            "&resp_ori=" + resp_ori +
            "&error=" + error +
            "&rt=" + rt +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result => {
            callback(result);
        }));
    }

    // Convenience function for hitting and quiting the trial coroutine.
    public void LogMemoryGameTrial(int participant_id, int roundnr,
        float opponent_capacity, string opponent_strategy, int trialnr,
        int nstim, int trial_onset, int stimulus_onset, int stimulus_offset,
        int resp_onset, int resp_offset, string stim_x, string stim_y,
        string stim_ori, int target_nr, string nontarget_ori, int target_ori,
        int resp_ori, int error, int rt, string player_claimed,
        string opponent_claimed, int opponent_error)
    {
        // Start the coroutine, and ignore the callback.
        StartCoroutine(PostMemoryGameTrial(participant_id, roundnr,
            opponent_capacity, opponent_strategy, trialnr, nstim, trial_onset,
            stimulus_onset, stimulus_offset, resp_onset, resp_offset, stim_x,
            stim_y, stim_ori, target_nr, nontarget_ori, target_ori, resp_ori,
            error, rt, player_claimed, opponent_claimed, opponent_error,
            result => { }));
    }

    public IEnumerator PostMemoryGameTrial(int participant_id, int roundnr,
        float opponent_capacity, string opponent_strategy, int trialnr,
        int nstim, int trial_onset, int stimulus_onset, int stimulus_offset,
        int resp_onset, int resp_offset, string stim_x, string stim_y,
        string stim_ori, int target_nr, string nontarget_ori, int target_ori,
        int resp_ori, int error, int rt, string player_claimed,
        string opponent_claimed, int opponent_error,
        System.Action<string> callback)
    {
        //This connects to a server side PHP script that will add the
        // participant info to a MySQL database.

        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(participant_id.ToString() + trialnr.ToString() +
            trial_onset.ToString() + stim_ori + player_claimed + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = addMemoryGameTrialURL +
            "participant_id=" + participant_id +
            "&roundnr=" + roundnr +
            "&opponent_capacity=" + opponent_capacity +
            "&opponent_strategy=" + WWW.EscapeURL(opponent_strategy) +
            "&trialnr=" + trialnr +
            "&nstim=" + nstim +
            "&trial_onset=" + trial_onset +
            "&stimulus_onset=" + stimulus_onset +
            "&stimulus_offset=" + stimulus_offset +
            "&resp_onset=" + resp_onset +
            "&resp_offset=" + resp_offset +
            "&stim_x=" + WWW.EscapeURL(stim_x) +
            "&stim_y=" + WWW.EscapeURL(stim_y) +
            "&stim_ori=" + WWW.EscapeURL(stim_ori) +
            "&target_nr=" + target_nr +
            "&nontarget_ori=" + WWW.EscapeURL(nontarget_ori) +
            "&target_ori=" + target_ori +
            "&resp_ori=" + resp_ori +
            "&error=" + error +
            "&rt=" + rt +
            "&player_claimed=" + WWW.EscapeURL(player_claimed) +
            "&opponent_claimed=" + WWW.EscapeURL(opponent_claimed) +
            "&opponent_error=" + opponent_error +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result => {
            callback(result);
        }));
    }

    // Convenience function for hitting and quiting the participant
    // coroutine, without waiting for the callback.
    public void LogRating(int participant_id, int roundnr,
        int questionnr, int response)
    {
        StartCoroutine(PostRating(participant_id, roundnr, questionnr,
            response, callback => { }));
    }

    public IEnumerator PostRating(int participant_id, int roundnr,
        int questionnr, int response, System.Action<string> callback)
    {
        // Construct a hash that will be compared server-side.
        string hash = Md5Sum(participant_id.ToString() + roundnr.ToString() +
            questionnr.ToString() + response.ToString() + secretKey);
        // Construct the URL to submit data to the PHP script with.
        string url = addRatingURL +
            "participant_id=" + participant_id +
            "&roundnr=" + roundnr +
            "&questionnr=" + questionnr +
            "&response=" + response +
            "&hash=" + hash;

        // Start the posting coroutine, and simply pass on its result.
        yield return StartCoroutine(PostURL(url, result => {
            callback(result);
        }));
    }

    IEnumerator PostURL(string url, System.Action<string> callback)
    {
        // Return straight away if we're in DEBUG mode.
        if (DEBUG)
        {
            callback("debug");
        }
        else
        {
            // Post the URL to the site and create an object to get the result.
            WWW post = new WWW(url);
            // Wait until the download is done.
            yield return post;

            // TODO: Escalate the error, or try again for a few times?
            if (post.error != null)
            {
                Debug.Log("There was an error posting the URL: " + post.error);
                callback("error");
            }
            else
            {
                Debug.Log("PostURL: " + post.text);
                // Set the latest return value.
                latestReturnValue = post.text;
                callback(post.text);
            }
        }
    }

}
