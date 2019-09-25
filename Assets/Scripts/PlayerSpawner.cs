using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    string appName = "FlappyBirdAI";

    public static PlayerSpawner instance;

    public bool play = true;
    public bool testBrain = true;

    public GameObject player;

    public int populationCount = 150;
    public PlayerController[] population;
    public int populationAlive;
    public float mutationRate = .15f;
    int generation = 1;
    float globalBestScore = 0;
    NeuralNetwork globalBestBrain;
    FitnessComparer fc;

    public string readFile;
    public string writeFile;

    public TextAsset premadeBrain;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Start();
    }

    private void Start()
    {
        // actors ignore each other
        Physics2D.IgnoreLayerCollision(10, 10, true);
        ImportPremadeBrain();

        // reset scores
        generation = 0;
        globalBestScore = 0;
        globalBestBrain = null;
        population = null;

        if (play)
        {
            Instantiate(player);
        }
        else
        {
            // assigns a brain
            if (testBrain)
            {
                NeuralNetwork testb = JsonUtility.FromJson<NeuralNetwork>(ReadBrain(readFile));
                StartCoroutine(AssignBrain(Instantiate(player).GetComponent<PlayerController>(), testb));
            }
            // starts genetic algorithm
            else
            {
                fc = new FitnessComparer();
                population = new PlayerController[populationCount];

                for (int j = 0; j < populationCount; j++)
                {
                    GameObject p = Instantiate(player);
                    population[j] = p.GetComponent<PlayerController>();
                }
                populationAlive = populationCount;
            }
        }
    }

    // if premade brain is not in record, add it
    void ImportPremadeBrain()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Resources/" + appName))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Resources/" + appName);
        }

        if (!GetAllReadableFiles().Contains(premadeBrain.name))
        {
            string path = Application.persistentDataPath + "/Resources/" + appName + "/" + premadeBrain.name + ".txt";
            StreamWriter sw = new StreamWriter(path, false);
            sw.Write(premadeBrain.text);
            sw.Close();
            PlayerPrefs.SetString(appName + premadeBrain.name, premadeBrain.text);
            UIItem.instance.UpdateReadDropdown();
        }
    }

    // delays a frame and assigns brain
    System.Collections.IEnumerator AssignBrain(PlayerController pc, NeuralNetwork nn)
    {
        yield return new WaitForEndOfFrame();
        pc.brain = nn;
        UIItem.instance.statusTitle.text = "Testing " + readFile;
        UIItem.instance.statusText.text = "Highest achieved in training: " + ReadScore(readFile);
    }

    public void ShowReadableFiles()
    {
        // debugger
        string message = "Readable Files: \n";
        foreach (string s in GetAllReadableFiles())
        {
            message += s + "\n";
        }
        UIItem.instance.statusText.text = message;
        Debug.Log(message);
    }

    public void ShowResourceFiles()
    {
        // debugger
        string message = "Resources Files: \n";
        string filePath = Application.persistentDataPath + "/Resources";
        //DirectoryInfo dir = new DirectoryInfo(filePath);
        //FileInfo[] info = dir.GetFiles("*.*");
        //for (int j = 0; j < info.Length; j++)
        //{
        //    message += j + " : " + Path.GetFileNameWithoutExtension(info[j].ToString()) + "\n";
        //}
        message += filePath;
        UIItem.instance.statusText.text = message;
        Debug.Log(message);
    }

    public void ShowNeighboringFiles()
    {
        // debugger
        string message = "Neighboring Files: \n";
        string filePath = Application.persistentDataPath;
        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*");
        for (int j = 0; j < info.Length; j++)
        {
            message += j + " : " + Path.GetFileNameWithoutExtension(info[j].ToString()) + "\n";
        }
        message += filePath;
        UIItem.instance.statusText.text = message;
        Debug.Log(message);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.T))
        {
            ShowReadableFiles();
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Y))
        {
            ShowResourceFiles();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            FastForward();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NormalSpeed();
        }

        if (play || testBrain || population == null)
        {
            return;
        }

        // check repopulation
        if (populationAlive == 0)
        {
            PipeSpawnerSpawner.instance.ResetCourse();
            Repopulate();
            generation++;
            UIItem.instance.statusText.text = ("Generation: " + generation + "\nBest: " + globalBestScore + "\nPopulation: " + populationCount + "\nMutation: " + mutationRate);
        }
    }

    public void FastForward()
    {
        Time.timeScale = 20;
    }

    public void NormalSpeed()
    {
        Time.timeScale = 1;
    }

    // creates a new population
    void Repopulate()
    {
        Array.Sort(population, fc);
        // records highest score
        if (population[0].score > globalBestScore)
        {
            globalBestScore = population[0].score;
            globalBestBrain = population[0].brain.Copy();
        }

        NeuralNetwork[] newPopulation = new NeuralNetwork[populationCount];
        for (int j = 0; j < populationCount; j += 2)
        {
            // creates 2 babies based on 2 genes
            NeuralNetwork parentA = PickOne(population).brain;
            NeuralNetwork parentB = PickOne(population).brain;
            NeuralNetwork[] babies = parentA.CrossOver(parentB);
            NeuralNetwork babyBrain1 = babies[0];
            NeuralNetwork babyBrain2 = babies[1];
            babyBrain1.Mutate(mutationRate);
            babyBrain2.Mutate(mutationRate);
            newPopulation[j] = babyBrain1;
            newPopulation[j + 1] = babyBrain2;
        }

        for (int j = 0; j < populationCount; j++)
        {
            population[j].Revive();
            population[j].brain = newPopulation[j];
        }

        populationAlive = populationCount;

        // saves info every new generation
        SaveBrain();
    }

    // pooling from population based on fitness
    PlayerController PickOne(PlayerController[] pool)
    {
        // gets sum to normalize score
        float sum = 0;
        foreach (PlayerController pc in pool)
        {
            sum += pc.score;
        }
        // creates array of normalized score
        float[] normalizedScore = new float[pool.Length];
        for (int j = 0; j < pool.Length; j++)
        {
            normalizedScore[j] = pool[j].score / sum;
        }
        // gets random number
        float rand = UnityEngine.Random.value;
        int index = 0;
        // sees where it drops
        while (rand > 0)
        {
            rand -= normalizedScore[index];
            index++;
        }
        index--;
        // returns pc
        return pool[index];
    }

    public void SetMutation(float f)
    {
        mutationRate = f;
    }

    public void SetPopulation(float f)
    {
        // only even populations
        populationCount = 50 + 2 * (int)(50 * f);
        // if changing population while training, restart training
        if (!play && !testBrain)
        {
            UIItem.instance.train.onClick.Invoke();
        }
    }

    // write to file(for visibility) and player pref
    // only read from player pref
    public void SaveBrain()
    {
        // if not training
        if (population == null)
        {
            return;
        }

        // save highest score if still running
        Array.Sort(population, fc);
        // records highest score
        if (population[0].score > globalBestScore)
        {
            globalBestScore = population[0].score;
            globalBestBrain = population[0].brain.Copy();
        }

        float score = globalBestScore;
        string s = JsonUtility.ToJson(globalBestBrain);
        string path = Application.persistentDataPath + "/Resources/" + appName + "/" + writeFile + ".txt";
        PrepareWriteFile();

        // file already exists with a score
        try
        {
            float previousScore = ReadScore(writeFile);

            // only write if score is better
            if (score > previousScore)
            {
                // Rewrite brain
                StreamWriter writer = new StreamWriter(path, false);
                writer.WriteLine(score);
                writer.WriteLine(s);
                writer.Close();

                // rewrite record
                PlayerPrefs.SetString(appName + writeFile, score + "\n" + s);
            }
        }
        // file is in wrong format
        catch
        {
            // Rewrite brain
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(score);
            writer.WriteLine(s);
            writer.Close();

            // rewrite record
            PlayerPrefs.SetString(appName + writeFile, score + "\n" + s);
        }
    }

    // reads the brain string from a file path
    string ReadBrain(string path)
    {
        // retreives text from asset
        String res = "";
        if (PlayerPrefs.HasKey(appName + path))
        {
            res = PlayerPrefs.GetString(appName + path);
        }
        // parces the JSON 
        res = res.Substring(res.IndexOf("{"));

        return res;
    }

    // returns score from a file path
    float ReadScore(string path)
    {
        // retreives text from asset
        String res = "";
        if (PlayerPrefs.HasKey(appName + path))
        {
            res = PlayerPrefs.GetString(appName + path);
        }
        Debug.Log("In Read Score" + "\nRes: " + res + "\nScore: " + res.IndexOf("{"));
        // parces the score 
        res = res.Substring(0, res.IndexOf("{")).Trim();
        float previousScore = float.Parse(res);
        return previousScore;
    }

    // gets all txt file names in Resources
    public List<string> GetAllReadableFiles()
    {
        string filePath = Application.persistentDataPath + "/Resources/" + appName;
        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*.txt");
        string[] res = new string[info.Length];
        for (int j = 0; j < info.Length; j++)
        {
            res[j] = Path.GetFileNameWithoutExtension(info[j].ToString());
        }
        return new List<string>(res);
    }

    // returns if the file to be written previously existed
    public bool PrepareWriteFile()
    {
        string path = Application.persistentDataPath + "/Resources/" + appName + "/" + writeFile + ".txt";
        // if not in filesystem
        if (!GetAllReadableFiles().Contains(writeFile))
        {
            StreamWriter sw = new StreamWriter(path, false);
            // if in record, update file system
            if (PlayerPrefs.HasKey(appName + writeFile))
            {
                Debug.Log("File does not exist, PP does exist");
                sw.Write(PlayerPrefs.GetString(appName + writeFile));
                sw.Close();
                UIItem.instance.UpdateReadDropdown();
                return true;
            }
            else // if not in record, create empty record and file
            {
                Debug.Log("File does not exist, PP does not exist");
                sw.Write("");
                sw.Close();
                PlayerPrefs.SetString(appName + writeFile, "");
                UIItem.instance.UpdateReadDropdown();
                return false;
            }
        }
        // if in filesystem
        else
        {
            // if in record, do nothing
            if (PlayerPrefs.HasKey(appName + writeFile))
            {
                Debug.Log("File does exist, PP does exist");
                return true;
            }
            // if not in record, commit to record
            else
            {
                PlayerPrefs.SetString(appName + writeFile, "");
                return true;
            }
        }
    }
}

public class FitnessComparer : IComparer<PlayerController>
{
    public int Compare(PlayerController x, PlayerController y)
    {
        return (int)(y.score * 100 - x.score * 100);
    }
}
