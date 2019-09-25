using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;

    public bool alive = true;
    float jumpForce = 7f;
    public float score = 0;

    public NeuralNetwork brain;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!PlayerSpawner.instance.play)
        {
            brain = new NeuralNetwork(3, 7, 1);
        }
    }

    private void Update()
    {
        if (!alive)
        {
            return;
        }

        score += Time.deltaTime / PipeSpawner.speed;

        if (PlayerSpawner.instance.play)
        {
            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!alive)
        {
            return;
        }

        if (!PlayerSpawner.instance.play)
        {
            PipeSpawner firstPipe = PipeSpawnerSpawner.instance.pipes.First.Value;
            float[] inputs = new float[3];
            inputs[0] = (transform.position.y + 4) / 8;
            inputs[1] = (firstPipe.transform.position.y + 4) / 8f;
            inputs[2] = (firstPipe.transform.position.x - transform.position.x) / 6;

            float[] outputs = brain.FeedForward(inputs);
            if (outputs[0] > .5f)
            {
                Jump();
            }
        }
    }

    void Jump()
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    public void Die()
    {
        if (alive)
        {
            if (PlayerSpawner.instance.play || PlayerSpawner.instance.testBrain)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                PlayerSpawner.instance.populationAlive--;
                alive = false;
                gameObject.SetActive(false);
            }
        }
    }

    public void Revive()
    {
        alive = true;
        score = 0;
        transform.position = Vector3.zero;
        gameObject.SetActive(true);
    }
}
