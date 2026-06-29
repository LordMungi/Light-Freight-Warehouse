using UnityEngine;

public class Crane : MonoBehaviour
{
    // Sugestion: convencion de nombres: SPEED en MAYUSCULAS sugiere const, pero es un campo serializado editable; nombralo en camelCase (speed) y reserva MAYUSCULAS para const/readonly.
    [SerializeField] private float SPEED = 1f;
    // Warning: modificar wallMaterial.mainTextureOffset en runtime altera el ASSET de material compartido (cambios persisten en el editor y afectan a todos los objetos que lo usan). Si fuera intencional documentalo; si no, usar material instanciado.
    [SerializeField] Material wallMaterial;

    [Header("Listener Events")]
    [SerializeField] private BlockEventChannel BlockPerfectLandEvent;
    [SerializeField] private BlockEventChannel BlockSuccessfulLandEvent;
    [SerializeField] private BlockEventChannel BlockFailedLandEvent;

    Vector3 targetPos;
    Vector3 camOffset;

    private void Awake()
    {
        targetPos = transform.position;
        camOffset = transform.position;
    }

    private void OnEnable()
    {
        BlockPerfectLandEvent.OnEventTriggered += SetTargetPositionFromBlock;
        BlockSuccessfulLandEvent.OnEventTriggered += SetTargetPositionFromBlock;
        BlockFailedLandEvent.OnEventTriggered += SetTargetPositionFromBlock;
    }
    private void OnDisable()
    {
        BlockPerfectLandEvent.OnEventTriggered -= SetTargetPositionFromBlock;
        BlockSuccessfulLandEvent.OnEventTriggered -= SetTargetPositionFromBlock;
        BlockFailedLandEvent.OnEventTriggered -= SetTargetPositionFromBlock;
    }
    void Update()
    {
        if (targetPos != transform.position)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * SPEED);
        }
        wallMaterial.mainTextureOffset = new Vector2(0, transform.position.y) / wallMaterial.mainTextureScale.y;
    }

    void SetTargetPositionFromBlock(Block b)
    {
        targetPos = new Vector3(0, b.transform.position.y, 0) + new Vector3(0, b.size.y / 2, 0) + camOffset;
    }
}
