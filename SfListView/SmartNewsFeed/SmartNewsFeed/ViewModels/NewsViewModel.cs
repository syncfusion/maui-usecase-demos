using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace SmartNewsFeed
{
    public class NewsViewModel : INotifyPropertyChanged
    {
        private DateTime _lastRefreshTime = DateTime.MinValue;
        private string _lastUpdatedText = string.Empty;
        private bool _isRefreshing;
        private int _newItemsCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<NewsItem> Articles { get; set; }

        public ICommand RefreshCommand { get; }

        public NewsViewModel()
        {
            Articles = new ObservableCollection<NewsItem>();

            RefreshCommand = new Command(async () => await RefreshAsync());

            LoadInitialData();
        }

        #region Properties

        public string LastUpdatedText
        {
            get => _lastUpdatedText;
            set
            {
                _lastUpdatedText = value;
                OnPropertyChanged(nameof(LastUpdatedText));
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public int NewItemsCount
        {
            get => _newItemsCount;
            set
            {
                _newItemsCount = value;
                OnPropertyChanged(nameof(NewItemsCount));
            }
        }

        #endregion

        #region Methods

        private void LoadInitialData()
        {
            Articles.Add(new NewsItem
            {
                Title = "💰 Economic Growth Up for Third Straight Quarter",
                Summary = "Strong consumer spending has supported steady GDP growth this quarter. Analysts point to rising wages and resilient services demand as key drivers. However, economists caution about inflationary pressures that could temper future gains.",
                ImageUrl = "economic_growth.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-5),
            });

            Articles.Add(new NewsItem
            {
                Title = "♻️ Breakthrough in Renewable Energy",
                Summary = "Researchers demonstrated a higher-efficiency solar cell that reduces production costs. Early pilot projects report promising real-world performance. Industry leaders are evaluating scale-up options to accelerate adoption.",
                ImageUrl = "renewable_energy.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-10)
            });

            Articles.Add(new NewsItem
            {
                Title = "📈 Global Markets Show Mixed Trends",
                Summary = "Stock indexes closed mixed as investors weighed corporate earnings and policy signals. Commodity markets responded to shifting demand forecasts. Traders noted increased sector rotation into defensive positions.",
                ImageUrl = "globalimage.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-15)
            });

            Articles.Add(new NewsItem
            {
                Title = "🤖 Tech Companies Expand AI Investments",
                Summary = "Major technology firms announced additional funding for AI research and tooling. The moves aim to accelerate product integration and developer adoption. Observers expect increased competition around model efficiency and safety features.",
                ImageUrl = "techcompanies.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-20)
            });

            Articles.Add(new NewsItem
            {
                Title = "🏥 Healthcare Innovation Gains Momentum",
                Summary = "New clinical trials report encouraging outcomes for a faster-acting therapy. Hospitals are preparing to adopt updated treatment protocols pending regulatory review. Stakeholders emphasize access and cost-effectiveness during rollout planning.",
                ImageUrl = "healthcare.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-25)
            });

            Articles.Add(new NewsItem
            {
                Title = "🚗 Electric Vehicles See Record Sales",
                Summary = "Sales of electric vehicles reached a new quarterly record driven by incentives and broader model availability. Charging infrastructure investments are expanding to meet demand. Automakers announce plans to increase production and reduce costs.",
                ImageUrl = "electric_vehicles.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-30)
            });

            Articles.Add(new NewsItem
            {
                Title = "🚀 Space Exploration Missions Accelerate",
                Summary = "Several launch providers completed successful missions this month, including satellite deployments and technology demonstrations. Collaboration between private and public entities continues to grow. The activity underscores renewed global interest in space capabilities.",
                ImageUrl = "spacex.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-40)
            });

            Articles.Add(new NewsItem
            {
                Title = "🔒 Cybersecurity Threats on the Rise",
                Summary = "A new wave of attacks targeted supply chain infrastructure, prompting urgent advisories. Organizations are urged to patch known vulnerabilities and review access controls. Cyber teams report increased coordination to mitigate ongoing threats.",
                ImageUrl = "cybersecurity.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-50)
            });

            Articles.Add(new NewsItem
            {
                Title = "🎓 Education Sector Embraces Digital Shift",
                Summary = "Institutions expand hybrid learning models and invest in digital platforms to enhance student outcomes. Educators report improved engagement through personalized learning tools. Policymakers discuss funding and equity to ensure broad access.",
                ImageUrl = "educationsector.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-60)
            });

            Articles.Add(new NewsItem
            {
                Title = "🌍 Climate Change Policies Tighten Globally",
                Summary = "New regulatory measures focus on emissions reporting and incentives for clean energy. Governments and businesses negotiate practical timelines for compliance. Environmental groups welcomed the commitments but called for clearer enforcement mechanisms.",
                ImageUrl = "climatechange.jpeg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-70)
            });

            Articles.Add(new NewsItem
            {
                Title = "💼 Startups Drive Innovation in Fintech",
                Summary = "Fintech startups are introducing novel payment rails and credit solutions, attracting venture funding. Traditional banks partner with agile players to modernize services. The landscape is shifting rapidly with a focus on user experience and compliance.",
                ImageUrl = "startup.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-80)
            });

            Articles.Add(new NewsItem
            {
                Title = "🛍️ Retail Industry Adapts to New Consumer Trends",
                Summary = "Retailers optimize omnichannel strategies and invest in supply chain visibility. Brands experiment with experiential retail and subscription models to retain customers. Analysts note improving margins as operations become more efficient.",
                ImageUrl = "retailindustry.jpg",
                PublishedAt = DateTime.UtcNow.AddMinutes(-90)
            });

            UpdateLastUpdated();
        }

        public async Task RefreshAsync()
        {
            if ((DateTime.UtcNow - _lastRefreshTime).TotalSeconds < 2)
                return;

            IsRefreshing = true;
            await Task.Delay(500);
            var newItems = GenerateNews();
            foreach (var item in newItems)
            {
                item.IsNew = true;
                item.IsLoading = true;
                Articles.Insert(0, item);
            }

            NewItemsCount = newItems.Count;
            await Task.Delay(1200);
            
            foreach (var item in newItems)
            {
                item.IsLoading = false;
            }

            _ = Task.Run(async () =>
            {
                await Task.Delay(6000);
                foreach (var item in newItems)
                {
                    item.IsNew = false;
                }
            });

            _lastRefreshTime = DateTime.UtcNow;
            UpdateLastUpdated();
            IsRefreshing = false;
        }

        private void UpdateLastUpdated()
        {
            if (_lastRefreshTime == DateTime.MinValue)
            {
                LastUpdatedText = string.Empty;
                return;
            }

            LastUpdatedText = "Last updated: 2 sec ago";
            _ = Task.Run(async () =>
            {
                await Task.Delay(30000);
                UpdateLastUpdatedText();
                while (_lastRefreshTime != DateTime.MinValue)
                {
                    await Task.Delay(60000);
                    UpdateLastUpdatedText();
                }
            });
        }

        private void UpdateLastUpdatedText()
        {
            if (_lastRefreshTime == DateTime.MinValue)
                return;

            var elapsed = DateTime.UtcNow - _lastRefreshTime;
            string timeText;
            if (elapsed.TotalSeconds < 15)
            {
                timeText = "1 sec ago";
            }
            else if (elapsed.TotalSeconds < 45)
            {
                timeText = "30 sec ago";
            }
            else if (elapsed.TotalSeconds < 150)
            {
                timeText = "1 min ago";
            }
            else if (elapsed.TotalSeconds < 600)
            {
                timeText = "5 min ago";
            }
            else if (elapsed.TotalSeconds < 1800)
            {
                timeText = "15 min ago";
            }
            else if (elapsed.TotalSeconds < 3600)
            {
                timeText = "30 min ago";
            }
            else
            {
                int hours = (int)elapsed.TotalHours;
                timeText = hours == 1 ? "1 hr ago" : $"{hours} hrs ago";
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                LastUpdatedText = $"Last updated: {timeText}";
            });
        }

        private ObservableCollection<NewsItem> GenerateNews()
        {
            var newsPool = new[]
            {
                new NewsItem { Title = "🤖 AI Revolution Accelerates Across Industries", Summary = "Companies invest heavily in AI solutions that automate complex workflows. Startups and incumbents alike race to productionize models, driving rapid tooling improvements. Analysts expect this trend to reshape multiple sectors in the coming years.", ImageUrl = "air_evolution.jpg" },
                new NewsItem { Title = "📊 Global Markets React to Policy Changes", Summary = "Markets moved sharply after recent policy announcements, reflecting investor uncertainty. Traders rebalanced portfolios while analysts sift through implications for growth. Short-term volatility is expected to continue as details emerge.", ImageUrl = "globalimage.jpg" },
                new NewsItem { Title = "💻 Cybersecurity Alert: New Threats Detected", Summary = "Security teams have observed an uptick in coordinated attacks targeting critical infrastructure. New malware variants employ evasive techniques to bypass defenses. Organizations are urged to patch systems and review access controls immediately.", ImageUrl = "cybersecurity.jpg" },
                new NewsItem { Title = "🔬 Biotech Advances Show Promise", Summary = "Researchers report promising results from a new gene-editing approach. Early trials indicate improved outcomes for previously untreatable conditions. Further studies are planned to validate safety and efficacy.", ImageUrl = "biotech.jpg" },
                new NewsItem { Title = "📱 Mobile Technology Trends 2026", Summary = "Manufacturers unveil thinner devices with longer battery life and better AI on-device. App ecosystems emphasize privacy-first features and seamless cross-device experiences. Consumers are responding positively to more efficient hardware and smarter software.", ImageUrl = "mobiletech.jpg" },
                new NewsItem { Title = "🚀 SpaceX Launches New Satellite Constellation", Summary = "A recent launch expanded low-earth orbit coverage, promising faster connectivity in remote regions. Engineers confirmed deployment success and began initial network testing. The program aims to reduce latency for global users.", ImageUrl = "spacex.jpg" },
                new NewsItem { Title = "💡 Quantum Computing Reaches Major Milestone", Summary = "A new prototype demonstrates improved error correction and longer coherence times. Researchers say this brings certain practical quantum workloads closer to reality. Industry partnerships are accelerating hardware and software co-design efforts.", ImageUrl = "quantum_computing.jpg" },
                new NewsItem { Title = "🌐 Internet Infrastructure Expands", Summary = "Investment in subsea cables and regional hubs boosts global bandwidth capacity. Service providers report increased demand from cloud and streaming platforms. The expansion aims to support growing data needs over the next decade.", ImageUrl = "internetinfra.jpg" },
                new NewsItem { Title = "🎯 Tech Giants Announce New Initiatives", Summary = "Several major companies revealed multi-year commitments to developer tools and open-source projects. The announcements include funding for ecosystems and new platform services. Observers expect enhanced competition and faster innovation cycles.", ImageUrl = "tech_giants.jpg" },
                new NewsItem { Title = "⚡ Energy Solutions Break Records", Summary = "Renewable installations hit new deployment records this quarter, aided by favorable policy and falling costs. Grid operators work to integrate intermittent sources with energy storage. Analysts see a faster-than-expected transition in several markets.", ImageUrl = "energy_solution.jpg" },
                new NewsItem { Title = "🌍 Climate Summit Produces Historic Agreement", Summary = "Leaders agreed on an ambitious set of emissions targets and financing mechanisms. Negotiators emphasized accountability and support for vulnerable regions. The pact will guide climate policy and corporate strategies worldwide.", ImageUrl = "climate_summit.jpg" },
                new NewsItem { Title = "🏥 Breakthrough in Cancer Treatment Research", Summary = "Scientists report encouraging results from a next-generation immunotherapy trial. The approach shows higher response rates in early cohorts with manageable side effects. Larger clinical trials are now being planned across multiple centers.", ImageUrl = "cancer_treatment.jpg" },
                new NewsItem { Title = "🏭 Manufacturing Embraces Green Technology", Summary = "Factories are adopting cleaner processes and electrification to reduce emissions. Supply chains are being redesigned around circular economy principles. Companies report cost savings alongside sustainability gains.", ImageUrl = "green_tech.jpg" },
                new NewsItem { Title = "🚗 Transportation Revolution Accelerates", Summary = "Electric and autonomous vehicle programs hit operational milestones, with wider pilot deployments. Regulators and manufacturers coordinate on safety frameworks. Infrastructure upgrades aim to support mass adoption.", ImageUrl = "transportation.jpg" },
                new NewsItem { Title = "🎓 Innovation in Education Technology", Summary = "AI-powered learning platforms enable personalized instruction at scale. Educators report improved engagement and measurable performance gains. Stakeholders discuss standards and equitable access to new tools.", ImageUrl = "innovation_education.jpg" },
                new NewsItem { Title = "🏆 Sports: Team Wins Championship Title", Summary = "A dramatic final delivered a surprise championship for the underdog squad. Fans celebrated across cities while analysts dissected the tactical masterstrokes. The victory is expected to boost franchise revenues and sponsorship interest.", ImageUrl = "sports.jpg" },
                new NewsItem { Title = "🎬 Entertainment: New Blockbuster Releases", Summary = "Studios released a heavily anticipated film with record pre-sales and strong reviews. Early box office figures point to a major opening weekend. Critics praise production values and performances while audiences share viral reactions.", ImageUrl = "entertainment.jpg" },
                new NewsItem { Title = "🍔 Food Industry Trends Shift", Summary = "Plant-based and alternative proteins gain broader supermarket distribution. Companies innovate on taste and texture, attracting mainstream consumers. Restaurants adapt menus to highlight sustainable sourcing.", ImageUrl = "food_industry.jpg" },
                new NewsItem { Title = "✈️ Travel: New Routes Announced", Summary = "Airlines launched new international routes to meet rising demand for leisure travel. Route planners emphasize connectivity to secondary cities. Travelers benefit from more direct flight options and competitive fares.", ImageUrl = "travel.jpg" },
                new NewsItem { Title = "🎵 Music: Festival Announces Lineup", Summary = "Organizers confirmed a diverse lineup spanning major headliners and emerging artists. Festival goers anticipate new stage experiences and curated programming. Ticket sales and travel packages are moving quickly.", ImageUrl = "music.jpg" }
            };

            var random = new Random();
            var selectedItems = new List<NewsItem>();
            var usedIndices = new HashSet<int>();

            while (selectedItems.Count < 5)
            {
                var randomIndex = random.Next(newsPool.Length);
                if (!usedIndices.Contains(randomIndex))
                {
                    usedIndices.Add(randomIndex);
                    var item = new NewsItem
                    {
                        Title = newsPool[randomIndex].Title,
                        Summary = newsPool[randomIndex].Summary,
                        PublishedAt = DateTime.UtcNow.AddSeconds(-random.Next(0, 61)),
                        ImageUrl = newsPool[randomIndex].ImageUrl
                    };

                    selectedItems.Add(item);
                }
            }

            var sorted = selectedItems.OrderByDescending(x => x.PublishedAt).ToList();
            return new ObservableCollection<NewsItem>(sorted);
        }

        #endregion
    }
}