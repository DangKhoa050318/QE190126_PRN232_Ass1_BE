using TaskTrack.Repo.Repositories;
using TaskTrack.Service.DTOs;
using TaskTrack.Service.Exceptions;
using TaskTrack.Service.Interfaces;
using TaskTrack.Service.Mappings;

namespace TaskTrack.Service.Implementations;

public class AccountService(ISystemAccountRepository repository, ICurrentUser currentUser) : IAccountService
{
    public async Task<List<AccountDetailDto>> GetAllAsync()
    {
        var accounts = await repository.GetAllAsync();
        var createdTasks = await repository.CountCreatedTasksAsync();
        return accounts.Select(a => a.ToDetailDto(createdTasks.GetValueOrDefault(a.AccountId))).ToList();
    }

    public async Task<AccountDetailDto> GetByIdAsync(int id)
    {
        var account = await repository.FindByIdAsync(id) ?? throw NotFound(id);
        return account.ToDetailDto(await repository.CountCreatedTasksAsync(id));
    }

    public async Task<AccountDetailDto> UpdateAsync(int id, UpdateAccountRequest request)
    {
        var account = await repository.GetByIdAsync(id) ?? throw NotFound(id);

        if (request.Role is { } role && role != account.Role)
        {
            // An admin can never demote themselves, so there is always at least one admin left.
            if (id == currentUser.AccountId)
                throw new BadRequestException("You cannot change your own role.");
            account.Role = role;
        }

        if (request.FullName is not null)
            account.FullName = request.FullName.Trim();

        await repository.UpdateAsync(account);
        return account.ToDetailDto(await repository.CountCreatedTasksAsync(id));
    }

    public async Task DeleteAsync(int id)
    {
        var account = await repository.GetByIdAsync(id) ?? throw NotFound(id);
        if (id == currentUser.AccountId)
            throw new BadRequestException("You cannot delete your own account.");

        var createdTasks = await repository.CountCreatedTasksAsync(id);
        if (createdTasks > 0)
            throw new BadRequestException(
                $"Cannot delete account \"{account.Email}\" because it has created {createdTasks} task(s).");

        await repository.DeleteAsync(account);
    }

    private static NotFoundException NotFound(int id) => new($"Account with id {id} was not found.");
}
