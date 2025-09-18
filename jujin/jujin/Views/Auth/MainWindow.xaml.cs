using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BCrypt.Net;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using jujin.Views.Main;

namespace jujin.Views.Auth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // JWT 토큰을 저장할 정적 변수
        public static string CurrentToken { get; set; }
        public static EmployeeInfoDto CurrentUserInfo { get; set; }
        
        public MainWindow()
        {
            InitializeComponent();
            LoadSavedUserId();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        public class EmployeeInfoDto
        {
            public string EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public DateTime? BirthDate { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Position { get; set; }
            public string Email { get; set; }
            public string DepartmentName { get; set; }
            public string Profile_Url { get; set; }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = IdTextBox.Text.Trim();
            string userPw = PwBox.Password;

            var client = new HttpClient();
            try
            {
                var response = await client.PostAsJsonAsync("http://localhost:5185/api/auth/login", new
                {
                    userId = userId,
                    Password = userPw
                });
                // 성공 시 처리 로직
                if (response.IsSuccessStatusCode)
                {
                    // 수정된 LoginResult 클래스를 사용하여 응답을 역직렬화
                    var result = await response.Content.ReadFromJsonAsync<LoginResult>();
                    
                    // 토큰과 사용자 정보를 정적 변수에 저장
                    CurrentToken = result.Token;
                    CurrentUserInfo = result.UserInfo;
                    
                    // 아이디 저장 처리
                    SaveUserIdIfChecked(userId);
                    
                    // 결과 객체에서 Token과 UserInfo 사용
                    MessageBox.Show($"로그인 성공! 토큰: { result.Token}\n사용자 이름 : {result.UserInfo.Profile_Url}");
                    // Main 화면으로 이동 (예시)
                    // (사용자 정보를 다음 화면으로 전달할 수 있음)
                    var main = new MainScreenWindow(result.UserInfo); 
                    main.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("로그인 실패: " + response.ReasonPhrase);
                    return;
                }

            }
            catch (HttpRequestException ex)
            {
                // 예외 상세 정보 출력
                Console.WriteLine($"HttpRequestException: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }

            
        }

        public class LoginResult
        {
            public string Token { get; set; }
            public EmployeeInfoDto UserInfo { get; set; }
        }

        // 토큰 유효성 검사 메서드
        public static bool IsTokenValid()
        {
            if (string.IsNullOrEmpty(CurrentToken))
                return false;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jsonToken = tokenHandler.ReadJwtToken(CurrentToken);
                
                // 토큰 만료 시간 확인
                if (jsonToken.ValidTo < DateTime.UtcNow)
                {
                    // 토큰이 만료된 경우 정리
                    CurrentToken = null;
                    CurrentUserInfo = null;
                    return false;
                }
                
                return true;
            }
            catch
            {
                // 토큰 파싱 오류 시 정리
                CurrentToken = null;
                CurrentUserInfo = null;
                return false;
            }
        }

        // 현재 로그인 상태 확인 메서드
        public static bool IsLoggedIn()
        {
            return IsTokenValid() && CurrentUserInfo != null;
        }

        // 아이디 저장 관련 메서드들
        private void SaveUserIdIfChecked(string userId)
        {
            if (SaveIdCheckBox.IsChecked == true)
            {
                SaveUserIdToCookie(userId);
            }
            else
            {
                // 체크박스가 해제된 경우 저장된 아이디 삭제
                DeleteSavedUserId();
            }
        }

        private void SaveUserIdToCookie(string userId)
        {
            try
            {
                // 간단한 파일 기반 저장 (실제 쿠키 대신)
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "jujin_saved_userid.txt");
                File.WriteAllText(filePath, userId);
            }
            catch (Exception ex)
            {
                // 저장 실패 시 무시 (사용자에게 알리지 않음)
                Console.WriteLine($"아이디 저장 실패: {ex.Message}");
            }
        }

        private void LoadSavedUserId()
        {
            try
            {
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "jujin_saved_userid.txt");
                if (File.Exists(filePath))
                {
                    string savedUserId = File.ReadAllText(filePath);
                    if (!string.IsNullOrEmpty(savedUserId))
                    {
                        IdTextBox.Text = savedUserId;
                        SaveIdCheckBox.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // 불러오기 실패 시 무시
                Console.WriteLine($"저장된 아이디 불러오기 실패: {ex.Message}");
            }
        }

        private void DeleteSavedUserId()
        {
            try
            {
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "jujin_saved_userid.txt");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                // 삭제 실패 시 무시
                Console.WriteLine($"저장된 아이디 삭제 실패: {ex.Message}");
            }
        }
    }
}