using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{
public enum Direction
    {
      Up,
      Down
    }

    public class Trend
    {
      public DateTime Start {get;set;}
      public DateTime End {get;set;}
      public float Change {get;set;}
      public Direction Direction {get;set;}
      public float StartingPrice {get;set;}

      public Trend()
      {
      }
    }


  public class Trends
  {
    public static void PrintTrendActivityForFund(DateTime endDate, OwnedFund fund, int overDays, float fluctuationAllowed)
    {
      List<Trend> trends = RetrieveTrends(endDate, fund, overDays, fluctuationAllowed);
      Console.WriteLine("Trends for " + fund.Symbol);
      int index = 1;
      Table table = new Table("", "Start", "End", "Direction", "Change ( >" + fluctuationAllowed + "% )");
      foreach (Trend trend in trends)
      {
        table.AddCell("Trend " + index++);
        table.AddCell(trend.Start);
        table.AddCell(trend.End);
        table.AddCell(trend.Direction.ToString());
        table.AddCell(trend.Change);
      }
      Console.WriteLine(table);
    }

    public static void PrintTrendActivityForAllFunds(DateTime endDate, List<OwnedFund> funds, int overDays, float fluctuationAllowed)
    {
      Console.WriteLine("Trends");
      Table table = new Table("Symbol", "Start", "End", "Direction", "Change ( >" + fluctuationAllowed + "% )");
      foreach (var fund in funds)
      { 
        List<Trend> trends = RetrieveTrends(endDate, fund, overDays, fluctuationAllowed);
        foreach (Trend trend in trends)
        {
          table.AddCell(fund.Symbol);
          table.AddCell(trend.Start);
          table.AddCell(trend.End);
          table.AddCell(trend.Direction.ToString());
          table.AddCell(trend.Change);
        }
      }
      Console.WriteLine(table);
    }

    public static void PrintLatestTrendActivityForAllFunds(DateTime endDate, List<OwnedFund> funds, int overDays, float fluctuationAllowed)
    {
      Console.WriteLine("Trends");
      Table table = new Table("Symbol", "Start", "End", "Direction", "Change ( >" + fluctuationAllowed + "% )");
      foreach (var fund in funds)
      { 
        List<Trend> trends = RetrieveTrends(endDate, fund, overDays, fluctuationAllowed);
        Trend trend = trends.Last();
        {
          table.AddCell(fund.Symbol);
          table.AddCell(trend.Start);
          table.AddCell(trend.End);
          table.AddCell(trend.Direction.ToString());
          table.AddCell(trend.Change);
        }
      }
      Console.WriteLine(table);
    }

    public static List<Trend> RetrieveTrends(DateTime endDate, OwnedFund fund, int overDays, float fluctuationAllowed)
    {
      List<Trend> trends = new List<Trend>();
      float previous_day = 0.0f;

      Trend current_trend = null;
      Trend fluctuating_trend = null;
      endDate = endDate.AddDays(-1 * overDays);
      bool first = true;
      float result = 0.0f;
      for (int i = 0; i < overDays; i++)
      { 
        result = fund.FundData.RetrievePriceOnDay(endDate);
        if (first)
        {
          first = false;
        }
        else
        {
          if (previous_day != result)
          {           
            if (current_trend == null)
            {//establish a new trend (only at the start)
              current_trend = new Trend();
              current_trend.StartingPrice = result;
              current_trend.Start = endDate;
              if (previous_day < result)
              {
                current_trend.Direction = Direction.Up;
              }
              else
              {
                current_trend.Direction = Direction.Down;
              }
            }

            bool changing = false;
            if (current_trend.Direction == Direction.Up && previous_day > result)
            { //trend may be ending
              changing = true;
            }
            if (current_trend.Direction == Direction.Down && previous_day < result)
            { //trend may be ending
              changing = true;
            }


            if (fluctuating_trend != null)
            {
              if ((fluctuating_trend.Direction == Direction.Down && result >= fluctuating_trend.StartingPrice) ||
                (fluctuating_trend.Direction == Direction.Up && result <= fluctuating_trend.StartingPrice))
              {//the original trend has recovered and this one never reached the limit to become its own trend, kill it off
                fluctuating_trend = null;
              }
              else
              {
                float amount_fluctuated = Utilities.AmountChanged(fluctuating_trend.StartingPrice, result);
                if (Math.Abs(amount_fluctuated) > fluctuationAllowed)
                {//we've changed enough towards the direction of this fluctuating trend, that it has become the new trend
                  current_trend.End = fluctuating_trend.Start;
                  current_trend.Change = Utilities.AmountChanged(current_trend.StartingPrice, fluctuating_trend.StartingPrice);                  
                  trends.Add(current_trend);
                  current_trend = fluctuating_trend;
                  fluctuating_trend = null;
                }
              }
            }

            if (changing)
            {
              if (fluctuating_trend == null)
              {
                fluctuating_trend = new Trend();
                fluctuating_trend.Start = endDate.AddDays(-1); //previous day is the start of this trend
                fluctuating_trend.StartingPrice = previous_day;
                fluctuating_trend.Direction = current_trend.Direction == Direction.Up ? Direction.Down : Direction.Up;
              }
            }
          }
        }        
        previous_day = result;
        endDate = endDate.AddDays(1);
      }
      current_trend.End = endDate;
      current_trend.Change = Utilities.AmountChanged(current_trend.StartingPrice, result);
      if (trends.Count == 0)
      {
        if (current_trend.Change > 0.0f && current_trend.Direction == Direction.Down)
        {
          current_trend.Direction = Direction.Up;
        }
        else if (current_trend.Change < 0.0f && current_trend.Direction == Direction.Up)
        { 
          current_trend.Direction = Direction.Down;
        }
      }
      trends.Add(current_trend);

      return trends;
    }
  }
}