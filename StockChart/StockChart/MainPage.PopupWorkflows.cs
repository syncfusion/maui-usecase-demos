using System.Runtime.InteropServices;
using StockChart.Helpers;
using StockChart.Models;
using StockChart.ViewModels;
using Microsoft.Maui.Graphics;
using Syncfusion.Maui.Popup;

namespace StockChart
{
    // Popup workflow presentation: sizes, positions, and shows/dismisses each dialog.
    // Workflow decisions and confirm logic live in StockChartViewModel.Workflows.
    public partial class MainPage
    {
        private void OnTimeRangeDropDownClosed(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel &&
                viewModel.SelectedTimeRange == "Custom" &&
                !viewModel.IsCustomDateRangeVisible)
            {
                ShowCustomDateRangePopup(viewModel);
            }
        }

        private void ShowCustomDateRangePopup(StockChartViewModel viewModel)
        {
            if (!CanUsePopup)
            {
                return;
            }

            viewModel.PrepareCustomDateRange();
            viewModel.IsCustomDateRangeVisible = true;
            _customDateRangePopupOpen = true;
        }
        private bool CanUsePopup => _isPageActive && _pageLoaded && Window is not null && Handler is not null;

        private void SafePopupOperation(Action operation)
        {
            if (!CanUsePopup)
            {
                return;
            }

            try
            {
                operation();
            }
            catch (InvalidOperationException)
            {
                // WinUI can detach a Popup before MAUI raises Disappearing.
            }
            catch (COMException)
            {
                // A native popup can reject an operation during window activation or teardown.
            }
        }
        private void ShowWorkflowPopup(SfPopup popup)
        {
            if (!CanUsePopup)
            {
                return;
            }

            var isMobile = Width > 0 && Width < MobileBreakpoint;
            if (isMobile)
            {
                var display = DeviceDisplay.MainDisplayInfo;
                PopupPresentation.ApplyBottomSheetLayout(popup, display.Width / display.Density, display.Height / display.Density);
                SafePopupOperation(() => popup.Show(popup.StartX, popup.StartY));
                return;
            }

            PopupPresentation.ApplyThemedDesktopCardStyle(popup);
            SafePopupOperation(() => popup.Show());
        }
        private void OnDisclaimerCloseClicked(object? sender, EventArgs e)
        {
            SafePopupOperation(DisclaimerPopup.Dismiss);
            SafePopupOperation(InformationPopup.Dismiss);
        }

        private void OnSettingsClicked(object? sender, EventArgs e)
        {
            if (!CanUsePopup || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            viewModel.OpenSettingsPopupCommand.Execute(null);
            if (Width > 0 && Width < MobileBreakpoint)
            {
                var display = DeviceDisplay.MainDisplayInfo;
                PopupPresentation.ApplyBottomSheetLayout(
                    SettingsPopup,
                    display.Width / display.Density,
                    display.Height / display.Density,
                    Math.Min(520, (display.Height / display.Density) * 0.78),
                    topCornerRadius: 20);
                SafePopupOperation(() => SettingsPopup.Show(SettingsPopup.StartX, SettingsPopup.StartY));
            }
            else
            {
                SettingsPopup.WidthRequest = 625;
                SettingsPopup.HeightRequest = 540;
                PopupPresentation.ApplyDesktopCardStyle(SettingsPopup);
                SafePopupOperation(() => SettingsPopup.Show());
            }
        }

        private void OnAddStockClicked(object? sender, EventArgs e)
        {
            if (!CanUsePopup || BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            viewModel.OpenAddStockPopupCommand.Execute(null);
            ConfigureSheet(AddStockPopup, 690, 450);
            ShowWorkflowPopup(AddStockPopup);
        }

        private void OnAddWatchlistFromSelectorClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel)
            {
                return;
            }

            WatchlistSelector.IsDropDownOpen = false;
            Dispatcher.Dispatch(() =>
            {
                if (!CanUsePopup)
                {
                    return;
                }

                viewModel.OpenNewWatchlistPopupCommand.Execute(null);
                ConfigureSheet(NewWatchlistPopup, 620, 360);
                ShowWorkflowPopup(NewWatchlistPopup);
            });
        }

        private void OnAddWatchlistFromSelectorTapped(object? sender, TappedEventArgs e)
        {
            OnAddWatchlistFromSelectorClicked(sender, e);
        }

