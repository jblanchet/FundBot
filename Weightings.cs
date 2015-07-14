using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{
  public class Weighting
  {
    public string Country {get;set;}
    public string Type    {get;set;}
    public float  Percent {get;set;}
  }

  public class Weightings
  {
    static List<Weighting> mDesiredWeightings = new List<Weighting>();

    static public void ReadWeightings(string filename)
    {
      foreach (var line in System.IO.File.ReadAllLines(filename))
      {
        var tokens = line.Split(',');
        Weighting weighting = new Weighting();
        weighting.Country = tokens[0];
        weighting.Type = tokens[1];
        weighting.Percent = Convert.ToSingle(tokens[2]);
        mDesiredWeightings.Add(weighting);
      }
    }

    static public void WeightAfterBuy(List<OwnedFund> ownedFunds, float amountToSpend)
    {
      Dictionary<Weighting, List<OwnedFund>> categorized_funds = new Dictionary<Weighting,List<OwnedFund>>();      

      foreach(var fund in ownedFunds)
      {    
        foreach (var weighting in mDesiredWeightings)
        {
          if (fund.Country == weighting.Country && fund.Type == weighting.Type)
          {
            if (categorized_funds.ContainsKey(weighting) == false)
            {
              categorized_funds.Add(weighting, new List<OwnedFund>());
            }
            categorized_funds[weighting].Add(fund);
          }
        }
      }

      float total_portfolio_worth_current = 0;
      foreach (var fund in ownedFunds)
      {
        total_portfolio_worth_current += fund.Buys.Sum(x => x.Units * fund.CurrentPrice);
      }

      float new_estimated_total_portfolio = total_portfolio_worth_current + amountToSpend;
      float quotient = new_estimated_total_portfolio / total_portfolio_worth_current;


      Table table = new Table("Type", "Country", "Funds", "Current Weight", "Desired Weight", "Money needed to reach desired");
        
      foreach (var weighting in mDesiredWeightings)
      {
        table.AddCell(weighting.Type);
        table.AddCell(weighting.Country);
        if (categorized_funds.ContainsKey(weighting))
        {
          table.AddCell(string.Join(",", categorized_funds[weighting].Select(x => x.Symbol).ToList()));
          float current_weight = categorized_funds[weighting].Sum(x => x.CurrentPercentageOfPortfolio) / quotient;
          table.AddCell(current_weight);
          table.AddCell(weighting.Percent);
          float difference_needed = new_estimated_total_portfolio * ((weighting.Percent - current_weight) / 100.0f);
          table.AddCell(difference_needed);
        }
        else
        {
          table.AddCell("-");
          table.AddCell("-");
          table.AddCell(weighting.Percent);
          float difference_needed = new_estimated_total_portfolio * ((weighting.Percent) / 100.0f);
          table.AddCell(difference_needed);
        }
      }

      Console.WriteLine(table);
    }

    static public void PrintWeightings(List<OwnedFund> ownedFunds)
    {
      Dictionary<Weighting, List<OwnedFund>> categorized_funds = new Dictionary<Weighting,List<OwnedFund>>();      

      foreach(var fund in ownedFunds)
      {    
        foreach (var weighting in mDesiredWeightings)
        {
          if (fund.Country == weighting.Country && fund.Type == weighting.Type)
          {
            if (categorized_funds.ContainsKey(weighting) == false)
            {
              categorized_funds.Add(weighting, new List<OwnedFund>());
            }
            categorized_funds[weighting].Add(fund);
          }
        }
      }

      float total_portfolio_worth_current = 0.0f;
      foreach (var fund in ownedFunds)
      {
        total_portfolio_worth_current += fund.Buys.Sum(x => x.Units * fund.CurrentPrice);
      }

      Table table = new Table("Type", "Country", "Funds", "Current Weight", "Desired Weight", "$ amount off by");
        
      foreach (var weighting in mDesiredWeightings)
      {
        table.AddCell(weighting.Type);
        table.AddCell(weighting.Country);
        if (categorized_funds.ContainsKey(weighting))
        {
          table.AddCell(string.Join(",", categorized_funds[weighting].Select(x => x.Symbol).ToList()));
          float current_weight = categorized_funds[weighting].Sum(x => x.CurrentPercentageOfPortfolio);
          table.AddCell(current_weight);
          table.AddCell(weighting.Percent);
          float difference_needed = total_portfolio_worth_current * ((weighting.Percent - current_weight) / 100.0f);
          table.AddCell(difference_needed);
        }
        else
        {
          table.AddCell("-");
          table.AddCell("-");
          table.AddCell(weighting.Percent);
          float difference_needed = total_portfolio_worth_current * ((weighting.Percent) / 100.0f);
          table.AddCell(difference_needed);
        }
      }

      Console.WriteLine(table);
    }
  }
}
