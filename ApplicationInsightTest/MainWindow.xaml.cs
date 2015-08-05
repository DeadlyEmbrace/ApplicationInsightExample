namespace ApplicationInsightTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constructor

        public MainWindow(DataContext context, ApplicationInsightHelper applicationInsightHelper)
        {
            DataContext = context;
            applicationInsightHelper.TrackPageView("MainWindow");
            InitializeComponent();
        }

        #endregion
    }
}
