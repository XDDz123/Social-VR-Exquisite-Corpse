using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TopicSelection : MonoBehaviour
{
    public Button[] topicButtons; 
    public Button[] voteButtons; 
    public List<string> topics; 
    public int numTopics = 3;
    private Dictionary<string, int> voteCounts; 
    void Start()
    {
        voteCounts = new Dictionary<string, int>();

        
        List<string> chosenTopics = new List<string>();
        for (int i = 0; i < numTopics; i++)
        {
            int randIndex = Random.Range(0, topics.Count);
            string topic = topics[randIndex];
            chosenTopics.Add(topic);
            topics.RemoveAt(randIndex);

            
            voteCounts[topic] = 0;
        }

        
        for (int i = 0; i < topicButtons.Length; i++)
        {
            if (i < chosenTopics.Count)
            {
                topicButtons[i].GetComponentInChildren<Text>().text = chosenTopics[i];
                topicButtons[i].interactable = true;
            }
            else
            {
                topicButtons[i].interactable = false;
            }
        }

        
        for (int i = 0; i < voteButtons.Length; i++)
        {
            voteButtons[i].GetComponentInChildren<Text>().text = "0";
        }
    }

    void Update()
    {
       
        for (int i = 0; i < voteButtons.Length; i++)
        {
            string topic = voteButtons[i].GetComponentInChildren<Text>().text;
            if (voteCounts.ContainsKey(topic))
            {
                int voteCount = voteCounts[topic];
                voteButtons[i].GetComponentInChildren<Text>().text = voteCount.ToString();
            }
        }
    }

    public void Vote(Button button)
    {
        Debug.Log("Clicked on topic button with text: " + button.GetComponentInChildren<Text>().text);
        
        string topic = button.GetComponentInChildren<Text>().text;

 
        voteCounts[topic]++;

        Debug.Log("Updated vote count for topic " + topic + " to " + voteCounts[topic]);

        for (int i = 0; i < voteButtons.Length; i++)
        {
            if (voteButtons[i].GetComponentInChildren<Text>().text == topic)
            {
                int voteCount = voteCounts[topic];
                voteButtons[i].GetComponentInChildren<Text>().text = voteCount.ToString();
                break;
            }
        }

        
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

      
        Debug.Log("Current leader: " + winningTopic + " with " + maxVotes + " votes.");
    }
}