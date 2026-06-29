using UnityEngine;

public class Block : MonoBehaviour
{
    // Sugestion: Falta private.
    [SerializeField] BlockConfig Config;

    [SerializeField] Rigidbody body;
    [SerializeField] Renderer model;
    // Warning: campo public mutable; cualquier script puede sobrescribir 'size'. Como solo se calcula en Awake y se lee desde afuera, exponelo como propiedad de solo lectura: public Vector3 Size { get; private set; }.
    public Vector3 size;
    private void Awake()
    {
        size = model.bounds.size;
        transform.rotation = Quaternion.Euler(0, Random.Range(-Config.RotationMaxAngle, Config.RotationMaxAngle), 0);
    }

    public void Drop()
    {
        body.isKinematic = false;
    }

    public void Freeze()
    {
        body.isKinematic = true;
    }

    public void Fall()
    {
        body.linearVelocity = new Vector3(0, -Config.MissFallSpeed, 0);
        body.detectCollisions = false;
    }
}
