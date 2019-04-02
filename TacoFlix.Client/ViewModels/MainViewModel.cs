using Sitecore.Framework.Messaging;
using Sitecore.Processing.Engine.Abstractions;
using Sitecore.Processing.Tasks.Messaging;
using Sitecore.Processing.Tasks.Messaging.Buses;
using Sitecore.Processing.Tasks.Options.DataSources.Search;
using Sitecore.Processing.Tasks.Options.Workers.ML;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using Sitecore.XConnect.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Imaging;
using Sitecore.Processing.Tasks.Abstractions;
using TacoFlix.Client.Common;
using TacoFlix.Client.Generators;
using TacoFlix.Client.Generators.Contacts;
using TacoFlix.Model;
using TacoFlix.Model.Configuration;
using TacoFlix.Model.Logging;
using TacoFlix.Model.Providers;
using TacoFlix.ProcessingEngine.Extensions;
using TacoFlix.Xconnect.Model;

namespace TacoFlix.Client.ViewModels
{
    public class MainViewViewModel : INotifyPropertyChanged
    {
        private const int TimeoutIntervalMinutes = 15;
        private const int ContactsToCreate = 10;

        private readonly ILogger _logger;
        private readonly IMovieInfoProvider _movieInfoProvider;
        private readonly IXconnectConfiguration _xconnectConfiguration;
        private readonly IContactGenerator _contactGenerator;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>> _taskRegistrationSyncBus;
        private readonly ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>> _taskProgressSyncBus;

        private Guid _appStoreActivityChannelId = new Guid("{03119E3C-5FC0-4216-ABCB-271BCEA239FD}");
        private XdbModel _xdbModel;

        private ObservableCollection<PersonMovieViewModel> _contacts = new ObservableCollection<PersonMovieViewModel>();
        public ObservableCollection<PersonMovieViewModel> Contacts
        {
            get => _contacts;
            set
            {
                _contacts = value;
                OnPropertyChanged(nameof(Contacts));
            }
        }

        private PersonMovieViewModel _selectedContact;
        public PersonMovieViewModel SelectedContact
        {
            get => _selectedContact;
            set
            {
                _selectedContact = value;
                OnPropertyChanged(nameof(SelectedContact));
                LoadContactMovieData();
            }
        }

        private ObservableCollection<PersonMovieViewModel> _recommendedMovies = new ObservableCollection<PersonMovieViewModel>();
        public ObservableCollection<PersonMovieViewModel> RecommendedMovies
        {
            get => _recommendedMovies;
            set
            {
                _recommendedMovies = value;
                OnPropertyChanged(nameof(RecommendedMovies));
            }
        }

