using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Windows.Threading;

/*
 * Developed by Helder Rodrigues
 * a11027@alunos.ipca.pt
 * You are not allowed to sell this code to anyone.
 * 
 * This is not my best work, or the best that i can do. A user of forum asked me to do something like that
 * for free, and i am sharing it with all of you and him. The php scrits to work with this program are also free
 * you can monetize it if you want, but you are not allowed to sell it.
 * This is just an exemple of proxys and it was made quickly.
*/
namespace Traffic_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        #region GlobalVars
        bool stop = false;
        private EventHandler timer_Tick;
        string version = "0.1";
        #endregion

        /// <summary>
        /// Before the window show the images, runs that code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //This is optional, only if you want to hava some views to your website with a real IP
            wbSample.Navigated += new NavigatedEventHandler(wbSample_Navigated);
            wbSample.Navigate("http://url");
        }

        /// <summary>
        /// Checking version
        /// </summary>
        /// <param name="url"></param>
        public void CheckV(string url)
        {
            WebClient wc = new WebClient();
            //get all html data
            byte[] raw = wc.DownloadData(url + "version.php");

            //encoding the data
            string webData = Encoding.UTF8.GetString(raw);

            if (version != webData)
                MessageBox.Show("New Version Available. Check http://alfs.pw");
        }

        /// <summary>
        /// Hidding JS errors
        /// </summary>
        /// <param name="wb"></param>
        public void HideJsScriptErrors(WebBrowser wb)
        {
            // IWebBrowser2 interface
            // Exposes methods that are implemented by the WebBrowser control 
            // Searches for the specified field, using the specified binding constraints.
            FieldInfo fld = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fld == null)
                return;
            object obj = fld.GetValue(wb);
            if (obj == null)
                return;
            // Silent: Sets or gets a value that indicates whether the object can display dialog boxes.
            // HRESULT IWebBrowser2::get_Silent(VARIANT_BOOL *pbSilent);HRESULT IWebBrowser2::put_Silent(VARIANT_BOOL bSilent);
            obj.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, obj, new object[] { true });
        }

        /// <summary>
        /// Once the browser is working, navigating
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void wbSample_Navigated(object sender, NavigationEventArgs e)
        {
            //Not breaking or showing the JS errors on web browser
            HideJsScriptErrors((WebBrowser)sender);
        }

        /// <summary>
        /// Button Start Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //vars
            string error = "";
            int duration = 0;
            string url = "http://url/";
            string complement = "verify.php?key=";

            CheckV(url);

            //checking if the textbox are empty
            if (txtSessionCode.Text.Trim() == "")
                error = "Please Insert Session Code\r\n";
            if (txtDuration.Text.Trim() == "")
                error = error + "Please Set A duration in seconds\r\n";
            if (txtWebSites.Text.Trim() == "")
                error = error + "Please put atleast one website to visit\r\n";

            //checking int values
            bool timming = int.TryParse(txtDuration.Text, out duration);
            if (!timming)
                error = error + "Please put only number values for seconds on timer";

            //starting doing magic or showing error messages
            if (error != "")
                MessageBox.Show(error);
            //magic here
            else
            {
                //adding the code to the url
                if (!AllowProgram(url + complement + txtSessionCode.Text))
                    MessageBox.Show("Please to use this program enter a valid key");
                else
                {
                    //now the program will work

                    //i will get the proxylist now
                    lblProxys.Content = "Down... Proxys";
                    progressB.Value = 10;
                    string p = getProxys(url);
                    string[] proxys = p.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    lblProxys.Content = "Starting.";
                    //One We have the proxys lets navigate
                    navigateWeb(proxys);
                        
                }
            }
        }

        private void navigateWeb()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// navigating the websites here.
        /// </summary>
        /// <param name="proxys"></param>
        private void navigateWeb(string[] proxys)
        {
                string[] links = txtWebSites.Text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                //rotating the links
                for (int i = 0; i < links.Length; i++)
                {
                    wbSample.Navigate(links[i]);
                    //rotating link for all the proxys
                    for (int j = 0; j < proxys.Length; j++)
                    {
                        if (stop)
                        {
                            break;
                        }
                        //creating http request
                        HttpWebRequest request;
                        if (links[i].StartsWith("http://") || links[i].StartsWith("http://"))
                        {
                            request = (HttpWebRequest)WebRequest.Create(links[i]);
                        }
                        else
                        {
                            request = (HttpWebRequest)WebRequest.Create("http://" + links[i]);
                        }

                        //spliting proxy by IP:PORT    
                        string[] help = proxys[j].Split(':');

                        //SETTING UP THE PROXY
                        WebProxy myproxy = new WebProxy(help[0], int.Parse(help[1]));

                        myproxy.BypassProxyOnLocal = false;
                        request.Proxy = myproxy;
                        request.Method = "GET";

                        HttpWebResponse response;

                        try
                        {
                            //GET THE RESPONSE OF WEBSITE
                            response = (HttpWebResponse)request.GetResponse();
                        }
                        catch (WebException wex)
                        {
                            // Handle your exception here (or don't, to effectively "ignore" it)
                        }
                        //sleep a bit
                        lblVisits.Content = (int.Parse(lblVisits.Content.ToString()) + 1).ToString();
                        DispatcherTimer timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(int.Parse(txtDuration.Text));
                        timer.Tick += timer_Tick;
                        timer.Start();
                    }
                }
        }

        /// <summary>
        /// Getting the proxy list
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string getProxys(string url)
        {
            progressB.Value = 1;
            string complement = "getproxys.php";
            //WebClient to check the website content
            WebClient wc = new WebClient();
            //get all html data
            byte[] raw = wc.DownloadData(url + complement);

            //encoding the data
            string webData = Encoding.UTF8.GetString(raw);

            string result = "";

            using (var webClient = new WebClient())
            {
                //getting data and using progress bar
                progressB.Value = 70;
                result = webClient.DownloadString(url + webData);
                progressB.Value = 90;
            }
            progressB.Value = 100;
            return result;
        }

        /// <summary>
        /// Gets the autorization to use program
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool AllowProgram(string url)
        {

            //WebClient to check the website content
            WebClient wc = new WebClient();

            //get all html data
            byte[] raw = wc.DownloadData(url);

            //encoding the data
            string webData = Encoding.UTF8.GetString(raw);

            //Parsing to bool the result
            bool result = bool.Parse(webData);

            return result;
        }

        /// <summary>
        /// Open the Browser to Get the code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetCode_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://sh.st/3MCov");
        }

        /// <summary>
        /// Stoping the work
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            stop = true;
            lblProxys.Content = "STOPPED";
        }
    }
}
