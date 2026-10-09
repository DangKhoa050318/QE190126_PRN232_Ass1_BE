using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskTrack.Service.Common;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Interfaces;

namespace TaskTrack.API.Controllers;

/// <remarks>Admin only: 401 without a token, 403 for a Staff token.</remarks>
[ApiController]
[Route("api/accounts")]
[Authorize(Roles = AccountRoles.AdminName)]
public class AccountsController(IAccountService service) : ControllerBase
{
    /// <summary>List all accounts with the number of tasks each has created.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AccountDetailDto>>> GetAll() =>
        Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AccountDetailDto>> GetById(int id) =>
        Ok(await service.GetByIdAsync(id));

    /// <summary>Update the name and/or role (0 = Staff, 1 = Admin). Admins cannot change their own role.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AccountDetailDto>> Update(int id, UpdateAccountRequest request) =>
        Ok(await service.UpdateAsync(id, request));

    /// <summary>Delete an account; returns 400 if it has created any task or if it is your own account.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
