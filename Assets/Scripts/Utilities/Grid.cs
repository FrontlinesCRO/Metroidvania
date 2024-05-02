using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public class Grid<T> where T : new()
    {
        public class Cell<V> where V : new()
        {
            private V _cellObject;
            private bool _isOccupied;

            public V CellObject => _cellObject;
            public bool IsOccupied => _isOccupied;

            public Cell()
            {
                _cellObject = default;
                _isOccupied = false;
            }

            public void Set(V cellObject)
            {
                _cellObject = cellObject;
                _isOccupied = _cellObject != null;
            }
        }

        private int _columns;
        private int _rows;
        private Cell<T>[,] _cells;

        public int Columns => _columns;
        public int Rows => _rows;

        public Cell<T> this[int column, int row]
        {
            get => GetCell(column, row);
        }

        public Grid(int columns, int rows)
        {
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
                return;
            }

            _columns = columns;
            _rows = rows;
            _cells = new Cell<T>[columns, rows];

            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    _cells[i, j] = new Cell<T>();
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
                cell = new Cell<T>();
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

        public Cell<T> GetFirstFreeCell()
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

        public Cell<T> GetFirstFreeCell(int minColumIgnoreIndex, int maxColumnIgnoreIndex, int minRowIgnoreIndex, int maxRowIgnoreIndex)
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
            // frees up all the cells in the grid
            for (var i = 0; i < _columns; i++)
            {
                for (var j = 0; j < _rows; j++)
                {
                    var cell = GetCell(i, j);
                    cell.Set(default);
                }
            }
        }
    }
}
