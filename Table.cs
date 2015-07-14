using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{

  public class Column
  {
    public string Text;
    public int Width;
    public bool AlignRight = true;
    public List<string> Rows = new List<string>();
  }

  public class Table
  {
    private string mTable = string.Empty;
		private List<Column> mColumns = new List<Column>();
    private string mDivider = string.Empty;
    private int mColumnIndex = 0;

    public Table(params string[] headers)
    {
      CreateTable(headers);
    }

    private void CreateTable(params string[] headers)
    {
      foreach(string header in headers)
      {
        AddHeader(header);
      }
    }

    public void AddHeader(string header)
    {
        Column new_column = new Column();
        new_column.Text = header;
        mColumns.Add(new_column);
    }

    public void AddCell(string text)
    {
      Column column = mColumns[mColumnIndex++];
      column.Rows.Add(text);
      if (mColumnIndex == mColumns.Count)
      {
        mColumnIndex = 0;
      }
    }

    public void AddCell(float text)
    {
      AddCell(Math.Round(text, 2).ToString());
    }

    public void AddCell(Currency text)
    {
      AddCell(text.ToString());
    }

    public void AddCell(DateTime text)
    {
      AddCell(text.ToString("dd-MM-yyyy"));
    }


    public override string ToString()
    {
      foreach (Column column in mColumns)
      {
        column.Width = Math.Max(column.Text.Length + 2, column.Rows.Max(x => x.Length + 2));
      }
      string output = string.Empty;
      foreach (Column column in mColumns)
      {
        output += "|" + CenterText(column, column.Text);
      }
      output += "|";
      string divider = "+" + new string('-', output.Length - 2) + "+" + System.Environment.NewLine;
      output = divider + output + System.Environment.NewLine + divider;

      int row_index = 0;
      while (row_index < mColumns[0].Rows.Count)
      {         
        foreach (var column in mColumns)
        {
          output += FillCell(column.Rows[row_index], column);
        }
        row_index++;
        output += "|" + System.Environment.NewLine;
      }
      output += divider;
      return output;
    }

		private string CenterText(Column column, string text)
		{
			int column_width = column.Width;
			int whitespace = column_width - text.Length;
			int half = whitespace / 2;
			int filler = 0;
			if ((half + half + text.Length) < column_width)
				filler++;
			return new string(' ', half) + text + new string(' ', half + filler);
		}

		private string FillCell(string text, Column column)
		{
			if (column.AlignRight == true)
				return "|" + new string(' ', column.Width - text.Length) + text;
			else
				return "|" + text + new string(' ', column.Width - text.Length);
		}
  }
}
