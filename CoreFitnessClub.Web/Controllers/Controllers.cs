using CoreFitnessClub.Application.DTOs;
using CoreFitnessClub.Application.Interfaces;
using CoreFitnessClub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CoreFitnessClub.Web.Controllers;

// ── HomeController ────────────────────────────────────────────────────────────

public class HomeController : Controller
{
    public IActionResult Index()   => View();
    public IActionResult Error404() => View();
}

// ── AccountController ─────────────────────────────────────────────────────────

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser>  _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService                  _userService;
    private readonly IBookingService               _bookingService;
    private readonly IMembershipService            _membershipService;

    public AccountController(
        UserManager<ApplicationUser>  userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserService                  userService,
        IBookingService               bookingService,
        IMembershipService            membershipService)
    {
        _userManager       = userManager;
        _signInManager     = signInManager;
        _userService       = userService;
        _bookingService    = bookingService;
        _membershipService = membershipService;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            Email     = dto.Email,
            UserName  = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        return View(dto);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(dto);

        var result = await _signInManager.PasswordSignInAsync(
            dto.Email, dto.Password, dto.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? Url.Action(nameof(Index))!);

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(dto);
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ── My Account (requires login) ───────────────────────────────────────────

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var bookings       = await _bookingService.GetUserBookingsAsync(user.Id);
        var currentPlan    = await _membershipService.GetUserCurrentPlanAsync(user.Id);

        ViewBag.Bookings    = bookings;
        ViewBag.CurrentPlan = currentPlan;

        var dto = new UpdateProfileDto
        {
            FirstName   = user.FirstName,
            LastName    = user.LastName,
            PhoneNumber = user.PhoneNumber
        };
        return View(dto);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        var ok = await _userService.UpdateProfileAsync(user.Id, dto);
        TempData[ok ? "Success" : "Error"] = ok
            ? "Profile updated successfully."
            : "Failed to update profile. Please try again.";

        return RedirectToAction(nameof(Index));
    }

    // ── Delete account ────────────────────────────────────────────────────────

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        await _signInManager.SignOutAsync();
        await _userService.DeleteAccountAsync(user.Id);

        TempData["Success"] = "Your account has been deleted.";
        return RedirectToAction("Index", "Home");
    }

    // ── Cancel booking ─────────────────────────────────────────────────────────

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelBooking(int bookingId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction(nameof(Login));

        await _bookingService.CancelBookingAsync(user.Id, bookingId);
        TempData["Success"] = "Booking cancelled.";
        return RedirectToAction(nameof(Index));
    }
}

// ── MembershipController ──────────────────────────────────────────────────────

public class MembershipController : Controller
{
    private readonly IMembershipService            _membershipService;
    private readonly UserManager<ApplicationUser>  _userManager;

    public MembershipController(
        IMembershipService           membershipService,
        UserManager<ApplicationUser> userManager)
    {
        _membershipService = membershipService;
        _userManager       = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var plans = await _membershipService.GetActivePlansAsync();
        return View(plans);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int planId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return RedirectToAction("Login", "Account");

        await _membershipService.JoinPlanAsync(user.Id, planId);
        TempData["Success"] = "You have successfully joined the plan!";
        return RedirectToAction("Index", "Account");
    }
}

// ── CustomerServiceController ─────────────────────────────────────────────────

public class CustomerServiceController : Controller
{
    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Index(ContactDto dto)
    {
        if (!ModelState.IsValid) return View(dto);

        // In a real system: send email / save to DB
        TempData["Success"] = "Your message has been sent! We will get back to you shortly.";
        return RedirectToAction(nameof(Index));
    }
}
