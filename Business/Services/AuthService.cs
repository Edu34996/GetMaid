using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enums;
using Microsoft.AspNetCore.Identity;
using Utils.Helpers;
using Utils.Responses;

namespace Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationUserRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGeocodingService _geocoding;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationUserRole> roleManager,
            IUnitOfWork unitOfWork,
            IGeocodingService geocoding)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
            _geocoding = geocoding;
        }

        public async Task<IResult> LoginAsync(LoginDTO model)
        {
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false
                );

                if (signInResult.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    return Result.Success();
                }

                if (signInResult.IsLockedOut)
                    return Result.Failure(new[] { "Your account is locked out, please contact support." });

                if (signInResult.IsNotAllowed)
                    return Result.Failure(new[] { "Your account is not approved yet." });

                return Result.Failure(new[] { "Invalid login attempt. Email or password is incorrect." });
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { "System error!", ex.Message });
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IResult> RegisterCustomerAsync(CustomerRegisterDTO model)
        {
            try
            {
                var geoQuery = string.IsNullOrWhiteSpace(model.Address)
                    ? model.City
                    : $"{model.Address}, {model.City}";
                var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);

                var customer = new Customer
                {
                    // IdentityUser
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    // ApplicationUser
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    
                    Latitude = lat,
                    Longitude = lon,
                    
                    CreatedAt = DateTime.UtcNow,
                    IdentityVerificationStatus = VerificationStatus.Unverified,

                    // Customer
                    FamilyStatus = model.FamilyStatus,
                    HasPets = model.HasPets,
                    NumberOfPets = model.NumberOfPets
                };

                var result = await _userManager.CreateAsync(customer, model.Password);

                if (!result.Succeeded)
                    return Result.Failure(result.Errors.Select(e => e.Description));

                if (!await _roleManager.RoleExistsAsync("Customer"))
                {
                    await _roleManager.CreateAsync(new ApplicationUserRole
                    {
                        Name = "Customer",
                        NormalizedName = "CUSTOMER",
                        Description = "Standard access role for Customers"
                    });
                }

                await _userManager.AddToRoleAsync(customer, "Customer");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { "System error!", ex.Message });
            }
        }

        public async Task<IResult> RegisterWorkerAsync(WorkerRegisterDTO model)
        {

            try
            {           
                var geoQuery = string.IsNullOrWhiteSpace(model.Address)
                    ? model.City
                    : $"{model.Address}, {model.City}";
                var (lat, lon) = await _geocoding.GeocodeAsync(geoQuery);

                // Guard against accidental min/max inversion
                if (model.MinHourlyRate.HasValue &&
                    model.MaxHourlyRate.HasValue &&
                    model.MinHourlyRate.Value > model.MaxHourlyRate.Value)
                {
                    return Result.Failure(new[] { "Minimum hourly rate cannot be greater than maximum hourly rate." });
                }

                var worker = new Worker
                {
                    // IdentityUser
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,

                    // ApplicationUser
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Bio = model.Bio ?? string.Empty,
                    City = model.City,
                    Address = model.Address,
                    
                    Latitude = lat,
                    Longitude = lon,
                    
                    CreatedAt = DateTime.UtcNow,
                    IdentityVerificationStatus = VerificationStatus.Unverified,

                    // Worker-specific
                    IsSmoker = model.IsSmoker,
                    ExperienceYears = model.ExperienceYears,
                    MinHourlyRate = model.MinHourlyRate,
                    MaxHourlyRate = model.MaxHourlyRate,
                    ProfilePictureUrl = string.IsNullOrWhiteSpace(model.ProfilePictureUrl)
                        ? null
                        : model.ProfilePictureUrl,
                    IntroductionVideoUrl = string.IsNullOrWhiteSpace(model.IntroductionVideoUrl)
                        ? null
                        : model.IntroductionVideoUrl,

                    OfferedServices = model.OfferedServices ?? new(),
                    Skills = model.Skills ?? new(),
                    ExperiencedAgeGroups = model.ExperiencedAgeGroups ?? new(),
                    LanguagesSpoken = model.LanguagesSpoken ?? new(),

                    PreferredArrangement = model.PreferredArrangement,
                    CommitmentPreference = model.CommitmentPreference,

                    MaxDaysPerWeek = model.MaxDaysPerWeek,
                    MaxHoursPerDay = model.MaxHoursPerDay,
                    PreferredWorkDays = model.PreferredWorkDays ?? new()
                };

                var result = await _userManager.CreateAsync(worker, model.Password);

                if (!result.Succeeded)
                    return Result.Failure(result.Errors.Select(e => e.Description));

                if (!await _roleManager.RoleExistsAsync("Worker"))
                {
                    await _roleManager.CreateAsync(new ApplicationUserRole
                    {
                        Name = "Worker",
                        NormalizedName = "WORKER",
                        Description = "Standard access role for Workers"
                    });
                }

                await _userManager.AddToRoleAsync(worker, "Worker");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(new[] { "System error!", ex.Message });
            }
        }
    }
}