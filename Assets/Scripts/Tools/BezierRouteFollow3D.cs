using UnityEngine;

public class BezierRouteFollow3D : MonoBehaviour
{
    [SerializeField] BezierPath3D[] Paths;

    // Sugestion: SPEED en MAYUSCULAS sugiere constante pero es un campo serializado; renombrar a camelCase (speed).
    [SerializeField, Range(0f, 10f)] float SPEED = 0.5f;

    private int currentRoute = 0;
    private float t = 0f;

    private void Awake()
    {
        Restart();
    }

    void Update()
    {
        if (t < 1)
        {
            t += Time.deltaTime * SPEED;
            transform.position = Paths[currentRoute].GetPointInCurve(t);
        }
        else
        {
            t = 0;
            currentRoute = currentRoute >= Paths.Length - 1 ? 0 : currentRoute + 1;
        }
    }

    public void Restart()
    {
        transform.position = Paths[0].GetPointInCurve(0);
        currentRoute = 0;
        t = 0f;
    }
}
