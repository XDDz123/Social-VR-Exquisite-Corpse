using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TopicSelection : MonoBehaviour
{
    public Button[] topicButtons; // The three buttons that will display the topics
    public Text[] voteTexts; // The three texts that will display the vote counts
    public List<string> topics; // The list of topics to choose from
    public int numTopics = 3; // The number of topics to display
    private Dictionary<string, int> voteCounts; // A dictionary that stores the number of votes for each topic

    void Start()
    {
        voteCounts = new Dictionary<string, int>();

        // Choose three random topics from the list
        List<string> chosenTopics = new List<string>();
        for (int i = 0; i < numTopics; i++)
        {
            int randIndex = Random.Range(0, topics.Count);
            string topic = topics[randIndex];
            chosenTopics.Add(topic);
            topics.RemoveAt(randIndex);

            // Initialize the vote count for this topic to 0
            voteCounts[topic] = 0;
        }

        // Set the text of each topic button to a random topic
        for (int i = 0; i < topicButtons.Length; i++)
        {
            if (i < chosenTopics.Count)
            {
                topicButtons[i].GetComponentInChildren<Text>().text = chosenTopics[i];
            }
            else
            {
                topicButtons[i].gameObject.SetActive(false);
            }
        }

        // Set the text of each vote text to 0
        for (int i = 0; i < voteTexts.Length; i++)
        {
            voteTexts[i].text = "0";
        }
    }

    public void Vote(Button button)
    {
        string topic = button.GetComponentInChildren<Text>().text;

        // Increment the vote count for this topic
        voteCounts[topic]++;

        // Update the text of the corresponding vote text to show the new vote count
        for (int i = 0; i < topicButtons.Length; i++)
        {
            if (topicButtons[i] == button)
            {
                voteTexts[i].text = voteCounts[topic].ToString();
                break;
            }
        }

        // Find the topic with the highest vote count
        string winningTopic = null;
        int maxVotes = 0;
        foreach (KeyValuePair<string, int> pair in voteCounts)
        {
            if (pair.Value > maxVotes)
            {
                maxVotes = pair.Value;
                winningTopic = pair.Key;
            }
        }

        // Display the winning topic
        Debug.Log("Current leader: " + winningTopic + " with " + maxVotes + " votes.");
    }
}
