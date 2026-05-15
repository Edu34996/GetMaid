using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Identity;
using Utils.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationUserRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        // Classic C# Constructor with RoleManager injected
        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationUserRole> roleManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<IResult> LoginAsync(LoginDTO model)
        {
            try
            {
                var signInResult = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (signInResult.Succeeded)
                {
                    // Update LastLoginDate upon successful login
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }
                    return Result.Success();
                }
                else if (signInResult.IsLockedOut)
                {
                    return Result.Failure(["Your account is locked out, please contact support!"]);
                }
                else if (signInResult.IsNotAllowed)
                {
                    return Result.Failure(["Your account is not approved yet!"]);
                }
                else
                {
                    return Result.Failure(["Invalid login attempt!", "Password or email address not correct!"]);
                }
            }
            catch (Exception ex)
            {
                return Result.Failure(["System error!", ex.Message]);
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
                // Manual mapping for Customer
                var customer = new Customer
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    City = model.City,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(customer, model.Password);

                if (result.Succeeded)
                {
                    // Inline Role Creation for Customer
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

                return Result.Failure(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                return Result.Failure(["System error!", ex.Message]);
            }
        }

        public async Task<IResult> RegisterWorkerAsync(WorkerRegisterDTO model)
        {
            try
            {
                // Manual mapping for Worker
                var worker = new Worker
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Bio = model.Bio,
                    HourlyRate = model.HourlyRate,
                    ExperienceYears = model.ExperienceYears,
                    ProvidesChildcare = model.ProvidesChildcare,
                    ProvidesMaidService = model.ProvidesMaidService,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(worker, model.Password);

                if (result.Succeeded)
                {
                    // Inline Role Creation for Worker
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

                return Result.Failure(result.Errors.Select(x => x.Description));
            }
            catch (Exception ex)
            {
                return Result.Failure(["System error!", ex.Message]);
            }
        }
    }
}