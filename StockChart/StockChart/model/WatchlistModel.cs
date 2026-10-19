using System.Collections.Specialized;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace StockChart.model;

public partial class WatchlistModel : ObservableObject
{
    public WatchlistModel(string name)
    {
        Name = name;
        Symbols.CollectionChanged += OnSymbolsCollectionChanged;
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    public ObservableCollection<string> Symbols { get; } = [];

    public event EventHandler? SymbolsChanged;

    private void OnSymbolsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => SymbolsChanged?.Invoke(this, EventArgs.Empty);
        
    /// <summary>
    /// Adds a symbol to the watchlist.
    /// </summary>
    /// <param name="symbol">The symbol to add.</param>
    public void AddSymbol(string symbol)
    {
        if (!Symbols.Contains(symbol))
        {
            Symbols.Add(symbol);
        }
    }
    
    /// <summary>
    /// Removes a symbol from the watchlist.
    /// </summary>
    /// <param name="symbol">The symbol to remove.</param>
    public void RemoveSymbol(string symbol)
    {
        Symbols.Remove(symbol);
    }
}