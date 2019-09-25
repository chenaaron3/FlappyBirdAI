using UnityEngine;
using System.Collections;

public class PipeSpawner : MonoBehaviour
{
    public static float speed = 3;
    public float gap = 2f;
    Vector2 bounds;

    public GameObject topPipe;
    public GameObject bottomPipe;

    Rigidbody2D rb;

    private void Start()
    {
        // gets center
        bounds = new Vector2(-4.5f + gap + .2f, 4.5f - gap - .2f);

        // starts at end with y offset
        transform.position = new Vector2(transform.position.x, Random.Range(bounds.x, bounds.y));

        // instantiates pipes
        GameObject tp = Instantiate(topPipe, transform);
        tp.transform.localPosition = new Vector2(-.5f, gap);
        GameObject bp = Instantiate(bottomPipe, transform);
        bp.transform.localPosition = new Vector2(-.5f, -1 * gap);

        // move contraption
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(-1 * speed, 0);
    }
}
