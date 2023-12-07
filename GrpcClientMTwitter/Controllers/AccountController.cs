using GrpcClientMTwitter.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

using Grpc.Net.Client;
using GrpcServiceMTwitter;
using Grpc.Core;
using System;

[AllowAnonymous]
public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    private readonly GrpcServiceMTwitter.Tweet.TweetClient _tweetClient;

    public AccountController(GrpcServiceMTwitter.Tweet.TweetClient tweetClient)
    {
        _tweetClient = tweetClient;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var authRequest = new UserCredentials
                {
                    Username = model.Username,
                    Password = model.Password
                };

                var authResponse = await _tweetClient.AuthenticateUserAsync(authRequest);


                if (authResponse.IsAuthenticated)
                {
                    var claims = new[]
                    {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.NameIdentifier, authResponse.Id.ToString()),
                    // Add more claims as needed
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Redirect to the Tweet view after successful login
                return RedirectToAction("Index", "Tweet");
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"gRPC error: {ex.Status.Detail}");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt");
        }

        // If the login fails, return to the login page with an error message
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"{claim.Type}: {claim.Value}");
        }
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Account");
    }
}