        private void OnWatchlistOverflowClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel || viewModel.SelectedWatchlist is null)
            {
                return;
            }

            ConfigureSheet(WatchlistMenuPopup, 240, viewModel.CanDeleteSelectedWatchlist ? 105 : 52);
            if (Width >= MobileBreakpoint)
            {
                PopupPresentation.ApplyDesktopCardStyle(WatchlistMenuPopup);
            }
            else
            {
                WatchlistMenuPopup.PopupStyle.CornerRadius = 0;
            }
            WatchlistMenuPopup.PopupStyle.Stroke = Colors.Transparent;
            WatchlistMenuPopup.PopupStyle.StrokeThickness = 0;
            Dispatcher.Dispatch(() =>
            {
                if (CanUsePopup && WatchlistOverflowButton.Handler is not null)
                {
                    SafePopupOperation(() => WatchlistMenuPopup.ShowRelativeToView(WatchlistOverflowButton, PopupRelativePosition.AlignBottomRight, 0, -20));
                }
            });
        }

        private void OnEditWatchlistFromMenuClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && viewModel.SelectedWatchlist is not null)
            {
                SafePopupOperation(WatchlistMenuPopup.Dismiss);
                Dispatcher.Dispatch(() =>
                {
                    if (!CanUsePopup)
                    {
                        return;
                    }

                    viewModel.OpenEditWatchlistPopupCommand.Execute(null);
                    ConfigureSheet(EditWatchlistPopup, 620, Width > 0 && Width < MobileBreakpoint ? 440 : 460);
                    ShowWorkflowPopup(EditWatchlistPopup);
                });
            }
        }

        private void OnDeleteWatchlistFromMenuClicked(object? sender, EventArgs e)
        {
            SafePopupOperation(WatchlistMenuPopup.Dismiss);
            if (BindingContext is StockChartViewModel viewModel && viewModel.CanDeleteSelectedWatchlist)
            {
                Dispatcher.Dispatch(() =>
                {
                    if (!CanUsePopup)
                    {
                        return;
                    }

                    viewModel.OpenDeleteWatchlistPopupCommand.Execute(null);
                    ConfigureSheet(DeleteWatchlistPopup, 530, Width > 0 && Width < MobileBreakpoint ? 215 : 200);
                    ShowWorkflowPopup(DeleteWatchlistPopup);
                });
            }
        }

        private void OnDeleteWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(DeleteWatchlistPopup.Dismiss);

        private void OnDeleteWatchlistConfirmClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ConfirmDeleteWatchlistCommand.Execute(null);
            }

            SafePopupOperation(DeleteWatchlistPopup.Dismiss);
        }

        private void OnAddStockCloseClicked(object? sender, EventArgs e) => SafePopupOperation(AddStockPopup.Dismiss);

        private void OnFavoriteClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel || !viewModel.CanOpenFavoritePopup)
            {
                return;
            }

            viewModel.OpenFavoriteOrRemovePopupCommand.Execute(null);
            if (viewModel.RequestedPopup == StockChartViewModel.WorkflowPopup.RemoveStock)
            {
                ConfigureSheet(RemoveStockPopup, 530, 360);
                ShowWorkflowPopup(RemoveStockPopup);
            }
            else
            {
                ConfigureSheet(FavoritePopup, 420, 360);
                ShowWorkflowPopup(FavoritePopup);
            }
        }

        private void OnStockMoreClicked(object? sender, EventArgs e)
        {
            if (BindingContext is not StockChartViewModel viewModel ||
                sender is not Element { BindingContext: StockModel stock })
            {
                return;
            }

            viewModel.OpenStockActionPopupCommand.Execute(stock);
            if (viewModel.RequestedPopup == StockChartViewModel.WorkflowPopup.RemoveStock)
            {
                ConfigureSheet(RemoveStockPopup, 530, 360);
                ShowWorkflowPopup(RemoveStockPopup);
            }
            else
            {
                ConfigureSheet(FavoritePopup, 420, 360);
                ShowWorkflowPopup(FavoritePopup);
            }
        }
        private void OnFavoriteCloseClicked(object? sender, EventArgs e) => SafePopupOperation(FavoritePopup.Dismiss);

        private void OnFavoriteApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ConfirmFavoriteStockCommand.Execute(null);
            }

            SafePopupOperation(FavoritePopup.Dismiss);
        }

        private void OnRemoveStockCloseClicked(object? sender, EventArgs e) => SafePopupOperation(RemoveStockPopup.Dismiss);

        private void OnRemoveStockApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ConfirmRemoveStockCommand.Execute(null);
            }

            SafePopupOperation(RemoveStockPopup.Dismiss);
        }

        private void OnAddStockApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && viewModel.PendingStock is not null)
            {
                viewModel.ConfirmAddStockCommand.Execute(null);
                SafePopupOperation(AddStockPopup.Dismiss);
            }
        }
        private void OnNewWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(NewWatchlistPopup.Dismiss);

        private void OnNewWatchlistCreateClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.NewWatchlistName))
            {
                viewModel.ConfirmNewWatchlistCommand.Execute(null);
                SafePopupOperation(NewWatchlistPopup.Dismiss);
            }
        }
        private void OnEditWatchlistCloseClicked(object? sender, EventArgs e) => SafePopupOperation(EditWatchlistPopup.Dismiss);

        private void OnEditWatchlistApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel && !string.IsNullOrWhiteSpace(viewModel.EditWatchlistName))
            {
                viewModel.ConfirmEditWatchlistCommand.Execute(null);
                SafePopupOperation(EditWatchlistPopup.Dismiss);
            }
        }
        private static void ConfigureSheet(SfPopup popup, double width, double height)
        {
            popup.WidthRequest = width;
            if (height > 0)
            {
                popup.HeightRequest = height;
            }
            else
            {
                popup.ClearValue(VisualElement.HeightRequestProperty);
            }
        }

        private void OnSettingsCloseClicked(object? sender, EventArgs e)
        {
            CloseSettingsPopup();
        }

        private void OnSettingsCancelClicked(object? sender, EventArgs e)
        {
            CloseSettingsPopup();
        }

        private void CloseSettingsPopup()
        {
            SafePopupOperation(SettingsPopup.Dismiss);
            SettingsPopup.ClearValue(VisualElement.WidthRequestProperty);
            SettingsPopup.ClearValue(VisualElement.HeightRequestProperty);
        }

        private void OnSettingsApplyClicked(object? sender, EventArgs e)
        {
            if (BindingContext is StockChartViewModel viewModel)
            {
                viewModel.ApplySettingsDraft();
                ApplyChartSettings(viewModel);
                viewModel.CompleteSettingsWorkflow();
            }

            CloseSettingsPopup();
        }
    }
}
