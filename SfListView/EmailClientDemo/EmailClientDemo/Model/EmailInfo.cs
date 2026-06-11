using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EmailClientDemo
{
	/// <summary>
	/// Represents email information model for EmailClientDemo.
	/// </summary>
	public class EmailInfo : INotifyPropertyChanged
	{
		#region Fields

		private string? profileName;
		private string? senderName;
		private string? subject;
		private string? description;
		private DateTime date;
		private string? image;
		private bool isAttached;
		private bool isOpened;
		private bool isFavorite;
		private bool isImportant;
		private Color? avatarBackgroundColor;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the SenderName and notifies user when collection value gets changed.
		/// </summary>
		public string? SenderName
		{
			get
			{
				return senderName;
			}
			set
			{
				senderName = value;
				OnPropertyChanged("SenderName");
			}
		}

		/// <summary>
		/// Gets or sets the ProfileName and notifies user when collection value gets changed.
		/// </summary>
		public string? ProfileName
		{
			get { return profileName; }
			set
			{
				profileName = value;
				OnPropertyChanged("ProfileName");
			}
		}

		/// <summary>
		/// Gets or sets the Subject and notifies user when collection value gets changed.
		/// </summary>
		public string? Subject
		{
			get
			{
				return subject;
			}

			set
			{
				subject = value;
				OnPropertyChanged("Subject");
			}
		}

		/// <summary>
		/// Gets or sets the Description and notifies user when collection value gets changed.
		/// </summary>
		public string? Description
		{
			get
			{
				return description;
			}

			set
			{
				description = value;
				OnPropertyChanged("Description");
			}
		}

		/// <summary>
		/// Gets or sets the Date and notifies user when collection value gets changed.
		/// </summary>
		public DateTime Date
		{
			get
			{
				return date;
			}

			set
			{
				date = value;
				OnPropertyChanged("Date");
			}
		}

		/// <summary>
		/// Gets or sets the Image and notifies user when collection value gets changed.
		/// </summary>
		public string? Image
		{
			get
			{
				return image;
			}

			set
			{
				image = value;
				OnPropertyChanged("Image");
			}
		}

		/// <summary>
		/// Gets or sets the IsAttached and notifies user when collection value gets changed.
		/// </summary>
		public bool IsAttached
		{
			get { return isAttached; }
			set
			{
				isAttached = value;
				OnPropertyChanged("IsAttached");
			}
		}

		/// <summary>
		/// Gets or sets the IsFavorite and notifies user when collection value gets changed.
		/// </summary>
		public bool IsFavorite
		{
			get { return isFavorite; }
			set
			{
				isFavorite = value;
				OnPropertyChanged("IsFavorite");
			}
		}

		/// <summary>
		/// Gets or sets the IsOpened and notifies user when collection value gets changed.
		/// </summary>
		public bool IsOpened
		{
			get { return isOpened; }
			set
			{
				isOpened = value;
				OnPropertyChanged("IsOpened");
			}
		}

		/// <summary>
		/// Gets or sets the IsImportant and notifies user when collection value gets changed.
		/// </summary>
		public bool IsImportant
		{
			get { return isImportant; }
			set
			{
				isImportant = value;
				OnPropertyChanged("IsImportant");
			}
		}

		/// <summary>
		/// Gets or sets the AvatarBackgroundColor and notifies user when collection value gets changed.
		/// </summary>
		public Color? AvatarBackgroundColor
		{
			get { return avatarBackgroundColor; }
			set
			{
				avatarBackgroundColor = value;
				OnPropertyChanged("AvatarBackgroundColor");
			}
		}

		#endregion

		#region Interface Member

		/// <summary>
		/// Represents the method that will handle the <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"></see> event raised when a property is changed on a component
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Triggers when Items Collections Changed.
		/// </summary>
		/// <param name="name">string type parameter represent propertyName as name</param>
		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}


    public class MailFolder
    {
        public string Name { get; set; }
        public string Icon { get; set; }   // ✅ Icon
        public int Count { get; set; }     // ✅ Unread count
        public bool IsHeader { get; set; } // ✅ For email ID section
        public ObservableCollection<MailFolder> Children { get; set; }
        public ObservableCollection<EmailInfo> Emails { get; set; }
        public MailFolder(string name)
        {
            Name = name;
            Children = new ObservableCollection<MailFolder>();
            Emails = new ObservableCollection<EmailInfo>();
        }
    }



}
