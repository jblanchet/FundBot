/*
 * 
 * 
 * 
 * Current issues:
 *  -does not support historical buys (needed to properly calculate annual gains and annual dividends)
 * 
http://finance.yahoo.com/d/quotes.csv?s=MSFT&f=xsnpjkm5m4wm3ll1


xsnpjkm5m4wm3

x=exchange
s=symbol
n=company name
p=last closing price
j=52 week low
k=52 week high
m5=change from 200 day moving avg
m4=200 day moving avg
m3=50 day moving avg
w=52 week range
l1=last trade (price)


historical

http://ichart.finance.yahoo.com/table.csv?s=MSFT&a=01&b=01&c=2011


dividends

http://stackoverflow.com/questions/6119867/stocks-splitting-api-google-or-yahoo
http://ichart.finance.yahoo.com/x?s=CPD.TO&a=00&b=01&c=2014&g=v

0 based month

 * */




using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{
  class Program
  {
    static List<OwnedFund> mOwnedFunds = new List<OwnedFund>();

    static void Main(string[] args)
    {
      DirectoryInfo info = new DirectoryInfo("Downloads");
      if (info.Exists == false)
      {
        info.Create();
      }
      LoadBuys("Buys.csv");
      Weightings.ReadWeightings("Weightings.csv");
      BeginInput();
     /* Fund msft = FundFetcher.GetFundData("MSFT", new DateTime(2012, 01, 01));
      float twohundred_avg = msft.Retrieve200DayAvg(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
      float fifty_avg = msft.Retrieve50DayAvg(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1));*/
    }

    static void BeginInput()
    {
      while (true)
      {
        Console.WriteLine("p - positions");
        Console.WriteLine("t - trends");
        Console.WriteLine("h - historical");
        Console.WriteLine("a - moving average");
        Console.WriteLine("w - weightings");
        Console.WriteLine("o - overall");
        Console.Write(">");
        string input = Console.ReadLine();
        if (input == "p")
        {
          PrintPositions();
        }
        else if (input == "h")
        {
          PrintHistorical();
        }
        else if (input == "t")
        {
          TrendsSubMenu();
        }
        else if (input.StartsWith("a"))
        {
          MovingAverageSubMenu();
        }
        else if (input.StartsWith("w"))
        {
          WeightingsSubMenu();          
        }
        else if (input.StartsWith("o"))
        {
          PrintOverall();
        }
      }
    }

    static void WeightingsSubMenu()
    {
      while (true)
      {
        Console.WriteLine("(o)verall");
        Console.WriteLine("(a)fter [money to spend]");
        Console.Write(">>");
        string input = Console.ReadLine();
        if (input == "q")
        {
          break;
        }
        else if (input == "o")
        {
          Weightings.PrintWeightings(mOwnedFunds);
        }
        else if (input.StartsWith("a"))
        {
          string[] split = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
          Weightings.WeightAfterBuy(mOwnedFunds, Convert.ToSingle(split[1]));
        }
      }
      Weightings.PrintWeightings(mOwnedFunds);
    }

    static void MovingAverageSubMenu()
    {
      while (true)
      {
        Console.WriteLine("(a)ll - 200 day vs 50 day ending now");
        Console.WriteLine("(c)ustom [days] [end date (dd/mm/yyyy)] [symbol] [over] - n day moving average over x many days for a symbol ending at end date");
        Console.WriteLine("(n)ow [days] [symbol] [over] - n day moving average over x many days for a symbol ending now");
        Console.WriteLine("(s)imple [days] [symbol] - n day moving average for a symbol ending now");
        Console.Write(">>");
        string input = Console.ReadLine();
        if (input == "q")
        {
          break;
        }
        else if (input.StartsWith("c"))
        {
          string[] split = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
          int over_days = Convert.ToInt32(split[4]);
          PrintMovingAverage(Convert.ToInt32(split[1]), DateTime.ParseExact(split[2], "dd/MM/yyyy", null), split[3], over_days);
        }
        else if (input.StartsWith("n"))
        {
          string[] split = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
          int over_days = Convert.ToInt32(split[3]);
          PrintMovingAverageTillNow(Convert.ToInt32(split[1]), split[2], over_days);          
        }
        else if (input.StartsWith("s"))
        {
          string[] split = input.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries);
          PrintMovingAverageSimple(Convert.ToInt32(split[1]), split[2]);
        }
        else if (input.StartsWith("a"))
        {
          PrintAllMovingAverage();
        }
      }
    } 

    static Dictionary<string, string> ParseArgs(string argsLine)
    {
      Dictionary<string, string> args = new Dictionary<string,string>();
      string[] split = argsLine.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
      bool first = true;
      foreach (string arg in split)
      {
        if (first)
        {
          first = false;
          continue;
        }
        args.Add(arg.Split('=')[0], arg.Split('=')[1]);
      }
      return args;
    }
    
    static void TrendsSubMenu()
    {
      while (true)
      {
        Console.WriteLine("(t)rend for fund s=symbol d=[end date (dd/mm/yyyy)] o=[days over] f=[fluctuation allowed]");
        Console.WriteLine("(a)ll trends, all funds d=[end date (dd/mm/yyyy)] o=[days over] f=[% fluctuation allowed]");
        Console.WriteLine("(l)atest trend, all funds d=[end date (dd/mm/yyyy)] o=[days over] f=[% fluctuation allowed]");
        Console.Write(">>");
        string input = Console.ReadLine();
        if (input == "q")
        {
          break;
        }
        else if (input.StartsWith("t"))
        {

          Dictionary<string, string> args = ParseArgs(input);
          DateTime date = args.GetDefaultDate("d", DateTime.Now);
          string symbol = args.GetDefaultString("s", string.Empty);
          int over_days = args.GetDefaultInt("o", 100);
          float fluctuation = args.GetDefaultFloat("f", 2.0f);
          OwnedFund fund = RetrieveFund(symbol);
          Trends.PrintTrendActivityForFund(date, fund, over_days, fluctuation);
        }
        else if (input.StartsWith("a"))
        {
          Dictionary<string, string> args = ParseArgs(input);
          DateTime date = args.GetDefaultDate("d", DateTime.Now);
          int over_days = args.GetDefaultInt("o", 100);
          float fluctuation = args.GetDefaultFloat("f", 2.0f);
          Trends.PrintTrendActivityForAllFunds(date, mOwnedFunds, over_days, fluctuation);
        }
        else if (input.StartsWith("l"))
        {
          Dictionary<string, string> args = ParseArgs(input);
          DateTime date = args.GetDefaultDate("d", DateTime.Now);
          int over_days = args.GetDefaultInt("o", 100);
          float fluctuation = args.GetDefaultFloat("f", 2.0f);
          Trends.PrintLatestTrendActivityForAllFunds(date, mOwnedFunds, over_days, fluctuation);
        }
      }
    }

    static void PrintMovingAverageSimple(int days, string symbol)
    {
      PrintMovingAverage(days, DateTime.Now, symbol, 1);
    }    

    static void PrintMovingAverageTillNow(int days, string symbol, int over_days)
    {
      PrintMovingAverage(days, DateTime.Now, symbol, over_days);
    }    

    static void PrintMovingAverage(int days, DateTime endDate, string symbol, int over_days)
    {
      var fund = RetrieveFund(symbol);

      Console.WriteLine(days + " day average");
      Table table = new Table("Symbol", "Result", "Start", "End");

      for (int i = 0; i < over_days; i++)
      { 
        endDate = endDate.AddDays(-1);
        float result = fund.FundData.RetrieveNDayAvg(days, endDate);
        table.AddCell(symbol);
        table.AddCell(result);
        table.AddCell(endDate.AddDays(-1 * days).ToString("dd/MM/yyyy"));
        table.AddCell(endDate.ToString("dd/MM/yyyy"));      
      }
      Console.WriteLine(table);
      Console.WriteLine();
    }

    static void PrintAllMovingAverage()
    {
      Console.WriteLine("200 vs 50 day moving average");
      Table table = new Table("Symbol", "200 day", "50 day", "Change Direction", "Change Date");
      foreach (var fund in mOwnedFunds)
      {
        float result_200 = fund.FundData.Retrieve200DayAvg(DateTime.Now);
        float result_50 = fund.FundData.Retrieve50DayAvg(DateTime.Now);
        table.AddCell(fund.Symbol);
        table.AddCell(result_200);
        table.AddCell(result_50);
       /* if ()*/

      }
    }

    static void PrintPositions()
    {
      Table table = new Table("Symbol", "Orig PurchasedOn", "Num Buys", "Units", "Currency", "Current", "% Portfolio", "Desc", "Current Value");
      foreach (var fund in mOwnedFunds.OrderBy(x => x.Buys.OrderBy(buy => buy.PurchasedOn).First().PurchasedOn))
      {
        table.AddCell(fund.Symbol);
        table.AddCell(fund.Buys.OrderBy(buy => buy.PurchasedOn).First().PurchasedOn);
        table.AddCell(fund.Buys.Count);
        table.AddCell(fund.Buys.Sum(x => x.Units));
        table.AddCell(fund.Currency);
        table.AddCell(fund.CurrentPrice);
        table.AddCell(fund.CurrentPercentageOfPortfolio);
        table.AddCell(fund.Description);
        table.AddCell(fund.CurrentValue);
      }
      Console.WriteLine(table);
    }

    static OwnedFund RetrieveFund(string symbol)
    {
      return mOwnedFunds.Where(x => x.Symbol == symbol.ToUpper()).Single();
    }

    static void PrintOverall()
    {
      CalculateOriginalSpend();
      CalculateHistoricalGains();

      Table overall_table = new Table();
      overall_table.AddHeader("Fund");
      overall_table.AddHeader("Original Worth");
      overall_table.AddHeader("Now");
      overall_table.AddHeader("% Change");

      float original_total = 0.0f;
      float current_total = 0.0f;
      foreach (var fund in mOwnedFunds)
      {
        original_total += fund.OriginalSpend;
        current_total += fund.CurrentValue + fund.TotalDividend;
        overall_table.AddCell(fund.Symbol);
        overall_table.AddCell(fund.OriginalSpend);
        overall_table.AddCell(fund.CurrentValue + fund.TotalDividend);
        overall_table.AddCell(Utilities.AmountChanged(fund.OriginalSpend, fund.CurrentValue + fund.TotalDividend).ToString() + "%");
      }
      overall_table.AddCell("Total");
      overall_table.AddCell(original_total);
      overall_table.AddCell(current_total);
      overall_table.AddCell(Utilities.AmountChanged(original_total, current_total).ToString() + "%");

      Console.WriteLine("Overall");
      Console.WriteLine(overall_table);
      Console.WriteLine();
    }

    static void PrintHistorical()
    {      
      CalculateHistoricalGains();

      Table gain_table = new Table("Symbol");
      int max_years = mOwnedFunds.Max(x => x.YearlyGains.Count);

      int current_year = System.DateTime.Now.Year;
      List<int> years = new List<int>();
      for (int i = max_years; i >= 1; i--)
      {
        int year = current_year - (i-1);
        years.Add(year);
        gain_table.AddHeader((year).ToString());
      }
      gain_table.AddHeader("Overall");

      foreach (var fund in mOwnedFunds)
      {
        gain_table.AddCell(fund.Symbol);
        foreach (int year in years)
        {
          var yearly_gain = fund.YearlyGains.Where(x => x.Year == year).SingleOrDefault();
          if (yearly_gain == null)
          {
            gain_table.AddCell("-");
          }
          else
          {
            gain_table.AddCell((float)Math.Round(yearly_gain.Gain,1) + "%");
          }
        }
        
        gain_table.AddCell((float)Math.Round(fund.TotalGain,1) + "%");
      }

      Console.WriteLine("Annual Gains");
      Console.WriteLine(gain_table);
      Console.WriteLine();

      Table dividend_table = new Table("Symbol");
      max_years = mOwnedFunds.Max(x => x.YearlyDividends.Count);
      years.Clear();

      for (int i = max_years; i >= 1; i--)
      {
        int year = current_year - (i-1);
        years.Add(year);
        dividend_table.AddHeader((year).ToString());
      }
      dividend_table.AddHeader("Overall");

      foreach (var fund in mOwnedFunds)
      {
        dividend_table.AddCell(fund.Symbol);
        foreach (int year in years)
        {
          var yearly_dividends = fund.YearlyDividends.Where(x => x.Year == year).SingleOrDefault();
          if (yearly_dividends == null)
          {
            dividend_table.AddCell("-");
          }
          else
          {
            dividend_table.AddCell((float)Math.Round(yearly_dividends.DividendAmount / 100,2));
          }
        }
        dividend_table.AddCell((float)Math.Round(fund.TotalDividend,2));
      }
      Console.WriteLine();
      Console.WriteLine("Dividends");
      Console.WriteLine(dividend_table);
    }

    static void CalculateOriginalSpend()
    {
      foreach (var owned in mOwnedFunds)
      {
        foreach (var buy in owned.Buys)
        {           
          owned.OriginalSpend += (buy.PurchasedAt * buy.Units);
        }
      }
    }

    static void CalculateHistoricalGains()
    {

      foreach (var owned in mOwnedFunds)
      {
        owned.YearlyGains.Clear();
        owned.YearlyDividends.Clear();
        owned.TotalDividend = 0.0f;
        float total_spent = 0.0f;
        float total_units_overall = 0.0f;
        foreach (var buy in owned.Buys)
        { 
          var quotes = owned.FundData.HistoricalQuotes.Where(x => x.Date >= buy.PurchasedOn).OrderBy(x => x.Date);
          owned.YearlyGains.AddRange(quotes.GroupBy(x => x.Date.Year).Select(x => new YearlyGain
            {
              Year = x.Key,
              Gain = Utilities.AmountChanged(x.First().Close, x.Last().Close), // ((x.Last().Close / x.First().Close) - 1) * 100
              Units = buy.Units
            }));
          total_spent += buy.Units * buy.PurchasedAt;
          total_units_overall += buy.Units;
          var dividends = owned.FundData.Dividends.Where(x => x.Date >= buy.PurchasedOn).OrderBy(x => x.Date).ToList();
          owned.YearlyDividends.AddRange(dividends.GroupBy(x => x.Date.Year).Select(x => new YearlyDividend
            {
              Year = x.Key,
              DividendAmount = x.Sum(dividend => dividend.AmountPerShare * buy.Units)
            }));    
          owned.TotalDividend += dividends.Sum(x => (x.AmountPerShare * buy.Units) / 100.0f);
        }
        float current_total = total_units_overall * owned.FundData.HistoricalQuotes.OrderBy(x => x.Date).Last().Close;
        owned.TotalGain = Utilities.AmountChanged(total_spent, current_total);
        /*
         * buy 1 unit at $100 in january, for that year rises 10% = $110
         * buy 1 unit at $101 in june, for that year rises 8.9%  = $110 
         * averages to 9.45%
         * i spent $201, I now have $220, which is 9.45%
         * 
         * 
         *  ((second - first)/first) * 100.0f;
         *  
         * 
         * buy 1 unit at $100 in january, for that year rises 10% = $110
         * buy 3 unit at $303 in june, for that year rises 8.9%  = $330 
         * averages to 9.45%
         * i spent $403, I now have $440, which is 9.175%
         * 
         * time doesnt matter, just the start/end prices
         * 
         * get % of years units for this buy * gain
         * add up all of the above
         * 6.675
         * 2.5
         * = 9.175
         * 
         * 
         * for total gain
         * 2013 - buy 1 share at $100, ends year at $110
         * 2014 - buy 3 shares at $115, ends year at $116
         * 
         * 2013 - 10%
         * 2014 - 5.45% and .87%
         * overall Ive paid $445 and now have $458 = 2.9%
         * 
         * how much have I paid in total, how much is it now worth in total?
         * 
         */ 
        if (owned.Buys.Count > 1)
        {
          int first_year = owned.Buys.OrderBy(x => x.PurchasedOn).First().PurchasedOn.Year;
          List<YearlyGain> aggregate_gains = new List<YearlyGain>();
          List<YearlyDividend> aggregate_dividends = new List<YearlyDividend>();
          for (int i = DateTime.Now.Year; i >= first_year; i--)
          {            
            IEnumerable<YearlyGain> gains = owned.YearlyGains.Where(x => x.Year == i);
            float total_units = gains.Sum(x => x.Units);
            float yearly_gain = gains.Sum(x => (x.Units / total_units) * x.Gain);
            YearlyGain aggregate_gain = new YearlyGain();
            aggregate_gain.Year = i;
            aggregate_gain.Units = total_units;
            aggregate_gain.Gain = yearly_gain;
            aggregate_gains.Add(aggregate_gain);

            YearlyDividend aggregate_dividend = new YearlyDividend();
            aggregate_dividend.Year = i;
            aggregate_dividend.DividendAmount = owned.YearlyDividends.Where(x => x.Year == i).Sum(x => x.DividendAmount);
            aggregate_dividends.Add(aggregate_dividend);
          }
          owned.YearlyDividends = aggregate_dividends;
          owned.YearlyGains = aggregate_gains;
        }

      }      
    }

    static void LoadBuys(string path)
    {
      foreach (string line in File.ReadAllLines(path))
      {
        var buy_info = line.Split(',');
        string symbol = buy_info[0];

        OwnedFund owned_fund;
        if (mOwnedFunds.Where(x => x.Symbol == symbol).Count() > 0)
        {//already own this
          owned_fund = mOwnedFunds.Where(x => x.Symbol == symbol).Single();
        }
        else
        {
          owned_fund = new OwnedFund();
          owned_fund.Symbol = symbol;
          owned_fund.Currency = (Currency)Enum.Parse(typeof(Currency), buy_info[4]);
          owned_fund.Buys = new List<Buy>();
          owned_fund.YearlyDividends = new List<YearlyDividend>();
          owned_fund.YearlyGains = new List<YearlyGain>();
          owned_fund.Country = buy_info[5];
          owned_fund.Type = buy_info[6];
          owned_fund.Description = owned_fund.Country + "-" + owned_fund.Type;
          mOwnedFunds.Add(owned_fund);
        }
        Buy buy = new Buy();
        buy.PurchasedOn = Convert.ToDateTime(buy_info[1]);
        buy.Units = Convert.ToSingle(buy_info[2]);
        buy.PurchasedAt = Convert.ToSingle(buy_info[3]);
        owned_fund.Buys.Add(buy);

      }
      foreach (var fund in mOwnedFunds)
      {
        Buy first_buy = fund.Buys.OrderBy(x => x.PurchasedOn).First();
        Fund current_quote = FundFetcher.GetFundData(fund.Symbol, first_buy.PurchasedOn);
        fund.CurrentPrice = current_quote.Price;
        fund.FundData = current_quote;
        float total_units = fund.Buys.Sum(x => x.Units);
        fund.CurrentValue = total_units * fund.CurrentPrice;
      }

      float total_portfolio_worth_current = 0.0f;
      foreach (var fund in mOwnedFunds)
      {
        total_portfolio_worth_current += fund.Buys.Sum(x => x.Units * fund.CurrentPrice);
      }
      
      foreach (var fund in mOwnedFunds)
      {        
        fund.CurrentPercentageOfPortfolio = (fund.Buys.Sum(x => x.Units * fund.CurrentPrice) / total_portfolio_worth_current) * 100.0f;
      }
    }
  }

  public static class Extensions
  {
  
      public static float GetDefaultFloat<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, float defaultValue)
      {
        if (dictionary.ContainsKey(key) == false)
          return defaultValue;
        else
          return Convert.ToSingle(dictionary[key]);
      }

      public static int GetDefaultInt<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, int defaultValue)
      {
        if (dictionary.ContainsKey(key) == false)
          return defaultValue;
        else
          return Convert.ToInt32(dictionary[key]);
      }

      public static string GetDefaultString<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, string defaultValue)
      {
        if (dictionary.ContainsKey(key) == false)
          return defaultValue;
        else
          return dictionary[key].ToString();
      }

      public static DateTime GetDefaultDate<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, DateTime defaultValue)
      {
        if (dictionary.ContainsKey(key) == false)
          return defaultValue;
        else
          return Convert.ToDateTime(dictionary[key]);
      }
  }
}
