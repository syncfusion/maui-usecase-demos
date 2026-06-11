using System.Collections.ObjectModel;

namespace EmailClientDemo
{
	/// <summary>
	/// Repository class to generate email information data.
	/// </summary>
	public class EmailInfoRepository
	{
		#region Fields

		private Random random = new Random();
		private Color[] avatarColors = new Color[]
		{
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
			Color.FromArgb("#9E9E9E"),  // Gray
		};

		#endregion

		#region Get email info

		/// <summary>
		/// Used to assign the Collection values to Model Properties.
		/// </summary>
		/// <returns>Added EmailInfos items</returns>
		internal ObservableCollection<EmailInfo> GetEmailInfo()
		{
			var emailInfo = new ObservableCollection<EmailInfo>();
			int k = 0;
			for (int i = 0; i < SenderNames.Count(); i++)
			{
				if (k > 5)
				{
					k = 0;
				}
				var record = new EmailInfo()
				{
					ProfileName = ProfileList[i],
					SenderName = SenderNames[i],
					Subject = Subjects[i],
					Date = i == 0 ? DateTime.Now.AddMinutes(-34) : 
					       i == 1 ? DateTime.Now.AddHours(-2) :
					       i == 2 ? DateTime.Now.AddHours(-5) :
					       i < 6 ? DateTime.Today.AddDays(-1).AddHours(-(i-2) * 3) :
					       i < 10 ? DateTime.Today.AddDays(-2).AddHours(-(i-5) * 2) :
					       DateTime.Today.AddDays(-(i-7)).AddHours(-8),
					Description = Descriptions[i],
					Image = Images[k],
					IsAttached = Attachments[i],
					IsImportant = Importants[i],
					IsOpened = Opens[i],
					IsFavorite = Favorites[i],
					AvatarBackgroundColor = avatarColors[i],
				};
				emailInfo.Add(record);
				k++;
			}
			return emailInfo;
		}

		/// <summary>
		/// Used to assign the Collection values to Model Properties while refreshing.
		/// </summary>
		/// <param name="count">Number of items to be added.</param>
		/// <returns>Added emailInfos items</returns>
		internal ObservableCollection<EmailInfo> AddRefreshItems(int count)
		{
			var emailInfo = new ObservableCollection<EmailInfo>();
			int k = 0;
			for (int i = 0; i < count; i++)
			{
				var j = random.Next(SenderNames.Count());

				if (k > 5)
				{
					k = 0;
				}
				var record = new EmailInfo()
				{
					ProfileName = ProfileList[j],
					SenderName = SenderNames[j],
					Subject = Subjects[j],
					Date = DateTime.Now.AddMinutes(-i * 5),
					Description = Descriptions[j],
					Image = Images[k],
					IsAttached = Attachments[j],
					IsImportant = false,
					IsOpened = false,
					IsFavorite = false,
					AvatarBackgroundColor = avatarColors[j],
				};
				emailInfo.Add(record);
				k++;
			}

			return emailInfo;
		}

		#endregion

		#region Email Data

		string[] ProfileList = new string[]
		{
			"Ji",
			"JS",
			"KN",
			"ES",
			"JA",
			"M",
			"MV",
			"T",
			"LI",
			"SO",
			"OT",
			"MA",
			"BT",
			"MS",
			"G",
		};

		string[] SenderNames = new string[]
		{
			"Jimbradna",
			"Jamie Spencer",
			"Kinasar Nitow",
			"Elliot Sale",
			"Jill Alvarez",
			"Microsoft",
			"Microsoft Viva",
			"Twitter",
			"LinkedIn",
			"Stack Overflow",
			"Outlook Team",
			"My Analytics",
			"Blog Team Site",
			"Microsoft Store",
			"GitHub",
		};

		string[] Images = new string[]
		{
			"bluecircle.png",
			"greencircle.png",
			"lightbluecircle.png",
			"redcircle.png",
			"violetcircle.png",
			"yellowcircle.png",
		};

		bool[] Attachments = new bool[]
		{
			false,
			false,
			false,
			true,
			true,
			false,
			false,
			true,
			true,
			true,
			false,
			true,
			false,
			true,
			false,
		};

		bool[] Importants = new bool[]
		{
			false,
			false,
			false,
			false,
			false,
			false,
			true,
			false,
			false,
			false,
			true,
			false,
			true,
			false,
			false,
		};

		bool[] Opens = new bool[]
		{
			true,   // 0: Jimbradna - read (show avatar)
			false,  // 1: Jamie Spencer - unread (show blue dot)
			true,   // 2: Kinasar Nitow - read (show avatar)
			true,   // 3: Elliot Sale - read
			true,   // 4: Jill Alvarez - read
			true,   // 5: Microsoft - read
			true,   // 6: Microsoft Viva - read
			true,   // 7: Twitter - read
			true,   // 8: LinkedIn - read
			false,   // 9: Stack Overflow - unread
			true,   // 10: Outlook Team - read
			true,   // 11: My Analytics - read
			true,   // 12: Blog Team Site - read
			true,   // 13: Microsoft Store - read
			true,   // 14: GitHub - read
		};

		bool[] Favorites = new bool[]
		{
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
			false,
		};

		string[] Subjects = new string[]
		{
			"Weekly meeting",
			"Service update",
			"Another question",
			"Ready for launch",
			"Sign documents",
			"Dev Essentials: Learn about the future of .NET",
			"Your daily briefing",
			"Be more recognizable",
			"You have two new messages",
			"Your friendly, fear-free guide to getting started",
			"Get to know what's new in Outlook",
			"My Analytics | Collaboration Edition",
			"You've joined the Blog Team Site group",
			"New apps and games available",
			"Your pull request was merged",
		};

		string[] Descriptions = new string[]
		{
			"Let's go over mareting presentation last...",
			"The changes were successfully installed.",
			"I have a follow-up question about...",
			"The updated slides are attached to this...",
			"Kindly review the documents and sign...",
			"Developer news, updates, and training resources.",
			"Dear developer, It's almost the end of the week",
			"Stand out with a profile photo.",
			"You have two new messages.",
			"How to learn and get started with Stack Overflow.",
			"Hello and welcome to Outlook.",
			"Discover your habits. Work smarter.",
			"Welcome to the Blog Team Site group.",
			"Check out the latest apps and games.",
			"Your contribution has been merged successfully.",
		};

		#endregion
	}
}
