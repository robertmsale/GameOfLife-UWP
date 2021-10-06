using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace GameOfLife_UWP
{
    public sealed partial class MainPage
    {
        #region Properties and Fields
        /// <summary>
        /// The viewmodel to go with the view and model!
        /// </summary>
        public ViewModel vm;
        private bool _timerRunning = false;
        /// <summary>
        /// Because DispatchTimer doesn't have an "Enabled" flag, must keep track of that externally.
        /// </summary>
        public bool TimerRunning
        {
            get => _timerRunning;
            set
            {
                _timerRunning = value;
                if (value && !timer.IsEnabled) timer.Start();
                if (!value && timer.IsEnabled) timer.Stop();
            }
        }
        /// <summary>
        /// String which toggles the Play/Pause symbol
        /// </summary>
        public string PlayPauseLabel
        {
            get => TimerRunning ? "Pause" : "Play";
        }
        public static SymbolIcon PlayIcon = new SymbolIcon(Symbol.Play);
        public static SymbolIcon PauseIcon = new SymbolIcon(Symbol.Pause);
        public SymbolIcon PlayPauseIcon
        {
            get => TimerRunning ? PauseIcon : PlayIcon;
        }
        /// <summary>
        /// HTTP client for getting universes off of the Lexicon
        /// </summary>
        HttpClient client = new HttpClient();
        /// <summary>
        /// UWP uses a different type of timer which handles asynchronous events more elegantly with multi-threaded UI
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();
        #endregion
        #region Modal Objects
        public AboutDialog aboutModal = new AboutDialog();
        public EditUniverseDialog editUniverseModal = new EditUniverseDialog();
        public ColorDialog colorModal = new ColorDialog();
        #endregion
    }
}