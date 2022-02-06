using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using server.Data.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using server.Data;
using server.Repositories;
using server.Services;

namespace tests;

[TestClass]
public class TaskServiceTests
{
    private readonly TaskService target;
    private readonly IDbContextFactory<WalletContext> _dbContextFactory;

    public TaskServiceTests()
    {

        var taskRepo = Mock.Of<IRepo<TaskDto>>();
        var statRepo = Mock.Of<IRepo<StatDto>>();
        var userRepo = Mock.Of<IRepo<UserDto>>();
        var spendingRepo = Mock.Of<IRepo<SpendingAccountDto>>();
        var accountHistoryRepo = Mock.Of<IRepo<AccountHistoryDto>>();
        var notifyService = Mock.Of<NotifyService>();
        _dbContextFactory = Mock.Of<IDbContextFactory<WalletContext>>();
        var state = Mock.Of<AppState>();
        target = new TaskService(taskRepo, statRepo, userRepo, spendingRepo, accountHistoryRepo,
            notifyService, _dbContextFactory, state);
    }
    
    [TestMethod]
    public async Task ApproveOK()
    {
        // arrange
        var taskId = 1;
        var expectedTask = new TaskDto()
        {

        };
        
        // act
        await target.Approve(1);

        // assert
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var actualTask = context.Tasks.FindAsync(1);
        // TODO: compare actual with expected
        // TODO: check that correct services were called
    }
}