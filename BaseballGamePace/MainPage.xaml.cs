using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Web.Http.Filters;

namespace BaseballGamePace {
    public sealed partial class MainPage : Page {
        ObservableCollection<GamePace> GamePaces = new ObservableCollection<GamePace>();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public MainPage() {
            this.InitializeComponent();
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = ColorHelper.FromArgb(255, 240, 240, 240);
            titleBar.BackgroundColor = ColorHelper.FromArgb(255, 60, 60, 60);
            titleBar.ButtonForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonBackgroundColor = ColorHelper.FromArgb(255, 60, 60, 60);
            titleBar.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonHoverBackgroundColor = ColorHelper.FromArgb(255, 90, 90, 90);
            titleBar.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            titleBar.ButtonPressedBackgroundColor = ColorHelper.FromArgb(255, 120, 120, 120);
            titleBar.InactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.InactiveBackgroundColor = ColorHelper.FromArgb(255, 60, 60, 60);
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Colors.Gray;
            titleBar.ButtonInactiveBackgroundColor = ColorHelper.FromArgb(255, 60, 60, 60);
            SetChrome();
            gamesGridView.ItemsSource = GamePaces;
            GetGameData();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1, 0);
            dispatcherTimer.Start();
        }
        private async void GetGameData() {
            var filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            var httpClient = new Windows.Web.Http.HttpClient(filter);
            Uri requestUri = new Uri("");
            Windows.Web.Http.HttpResponseMessage httpResponse = new Windows.Web.Http.HttpResponseMessage();
            string httpResponseBody = "";
            try {
                httpResponse = await httpClient.GetAsync(requestUri);
                httpResponse.EnsureSuccessStatusCode();
                httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
            } catch (Exception ex) {
                httpResponseBody = "Error: " + ex.HResult.ToString("X") + " Message: " + ex.Message;
                ShowGameDataError(httpResponseBody);
                return;
            }
            ParseGameData(httpResponseBody);
        }
        private void ShowGameDataError(string error) {
            gamesGridView.Visibility = Visibility.Collapsed;
            errorMsgText.Text = error;
            errorMsg.Visibility = Visibility.Visible;
        }
        private void ParseGameData(string data) {
            JsonArray obj = JsonValue.Parse(data).GetArray();
            try {
                string msg = obj.GetObjectAt(0).GetNamedString("message");
                ShowGameDataError(msg);
                return;
            } catch (Exception ex) {}
            for (uint i = 0; i < obj.Count; i++) {
                var pace = new GamePace {
                    Away = obj.GetObjectAt(i).GetNamedString("away"),
                    Home = obj.GetObjectAt(i).GetNamedString("home"),
                    Start = obj.GetObjectAt(i).GetNamedString("start"),
                    Inning = obj.GetObjectAt(i).GetNamedString("inning"),
                    Frame = obj.GetObjectAt(i).GetNamedString("frame"),
                    IsOver = obj.GetObjectAt(i).GetNamedBoolean("is_over"),
                };
                bool found = false;
                for (int j = 0; j < GamePaces.Count; j++) {
                    if (GamePaces[j].Away != obj.GetObjectAt(i).GetNamedString("away") || GamePaces[j].Home != obj.GetObjectAt(i).GetNamedString("home")) continue;
                    found = true;
                    //if (GamePaces[j].Inning != obj.GetObjectAt(i).GetNamedString("inning") || GamePaces[j].Frame != obj.GetObjectAt(i).GetNamedString("frame") || GamePaces[j].IsOver != obj.GetObjectAt(i).GetNamedBoolean("is_over")) {
                        GamePaces[j] = pace;
                    //}
                }
                if (!found) {
                    GamePaces.Add(pace);
                }
            }
        }
        private void SetChrome() {
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            DateTime now = DateTime.Now;
            DateTime nowConverted = TimeZoneInfo.ConvertTime(now, localZone);
            this.timer.Text = nowConverted.ToString("yyyy-dd-MM h:mm tt");
        }
        private void DispatcherTimer_Tick(object sender, object e) {
            SetChrome();
            GetGameData();
        }
        private async void Creator_Tapped(object sender, TappedRoutedEventArgs e) {
            var url = new Uri(@"https://craigdietrich.com/");
            var success = await Windows.System.Launcher.LaunchUriAsync(url);
        }
        private async void Scoreboard_Tapped(object sender, TappedRoutedEventArgs e) {
            var url = new Uri(@"https://www.espn.com/mlb/scoreboard");
            var success = await Windows.System.Launcher.LaunchUriAsync(url);
        }
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e) {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Hand, 1);
        }
        private void Button_PointerExited(object sender, PointerRoutedEventArgs e) {
            Windows.UI.Xaml.Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
        }
    }

    class GamePace {
        private string _away;
        private string _home;
        private string _start;
        private string _inning;
        private string _frame;
        private string _stored_pace = "";
        private bool _is_over = false;
        public string Away {
            get {
                return _away;
            }
            set {
                _away = value;
            }
        }
        public string Home {
            get {
                return _home;
            }
            set {
                _home = value;
            }
        }
        public string Start {
            get {
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                DateTime dt = DateTime.Parse(_start, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime cdt = TimeZoneInfo.ConvertTime(dt, localZone);
                return cdt.ToString("t").ToLower();
            }
            set {
                _start = value;
            }
        }
        public string Inning {
            get {
                if (_is_over) return "";
                return _inning;
            }
            set {
                _inning = value;
            }
        }
        public string Frame {
            get {
                if (_is_over) return "";
                return _frame;
            }
            set {
                _frame = value;
            }
        }
        public string Pace {
            get {
                if (_is_over) return "Complete";
                // Ellapsed minutes
                TimeZoneInfo localZone = TimeZoneInfo.Local;
                DateTime start = DateTime.Parse(_start, null, System.Globalization.DateTimeStyles.RoundtripKind);
                DateTime startConverted = TimeZoneInfo.ConvertTime(start, localZone);
                DateTime now = DateTime.Now;
                DateTime nowConverted = TimeZoneInfo.ConvertTime(now, localZone);
                Debug.WriteLine("nowConverted: " + nowConverted + " startConverted: " + startConverted);
                if (nowConverted.ToUniversalTime() < startConverted.ToUniversalTime()) return "";
                TimeSpan duration = now - startConverted;
                int startMinutes = (int) 0;
                int timePerFrame = (int)10;
                int ellapsedFromStart = (int) duration.TotalMinutes;
                Debug.WriteLine("startMinutes: " + startMinutes + " ellapsedFromStart: " + ellapsedFromStart);
                // Number of innings that have passed if the game were on pace (e.g., 2.5 = in the Bottom of the 3rd)
                int closestInningStart = (int) 0;
                double inningsShouldHavePassed = 0;
                while (closestInningStart < ellapsedFromStart) {
                    if (closestInningStart + timePerFrame > ellapsedFromStart) break;
                    closestInningStart = closestInningStart + timePerFrame;
                    inningsShouldHavePassed = (double) closestInningStart / ((double) timePerFrame * 2);
                }
                // Actual innings that have passed (e.g., 2.5 = in the Bottom of the 3rd)
                double actualInningsPassed = (Convert.ToDouble(this._inning) - 1);
                if ("Bot" == this._frame) actualInningsPassed = actualInningsPassed + 0.5;
                Debug.WriteLine("closestInningStart: " + closestInningStart + " inningsShouldHavePassed: " + inningsShouldHavePassed + " actualInningsPassed: " + actualInningsPassed);
                // Compare the actual inning to the pace inning (e.g., pace 3 vs current 3.5)
                double inningDiff = actualInningsPassed - inningsShouldHavePassed;
                double pace = inningDiff * (timePerFrame * 2);
                // Convert to human string
                string humanPace = "Even";
                if (pace < 0) humanPace = "+ " + Math.Abs(pace).ToString() + " min";
                if (pace > 0) humanPace = "- " + Math.Abs(pace).ToString() + " min";
                Debug.WriteLine("inningDiff: " + inningDiff + " pace: " + pace + " humanPace: " + humanPace);
                _stored_pace = humanPace;
                return humanPace;
            }
        }
        public string Color {
            get {
                string the_pace = "";
                if ("" == _stored_pace) {
                    the_pace = this.Pace;
                } else {
                    the_pace = _stored_pace;
                }
                if (String.IsNullOrEmpty(the_pace)) return "White";
                if ("-" == the_pace[0].ToString()) return "#00FF00";
                if ("+" == the_pace[0].ToString()) return "#FF4444";
                if ("even" == the_pace.ToLower()) return "#FFFF44";
                return "White";
            }
        }
        public bool IsOver {
            get {
                return _is_over;
            }
            set {
                _is_over = value;
            }
        }
    }
}
