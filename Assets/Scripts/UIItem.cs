using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class UIItem : MonoBehaviour
{
    public static UIItem instance;

    // display
    public Dropdown option;

    // parts
    public Button play;
    public Button train;
    public Scrollbar population;
    public Scrollbar mutation;
    public Button test;
    public InputField write;
    public Dropdown read;
    public Text statusTitle;
    public Text statusText;
    public Text scoreText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void Start()
    {
        train.transform.parent.gameObject.SetActive(false);
        test.transform.parent.gameObject.SetActive(false);
        option.onValueChanged.AddListener(delegate
        {
            string mode = option.options[option.value].text;
            play.transform.parent.gameObject.SetActive(false);
            train.transform.parent.gameObject.SetActive(false);
            test.transform.parent.gameObject.SetActive(false);
            if(mode.Equals("Play"))
            {
                play.transform.parent.gameObject.SetActive(true);
            }
            else if (mode.Equals("Train"))
            {
                train.transform.parent.gameObject.SetActive(true);
            }
            else if(mode.Equals("Test"))
            {
                test.transform.parent.gameObject.SetActive(true);            
            }
        });

        play.onClick.AddListener(delegate
        {
            PlayerSpawner.instance.SaveBrain();
            PlayerSpawner.instance.play = true;
            SceneManager.LoadScene(0);
            statusTitle.text = "Playing";
            statusText.text = "Play around to see what the birds are trying to learn.";
        });

        train.onClick.AddListener(delegate
        {
            PlayerSpawner.instance.SaveBrain();
            PlayerSpawner.instance.play = false;
            PlayerSpawner.instance.testBrain = false;
            SceneManager.LoadScene(0);
            statusTitle.text = "Training";
            statusText.text = "Generation: 0 \nBest: 0 \nPopulation: " + PlayerSpawner.instance.populationCount + "\nMutation: " + PlayerSpawner.instance.mutationRate;
        });

        test.onClick.AddListener(delegate
        {
            if (PlayerSpawner.instance.GetAllReadableFiles().Count == 0)
            {
                statusText.text = "No trained birds to test!";
            }
            else
            {
                PlayerSpawner.instance.SaveBrain();
                PlayerSpawner.instance.play = false;
                PlayerSpawner.instance.testBrain = true;
                SceneManager.LoadScene(0);
                statusTitle.text = "Testing " + PlayerSpawner.instance.readFile;
                statusText.text = "";
            }
        });

        write.onEndEdit.AddListener(delegate
        {
            PlayerSpawner.instance.writeFile = write.text;
        });

        UpdateReadDropdown();
        PlayerSpawner.instance.readFile = read.options[0].text;
        read.onValueChanged.AddListener(delegate
        {
            PlayerSpawner.instance.readFile = read.options[read.value].text;
        });
    }

    public void UpdateReadDropdown()
    {
        read.ClearOptions();
        read.AddOptions(PlayerSpawner.instance.GetAllReadableFiles());
    }
}
