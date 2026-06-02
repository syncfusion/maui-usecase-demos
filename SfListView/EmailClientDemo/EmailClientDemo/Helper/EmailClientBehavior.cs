using Microsoft.Maui.Platform;
using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using Syncfusion.Maui.PullToRefresh;
using Syncfusion.Maui.TreeView;
using System.Collections.ObjectModel;

#nullable disable
namespace EmailClientDemo
{
	/// <summary>
	/// Behavior class for EmailClientDemo sample.
	/// </summary>
	public class EmailClientBehavior : Behavior<ContentPage>
	{
		private SfPullToRefresh pullToRefresh;
		private Syncfusion.Maui.ListView.SfListView listView;
		private EmailClientViewModel viewModel;
		private SfEntry searchBar;
        private EmailInfo currentSelectedEmail;
        private Grid previewGrid;
        private VerticalStackLayout verticalStack;
        private SfTreeView treeView;



        /// <summary>
        /// You can override this method to subscribe to AssociatedObject events and initialize properties.
        /// </summary>
        /// <param name="bindable">SampleView type parameter named as bindable.</param>
        protected override void OnAttachedTo(ContentPage bindable)
		{
			this.viewModel = new EmailClientViewModel();
			bindable.BindingContext = viewModel;
			this.pullToRefresh = bindable.FindByName<SfPullToRefresh>("pullToRefresh");
			this.listView = bindable.FindByName<Syncfusion.Maui.ListView.SfListView>("listView");
			this.searchBar = bindable.FindByName<SfEntry>("searchBar");
            this.previewGrid = bindable.FindByName<Grid>("previewGrid");
            this.verticalStack = bindable.FindByName<VerticalStackLayout>("verticalStack");
            this.treeView = bindable.FindByName<SfTreeView>("treeView");

            if (this.treeView != null)
            {
                this.treeView.SelectionChanged += TreeView_SelectionChanged;
            }


            if (this.pullToRefresh != null)
			{
				this.pullToRefresh.Refreshing += PullToRefresh_Refreshing;
				this.pullToRefresh.Refreshed += PullToRefresh_Refreshed;
			}

			this.searchBar.TextChanged += SearchBar_TextChanged;
			this.listView.QueryItemSize += ListView_QueryItemSize;

            this.listView!.ItemTapped += ListView_ItemTapped;

            this.listView!.DataSource!.SortDescriptors.Add(new SortDescriptor()
			{
				PropertyName = "Date",
				Direction = Syncfusion.Maui.DataSource.ListSortDirection.Descending,
			});

				// Configure grouping
			this.listView.DataSource.GroupDescriptors.Add(new GroupDescriptor()
			{
				PropertyName = "Date",
				KeySelector = (obj) =>
				{
					var groupDate = ((EmailInfo)obj).Date;
					return GetGroupKey(groupDate);
				},
				Comparer = new EmailGroupComparer(),
			});

			this.listView.DataSource.LiveDataUpdateMode = LiveDataUpdateMode.AllowDataShaping;

			// Initialize filter
			this.listView.DataSource.Filter = FilterEmails;

			base.OnAttachedTo(bindable);
		}

        private void TreeView_SelectionChanged(object sender, Syncfusion.Maui.TreeView.ItemSelectionChangedEventArgs e)
        {
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            var selectedItem = e.AddedItems[0];

            if (selectedItem is MailFolder folder)
            {
                // ✅ Load that folder mails in middle panel
                if (folder.Emails != null && folder.Emails.Count > 0)
                {
                    viewModel.EmailInfos = folder.Emails;
                }
                else
                {
                    // ✅ If no direct emails (like parent folder)
                    var merged = new ObservableCollection<EmailInfo>();

                    foreach (var child in folder.Children)
                    {
                        foreach (var mail in child.Emails)
                        {
                            merged.Add(mail);
                        }
                    }

                    viewModel.EmailInfos = merged;
                }
            }
        }


        /// <summary>
        /// Fired when search bar text changed.
        /// </summary>
        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
		{
			viewModel.SearchText = e.NewTextValue;
			if (listView.DataSource != null)
			{
				listView.DataSource.RefreshFilter();
			}
		}

        private void ApplyFolderFilter(string folderName)
        {
            if (viewModel == null || listView == null)
                return;

            switch (folderName)
            {
                case "Inbox":
                    listView.ItemsSource = viewModel.FocusedEmails;
                    break;

                case "Sent Items":
                    listView.ItemsSource = viewModel.OtherEmails;
                    break;

                case "Archive":
                    listView.ItemsSource = viewModel.ArchivedEmails;
                    break;

                case "Deleted":
                    listView.ItemsSource = new ObservableCollection<EmailInfo>();
                    break;

                default:
                    listView.ItemsSource = viewModel.EmailInfos;
                    break;
            }
        }

