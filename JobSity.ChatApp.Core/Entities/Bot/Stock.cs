using System;

namespace JobSity.ChatApp.Core.Entities.Bot
{
    public class Stock
    {
        public string Symbol {get; set; }	
        public string Date { get; set; }	
        public string Time {get; set; }	
        public double Open { get; set; }
        public double	High { get; set; }	
        public double Low { get; set; }	
        public double Close { get; set; }	
        public int Volume { get; set; }
    }
}