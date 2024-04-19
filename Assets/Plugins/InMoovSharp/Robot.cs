using Demonixis.InMoovSharp.Services;
using Demonixis.InMoovSharp.Settings;
using Demonixis.InMoovSharp.Systems;
using Demonixis.InMoovSharp.Utils;
using System;
using System.Collections.Generic;

namespace Demonixis.InMoovSharp
{
    public class Robot : IDisposable
    {
        public const string ServiceListFilename = "services.json";
        public const string SystemListFilename = "systems.json";
        private static Robot? _instance;

        private List<RobotService> _currentServices;
        private List<RobotService> _registeredServices;
        private List<RobotSystem> _registeredSystems;
        private List<Action> _waitingStartCallbacks;
        private TimeManager _timeManager;
        private bool _disposed;

        /// <summary>
        /// Gets a static instance of the robot.
        /// </summary>
        public static Robot Instance
        {
            get
            {
                _instance ??= new Robot();
                return _instance;
            }
        }

        public CoroutineManager CoroutineManager { get; private set; }

        /// <summary>
        /// Retrieve an array of active services.
        /// </summary>
        public RobotService[] Services => _currentServices?.ToArray() ?? Array.Empty<RobotService>();
        public RobotSystem[] Systems => _registeredSystems?.ToArray() ?? Array.Empty<RobotSystem>();

        public BrainWorldContext WorldContext { get; private set; }
        public bool Started { get; private set; }
        public bool LogEnabled { get; set; } = true;
        
        public event Action<RobotService, RobotService> ServiceChanged;
        public event Action<RobotService, bool> ServicePaused;

        public event Action<Robot> ServicePreInit;
        public event Action<Robot> SystemPreInit;

        public static void Log(string message)
        {
            if (!Instance.LogEnabled) return;
#if INMOOV_UNITY
            UnityEngine.Debug.Log(message);
#else
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
#endif
        }

        public Robot()
        {
            if (_instance != null && _instance != this)
            {
                Log("Double Instance detected.");
                return;
            }

            _instance ??= this;
            _registeredServices = new List<RobotService>();
            _registeredSystems = new List<RobotSystem>();

            CoroutineManager = new CoroutineManager();

            WorldContext = new BrainWorldContext();
            _currentServices = new List<RobotService>();
            _waitingStartCallbacks = new List<Action>();
            _timeManager = new TimeManager();
        }

        protected virtual void RegisterServices()
        {
            AddService(new AIMLNetService());
            AddService(new OpenAIChatbot());
            AddService(new ServoMixerService());
            AddService(new ComputerVisionService());
            AddService(new NavigationService());
            AddService(new SpeechSynthesisService());
            AddService(new VoiceRecognitionService());
            AddService(new XRService());
        }

        protected virtual void RegisterSystems()
        {
            AddSystem(new JawMechanism());
            AddSystem(new RandomAnimation());
        }

        public void InitializeRobot()
        {
            if (Started)
            {
                Log("The Robot was already started.");
                return;
            }

            RegisterServices();
            RegisterSystems();

            // TODO in prevision of deferred load.
            ServicePreInit?.Invoke(this);
            InitializeServices();

            Started = true;

            SystemPreInit?.Invoke(this);
            InitializeSystems();

            if (_waitingStartCallbacks.Count <= 0) return;
            foreach (var callback in _waitingStartCallbacks)
                callback?.Invoke();

            _waitingStartCallbacks.Clear();
        }
        