        private ObservableCollection<TaskStatusViewModel> _tasks = new ObservableCollection<TaskStatusViewModel>();
        public ObservableCollection<TaskStatusViewModel> Tasks
        {
            get => _tasks;
            set
            {
                _tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        private ProcessingTaskStatus _projectionTaskStatus;
        public ProcessingTaskStatus ProjectionTaskStatus
        {
            get => _projectionTaskStatus;
            set
            {
                _projectionTaskStatus = value;
                OnPropertyChanged(nameof(ProjectionTaskStatus));
            }
        }

        private ProcessingTaskStatus _mergeTaskStatus;
        public ProcessingTaskStatus MergeTaskStatus
        {
            get => _mergeTaskStatus;
            set
            {
                _mergeTaskStatus = value;
                OnPropertyChanged(nameof(MergeTaskStatus));
            }
        }

        private ProcessingTaskStatus _recommendationTaskStatus;
        public ProcessingTaskStatus RecommendationTaskStatus
        {
            get => _recommendationTaskStatus;
            set
            {
                _recommendationTaskStatus = value;
                OnPropertyChanged(nameof(RecommendationTaskStatus));
            }
        }

        private ProcessingTaskStatus _storageTaskStatus;
        public ProcessingTaskStatus StorageTaskStatus
        {
            get => _storageTaskStatus;
            set
            {
                _storageTaskStatus = value;
                OnPropertyChanged(nameof(StorageTaskStatus));
            }
        }

        private bool _areButtonsEnabled = true;
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set
            {
                _areButtonsEnabled = value;
                OnPropertyChanged(nameof(AreButtonsEnabled));
            }
        }

        public MainViewViewModel(ILogger logger, IMovieInfoProvider movieInfoProvider, IXconnectConfiguration xconnectConfiguration, IContactGenerator contactGenerator, IRandomGenerator randomGenerator, ISynchronizedMessageBusContext<IMessageBus<TaskRegistrationProducer>> taskRegistrationSyncBus, ISynchronizedMessageBusContext<IMessageBus<TaskProgressProducer>> taskProgressSyncBus)
        {
            _logger = logger;
            _movieInfoProvider = movieInfoProvider;
            _xconnectConfiguration = xconnectConfiguration;
            _contactGenerator = contactGenerator;
            _randomGenerator = randomGenerator;
            _taskRegistrationSyncBus = taskRegistrationSyncBus;
            _taskProgressSyncBus = taskProgressSyncBus;
            _xdbModel = MovieModel.Model;
        }

        private RelayCommand _serializeModel;
        public virtual RelayCommand SerializeModel => _serializeModel ?? (_serializeModel = new RelayCommand(c => SerializeModelHandler()));
        private void SerializeModelHandler()
        {
            var json = XdbModelWriter.Serialize(_xdbModel);

            var filename = _xdbModel + ".json";
            File.WriteAllText(filename, json);

            _logger.Info($"Model written to {filename}");
        }

        private RelayCommand _generateRentalData;
        public virtual RelayCommand GenerateRentalData => _generateRentalData ?? (_generateRentalData = new RelayCommand(c => GenerateRentalDataHandler()));
        private void GenerateRentalDataHandler()
        {
            AreButtonsEnabled = false;
            var movies = _movieInfoProvider.GetPopularMovies();

            var xconnectClient = CreateXconnectClient();

            var contacts = new List<Contact>();

            for (var i = 0; i < ContactsToCreate; i++)
            {
                var person = _contactGenerator.CreateContact();
                var contactId = new ContactIdentifier(_xconnectConfiguration.ContactSource, person.Identifier.ToString(), ContactIdentifierType.Known);
                var contact = new Contact(contactId);

                var infoFacet = new PersonalInformation()
                {
                    FirstName = person.FirstName,
                    LastName = person.LastName
                };
                xconnectClient.SetFacet(contact, PersonalInformation.DefaultFacetKey, infoFacet);

                var emailFacet = new EmailAddressList(new EmailAddress(person.EmailAddress, true), _xconnectConfiguration.ContactSource);
                xconnectClient.SetFacet(contact, EmailAddressList.DefaultFacetKey, emailFacet);

                contacts.Add(contact);

                var personViewModel = CreatePersonMovieViewModel(person, movies[_randomGenerator.GetInteger(0, movies.Count)]);

                var movieRenterOutcome = new Outcome(new Guid(_xconnectConfiguration.MovieRenterEraOutcomeId), DateTime.UtcNow, "USD", 0);
                var movieRentedOutcome = new MovieRentedOutcome(new Guid(_xconnectConfiguration.MovieRentedOutcomeId), DateTime.UtcNow, "USD", 5)
                {
                    Movie = personViewModel.Movie
                };
                var movieRentedinteraction = new Interaction(contact, InteractionInitiator.Contact, _appStoreActivityChannelId, _xconnectConfiguration.InteractionUserAgent);
                movieRentedinteraction.Events.Add(movieRenterOutcome);
                movieRentedinteraction.Events.Add(movieRentedOutcome);

                xconnectClient.AddContact(contact);
                xconnectClient.AddInteraction(movieRentedinteraction);
                _logger.Info($"Created contact for {infoFacet.FirstName} {infoFacet.LastName}");

                Contacts.Add(personViewModel);
            }
            xconnectClient.Submit();
        
            AreButtonsEnabled = true;
        }

        private RelayCommand _getRecommendations;
        public virtual RelayCommand GetRecommendations => _getRecommendations ?? (_getRecommendations = new RelayCommand(c => GetRecommendationsHandler()));
        private async void GetRecommendationsHandler()
        {
            _logger.Info($"Started registering recommendation tasks...");
            Tasks.Clear();

            var taskManager = GetTaskManager();
            var xConnectClient = CreateXconnectClient();
            var taskTimeout = TimeSpan.FromMinutes(10);
            var storageTimeout = TimeSpan.FromMinutes(30);

            // Prepare data source query
            var query = xConnectClient.Contacts.Where(contact =>
                contact.Interactions.Any(interaction =>
                    interaction.Events.OfType<MovieRentedOutcome>().Any() &&
                    interaction.EndDateTime > DateTime.UtcNow.AddMinutes(-TimeoutIntervalMinutes)
                )
            );

            var expandOptions = new ContactExpandOptions
            {
                Interactions = new RelatedInteractionsExpandOptions()
            };

            query = query.WithExpandOptions(expandOptions);

            var searchRequest = query.GetSearchRequest();

            // Task for projection
            var dataSourceOptions = new ContactSearchDataSourceOptionsDictionary(
                searchRequest, // searchRequest
                30, // maxBatchSize
                50 // defaultSplitItemCount
            );

            var projectionOptions = new ContactProjectionWorkerOptionsDictionary(
                typeof(MovieRecommendationModel).AssemblyQualifiedName, // modelTypeString
                storageTimeout, // timeToLive
                "recommendation", // schemaName
                new Dictionary<string, string> // modelOptions
                {
                    { MovieRecommendationModel.OptionTableName, "contactMovies" }
                }
            );

            var projectionTaskId = await taskManager.RegisterDistributedTaskAsync(
                dataSourceOptions, // datasourceOptions
                projectionOptions, // workerOptions
                null, // prerequisiteTaskIds
                taskTimeout // expiresAfter
            );

            Tasks.Add(new TaskStatusViewModel()
            {
                FriendlyTaskName = "Projection Task",
                TaskId = projectionTaskId,
                Status = "Pending"
            });

            // Task for merge
            var mergeOptions = new MergeWorkerOptionsDictionary(
                "contactMoviesFinal", // tableName
                "contactMovies", // prefix
                storageTimeout, // timeToLive
                "recommendation" // schemaName
            );

            var mergeTaskId = await taskManager.RegisterDeferredTaskAsync(
                mergeOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    projectionTaskId
                },
                taskTimeout // expiresAfter
            );

            Tasks.Add(new TaskStatusViewModel()
            {
                FriendlyTaskName = "Merge Task",
                TaskId = mergeTaskId,
                Status = "Pending"
            });

            var workerOptions = new DeferredWorkerOptionsDictionary(
                typeof(MovieRecommendationWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { MovieRecommendationWorker.OptionSourceTableName, "contactMoviesFinal" },
                    { MovieRecommendationWorker.OptionTargetTableName, "contactRecommendations" },
                    { MovieRecommendationWorker.OptionSchemaName, "recommendation" },
                    { MovieRecommendationWorker.OptionLimit, "5" }
                });

            var recommendationTaskId = await taskManager.RegisterDeferredTaskAsync(
                workerOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    mergeTaskId
                },
                taskTimeout // expiresAfter
            );

