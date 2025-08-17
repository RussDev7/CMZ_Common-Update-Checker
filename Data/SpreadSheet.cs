using System;
using System.Collections.Generic;

namespace DNA.Data
{
	public class SpreadSheet<T>
	{
		public List<SpreadSheet<T>.Page> Pages = new List<SpreadSheet<T>.Page>();

		public class Page
		{
			public T this[int row, int column]
			{
				get
				{
					return this.Cells[row, column];
				}
				set
				{
					this.Cells[row, column] = value;
				}
			}

			public int RowCount
			{
				get
				{
					return this.Cells.GetLength(0);
				}
			}

			public int ColumnCount
			{
				get
				{
					return this.Cells.GetLength(1);
				}
			}

			public Page()
			{
			}

			public Page(int rows, int cols)
			{
				this.Init(rows, cols);
			}

			public void Init(int rows, int columns)
			{
				this.Cells = new T[rows, columns];
			}

			private T[,] Cells = new T[0, 0];
		}
	}
}