		/// <summary>
		/// Filter method for emails based on search text.
		/// </summary>
		private bool FilterEmails(object obj)
		{
			var email = obj as EmailInfo;
			if (email == null)
				return true;

			// If no search text, show all emails
			if (string.IsNullOrWhiteSpace(viewModel.SearchText))
				return true;

			var searchTerm = viewModel.SearchText.ToLower();

			// Search in sender name, subject, and description
			return email.SenderName?.ToLower().Contains(searchTerm) == true ||
				   email.Subject?.ToLower().Contains(searchTerm) == true ||
				   email.Description?.ToLower().Contains(searchTerm) == true;
		}

		/// <summary>
		/// Fired when pulltorefresh view is refreshed.
		/// </summary>
		private void PullToRefresh_Refreshed(object sender, EventArgs e)
		{
#if ANDROID
			(this.listView!.Handler!.PlatformView as Android.Views.View)!.InvalidateMeasure(this.listView as IView);
#endif
		}

		/// <summary>
		/// Fired when pullToRefresh View is refreshing.
		/// </summary>
		private async void PullToRefresh_Refreshing(object sender, EventArgs e)
		{
			this.pullToRefresh!.IsRefreshing = true;
			await Task.Delay(2500);
			this.viewModel!.AddItemsRefresh(3);
			this.pullToRefresh.IsRefreshing = false;
		}

		/// <summary>
		/// Fired when taps the listview item.
		/// </summary>
		private void ListView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
		{

            if (e.DataItem is EmailInfo selectedEmail)
            {
                currentSelectedEmail = selectedEmail;

                // Mark email as opened
                selectedEmail.IsOpened = true;

#if WINDOWS || MACCATALYST

                // Display email details in preview pane
                DisplayEmailPreview(selectedEmail);
#endif
            }

        }

        /// <summary>
        /// Displays the email details in the preview grid - Outlook-style preview.
        /// </summary>
        private void DisplayEmailPreview(EmailInfo email)
        {
            if (email == null) return;

            // Hide the default view
            verticalStack.IsVisible = false;

            // Create ScrollView for the email content
            var scrollView = new ScrollView
            {
                Padding = new Thickness(0)
            };

            // Main container
            var previewContent = new VerticalStackLayout
            {
                Spacing = 0,
                Padding = new Thickness(24, 20)
            };

            // Action buttons bar (like Outlook)
            var actionBar = new HorizontalStackLayout
            {
                Spacing = 8,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var replyButton = CreateActionButton("\uE744", "Reply");
            var replyAllButton = CreateActionButton("\uE744", "Reply All");
            var forwardButton = CreateActionButton("\uE791", "Forward");

            actionBar.Add(replyButton);
            actionBar.Add(replyAllButton);
            actionBar.Add(forwardButton);
            previewContent.Add(actionBar);

            // Subject line (large and bold) - expanded with more context
            var expandedSubject = email.Subject;

            var subjectLabel = new Label
            {
                Text = expandedSubject,
                FontSize = 21,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 0, 0, 16),
                TextColor = Colors.Black,
                MaxLines = -1  // Allow unlimited lines
            };
            previewContent.Add(subjectLabel);

            // Header section with sender info
            var headerGrid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnSpacing = 12,
                RowSpacing = 4,
                Margin = new Thickness(0, 0, 0, 16)
            };

