using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Dynamic Square Grid Layout Group")]
public class DynamicSquareGridLayoutGroup : LayoutGroup
{
    [SerializeField] private Vector2 cellSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 spacing = new Vector2(10f, 10f);

    public Vector2 CellSize
    {
        get => cellSize;
        set
        {
            cellSize = value;
            SetDirty();
        }
    }

    public Vector2 Spacing
    {
        get => spacing;
        set
        {
            spacing = value;
            SetDirty();
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        int gridSize = GetGridSize();

        float width =
            padding.horizontal +
            gridSize * cellSize.x +
            Mathf.Max(0, gridSize - 1) * spacing.x;

        SetLayoutInputForAxis(width, width, -1, 0);
    }

    public override void CalculateLayoutInputVertical()
    {
        int gridSize = GetGridSize();

        float height =
            padding.vertical +
            gridSize * cellSize.y +
            Mathf.Max(0, gridSize - 1) * spacing.y;

        SetLayoutInputForAxis(height, height, -1, 1);
    }

    public override void SetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    public override void SetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    private int GetGridSize()
    {
        if (rectChildren.Count == 0)
            return 0;

        return Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
    }

    private void SetCellsAlongAxis(int axis)
    {
        int childCount = rectChildren.Count;

        if (childCount == 0)
            return;

        int gridSize = GetGridSize();

        float gridWidth =
            gridSize * cellSize.x +
            Mathf.Max(0, gridSize - 1) * spacing.x;

        float gridHeight =
            gridSize * cellSize.y +
            Mathf.Max(0, gridSize - 1) * spacing.y;

        float startX = GetStartOffset(0, gridWidth);
        float startY = GetStartOffset(1, gridHeight);

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = rectChildren[i];

            int column = i % gridSize;
            int row = i / gridSize;

            if (axis == 0)
            {
                float x = startX + column * (cellSize.x + spacing.x);

                SetChildAlongAxis(
                    child,
                    0,
                    x,
                    cellSize.x
                );
            }
            else
            {
                float y = startY + row * (cellSize.y + spacing.y);

                SetChildAlongAxis(
                    child,
                    1,
                    y,
                    cellSize.y
                );
            }
        }
    }
}