using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public Tile[] tileOptions;
    public int cellX;  // grid X coordinate
    public int cellY;  // grid Y coordinate

    // Now include x and y coordinates when creating the cell.
    public void CreateCell(bool collapseState, Tile[] tiles, int x, int y)
    {
        collapsed = collapseState;
        tileOptions = tiles;
        cellX = x;
        cellY = y;
    }

    public void RecreateCell(Tile[] tiles)
    {
        tileOptions = tiles;
    }
}