            // Avatar
            var avatarBorder = new Border
            {
                HeightRequest = 48,
                WidthRequest = 48,
                BackgroundColor = email.AvatarBackgroundColor ?? Color.FromArgb("#0078D4"),
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start,
                StrokeThickness = 0,
                Padding = new Thickness(0)
            };
            avatarBorder.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 };
            avatarBorder.Content = new Label
            {
                Text = email.ProfileName,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetRow(avatarBorder, 0);
            Grid.SetColumn(avatarBorder, 0);
            Grid.SetRowSpan(avatarBorder, 2);
            headerGrid.Add(avatarBorder);

            // Sender name
            var senderLabel = new Label
            {
                Text = email.SenderName,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                LineBreakMode = LineBreakMode.TailTruncation,
                VerticalOptions = LayoutOptions.End
            };
            Grid.SetRow(senderLabel, 0);
            Grid.SetColumn(senderLabel, 1);
            headerGrid.Add(senderLabel);

            // Date/time (right aligned)
            var dateLabel = new Label
            {
                Text = email.Date.ToString("ddd M/d/yyyy h:mm tt"),
                FontSize = 13,
                TextColor = Color.FromArgb("#666666"),
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.End
            };
            Grid.SetRow(dateLabel, 0);
            Grid.SetColumn(dateLabel, 2);
            headerGrid.Add(dateLabel);

            // To/Cc line
            var recipientLabel = new Label
            {
                Text = "To  Chennai Product Teams; TSM\nCc  Extension Team",
                FontSize = 13,
                TextColor = Color.FromArgb("#666666"),
                LineBreakMode = LineBreakMode.WordWrap,
                VerticalOptions = LayoutOptions.Start
            };
            Grid.SetRow(recipientLabel, 1);
            Grid.SetColumn(recipientLabel, 1);
            Grid.SetColumnSpan(recipientLabel, 2);
            headerGrid.Add(recipientLabel);

            previewContent.Add(headerGrid);

            // Separator line
            previewContent.Add(new BoxView
            {
                Color = Color.FromArgb("#E1E1E1"),
                HeightRequest = 1,
                Margin = new Thickness(0, 8, 0, 20)
            });

            // Email body content - expanded with more detailed text
            var expandedBody = GenerateExpandedEmailBody(email);

            var bodyLabel = new Label
            {
                Text = expandedBody,
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap,
                TextColor = Color.FromArgb("#333333"),
                LineHeight = 1.5,
                MaxLines = -1  // Allow unlimited lines
            };
            previewContent.Add(bodyLabel);

            // Set scroll view content
            scrollView.Content = previewContent;

            // Update the preview grid
            previewGrid.Children.Clear();
            previewGrid.Add(scrollView);
        }

