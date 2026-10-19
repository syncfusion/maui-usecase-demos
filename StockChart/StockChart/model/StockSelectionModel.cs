using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.model;

public partial class StockSelectionModel : ObservableObject
{
    public StockSelectionModel(StockModel stock, bool isSelected = false)
    {
        Stock = stock;
        IsSelected = isSelected;
    }
    
    public StockSelectionModel(string symbol, string company, string officialName, bool isSelected = false)
    {
        // Create a minimal StockModel for display purposes
        Stock = new StockModel
        {
            Symbol = symbol,
            Company = company,
            OfficialName = officialName
        };
        IsSelected = isSelected;
    }

    public StockModel Stock { get; }
    public string Company => Stock.Company;
    public string OfficialName => Stock.OfficialName;
    public string Symbol => Stock.Symbol;
    public string LogoImageSource => Stock.LogoImageSource;

    [ObservableProperty]
    public partial bool IsSelected { get; set; }
}