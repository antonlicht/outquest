using UnityEngine;

public class RenderGrid
{
    private Grid<RenderCell> _grid;

    public RefCountedSprite this[int x, int y]
    {
        get
        {
            RenderCell cell = _grid[x, y];
            if (Equals(cell, default(RenderCell)))
                return null;
            return cell.GetContent();
        }
        set
        {
            RenderCell cell = _grid[x, y];
            if (Equals(cell, default(RenderCell)))
                return;
            cell.SetContent(value);
        }
    }

    public RenderGrid(int width, int height)
    {
        _grid = new Grid<RenderCell>(width, height);
    }

    public RenderGrid(RenderCell[] cells, int width, int height)
    {
        _grid = new Grid<RenderCell>(cells, width, height);
    }

    public void Shift(int deltaX, int deltaY)
    {
        Grid<RefCountedSprite> contentGrid = Grid<RefCountedSprite>.CreateEmpty(_grid);
        contentGrid.SetIDOffset(_grid.IDOffsetX, _grid.IDOffsetY);

        for (int i = contentGrid.IDOffsetY; i < contentGrid.IDOffsetY + contentGrid.Height; i++)
        {

            for (int j = contentGrid.IDOffsetX; j < contentGrid.IDOffsetX + contentGrid.Width; j++)
            {
                RefCountedSprite temp = _grid[j, i].GetContent();
                if (temp != null)
                    temp.AddReference();
                contentGrid[j, i] = temp;
            }
        }
        _grid.ShiftIDs(-deltaX, -deltaY);
        for (int i = _grid.IDOffsetY; i < _grid.IDOffsetY + _grid.Height; i++)
        {

            for (int j = _grid.IDOffsetX; j < _grid.IDOffsetX + _grid.Width; j++)
            {
                if (_grid[j, i] is Object)
                    (_grid[j, i] as Object).name = "cell_" + j + "_" + i;
                _grid[j, i].SetContent(contentGrid[j, i]);
            }
        }

        for (int i = contentGrid.IDOffsetY; i < contentGrid.IDOffsetY + contentGrid.Height; i++)
        {

            for (int j = contentGrid.IDOffsetX; j < contentGrid.IDOffsetX + contentGrid.Width; j++)
            {
                RefCountedSprite temp = contentGrid[j, i];
                if (temp != null)
                    temp.Release();
            }
        }

    }

    public int Width { get { return _grid.Width; } }
    public int Height { get { return _grid.Height; } }

    public int OffsetX { get { return _grid.IDOffsetX; } }
    public int OffsetY { get { return _grid.IDOffsetY; } }

}
