using System.Diagnostics;
using Unity.Collections;
using UnityEngine;

public static class GlobalUserData
{
    public static string userName;
    public static string currentEmotion;
    public static Vector3 currentSpawnPoint;
}

public class ServerError
{
    public string message;
}

public static class GenerationProgressTracker
{
    // Static properties to track the progress
    public static int TotalCells { get; private set; }
    public static int CellsCollapsed { get; private set; }
    public static float Progress => (float)CellsCollapsed / TotalCells * 100;

    // Method to initialize progress tracking
    public static void Initialize(int totalCells)
    {
        TotalCells = totalCells;
        CellsCollapsed = 0;
    }

    // Method to update progress (call this every time a cell is collapsed)
    public static void UpdateProgress()
    {
        CellsCollapsed++;

    }

    // Method to reset progress
    public static void ResetProgress()
    {
        CellsCollapsed = 0;
    }

    public static bool IsComplete()
    {
        return CellsCollapsed >= TotalCells;
    }
} 
