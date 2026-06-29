using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour
{
    [SerializeField] GameConfig gameConfig;
    [SerializeField] private GameObject SpawnerParent;
    [SerializeField] private GameObject TowerParent;
    [SerializeField] private Block BlockPrefab;

    private Stack<Block> TowerBlocks;
    private Block hookedBlock;
    private float topBlockCenter;
    private float topBlockWidth;
    private float towerHeight;

    private Vector3[] WobblePoints;
    private int goingToPointIndex = 0;
    private float wobbleIntensity = 0;
    private float wobbleSpeed = 0;
    private float totalOffset;
    private List<float> lastBlocksOffset;

    // Sugestion: este manager concentra logica de spawn, deteccion de aterrizaje, wobble y reproduccion de SFX; podria separarse (p.ej. un componente de audio que escuche los event channels, como ya hace UIManager) para bajar el acoplamiento.
    [Header("Sounds")]
    [SerializeField] private AudioSource BlockLandSFX;
    [SerializeField] private AudioSource BlockPerfectSFX;
    [SerializeField] private AudioSource BlockFailSFX;
    [SerializeField] private AudioSource BlockMissSFX;

    [Header("Broadcast Events")]
    [SerializeField] private BlockEventChannel BlockSuccessfulLandEvent;
    [SerializeField] private BlockEventChannel BlockPerfectLandEvent;
    [SerializeField] private BlockEventChannel BlockFailedLandEvent;
    [SerializeField] private BlockEventChannel BlockMissedLandEvent;

    [Header("Listener Events")]
    [SerializeField] private EventChannel BlockCreateEvent;
    [SerializeField] private EventChannel BlockDropEvent;
    [SerializeField] private BlockEventChannel BlockLandEvent;

    private void Awake()
    {
        TowerBlocks = new Stack<Block>();
        WobblePoints = new Vector3[2];
        WobblePoints[0] = Vector3.zero;
        WobblePoints[1] = Vector3.zero;
        lastBlocksOffset = new List<float>();
    }
    private void OnEnable()
    {
        BlockCreateEvent.OnEventTriggered += CreateBlock;
        BlockDropEvent.OnEventTriggered += DropBlock;
        BlockLandEvent.OnEventTriggered += LandBlock;
    }
    private void OnDisable()
    {
        BlockCreateEvent.OnEventTriggered -= CreateBlock;
        BlockDropEvent.OnEventTriggered -= DropBlock;
        BlockLandEvent.OnEventTriggered -= LandBlock;
    }

    private void Update()
    {
        TowerParent.transform.position = Vector3.MoveTowards(TowerParent.transform.position, WobblePoints[goingToPointIndex], Time.deltaTime * wobbleSpeed);
        // Warning: comparar floats con == (position.x == WobblePoints.x) es fragil por la precision de punto flotante; MoveTowards garantiza llegar exacto al objetivo, pero si wobbleSpeed es 0 nunca se cumple. Mejor usar Mathf.Approximately o comparar la distancia con un epsilon.
        if (TowerParent.transform.position.x == WobblePoints[goingToPointIndex].x)
            goingToPointIndex = goingToPointIndex == 0 ? 1 : 0;
    }

    private void CreateBlock()
    {
        if (!hookedBlock)
        {
            hookedBlock = Instantiate(BlockPrefab, SpawnerParent.transform);
            hookedBlock.transform.position -= new Vector3(0, hookedBlock.size.y / 2, 0);
        }
    }

    private void DropBlock()
    {
        if (hookedBlock)
        {
            hookedBlock.transform.SetParent(transform);
            hookedBlock.Drop();
        }
    }

    private void LandBlock(Block b)
    {
        if (TowerBlocks.Count > 0)
            SetTopBlockValues();

        if (hookedBlock != b)
            Destroy(hookedBlock.gameObject);
        hookedBlock = null;

        float absOffset = Mathf.Abs(topBlockCenter - b.transform.position.x);


        if (TowerBlocks.Count < 1)
        {
            b.Freeze();
            AddBlockToTower(b);

            BlockSuccessfulLandEvent.RaiseEvent(b);
            BlockLandSFX.Play();
        }
        else if (absOffset < gameConfig.PerfectOffset)
        {
            b.Freeze();
            AddBlockToTower(b);

            b.transform.position = new Vector3(topBlockCenter, b.transform.position.y, b.transform.position.z);
            BlockPerfectLandEvent.RaiseEvent(b);
            BlockPerfectSFX.Play();
        }
        else if (absOffset < topBlockWidth / 2)
        {
            b.Freeze();
            AddBlockToTower(b);

            BlockSuccessfulLandEvent.RaiseEvent(b);
            BlockLandSFX.Play();
        }
        else if (absOffset > topBlockWidth)
        {
            b.Fall();
            Destroy(b.gameObject, 2f);

            BlockMissedLandEvent.RaiseEvent(b);
            BlockMissSFX.Play();
        }
        else
        {
            Destroy(b.gameObject);
            if (TowerBlocks.Count > 1)
                RemoveBlockFromTower();

            BlockFailedLandEvent.RaiseEvent(TowerBlocks.Peek());
            BlockFailSFX.Play();
        }
        SetTowerWobble();

        CreateBlock();
    }
    private void SetTopBlockValues()
    {
        topBlockCenter = TowerBlocks.Peek().transform.position.x;
        topBlockWidth = TowerBlocks.Peek().size.x;
    }
    private void AddBlockToTower(Block b)
    {
        TowerBlocks.Push(b);
        b.transform.SetParent(TowerParent.transform);

        lastBlocksOffset.Add(Mathf.Abs(topBlockCenter - b.transform.position.x));
        if (lastBlocksOffset.Count > gameConfig.BlocksCountedForWobble + gameConfig.InitialLives)
            lastBlocksOffset.RemoveAt(0);

        towerHeight++;
    }

    private void RemoveBlockFromTower()
    {
        Destroy(TowerBlocks.Pop().gameObject);
        // Error: List<float>.Remove elimina por VALOR, no por indice. Aca estas intentando borrar el ultimo elemento pero le pasas el indice (Count-1) como si fuera un valor float a buscar; borra un offset que probablemente no exista. Deberia ser lastBlocksOffset.RemoveAt(lastBlocksOffset.Count - 1).
        lastBlocksOffset.Remove(lastBlocksOffset.Count - 1);
        towerHeight--;
    }

    private void SetTowerWobble()
    {
        totalOffset = 0;
        for (int i = 0; i < gameConfig.BlocksCountedForWobble; i++)
        {
            int index = lastBlocksOffset.Count - 1 - i;
            if (index >= 0)
                totalOffset += lastBlocksOffset[index];
            else
                break;
        }

        wobbleIntensity = Mathf.Clamp((towerHeight - 1) * gameConfig.WobbleScale * totalOffset, 0, gameConfig.TowerMaxWobble);
        wobbleSpeed = Mathf.Clamp(wobbleIntensity, 0, gameConfig.MaxWobbleSpeed);

        WobblePoints[0].x = wobbleIntensity;
        WobblePoints[1].x = -wobbleIntensity;
    }
}