            Tasks.Add(new TaskStatusViewModel()
            {
                FriendlyTaskName = "Recommendation Task",
                TaskId = recommendationTaskId,
                Status = "Pending"
            });

            var storageOptions = new DeferredWorkerOptionsDictionary(
                typeof(RecommendationFacetStorageWorker).AssemblyQualifiedName, // workerType
                new Dictionary<string, string> // options
                {
                    { RecommendationFacetStorageWorker.OptionTableName, "contactRecommendations" },
                    { RecommendationFacetStorageWorker.OptionSchemaName, "recommendation" }
                });

            var storeFacetTaskId = await taskManager.RegisterDeferredTaskAsync(
                storageOptions, // workerOptions
                new[] // prerequisiteTaskIds
                {
                    recommendationTaskId
                },
                taskTimeout // expiresAfter
            );

            Tasks.Add(new TaskStatusViewModel()
            {
                FriendlyTaskName = "Storage Task",
                TaskId = storeFacetTaskId,
                Status = "Pending"
            } );

            foreach (var task in Tasks)
            {
                _logger.Info($"Registered task {task.TaskId}");
            }

            MonitorTaskStatus();
        }

        private async void LoadContactMovieData()
        {
            if (SelectedContact != null)
            {
                RecommendedMovies.Clear();
                var client = CreateXconnectClient();
                var contactReference = new IdentifiedContactReference(_xconnectConfiguration.ContactSource, SelectedContact.Person.Identifier.ToString());
                var contact = await client.GetContactAsync(contactReference, new ContactExpandOptions(MovieRecommendationFacet.DefaultFacetName));
                var facet = contact?.GetFacet<MovieRecommendationFacet>(MovieRecommendationFacet.DefaultFacetName);

                if (facet != null)
                {

                    foreach (var movie in facet.MovieRecommendations)
                    {
                        RecommendedMovies.Add(CreatePersonMovieViewModel(null, movie));
                    }
                }
            }
        }

