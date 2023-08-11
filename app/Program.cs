using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ContactApiConsoleApp
{
      class Program
      {
            private const string _userName = "Supervisor";
            private const string _userPassword = "Supervisor";
            private const string _apiUrl = "https://01195748-5-demo.creatio.com/0/odata/Contact?$top=1";
            private const string _apiUrlAuth = "https://01195748-5-demo.creatio.com/";
            private CookieContainer _authCookie = new CookieContainer();

            static async Task Main(string[] args)
            {

                  Program program = new Program();
                  program.TryLogin();
                  try
                  {
                        // Program program = new Program();
                        string apiResponse = await program.MakeGetRequestAsync();
                        string filePath = "C:\\Users\\shevc\\Desktop\\testCsharp\\app\\api_response.txt";
                        File.WriteAllText(filePath, apiResponse);
                        Console.WriteLine("The result is saved in api_response file.");
                  }
                  catch (Exception ex)
                  {
                        Console.WriteLine("Error: " + ex.Message);
                  }
                  // await program.TryLogin();
            }



            public async Task TryLogin()
            {
                  var authData = @"{
                ""UserName"":""" + _userName + @""",
                ""UserPassword"":""" + _userPassword + @"""
            }";
                  var request = CreateRequest(_apiUrlAuth, authData);
                  request.CookieContainer = _authCookie;

                  using (var response = (HttpWebResponse)request.GetResponse())
                  {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                              using (var reader = new StreamReader(response.GetResponseStream()))
                              {
                                    var responseMessage = reader.ReadToEnd();
                                    Console.WriteLine(responseMessage);
                                    if (responseMessage.Contains("\"Code\":1"))
                                    {
                                          throw new UnauthorizedAccessException($"Unauthorized {_userName} for {_apiUrl}");
                                    }
                              }
                              string authName = ".ASPXAUTH";
                              string authCookieValue = response.Cookies[authName].Value;
                              _authCookie.Add(new Uri(_apiUrl), new Cookie(authName, authCookieValue));
                        }
                  }
            }

            private HttpWebRequest CreateRequest(string url, string requestData = null)
            {
                  HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                  request.ContentType = "application/json";
                  request.Method = "POST";
                  request.KeepAlive = true;
                  if (!string.IsNullOrEmpty(requestData))
                  {
                        using (var requestStream = request.GetRequestStream())
                        {
                              using (var writer = new StreamWriter(requestStream))
                              {
                                    writer.Write(requestData);
                              }
                        }
                  }
                  return request;
            }

            private void AddCsrfToken(HttpWebRequest request)
            {
                  var cookie = request.CookieContainer.GetCookies(new Uri(_apiUrl))["BPMCSRF"];
                  if (cookie != null)
                  {
                        request.Headers.Add("BPMCSRF", cookie.Value);
                  }
            }
            public async Task<string> MakeGetRequestAsync()
            {
                  using (HttpClient client = new HttpClient())
                  {
                        client.BaseAddress = new Uri(_apiUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage response = await client.GetAsync(_apiUrl);
                        response.EnsureSuccessStatusCode();

                        return await response.Content.ReadAsStringAsync();
                  }
            }
      }
}