        public void WhenStarted(Action callback)
        {
            if (Started)
                callback?.Invoke();
            else
                _waitingStartCallbacks.Add(callback);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void UpdateRobot()
        {
            _timeManager.Update();
            CoroutineManager.Update();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            var serviceList = new ServiceList();

            foreach (var service in _currentServices)
            {
                if (service is ChatbotService)
                    serviceList.Chatbot = service.ServiceName;
                else if (service is VoiceRecognitionService)
                    serviceList.VoiceRecognition = service.ServiceName;
                else if (service is SpeechSynthesisService)
                    serviceList.SpeechSynthesis = service.ServiceName;
                else if (service is ServoMixerService)
                    serviceList.ServoMixer = service.ServiceName;
                else if (service is NavigationService)
                    serviceList.Navigation = service.ServiceName;
                else if (service is ComputerVisionService)
                    serviceList.ComputerVision = service.ServiceName;
                else if (service is XRService)
                    serviceList.XR = service.ServiceName;
            }

            // Save the list of used services
            SaveGame.SaveData(serviceList, ServiceListFilename, "Config");

            ClearCurrentServices();

            // Save the list of used systems
            var activeSystems = new List<string>();
            foreach (var system in _registeredSystems)
            {
                if (!system.Started) continue;
                activeSystems.Add(system.GetType().Name);
                system.SetActive(false);
            }

            SaveGame.SaveData(activeSystems.ToArray(), SystemListFilename, "Config");

            GlobalSettings.Save();

            _disposed = true;
        }

        #region Service Management

        public void AddService(RobotService service)
        {
            if (_registeredServices.Contains(service)) return;
            _registeredServices.Add(service);
        }

        /// <summary>
        /// Initialize services to make the robot alive
        /// </summary>
        private void InitializeServices()
        {
            var serviceList = SaveGame.LoadData<ServiceList>(ServiceListFilename, "Config");

            if (!serviceList.IsValid())
                serviceList = ServiceList.New();

            // Select service selected by the user
            var chatbotService = SelectService<ChatbotService>(serviceList.Chatbot);
            var voiceRecognition = SelectService<VoiceRecognitionService>(serviceList.VoiceRecognition);
            var speechSynthesis = SelectService<SpeechSynthesisService>(serviceList.SpeechSynthesis);
            SelectService<ServoMixerService>(serviceList.ServoMixer);
            SelectService<XRService>(serviceList.XR);
            SelectService<NavigationService>(serviceList.Navigation);
            SelectService<ComputerVisionService>(serviceList.ComputerVision);

            WorldContext.Setup(this, chatbotService, voiceRecognition, speechSynthesis);
        }

        /// <summary>
        /// Select the service T by its name and use a fallback if not available
        /// </summary>
        /// <param name="targetService">The service name</param>
        /// <typeparam name="T">The service type</typeparam>
        /// <returns>It returns a RobotService</returns>
        public T SelectService<T>(string targetService) where T : RobotService
        {
            var services = GetServicesOfType<T>();
            T selectedService = null;

            foreach (var service in services)
            {
                if (service.ServiceName == targetService && service.IsSupported())
                    selectedService = (T)service;

                if (selectedService == null && service.IsSupported())
                    selectedService = (T)service;
            }

            if (selectedService == null)
            {
                throw new Exception(
                    $"Service {targetService} is not available and there is no suitable service found.");
            }

            _currentServices.Add(selectedService);

            selectedService.Initialize();

            Log($"[{typeof(T)} service: {selectedService}");

            return selectedService;
        }

        public RobotService[] GetServicesOfType<T>() where T : RobotService
        {
            var list = new List<T>();

            foreach (var service in _registeredServices)
            {
                if (service is T)
                    list.Add((T)service);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Shutdown loaded services and free the resources.
        /// </summary>
        private void ClearCurrentServices()
        {
            if (_currentServices.Count <= 0) return;

            foreach (var service in _currentServices)
                service.Dispose();

            _currentServices.Clear();
        }

        /// <summary>
        /// Gets the active service of type T.
        /// </summary>
        /// <typeparam name="T">The type of serice.</typeparam>
        /// <returns>It returns a service.</returns>
        public T GetService<T>() where T : RobotService
        {
            var type = typeof(T);
            foreach (var current in _currentServices)
            {
                if (current is T service)
                    return service;
            }

            Log($"Service {type} was not found. It's not really possible...");
            return null;
        }

        public bool TryGetService<T>(out T outService) where T : RobotService
        {
            var type = typeof(T);
            foreach (var current in _currentServices)
            {
                if (current is not T service) continue;
                outService = service;
                return true;
            }

            Log($"Service {type} was not found. It's not really possible...");
            outService = null;
            return false;
        }

        /// <summary>
        /// Change a service by another one using a name.
        /// </summary>
        /// <typeparam name="T">The type of service</typeparam>
        /// <param name="serviceName">The new service name</param>
        public void ReplaceService<T>(string serviceName) where T : RobotService
        {
            if (TryFindServiceByName(serviceName, out T newService))
            {
                var oldService = GetService<T>();
                oldService.Dispose();
                _currentServices.Remove(oldService);

                newService.Initialize();
                _currentServices.Add(newService);

                ServiceChanged?.Invoke(oldService, newService);
            }
            else
                Log($"Robot::ReplaceService - I wasn't able to find the service {serviceName}.");
        }

        public void SwapServices<T>(T newService) where T : RobotService
        {
            var current = GetService<T>();
            if (current == null)
            {
                Log($"Service {typeof(T).Name} was not found!");
                return;
            }

            current.Dispose();
            SelectService<T>(newService.ServiceName);
        }

        /// <summary>
        /// Try to find a service using its name.
        /// </summary>
        /// <param name="serviceName">The sercice name</param>
        /// <param name="outService">The service instance</param>
        /// <typeparam name="T">It returns true if the service was found, otherwise it returns false.</typeparam>
        /// <returns></returns>
        private bool TryFindServiceByName<T>(string serviceName, out T outService) where T : RobotService
        {
            var services = GetServicesOfType<T>();

            foreach (T newService in services)
            {
                if (newService.ServiceName != serviceName) continue;
                outService = newService;
                return true;
            }

            outService = null;
            return false;
        }

        /// <summary>
        /// Pause a service by its type.
        /// </summary>
        /// <typeparam name="T">The type of service.</typeparam>
        /// <param name="paused">Set to true to pause the service and false to unpause it.</param>
        public void SetServicePaused<T>(bool paused) where T : RobotService
        {
            if (!TryGetService(out T service)) return;
            service.SetPaused(paused);
            ServicePaused?.Invoke(service, paused);
        }

        #endregion

        #region Systems Management

        public void AddSystem(RobotSystem system)
        {
            if (_registeredSystems.Contains(system)) return;
            _registeredSystems.Add(system);
        }

        private void InitializeSystems()
        {
            var systemsList = SaveGame.LoadData<string[]>(SystemListFilename, "Config");

            if (systemsList == null || systemsList.Length == 0) return;

            foreach (var system in _registeredSystems)
            {
                if (Array.IndexOf(systemsList, system.GetType().Name) == -1) continue;
                system.SetActive(true);
            }
        }

        public bool SetSystemEnabled<T>(bool enabled) where T : RobotSystem
        {
            foreach (var system in _registeredSystems)
            {
                if (system is T)
                {
                    system.SetActive(enabled);
                    Log($"{(enabled ? "Enabling" : "Disabling")} {system.GetType().Name}");
                    return true;
                }
            }

            return false;
        }

        public bool TryGetSystem<T>(out T robotSystem) where T : RobotSystem
        {
            foreach (var system in _registeredSystems)
            {
                if (system is T)
                {
                    robotSystem = (T)system;
                    return true;
                }
            }

            robotSystem = null;
            return false;
        }

        #endregion
    }
}