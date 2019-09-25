using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PipeSpawnerSpawner : MonoBehaviour
{
    public static PipeSpawnerSpawner instance;

    public GameObject pipeSpawner;
    float distanceFromPlayer = 12;
    float timeBetweenPipes = 2;
    public int score = 0;

    public LinkedList<PipeSpawner> pipes;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            pipes = new LinkedList<PipeSpawner>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetCourse();
    }

    // spawn pipes on a timer
    void SpawnPipeSpawner()
    {
        GameObject pipe = Instantiate(pipeSpawner);
        pipe.transform.position = new Vector2(distanceFromPlayer, pipe.transform.position.y);
        pipes.AddLast(pipe.GetComponent<PipeSpawner>());
        Invoke("SpawnPipeSpawner", timeBetweenPipes);
    }

    private void FixedUpdate()
    {
        // if pipe passes center
        if (pipes.First.Value.transform.position.x < -.5f)
        {
            // destroy pipe and increment score
            float pipeYPos = pipes.First.Value.transform.position.y;
            Destroy(pipes.First.Value.gameObject, 3);
            pipes.RemoveFirst();
            score++;
            UIItem.instance.scoreText.text = "" + score;

            //if training
            if (!PlayerSpawner.instance.play && !PlayerSpawner.instance.testBrain)
            {
                foreach (PlayerController pc in PlayerSpawner.instance.population)
                {
                    // increments all pc that are still alive
                    if (pc.alive)
                    {
                        // boost if near center
                        if (pc.transform.position.y > pipeYPos - .35f && pc.transform.position.y < pipeYPos)
                        {
                            pc.score += 3;
                        }
                    }
                }
            }
        }
    }

    public void ResetCourse()
    {
        // cancel previous pip spawning
        CancelInvoke("SpawnPipeSpawner");

        // destroy all the pipes 
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Pipe"))
        {
            Destroy(p);
        }
        pipes.Clear();

        // updates score
        score = 0;
        UIItem.instance.scoreText.text = "" + score;

        // spawn pipes
        GameObject pipe1 = Instantiate(pipeSpawner);
        pipe1.transform.position = new Vector2(6, pipe1.transform.position.y);
        pipes.AddLast(pipe1.GetComponent<PipeSpawner>());
        SpawnPipeSpawner();
    }
}
