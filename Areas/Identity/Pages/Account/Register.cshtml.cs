// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using LuxRentals.Data;
using LuxRentals.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using LuxRentals.Services;
using Microsoft.EntityFrameworkCore;

namespace LuxRentals.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserStore<IdentityUser> _userStore;
        private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly LuxRentalsDbContext _db;
        private readonly IConfiguration _config;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            IUserStore<IdentityUser> userStore,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            LuxRentalsDbContext db,
            IConfiguration config)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _db = db;
            _config = config;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required]
            [StringLength(40, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 40 characters.")]
            [RegularExpression(@"^[a-zA-Z\s'-]+$",
                ErrorMessage = "First name can only contain letters, spaces, hyphens, and apostrophes.")]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(40, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 40 characters.")]
            [RegularExpression(@"^[a-zA-Z\s'-]+$",
                ErrorMessage = "Last name can only contain letters, spaces, hyphens, and apostrophes.")]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [Required]
            [StringLength(20, MinimumLength = 7,
                ErrorMessage = "Driver's license number must be between 7 and 20 characters.")]
            [Display(Name = "Driver's License Number")]
            public string DriversLicenseNumber { get; set; }

            [Required]
            [Phone] //phone regex accepts formats like: (604) 555-1234, 604-555-1234, 6045551234, or +1-604-555-1234.
            [RegularExpression(@"^(\+1[-.\s]?)?(\(?\d{3}\)?[-.\s]?)?\d{3}[-.\s]?\d{4}$",
                ErrorMessage = "Please enter a valid Canadian phone number.")]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }


            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Password is required")]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 8)]
            [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
                ErrorMessage =
                    "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public required string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ViewData["SiteKey"] = _config["RECAPTCHA_SITEKEY"];
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // reCAPTCHA validation
            string captchaResponse = Request.Form["g-Recaptcha-Response"];
            string secret = _config["RECAPTCHA_SECRETKEY"];

            ReCaptcha.ReCaptchaValidationResult resultCaptcha =
                ReCaptcha.ReCaptchaValidator.IsValid(secret, captchaResponse);

            if (!resultCaptcha.Success)
            {
                ViewData["SiteKey"] = _config["RECAPTCHA_SITEKEY"];
                ModelState.AddModelError(string.Empty, "The ReCaptcha is invalid.");
                return Page();
            }

            if (ModelState.IsValid)
            {
                //Check if driver's license already exists
                var existingLicense = await _db.Customers
                    .FirstOrDefaultAsync(c => c.DriverLicenceNo == Input.DriversLicenseNumber.Trim());

                if (existingLicense != null)
                {
                    ViewData["SiteKey"] = _config["RECAPTCHA_SITEKEY"];
                    ModelState.AddModelError(nameof(Input.DriversLicenseNumber),
                        "This driver's license number is already registered.");
                    return Page();
                }

                // Create a new AspUser
                var user = CreateUser();
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    //Check for a duplicate
                    var existingCustomer = await _db.Customers
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    if (existingCustomer != null)
                    {
                        ViewData["SiteKey"] = _config["RECAPTCHA_SITEKEY"];

                        // Rollback: delete the Identity user we just created
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError("", "Customer already exists for this user.");
                        return Page();
                    }


                    var newCustomer = new Customer()
                    {
                        UserId = user.Id,
                        FirstName = Input.FirstName.Trim(),
                        LastName = Input.LastName.Trim(),
                        Email = user.Email,
                        DriverLicenceNo = Input.DriversLicenseNumber.Trim(),
                        PhoneNumber = Input.PhoneNumber.Trim(),
                        LicenceVerified = false,
                    };

                    using var transaction = await _db.Database.BeginTransactionAsync();
                    try
                    {
                        _db.Customers.Add(newCustomer);
                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("User and Customer created successfully for {Email}", user.Email);
                    }
                    catch (DbUpdateException ex)
                    {
                        await transaction.RollbackAsync();

                        // Rollback: delete the Identity user
                        await _userManager.DeleteAsync(user);

                        _logger.LogError(ex, "Failed to create customer for user {Email}", user.Email);

                        if (ex.InnerException?.Message.Contains("duplicate key") == true)
                        {
                            ModelState.AddModelError("", "A customer with these details already exists.");
                        }
                        else
                        {
                            ModelState.AddModelError("", "Failed to create customer account. Please try again.");
                        }

                        return Page();
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation",
                            new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                                                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<IdentityUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }

            return (IUserEmailStore<IdentityUser>)_userStore;
        }
    }
}