        /// <summary>
        /// Generates expanded email body content with more detailed text.
        /// </summary>
        private string GenerateExpandedEmailBody(EmailInfo email)
        {
            if (email == null)
                return string.Empty;

            string subject = email.Subject ?? "";
            string body = $"Hi Team,\n\n";

            // ✅ PROJECT / WORK MAILS
            if (subject.Contains("Project", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This is regarding the project update.\n\n";
                body += $"Task details:\n";
                body += $"• Progress updated successfully\n";
                body += $"• Pending items need review\n";
                body += $"• Timeline adjustments are required\n\n";
                body += $"Please review the current status and share your feedback.\n\n";
            }

            // ✅ MEETING MAILS
            else if (subject.Contains("Meeting", StringComparison.OrdinalIgnoreCase))
            {
                body += $"You are invited to attend the scheduled meeting.\n\n";
                body += $"Meeting Details:\n";
                body += $"• Duration: 1 hour\n";
                body += $"• Mode: Teams / Conference Room\n";
                body += $"• Agenda: Discussion & Planning\n\n";
                body += $"Kindly join on time and come prepared.\n\n";
            }

            // ✅ FINANCE / INVOICE MAILS
            else if (subject.Contains("Invoice", StringComparison.OrdinalIgnoreCase))
            {
                body += $"Invoice details are shared for your review.\n\n";
                body += $"• Payment status: Pending\n";
                body += $"• Amount due: As per attached invoice\n";
                body += $"• Due date: Within 7 days\n\n";
                body += $"Please complete the payment process.\n\n";
            }

            // ✅ SUPPORT / ISSUE MAILS
            else if (subject.Contains("Issue", StringComparison.OrdinalIgnoreCase) ||
                     subject.Contains("Ticket", StringComparison.OrdinalIgnoreCase))
            {
                body += $"A support issue has been reported.\n\n";
                body += $"Details:\n";
                body += $"• Issue reproduced successfully\n";
                body += $"• Root cause under investigation\n\n";
                body += $"Our team is working to resolve this at the earliest.\n\n";
            }

            // ✅ RELEASE / UPDATES
            else if (subject.Contains("Release", StringComparison.OrdinalIgnoreCase))
            {
                body += $"A new release has been deployed.\n\n";
                body += $"Highlights:\n";
                body += $"• Bug fixes and stability improvements\n";
                body += $"• Performance enhancements\n";
                body += $"• New feature additions\n\n";
                body += $"Please explore and share feedback.\n\n";
            }

            // ✅ DRAFT MAILS
            else if (subject.Contains("Draft", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This is a saved draft email.\n\n";
                body += $"Please review and update the content before sending.\n\n";
            }

            // ✅ SENT MAILS
            else if (subject.Contains("Sent", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This email has been successfully delivered.\n\n";
                body += $"Delivery confirmation received for all recipients.\n\n";
            }

            // ✅ DELETED MAILS
            else if (subject.Contains("Deleted", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This message was moved to deleted folder.\n\n";
                body += $"It will be permanently removed after retention period.\n\n";
            }

            // ✅ ARCHIVE MAILS
            else if (subject.Contains("Archived", StringComparison.OrdinalIgnoreCase) ||
                     subject.Contains("Archive", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This email is stored for historical reference.\n\n";
                body += $"You can restore it if needed for future use.\n\n";
            }

            // ✅ IMPORTANT MAILS
            else if (subject.Contains("Important", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This is a high priority message.\n\n";
                body += $"Immediate action is required from your end.\n\n";
            }

            // ✅ STARRED MAILS
            else if (subject.Contains("Starred", StringComparison.OrdinalIgnoreCase))
            {
                body += $"This mail is marked as important for quick access.\n\n";
                body += $"Please follow up as required.\n\n";
            }

            // ✅ DEFAULT (Inbox General)
            else
            {
                body += $"{email.Description}\n\n";
                body += $"Please let us know if any clarification is required.\n\n";
            }

            body += $"Best Regards,\n";
            body += email.SenderName;

            return body;
        }

        /// <summary>
        /// Creates an action button for the email preview (Reply, Forward, etc.)
        /// </summary>
        private Border CreateActionButton(string icon, string tooltip)
        {
            var border = new Border
            {
                BackgroundColor = Colors.Transparent,
                Stroke = Color.FromArgb("#E1E1E1"),
                StrokeThickness = 1,
                Padding = new Thickness(12, 8),
                Margin = new Thickness(0, 0, 4, 0)
            };
            border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 4 };

            var stack = new HorizontalStackLayout
            {
                Spacing = 6
            };

            var iconLabel = new Label
            {
                Text = icon,
                FontFamily = "MaterialAssets",
                FontSize = 14,
                TextColor = Color.FromArgb("#333333"),
                VerticalOptions = LayoutOptions.Center
            };

            var textLabel = new Label
            {
                Text = tooltip,
                FontSize = 13,
                TextColor = Color.FromArgb("#333333"),
                VerticalOptions = LayoutOptions.Center
            };

            stack.Add(iconLabel);
            stack.Add(textLabel);
            border.Content = stack;

            // Add tap gesture
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                // Handle button click (Reply, Forward, etc.)
            };
            border.GestureRecognizers.Add(tapGesture);

            return border;
        }

        /// <summary>
        /// Fires whenever an item comes to view.
        /// </summary>
        private void ListView_QueryItemSize(object sender, QueryItemSizeEventArgs e)
		{

		}

		/// <summary>
		/// You can override this method while View was detached from window.
		/// </summary>
		protected override void OnDetachingFrom(ContentPage bindable)
		{
			this.listView!.ItemTapped -= ListView_ItemTapped;
			this.listView.QueryItemSize -= ListView_QueryItemSize;
			this.pullToRefresh!.Refreshing -= PullToRefresh_Refreshing;
			this.pullToRefresh.Refreshed -= PullToRefresh_Refreshed;
			this.searchBar.TextChanged -= SearchBar_TextChanged;
            this.treeView.SelectionChanged -= TreeView_SelectionChanged;
            this.treeView = null;
            this.pullToRefresh = null;
			this.listView = null;
			this.viewModel = null;
			this.searchBar = null;
		}

		private EmailGroupName GetGroupKey(DateTime groupDate)
		{
			int compare = groupDate.Date.CompareTo(DateTime.Now.Date);

			if (compare == 0)
			{
				return EmailGroupName.Today;
			}
			else if (groupDate.Date.CompareTo(DateTime.Now.AddDays(-1).Date) == 0)
			{
				return EmailGroupName.Yesterday;
			}
			else
			{
				return EmailGroupName.Earlier;
			}
		}
	}

	#region EmailGroupComparer

	/// <summary>
	/// Comparer class helps to sort the groups based on Date property.
	/// </summary>
	public class EmailGroupComparer : IComparer<GroupResult>
	{
		public int Compare(GroupResult x, GroupResult y)
		{
			var xenum = (EmailGroupName)x!.Key;
			var yenum = (EmailGroupName)y!.Key;

			if (xenum.CompareTo(yenum) == -1)
			{
				return -1;
			}
			else if (xenum.CompareTo(yenum) == 1)
			{
				return 1;
			}

			return 0;
		}
	}

	#endregion

	#region EmailGroupName Enum

	/// <summary>
	/// Enum for email group names.
	/// </summary>
	public enum EmailGroupName
	{
		Today = 0,
		Yesterday,
		Earlier
	}

	#endregion
}
