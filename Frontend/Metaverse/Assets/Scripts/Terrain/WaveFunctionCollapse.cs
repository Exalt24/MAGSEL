using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    public int dimensions;
    public SkyboxController skyboxController;
    private System.Random rng;
    public Tile[] sadnessTileObjects;
    public Tile[] joyTileObjects;
    public Tile[] loveTileObjects;
    public Tile[] angerTileObjects;
    public Tile[] fearTileObjects;
    public Tile[] surpriseTileObjects;
    public Tile sadnessBackupTile;
    public Tile joyBackupTile;
    public Tile loveBackupTile;
    public Tile angerBackupTile;
    public Tile fearBackupTile;
    public Tile surpriseBackupTile;
    private Tile[] tileObjects;

    public List<Cell> gridComponents;
    public Cell cellObj;
    private Tile backupTile;
    private int iteration;
    private float initStartTime;
    private int cellsCollapsed; // Added to track number of cells collapsed

    private void Awake()
    {
        // Optionally, initialize gridComponents here if needed.
        gridComponents = new List<Cell>();
    }

    public void InitializeTerrain(int seed, string emotion)
    {
        initStartTime = Time.realtimeSinceStartup;
        cellsCollapsed = 0; // Reset counter
        
        Debug.Log($"[Benchmarking] Starting terrain generation at {initStartTime}");
        // Stop any running coroutines so they don't try to access old/destroyed cells.
        StopAllCoroutines();

        rng = new System.Random(seed);

        // Clear out any previous grid data.
        gridComponents = new List<Cell>();
        
        GameManager.isInputEnabled = false;
        InitializeForEmotion(emotion);
        
        if (skyboxController != null)
            skyboxController.UpdateSkyboxForEmotion(emotion);
            
        float initPhaseTime = (Time.realtimeSinceStartup - initStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Initialization phase completed in {initPhaseTime:F1} ms");
    }

    public void InitializeForEmotion(string emotion)
    {
        float phaseStartTime = Time.realtimeSinceStartup;
        
        // Get or create the parent object for generated cells.
        Transform worldParent = GameObject.Find("GeneratedWorld")?.transform;
        if (worldParent == null)
        {
            worldParent = new GameObject("GeneratedWorld").transform;
        }
        worldParent.SetSiblingIndex(0);

        // Destroy all children (old cells) of the GeneratedWorld.
        foreach (Transform child in worldParent)
        {
            Destroy(child.gameObject);
        }
        
        // Reset gridComponents again to ensure no old references remain.
        gridComponents = new List<Cell>();

        Debug.Log($"Initializing grid for emotion: {emotion}");

        switch (emotion.ToLower())
        {
            case "sadness":
                tileObjects = sadnessTileObjects;
                backupTile = sadnessBackupTile;
                break;
            case "joy":
                tileObjects = joyTileObjects;
                backupTile = joyBackupTile;
                break;
            case "love":
                tileObjects = loveTileObjects;
                backupTile = loveBackupTile;
                break;
            case "anger":
                tileObjects = fearTileObjects;
                backupTile = fearBackupTile;
                break;
            case "fear":
                tileObjects = angerTileObjects;
                backupTile = angerBackupTile;
                break;
            case "surprise":
                tileObjects = surpriseTileObjects;
                backupTile = surpriseBackupTile;
                break;
            default:
                Debug.LogWarning($"Unknown emotion: {emotion}. Using default joy tiles.");
                tileObjects = joyTileObjects;
                backupTile = joyBackupTile;
                break;
        }

        if (tileObjects == null || tileObjects.Length == 0)
        {
            Debug.LogError("tileObjects is NULL or EMPTY! Cannot generate terrain.");
            return;
        }

        float emotionSetupTime = (Time.realtimeSinceStartup - phaseStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Emotion setup completed in {emotionSetupTime:F1} ms");
        
        InitializeGrid(worldParent);
    }

    void InitializeGrid(Transform worldParent)
    {
        float gridStartTime = Time.realtimeSinceStartup;
        
        iteration = 0;
        // Clear the list in case it has any remnants.
        gridComponents.Clear();

        // Use the scaling factor from the first tile to adjust spacing.
        float tileSize = tileObjects[0].scalingFactor;
        GenerationProgressTracker.Initialize(dimensions * dimensions);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                // Adjust the cell position based on tileSize.
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);
                Cell newCell = Instantiate(cellObj, position, Quaternion.identity, worldParent);
                newCell.name = $"Cell_{x}_{y}";
                // Pass the coordinates to CreateCell.
                newCell.CreateCell(false, tileObjects, x, y);
                gridComponents.Add(newCell);
            }
        }

        float gridCreationTime = (Time.realtimeSinceStartup - gridStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Grid creation completed in {gridCreationTime:F1} ms for {dimensions}x{dimensions} grid");
        
        StartCoroutine(CheckEntropy());
    }

    IEnumerator CheckEntropy()
    {
        float iterationStartTime = Time.realtimeSinceStartup;
        
        // Create a copy of gridComponents and filter out any null references.
        List<Cell> tempGrid = gridComponents.Where(c => c != null && !c.collapsed).ToList();
        if (tempGrid.Count == 0)
        {
            float totalMs = (Time.realtimeSinceStartup - initStartTime) * 1000f;
            Debug.Log($"[Benchmarking] Generation complete in {totalMs:F1} ms ({cellsCollapsed}/{dimensions * dimensions} cells)");
            GameManager.isInputEnabled = true;
            yield break; // No valid cells to process.
        }
        tempGrid.Sort((a, b) => a.tileOptions.Length - b.tileOptions.Length);
        tempGrid.RemoveAll(a => a.tileOptions.Length != tempGrid[0].tileOptions.Length);

        float entropyCalcTime = (Time.realtimeSinceStartup - iterationStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Entropy calculation for iteration {iteration} took {entropyCalcTime:F1} ms");

        yield return new WaitForSeconds(0.025f);

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {
        float collapseStartTime = Time.realtimeSinceStartup;
        
        // Filter for edge cells.
        List<Cell> edgeCells = tempGrid.Where(c => c != null && 
            (c.cellX == 0 || c.cellX == dimensions - 1 || c.cellY == 0 || c.cellY == dimensions - 1)).ToList();

        Cell cellToCollapse = null;
        if(edgeCells.Count > 0)
        {
            // If there are any edge cells, pick one at random from those.
            int randIndex = rng.Next(0, edgeCells.Count);
            cellToCollapse = edgeCells[randIndex];
        }
        else if (tempGrid.Count > 0)
        {
            // Otherwise, pick from all available cells.
            int randIndex = rng.Next(0, tempGrid.Count);
            cellToCollapse = tempGrid[randIndex];
        }
        else
        {
            Debug.LogWarning("No valid cells available to collapse.");
            return;
        }
        
        // Ensure the cell hasn't already been destroyed.
        if (cellToCollapse == null)
        {
            Debug.LogWarning("Selected cell is null, skipping collapse.");
            return;
        }

        cellToCollapse.collapsed = true;
        cellsCollapsed++; // Increment counter

        // If the cell is on the border, force the backup tile.
        if (cellToCollapse.cellX == 0 || cellToCollapse.cellX == dimensions - 1 ||
            cellToCollapse.cellY == 0 || cellToCollapse.cellY == dimensions - 1)
        {
            cellToCollapse.tileOptions = new Tile[] { backupTile };
        }
        else
        {
            try
            {
                Tile selectedTile = cellToCollapse.tileOptions[rng.Next(0, cellToCollapse.tileOptions.Length)];
                cellToCollapse.tileOptions = new Tile[] { selectedTile };
            }
            catch
            {
                cellToCollapse.tileOptions = new Tile[] { backupTile };
            }
        }

        Tile foundTile = cellToCollapse.tileOptions[0];

        Transform worldParent = GameObject.Find("GeneratedWorld")?.transform;
        if (worldParent == null)
        {
            worldParent = new GameObject("GeneratedWorld").transform;
        }

        // Instantiate the tile at the cell's position.
        GameObject instantiatedTile = Instantiate(foundTile.gameObject, cellToCollapse.transform.position, foundTile.transform.rotation, worldParent);

        // Apply the scaling factor.
        instantiatedTile.transform.localScale = Vector3.one * foundTile.scalingFactor;
        cellToCollapse.transform.localScale = Vector3.one * foundTile.scalingFactor;

        float singleCollapseTime = (Time.realtimeSinceStartup - collapseStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Cell collapse {cellsCollapsed}/{dimensions * dimensions} took {singleCollapseTime:F1} ms");

        UpdateGeneration();

        GenerationProgressTracker.UpdateProgress();
    }

    void UpdateGeneration()
    {
        float updateStartTime = Time.realtimeSinceStartup;
        
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;

                if (gridComponents[index].collapsed)
                {
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>(tileObjects);

                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].upNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (x < dimensions - 1)
                    {
                        Cell left = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].leftNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].downNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    if (x > 0)
                    {
                        Cell right = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var validOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[validOption].rightNeighbours;
                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = options.ToArray();
                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;
        iteration++;

        float updateTime = (Time.realtimeSinceStartup - updateStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Generation update for iteration {iteration} took {updateTime:F1} ms");
        
        // Report progress statistics
        float elapsedMs = (Time.realtimeSinceStartup - initStartTime) * 1000f;
        Debug.Log($"[Benchmarking] Progress: {cellsCollapsed}/{dimensions * dimensions} cells ({(cellsCollapsed * 100f / (dimensions * dimensions)):F1}%) in {elapsedMs:F1} ms");

        if (iteration < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }
        else
        {
            // Final completion report
            float totalMs = (Time.realtimeSinceStartup - initStartTime) * 1000f;
            Debug.Log($"[Benchmarking] Generation complete in {totalMs:F1} ms ({cellsCollapsed}/{dimensions * dimensions} cells)");
            GameManager.isInputEnabled = true;
        }
    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            if (!validOption.Contains(optionList[x]))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}