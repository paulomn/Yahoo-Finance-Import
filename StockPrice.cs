//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YahooFinanceImport
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockPrice
    {
        public int StockPriceId { get; set; }
        public short StockId { get; set; }
        public System.DateTime PriceDate { get; set; }
        public int StockPriceGroupId { get; set; }
        public decimal PriceOpen { get; set; }
        public decimal PriceHigh { get; set; }
        public decimal PriceLow { get; set; }
        public decimal PriceClose { get; set; }
        public decimal PriceVolume { get; set; }
        public decimal PriceAdjClose { get; set; }
    
        public virtual Stock Stock { get; set; }
    }
}