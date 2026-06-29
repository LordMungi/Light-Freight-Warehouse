using System.Collections.Generic;
using UnityEngine;

public class RackSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] RackPrefabs;

    [SerializeField] Transform parent;
    [SerializeField] Renderer firstRackMesh;
    [SerializeField] int racksPerRow = 5;
    [SerializeField] int maxActiveRows = 4;
    [SerializeField] int racksUntilDeletion = 2;
    [SerializeField] float horizontalSpacing = 1f;
    [SerializeField] float verticalSpacing = 1f;

    [Header("Listener Events")]
    [SerializeField] private BlockEventChannel BlockLandEvent; 

    private Queue<GameObject> racks;
    private Vector3 rackSize;
    private int row = 0;

    private void Awake()
    {
        racks = new Queue<GameObject>();
        rackSize = firstRackMesh.bounds.size;

        for (int i = 0; i < maxActiveRows; i++)
           CreateRow(); 
    }
    private void OnEnable()
    {
        BlockLandEvent.OnEventTriggered += CheckForRowcreation;
    }
    private void OnDisable()
    {
        BlockLandEvent.OnEventTriggered -= CheckForRowcreation;
    }

    private void OnDrawGizmos()
    {
        if (rackSize == Vector3.zero)
            rackSize = firstRackMesh.bounds.size;
        Vector3 drawPos = parent.transform.position + rackSize / 2;

        Gizmos.color = new Color(0, 0, 1, 0.3f);
        for (int i = 0; i < racksPerRow; i++)
        {
            Gizmos.DrawCube( drawPos + new Vector3((rackSize.x + horizontalSpacing) * i, 0, 0), rackSize);
            Gizmos.DrawCube(drawPos + new Vector3((rackSize.x + horizontalSpacing) * i, (rackSize.y + verticalSpacing), 0), rackSize);
        }
    }
    void CreateRow()
    {
        for (int i = 0; i < racksPerRow; i++)
        {
            // Sugestion: RackPrefabs es un array pero siempre se usa el indice [0]; o aprovechas la variedad (p.ej. Random.Range para distintos modelos) o lo reemplazas por un unico campo GameObject.
            GameObject newRack = Instantiate(RackPrefabs[0], parent.transform);
            newRack.transform.position += new Vector3((rackSize.x + horizontalSpacing) * i, (rackSize.y + verticalSpacing) * row, 0);
            racks.Enqueue(newRack);
        }
        row++;

        if (row > maxActiveRows)
        {
            for (int i = 0; i < racksPerRow; i++)
            {
                Destroy(racks.Dequeue().gameObject);
            }
        }
    }

    private void CheckForRowcreation(Block b)
    {
        // Warning: racks.Peek() sin chequear que la Queue tenga elementos; si quedara vacia lanza InvalidOperationException. Conviene validar racks.Count > 0.
        if (b.transform.position.y > racks.Peek().transform.position.y + rackSize.y * racksUntilDeletion)
            CreateRow();
    }
}
