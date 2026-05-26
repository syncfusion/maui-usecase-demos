using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EmailClientDemo
{
	/// <summary>
	/// ViewModel for EmailClientDemo sample.
	/// </summary>
	public class EmailClientViewModel : INotifyPropertyChanged
	{
		#region Fields

		private ObservableCollection<EmailInfo>? emailInfos;
		private ObservableCollection<EmailInfo>? focusedEmails;
		private ObservableCollection<EmailInfo>? otherEmails;
		private ObservableCollection<EmailInfo>? archivedEmails;
		private Command? favoriteCommand;
		private Command? deleteCommand;
		private Command? archiveCommand;
		private Command? undoCommand;
		private Command? switchTabCommand;
		private bool isDeleted;
		private EmailInfo? currentEmailItem;
		private int currentEmailItemIndex;
		private string? popUpText;
		private string? searchText;
		private bool isFocusedTabSelected = true;

		#endregion

	    #region EventHandler

		/// <summary>
		/// Event to reset swipe view.
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the EmailClientViewModel class.
        /// </summary>
        public EmailClientViewModel()
		{
			// Ensure any additional initialization is applied
			GenerateSource();
		}

		#endregion

        #region Generate Source

        /// <summary>
        /// Ensures collections and commands are initialized.
        /// </summary>
        private void GenerateSource()
		{
			EmailInfoRepository repository = new EmailInfoRepository();

			if (this.archivedEmails == null)
				this.archivedEmails = new ObservableCollection<EmailInfo>();

			// Get all emails
			var allEmails = repository.GetEmailInfo();
			
			// Split emails into Focused and Other
			if (this.focusedEmails == null)
			{
				this.focusedEmails = new ObservableCollection<EmailInfo>();
				// Add emails that are unread or important to Focused
				foreach (var email in allEmails)
				{
					if (!email.IsOpened || email.IsImportant)
					{
						this.focusedEmails.Add(email);
					}
				}
			}
			
			if (this.otherEmails == null)
			{
				this.otherEmails = new ObservableCollection<EmailInfo>();
				// Add emails that are read and not important to Other
				foreach (var email in allEmails)
				{
					if (email.IsOpened && !email.IsImportant)
					{
						this.otherEmails.Add(email);
					}
				}
			}

			// Initialize EmailInfos with FocusedEmails by default
			if (this.emailInfos == null)
				this.emailInfos = new ObservableCollection<EmailInfo>(this.focusedEmails);

			if (this.deleteCommand == null)
				this.deleteCommand = new Command(Delete);

			if (this.undoCommand == null)
				this.undoCommand = new Command(UndoAction);

			if (this.favoriteCommand == null)
				this.favoriteCommand = new Command(SetFavorite);

			if (this.archiveCommand == null)
				this.archiveCommand = new Command(Archive);

			if (this.switchTabCommand == null)
				this.switchTabCommand = new Command<string>(SwitchTab);
		}
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the EmailInfos type of ObservableCollection and notifies user when collection value gets changed.
        /// </summary>
        public ObservableCollection<EmailInfo>? EmailInfos
		{
			get { return emailInfos; }
			set { emailInfos = value; OnPropertyChanged("EmailInfos"); }
		}

        /// <summary>
        /// Gets or sets the FocusedEmails type of ObservableCollection.
        /// </summary>
        public ObservableCollection<EmailInfo>? FocusedEmails
		{
			get { return focusedEmails; }
			set { focusedEmails = value; OnPropertyChanged("FocusedEmails"); }
		}

		/// <summary>
		/// Gets or sets the OtherEmails type of ObservableCollection.
		/// </summary>
		public ObservableCollection<EmailInfo>? OtherEmails
		{
			get { return otherEmails; }
			set { otherEmails = value; OnPropertyChanged("OtherEmails"); }
		}

		/// <summary>
		/// Gets or sets the ArchivedEmails type of ObservableCollection and notifies user when collection value gets changed.
		/// </summary>
		public ObservableCollection<EmailInfo>? ArchivedEmails
		{
			get { return archivedEmails; }
			set { archivedEmails = value; OnPropertyChanged("ArchivedEmails"); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Focused tab is selected.
		/// </summary>
		public bool IsFocusedTabSelected
		{
			get { return isFocusedTabSelected; }
			set { isFocusedTabSelected = value; OnPropertyChanged("IsFocusedTabSelected"); }
		}

		/// <summary>
		/// Gets or sets the FavoriteCommand.
		/// </summary>
		public Command? FavoriteCommand
		{
			get { return favoriteCommand; }
			protected set { favoriteCommand = value; }
		}

		/// <summary>
		/// Gets or sets the DeleteCommand.
		/// </summary>
		public Command? DeleteCommand
		{
			get { return deleteCommand; }
			protected set { deleteCommand = value; }
		}

		/// <summary>
		/// Gets or sets the ArchiveCommand.
		/// </summary>
		public Command? ArchiveCommand
		{
			get { return archiveCommand; }
			protected set { archiveCommand = value; }
		}

		/// <summary>
		/// Gets or sets the UndoCommand.
		/// </summary>
		public Command? UndoCommand
		{
			get { return undoCommand; }
			protected set { undoCommand = value; }
		}

		/// <summary>
		/// Gets or sets the SwitchTabCommand.
		/// </summary>
		public Command<string>? SwitchTabCommand
		{
			get { return (Command<string>?)switchTabCommand; }
			protected set { switchTabCommand = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether IsDeleted.
		/// </summary>
		public bool IsDeleted
		{
			get { return isDeleted; }
			set { isDeleted = value; OnPropertyChanged("IsDeleted"); }
		}

		/// <summary>
		/// Gets or sets the PopUpText.
		/// </summary>
		public string? PopUpText
		{
			get { return popUpText; }
			set { popUpText = value; OnPropertyChanged("PopUpText"); }
		}

		/// <summary>
		/// Gets or sets the SearchText for filtering.
		/// </summary>
		public string? SearchText
		{
			get { return searchText; }
			set { searchText = value; OnPropertyChanged("SearchText"); }
		}

		/// <summary>
		/// This method helps to add emails while refreshing.
		/// </summary>
		/// <param name="count">Represent number of emails to be added.</param>
		public void AddItemsRefresh(int count)
		{
			EmailInfoRepository repository = new EmailInfoRepository();

			foreach (var item in repository.AddRefreshItems(count))
			{
				this.emailInfos!.Insert(0, item);
			}
		}

		/// <summary>
		/// Deletes the email item.
		/// </summary>
		/// <param name="item">Email item to delete.</param>
		private async void Delete(object? item)
		{
			PopUpText = "Deleted";
			currentEmailItem = (EmailInfo?)item;
			if (currentEmailItem != null)
			{
				currentEmailItemIndex = emailInfos!.IndexOf(currentEmailItem);
				emailInfos!.Remove(currentEmailItem);
				this.IsDeleted = true;
				await Task.Delay(3000);
				this.IsDeleted = false;
			}
		}

		/// <summary>
		/// Archives the email item.
		/// </summary>
		/// <param name="item">Email item to archive.</param>
		private async void Archive(object? item)
		{
			PopUpText = "Archived";
			currentEmailItem = (EmailInfo?)item;
			if (currentEmailItem != null)
			{
				currentEmailItemIndex = emailInfos!.IndexOf(currentEmailItem);
				emailInfos!.Remove(currentEmailItem);
				archivedEmails!.Add(currentEmailItem);
				this.IsDeleted = true;
				await Task.Delay(3000);
				this.IsDeleted = false;
			}
		}

		/// <summary>
		/// Undoes the last delete or archive action.
		/// </summary>
		private void UndoAction()
		{
			this.IsDeleted = false;
			if (currentEmailItem != null)
			{
				emailInfos!.Insert(currentEmailItemIndex, currentEmailItem);
			}
			currentEmailItemIndex = 0;
			currentEmailItem = null;
		}

		/// <summary>
		/// Toggles the favorite status of the email item.
		/// </summary>
		/// <param name="item">Email item to toggle favorite.</param>
		private void SetFavorite(object? item)
		{
			var emailItem = item as EmailInfo;
			if (emailItem != null)
			{
				emailItem.IsFavorite = !emailItem.IsFavorite;
			}
		}

		/// <summary>
		/// Switches between Focused and Other tabs.
		/// </summary>
		/// <param name="tabName">Name of the tab to switch to ("Focused" or "Other").</param>
		private void SwitchTab(string? tabName)
		{
			if (tabName == "Focused")
			{
				IsFocusedTabSelected = true;
				EmailInfos = new ObservableCollection<EmailInfo>(FocusedEmails ?? new ObservableCollection<EmailInfo>());
			}
			else if (tabName == "Other")
			{
				IsFocusedTabSelected = false;
				EmailInfos = new ObservableCollection<EmailInfo>(OtherEmails ?? new ObservableCollection<EmailInfo>());
			}
		}

		#endregion
	}
}
