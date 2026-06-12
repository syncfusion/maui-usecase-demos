using Syncfusion.Maui.Core;
using Syncfusion.Maui.Popup;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace FileExplorerApp
{

    public class PopupService
    {
        private SfPopup popup;

        // ✅ Rename Popup
        public void ShowRenamePopup(Page page, string currentName, Action<string> onSave, Action onCancel)
        {
            var entry = new Entry { Text = currentName };

            popup = new SfPopup
            {
                WidthRequest = 300,
                HeightRequest = 200,
                ShowHeader = false,
                ShowFooter = true,
                AcceptButtonText = "Save",
                DeclineButtonText = "Cancel",
                ShowOverlayAlways = false,
                AppearanceMode = PopupButtonAppearanceMode.TwoButton,
                ContentTemplate = new DataTemplate(() =>
                {
                    return new StackLayout
                    {
                        Padding = 30, Spacing = 20,
                        Children =
                    {
                        new Label { Text="Rename Folder", FontSize=18 },
                        entry,
                    }
                    };
                })
            };
            popup.AcceptCommand = new Command(() =>
            {
                popup.IsOpen = false;
                onSave?.Invoke(entry.Text);
            });

            popup.DeclineCommand = new Command(() =>
            {
                popup.IsOpen = false;
                onCancel?.Invoke();
            });
            popup.Show();


        }


        public void ShowDeletePopup(Page page, Action onConfirm, Action onCancel)
        {
            popup = new SfPopup
            {
                WidthRequest = 300,
                HeightRequest = 150,
                ShowHeader = false,
                ShowFooter = true,
                AcceptButtonText = "Delete",
                DeclineButtonText = "Cancel",
                ShowOverlayAlways = false,
                AppearanceMode = PopupButtonAppearanceMode.TwoButton,
                ContentTemplate = new DataTemplate(() =>
                {
                    return new StackLayout
                    {
                        Padding = 40,
                        Children =
                    {
                        new Label { Text="Delete selected item(s)?"},
                    }
                    };
                })
            };
            popup.AcceptCommand = new Command(() =>
            {
                popup.IsOpen = false;
                onConfirm?.Invoke();
            });

            popup.DeclineCommand = new Command(() =>
            {
                popup.IsOpen = false;
                onCancel?.Invoke();
            });

            popup.Show();
        }

    }

}
