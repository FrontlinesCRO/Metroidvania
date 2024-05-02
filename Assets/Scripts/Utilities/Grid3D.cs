using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class Grid3D<T> where T : new()
    {
        public class Cell<V> where V : new()
        {
            private Vector3 _min;
            private Vector3 _max;
            private Vector2 _size;
            private V _cellObject;
            private bool _isOccupied;

            public Vector3 Min => _min;
            public Vector3 Max => _max;
            public Vector2 Size => _size;
            public V CellObject => _cellObject;
            public bool IsOccupied => _isOccupied;

            public Cell(Vector3 min, Vector3 max, Vector2 size)
            {
                _min = min;
                _max = max;
                _size = size;
                _cellObject = default;
                _isOccupied = false;
            }

            public void SetMinMax(Vector3 min, Vector3 max)
            {
                _min = min;
                _max = max;
                _size = (_max - _min).Abs();
            }

            public void SetMinMax(Vector3 min, Vector3 max, Vector2 size)
            {
                _min = min;
                _max = max;
                _size = size;
            }

            public void SetCellObject(V cellObject)
            {
                _cellObject = cellObject;
                _isOccupied = _cellObject != null;
            }
        }

        private int _columns;
        private int _rows;
        private Vector3 _gridRoot;
        private Vector2 _cellSize;
        private Cell<T>[,] _cells;

        public int Columns => _columns;
        public int Rows => _rows;
        public float Height { get; private set; }
        public float Width { get; private set; }

        public Cell<T> this[int column, int row]
        {
            get => GetCell(column, row);
        }

        public Grid3D(int columns, int rows, Vector3 gridRoot, Vector2 cellSize)
        {
            _gridRoot = gridRoot;
            _cellSize = cellSize;

            GenerateGrid(columns, rows);
        }

        private void CopyCells(Cell<T>[,] from, Cell<T>[,] to)
        {
            var columns = Mathf.Min(from.GetLength(0), to.GetLength(0));
            var rows = Mathf.Min(from.GetLength(1), to.GetLength(1));

            for (var i = 0; i < columns; i++)
            {
                for (var j = 0; j < rows; j++)
                {
                    to[i, j] = from[i, j];
                }
            }
        }

        public void GenerateGrid(int columns, int rows)
        {
            if (columns == _columns && rows == _rows)
                return;

            if (_cells != null)
            {
                if (columns > _columns || rows > _rows)
                {
                    var newCells = new Cell<T>[columns, rows];
                    CopyCells(_cells, newCells);
                    _cells = newCells;
                }

                _columns = columns;
                _rows = rows;

                Height = rows * _cellSize.y;
                Width = columns * _cellSize.x;
                return;
            }

            _columns = columns;
            _rows = rows;
            _cells = new Cell<T>[columns, rows];

            Height = rows * _cellSize.y;
            Width = columns * _cellSize.x;

            for (var i = 0; i < _columns; i++)
            {
                for (var j= 0; j < _rows; j++)
                {
                    var min = _gridRoot + new Vector3(i * _cellSize.x, j * _cellSize.y, 0f);
                    var max = _gridRoot + new Vector3((i + 1) * _cellSize.x, (j + 1) * _cellSize.y, 0f);

                    _cells[i, j] = new Cell<T>(min, max, _cellSize);
                }
            }
        }

        public void SetGridRootAndCellSize(Vector3 gridRoot, Vector2 cellSize)
        {
            _gridRoot = gridRoot;
            _cellSize = cellSize;

            Height = _rows * _cellSize.y;
            Width = _columns * _cellSize.x;

            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var min = _gridRoot + new Vector3(i * _cellSize.x, j * _cellSize.y, 0f);
                    var max = _gridRoot + new Vector3((i + 1) * _cellSize.x, (j + 1) * _cellSize.y, 0f);

                    var cell = GetCell(i, j);
                    cell.SetMinMax(min, max);
                }
            }
        }

        public void SetCellSize(Vector2 cellSize)
        {
            _cellSize = cellSize;

            Height = _rows * _cellSize.y;
            Width = _columns * _cellSize.x;

            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var min = _gridRoot + new Vector3(i * _cellSize.x, j * _cellSize.y, 0f);
                    var max = _gridRoot + new Vector3((i + 1) * _cellSize.x, (j + 1) * _cellSize.y, 0f);

                    var cell = GetCell(i, j);
                    cell.SetMinMax(min, max, cellSize);
                }
            }
        }

        public void SetGridRoot(Vector3 gridRoot)
        {
            _gridRoot = gridRoot;

            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var min = _gridRoot + new Vector3(i * _cellSize.x, j * _cellSize.y, 0f);
                    var max = _gridRoot + new Vector3((i + 1) * _cellSize.x, (j + 1) * _cellSize.y, 0f);

                    var cell = GetCell(i, j);
                    cell.SetMinMax(min, max);
                }
            }
        }

        public Cell<T> GetCell(int column, int row)
        {
            if (column < 0 || column >= _columns)
            {
                Debug.LogError("Column index out of range.");
                return default;
            }
            else if (row < 0 || row >= _rows)
            {
                Debug.LogError("Row index out of range.");
                return default;
            }

            var cell = _cells[column, row];
            if (cell == null)
            {
                var min = _gridRoot + new Vector3(column * _cellSize.x, row * _cellSize.y, 0f);
                var max = _gridRoot + new Vector3((column + 1) * _cellSize.x, (row + 1) * _cellSize.y, 0f);

                cell = new Cell<T>(min, max, _cellSize);
                _cells[column, row] = cell;
            }

            return cell;
        }

        public Cell<T> GetMinCell()
        {
            return _cells[0, 0];
        }

        public Cell<T> GetMaxCell()
        {
            return _cells[_columns - 1, _rows - 1];
        }

        public Cell<T> GetFirstUnoccupiedCell()
        {
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var cell = GetCell(i, j);

                    if (!cell.IsOccupied)
                        return cell;
                }
            }

            return default;
        }

        public Cell<T> GetFirstUnoccupiedCell(int minColumIgnoreIndex, int maxColumnIgnoreIndex, int minRowIgnoreIndex, int maxRowIgnoreIndex)
        {
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var cell = GetCell(i, j);

                    var isWithinColumnIgnoreRange = i >= minColumIgnoreIndex && i <= maxColumnIgnoreIndex;
                    var isWithinRowIgnoreRange = j >= minRowIgnoreIndex && j <= maxRowIgnoreIndex;
                    if (isWithinColumnIgnoreRange && isWithinRowIgnoreRange)
                        continue;

                    if (!cell.IsOccupied)
                        return cell;
                }
            }

            return default;
        }

        public void Clear()
        {
            // unoccupies all the cells in the grid
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var cell = GetCell(i, j);
                    cell.SetCellObject(default);
                }
            }
        }
    }
}
