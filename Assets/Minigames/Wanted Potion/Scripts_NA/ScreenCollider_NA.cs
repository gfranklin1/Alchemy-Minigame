using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class ScreenCollider : MonoBehaviour
{
    EdgeCollider2D edgeCollider;
    void Start()
    {
        edgeCollider = GetComponent<EdgeCollider2D>();
        createEdgeCollider();
    }
    void createEdgeCollider()
    {
        List<Vector2> edges = new List<Vector2>();
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, 0)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)));
        edges.Add(Camera.main.ScreenToWorldPoint(new Vector2(0, Screen.height)));
        edges.Add(Camera.main.ScreenToWorldPoint(Vector2.zero));
        edgeCollider.SetPoints(edges);
    }
}
