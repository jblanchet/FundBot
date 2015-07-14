using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{

  public class Quote
  {
    public DateTime Date  {get;set;}
    public float    Open  {get;set;}
    public float    Close {get;set;}
    public float    High  {get;set;}
    public float    Low   {get;set;}
  }

  public class Dividend
  {
    public DateTime Date {get;set;}
    public float    AmountPerShare {get;set;}
  }

  public static class FundFetcher
  {
    static string mCompanyInfoURL = @"http://finance.yahoo.com/d/quotes.csv?s={0}&f=xsnl1jkm4m3";
    static string mHistoricalQuotesURL = @"http://ichart.finance.yahoo.com/table.csv?s={0}&a={1}&b={2}&c={3}";
    static string mDividendURL = @"http://ichart.finance.yahoo.com/x?s={0}&a={1}&b={2}&c={3}&g=v";

    static int mHistoryYears = 7;

    public static float CellToFloat(string text)
    {//yahoo api is flakey
      text = text.Trim();
      if (text == "N/A")
        return -1.0f;
      else
        return Convert.ToSingle(text);
    }

    public static Fund GetFundData(string tickerSymbol, DateTime fromDate)
    {
      int years_to_fetch = mHistoryYears;
      string info_path, historical_path, dividend_path;
      while (true)
      {
        try
        {
          fromDate = fromDate.AddYears(-1 * years_to_fetch); //it doesnt like when they're too soon, so new buys would fail

          WebClient client = new WebClient();
          info_path = ".\\Downloads\\" + tickerSymbol + "_quote.csv";
          client.DownloadFile(string.Format(mCompanyInfoURL, tickerSymbol), info_path);

          historical_path = ".\\Downloads\\" + tickerSymbol + "_historical_quotes.csv";
          string historical_url = string.Format(mHistoricalQuotesURL, tickerSymbol, fromDate.Day, fromDate.Month - 1, fromDate.Year);
          client.DownloadFile(historical_url, historical_path);

          dividend_path = ".\\Downloads\\" + tickerSymbol + "_dividends.csv";
          string dividend_url = string.Format(mDividendURL, tickerSymbol, fromDate.Day, fromDate.Month - 1, fromDate.Year);
          client.DownloadFile(dividend_url, dividend_path);
          break;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Error retrieving " + years_to_fetch + " years for " + tickerSymbol + "; trying for " + (years_to_fetch - 1));
          years_to_fetch--;
        }
      }

      string text = File.ReadAllText(info_path);
      text = text.Replace(", ", " ");
      string[] split_info = text.Split(',');
      Fund fund = new Fund();
      fund.Exchange = split_info[0];
      fund.Symbol = split_info[1];
      fund.Name = split_info[2];
      fund.Price = CellToFloat(split_info[3]);
      fund.Low = CellToFloat(split_info[4]);
      fund.High = CellToFloat(split_info[5]);
      fund.DayAvg200 = CellToFloat(split_info[6]);
      fund.DayAvg50 = CellToFloat(split_info[7]);

      bool first = true;
      foreach (var line in File.ReadAllLines(historical_path))
      {
        if (first)
        {
          first = false;
          continue;
        }
        string[] quote_info = line.Split(',');
        Quote quote = new Quote();
        quote.Date = Convert.ToDateTime(quote_info[0]);
        quote.Open = Convert.ToSingle(quote_info[1]);
        quote.High = Convert.ToSingle(quote_info[2]);
        quote.Low = Convert.ToSingle(quote_info[3]);
        quote.Close = Convert.ToSingle(quote_info[4]);
        fund.HistoricalQuotes.Add(quote);
      }
      
      first = true;
      foreach (var line in File.ReadAllLines(dividend_path))
      {
        if (first)
        {
          first = false;
          continue;
        }
        string[] dividend_info = line.Split(',');
        if (dividend_info[0] == "DIVIDEND")
        {
          Dividend dividend = new Dividend();
          dividend.Date = DateTime.ParseExact(dividend_info[1].Trim(), "yyyyMMdd", null);
          dividend.AmountPerShare = Convert.ToSingle(dividend_info[2]) * 100.0f;
          fund.Dividends.Add(dividend);
        }
      }
      return fund;
    }
    
  }
}
