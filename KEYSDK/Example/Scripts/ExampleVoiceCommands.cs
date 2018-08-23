using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Linq;
#if NET_4_6
using PracticalManaged.Practical.SpatialSpawning;

public class ExampleVoiceCommands : MonoBehaviour
{
    public GameObject RedDisk;
    public GameObject BlueDisk;
    public GameObject YellowDisk;

    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    private KeywordRecognizer keywordRecognizer = null;

    private void Start()
    {
        voiceCommandsInitialize();

        foreach (var item in PracticalLocator.Instance.DefinedGroups)
        {
            Debug.Log("Group Name: " + item.groupName);
            foreach (var item2 in item.spawnLocations)
            {
                Debug.Log("Spawn Type: " + item2.selectedSpawnType);
            }
        }
    }
    private void voiceCommandsInitialize()
    {
        keywords.Add("Red Spawn", () =>
        {
            RedSpawn();
        });

        keywords.Add("Blue Spawn", () =>
        {
            BlueSpawn();
        });

        keywords.Add("Yellow Spawn", () =>
        {
            YellowSpawn();
        });


        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private async void RedSpawn()
    {
        Debug.Log("Running Query");
        SpawnLocation returnedLocation = await PracticalLocator.Instance.GetSpawnPoint("Red Group");
        if (returnedLocation != null)
        {
            Debug.Log("Position: " + returnedLocation.Position);
            Instantiate(RedDisk, returnedLocation.Position, returnedLocation.Rotation);
            Debug.Log("Red disk placed");
        }
    }

    private async void BlueSpawn()
    {
        Debug.Log("Running Query");
        SpawnLocation returnedLocation = await PracticalLocator.Instance.GetSpawnPoint("Blue Group");
        if (returnedLocation != null)
        {
            Debug.Log("Position: " + returnedLocation.Position);
            Instantiate(BlueDisk, returnedLocation.Position, returnedLocation.Rotation);
            Debug.Log("Blue disk placed");
        }
    }

    private async void YellowSpawn()
    {
        Debug.Log("Running Query");
        SpawnLocation returnedLocation = await PracticalLocator.Instance.GetSpawnPoint("Yellow Group");
        if (returnedLocation != null)
        {
            Debug.Log("Position: " + returnedLocation.Position);
            Instantiate(YellowDisk, returnedLocation.Position, returnedLocation.Rotation);
            Debug.Log("Yellow disk placed");
        }
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }
}
#endif