using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundBot
{


  public class YearlyGain
  {
    public float  Gain {get;set;}
    public int    Year  {get;set;}
    public float Units {get;set;}
  }

  
  public class YearlyDividend
  {
    public float  DividendAmount {get;set;}
    public int    Year  {get;set;}
  } 

  public enum Currency
  {
    CAD,
    USD
  }

  public class Buy
  {
    public DateTime PurchasedOn {get;set;}
    public float    PurchasedAt {get;set;}
    public float    Units       {get;set;}
  }


  public class OwnedFund
  {
    public string   Symbol      {get;set;}
    public string   Description {get;set;}
    public string   Country     {get;set;}
    public string   Type        {get;set;}
    public float    CurrentPrice{get;set;}
    public float    CurrentPercentageOfPortfolio  {get;set;}
    public float    TotalGain   {get;set;}
    public float    TotalDividend {get;set;}
    public float    OriginalValue {get;set;}
    public float    OriginalSpend {get;set;}
    public float    CurrentValue  {get;set;}
    public Currency Currency    {get;set;}
    public string   Market      {get;set;}
    public List<YearlyGain> YearlyGains {get;set;}
    public List<YearlyDividend> YearlyDividends {get;set;}
    public Fund     FundData    {get;set;}    
    public List<Buy>  Buys      {get;set;}
  }



  public class Fund
  {
    public string Symbol    {get;set;}
    public string Exchange  {get;set;}
    public string Name      {get;set;}
    public float  Price     {get;set;}
    public float  High      {get;set;}
    public float  Low       {get;set;}
    public float  DayAvg200 {get;set;}
    public float  DayAvg50  {get;set;}

    public List<Quote> HistoricalQuotes {get;set;}
    public List<Dividend> Dividends {get;set;}
    static List<DateTime> sHolidays = new List<DateTime>();

    static Fund()
    {      
			sHolidays.Add(new DateTime(2014, 1, 1));
      sHolidays.Add(new DateTime(2014, 1, 20));
			sHolidays.Add(new DateTime(2014, 2, 17));
			sHolidays.Add(new DateTime(2014, 5, 26));
			sHolidays.Add(new DateTime(2014, 7, 4));
			sHolidays.Add(new DateTime(2014, 9, 1));
			sHolidays.Add(new DateTime(2014, 10, 13));
			sHolidays.Add(new DateTime(2014, 11, 11));
			sHolidays.Add(new DateTime(2014, 11, 27));
			sHolidays.Add(new DateTime(2014, 12, 25));
    }

    public Fund()
    {
      HistoricalQuotes = new List<Quote>();
      Dividends = new List<Dividend>();
    }

    public float Retrieve200DayAvg(DateTime endDate)
    {
      Quote last_quote = HistoricalQuotes.Where(x => x.Date <= endDate).OrderByDescending(x => x.Date).First();
      int start_index = HistoricalQuotes.IndexOf(last_quote);
      int end_index = Math.Min(HistoricalQuotes.Count(), start_index + 200);
      var quotes =  HistoricalQuotes.Where((quote,index) => index < end_index && index >= start_index);
      return quotes.Average(x => x.Close);
    }

    public float RetrieveNDayAvg(int days, DateTime endDate)
    {
      Quote last_quote = HistoricalQuotes.Where(x => x.Date <= endDate).OrderByDescending(x => x.Date).First();
      int start_index = HistoricalQuotes.IndexOf(last_quote);
      int end_index = Math.Min(HistoricalQuotes.Count(), start_index + days);
      var quotes =  HistoricalQuotes.Where((quote,index) => index < end_index && index >= start_index);
      return quotes.Average(x => x.Close);
    }

    public float Retrieve50DayAvg(DateTime endDate)
    {
      Quote last_quote = HistoricalQuotes.Where(x => x.Date <= endDate).OrderByDescending(x => x.Date).First();
      int start_index = HistoricalQuotes.IndexOf(last_quote);
      int end_index = Math.Min(HistoricalQuotes.Count(), start_index + 50);
      var quotes =  HistoricalQuotes.Where((quote,index) => index < end_index && index >= start_index);
      return quotes.Average(x => x.Close);
    }

    public float RetrievePriceOnDay(DateTime date)
    {
      return HistoricalQuotes.Where(x => x.Date <= date).OrderByDescending(x => x.Date).First().Close;
    }

    private DateTime NormalDaysFromBusinessDays(DateTime endDate, int businessDays)
    {
      int count = 0;
      DateTime iterator = endDate;
      while (true)
      {
        if (iterator.DayOfWeek != DayOfWeek.Saturday && iterator.DayOfWeek != DayOfWeek.Sunday && sHolidays.Where(x => x.Day == iterator.Day && x.Month == iterator.Month).Count() == 0)
        {
          count++;
          if (count == businessDays)
            break;
        }
        iterator = iterator.AddDays(-1);
      }
      return iterator;
    }

  } 
}
