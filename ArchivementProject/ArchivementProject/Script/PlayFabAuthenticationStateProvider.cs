using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabAuthenticationStateProvider : AuthenticationStateProvider
{
    public string errorMessage;
    public static PlayFabAuthenticationStateProvider Instance;
    internal readonly IJSRuntime jsRuntime;
    internal readonly NavigationManager navigationManager;

    public PlayFabResult<LoginResult> loginResult;
    public PlayFabAuthenticationStateProvider(IJSRuntime jsRuntime,NavigationManager navigationManager)
    {
        this.jsRuntime = jsRuntime;
        this.navigationManager = navigationManager;
        Instance = this;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var isLoggedIn = await IsUserLoggedInAsync();

        if (isLoggedIn)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "user_email") // You can use a placeholder email here
            }, "PlayFabAuthentication");

            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        else
        {
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    public async Task<AuthenticationState> LoginAsync(string email, string password)
    {
        Console.WriteLine("Login Async");
        try
        {
                var request = new LoginWithEmailAddressRequest
                {
                    Email = email,
                    Password = password,
                };

                loginResult = await PlayFabClientAPI.LoginWithEmailAddressAsync(request);

                if (loginResult.Error != null)
                {
                    // Handle login error here, e.g., throw an exception or return an error status.
                    throw new Exception("Login failed: " + loginResult.Error.ErrorMessage);
                }
                Console.WriteLine(loginResult.Result.SessionTicket);
                var getAccountInfoRequestrequest = new GetAccountInfoRequest();
                
                var accountInfo = await PlayFabClientAPI.GetAccountInfoAsync(getAccountInfoRequestrequest);
                Console.WriteLine(accountInfo.Result.AccountInfo.CustomIdInfo);
                // Login successful, you can now access PlayFabClientAPI with the session ticket.
                // Store the session token in local storage
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", "Email", email);
                await jsRuntime.InvokeVoidAsync("localStorage.setItem", "Password", password);
                
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, loginResult.Result.PlayFabId)
                }, "PlayFabAuthentication");

                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
            
                
                return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            // Handle exceptions or errors during login, e.g., log them or return an error status.
            // You can customize this error handling based on your requirements.
            Console.WriteLine("Login error: " + ex.Message);
            return new AuthenticationState(new ClaimsPrincipal());
        }
    }

    public async Task LogoutAsync()
    {
        Console.WriteLine("log out");
        try
        {
            // Clear the saved email and password from local storage
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", "sessionToken");
            // Create a new, empty ClaimsPrincipal to represent a logged-out user
            var anonymousIdentity = new ClaimsIdentity();
            var anonymousUser = new ClaimsPrincipal(anonymousIdentity);

            PlayFabClientAPI.ForgetAllCredentials();
            
            // Notify that the authentication state has changed to represent a logged-out user
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymousUser)));
            // Rest of the logout logic...
        }
        catch (Exception ex)
        {
            // Handle logout errors...
        }
    }

    public async Task<bool> IsUserLoggedInAsync()
    {
        Console.WriteLine("check logged");
        try
        {
            // Check if both email and password are present in local storage
            //var sessionToken = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "sessionToken");
            var email = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "Email");
            var password = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "Password");

            if (email != null&&password!=null)
            {
                var authState = await LoginAsync(email, password);
                // Check if the login was successful
                if (authState.User.Identity.IsAuthenticated)
                {
                    // Make a request to get the player's profile
                    var request = new GetPlayerProfileRequest
                    {
                        ProfileConstraints = new PlayerProfileViewConstraints
                        {
                            ShowDisplayName = true,
                            ShowStatistics = true // You can customize this to retrieve specific data
                        }
                    };
            
                    var result = await PlayFabClientAPI.GetPlayerProfileAsync(request);
                    Console.WriteLine(result.Result.PlayerProfile.DisplayName);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            // Handle any potential exceptions
            return false;
        }

        return false;
    }
    
    // public async Task<string> GetPlayFabIdAsync()
    // {
    //     Console.WriteLine("GetId");
    //     
    //     var email = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "Email");
    //     var password = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "Password");
    //     
    //     if (email != null&&password!=null)
    //     {
    //         var authState = await LoginAsync(email, password);
    //         // Check if the login was successful
    //         if (authState.User.Identity.IsAuthenticated)
    //         {
    //             // Make a request to get the player's profile
    //             var request = new GetPlayerProfileRequest
    //             {
    //                 ProfileConstraints = new PlayerProfileViewConstraints
    //                 {
    //                     ShowDisplayName = true,
    //                     ShowStatistics = true // You can customize this to retrieve specific data
    //                 }
    //             };
    //     
    //             var result = await PlayFabClientAPI.GetPlayerProfileAsync(request);
    //             Console.WriteLine(result.Result.PlayerProfile.DisplayName);
    //             return loginResult.Result.PlayFabId;
    //         }
    //
    //         navigationManager.NavigateTo("/Login");
    //     }
    //
    //     throw new InvalidOperationException();
    // }
}