        private XConnectClient CreateXconnectClient()
        {
            var xconnectConfig = new XConnectClientConfiguration(_xdbModel, new Uri(_xconnectConfiguration.XconnectUrl));
            xconnectConfig.Initialize();

            return new XConnectClient(xconnectConfig);
        }

        private TaskManager GetTaskManager()
        {
            var options = new TaskManagerOptions(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(60));
            var taskManager = new TaskManager(options, _taskRegistrationSyncBus, _taskProgressSyncBus);
            return taskManager;
        }

        private PersonMovieViewModel CreatePersonMovieViewModel(Person person, Movie movie)
        {
            var viewModel = new PersonMovieViewModel()
            {
                Person = person,
                Movie = movie
            };

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri($"https://image.tmdb.org/t/p/w300_and_h450_bestv2{movie.PosterPath}", UriKind.Absolute);
            bitmap.EndInit();

            viewModel.MoviePoster = bitmap;

            return viewModel;
        }

        private void MonitorTaskStatus()
        {
            if (Tasks.Any())
            {
                _logger.Info("Monitoring task status...");
                var worker = new BackgroundWorker
                {
                    WorkerReportsProgress = true
                };
                worker.DoWork += (sender, args) =>
                {
                    var isStillGoing = true;
                    while (isStillGoing)
                    {
                        var taskManager = GetTaskManager();

                        var status = string.Empty;
                        foreach (var task in Tasks)
                        {
                            var progress = taskManager.GetTaskProgressAsync(task.TaskId).Result;
                            if (progress != null)
                            {
                                status += $"{progress.Status.ToString()};";
                            }
                        }
                        worker.ReportProgress(0, status);
                        isStillGoing = status != "Completed;Completed;Completed;Completed;";
                        Thread.Sleep(500);
                    }
                };
                worker.ProgressChanged += (sender, args) =>
                {
                    var statuses = args.UserState.ToString().Split(';');
                    for (var i = 0; i < statuses.Length - 1; i++)
                    {
                        _logger.Info($"Task {i + 1}: {statuses[i]}");
                    }
                    _logger.Info("------------------------------------");

                    ProjectionTaskStatus = (ProcessingTaskStatus)Enum.Parse(typeof(ProcessingTaskStatus), statuses[0]);
                    MergeTaskStatus = (ProcessingTaskStatus)Enum.Parse(typeof(ProcessingTaskStatus), statuses[1]);
                    RecommendationTaskStatus = (ProcessingTaskStatus)Enum.Parse(typeof(ProcessingTaskStatus), statuses[2]);
                    StorageTaskStatus = (ProcessingTaskStatus)Enum.Parse(typeof(ProcessingTaskStatus), statuses[3]);

                    _logger.Info("Pausing before checking again...");
                };
                worker.RunWorkerCompleted += (sender, args) =>
                {
                    _logger.Info("Monitoring of tasks completed.");
                    AreButtonsEnabled = true;
                };
                AreButtonsEnabled = false;
                worker.RunWorkerAsync();
            }
            else
            {
                _logger.Info("There were no tasks found to monitor. :(");
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
