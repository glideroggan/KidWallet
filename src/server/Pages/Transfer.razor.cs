﻿using Microsoft.AspNetCore.Components;
using server.Data;
using server.Data.User;
using server.Services;
using server.Services.Exceptions;

namespace server.Pages;

public class TransferBase : PageBase
{
    [CascadingParameter] public Action<string> NotificationCallback { get; set; }
    [Parameter] public string? Name { get; set; }
    [Inject] private UserService UserService { get; set; }
    [Inject] private AccountService AccountService { get; set; }
    protected class PayTransferModel
    {
        public string Description { get; set; }
    };

    protected class PageModel
    {
        public UserModel? Child { get; set; }
        public int Cost { get; set; }
        public PayTransferModel PayTransfer { get; set; }
    }

    protected PayTransferModel PayModel { get; set; } = new();
    protected PageModel ModelPage { get; set; } = new();
    protected TransferTypeEnum TransferType;

    protected enum TransferTypeEnum
    {
        Transfer, Pay
    }

    protected void ChildChanged(string val)
    {
        ModelPage.Child = children
            .First(x => string.Equals(x.Name, val, StringComparison.InvariantCulture));
        StateHasChanged();
    }

    protected List<UserModel> children = new();
    protected override async Task OnInitializedAsyncCallback()
    {
        PayModel = new();
        ModelPage = new();
        ModelPage.PayTransfer = PayModel;
        // get kids under logged in parent
        children = await UserService.GetChildrenAsync(State.User.Id);
        
        // TODO: if we get name from page, we should already check that radio input
    }

    protected void TransferTypeChanged(string val)
    {
        TransferType = Enum.Parse<TransferTypeEnum>(val);
    }

    protected async Task DoTransfer()
    {
        // TODO: handle any errors, like rollback
        // Now that is handled inside the service, but we get no feedback back here that it failed
        // and that is bad, so we need to handle it
        // TODO: fix waiting
        
        // TODO: transfer
        if (ModelPage.PayTransfer != null)
        {
            var transferModel = new KidBuyModel(ModelPage.Child, ModelPage.Cost, ModelPage.PayTransfer.Description);
            try
            {
                await AccountService.KidBuyAsync(transferModel);
            }
            catch (ServiceException e)
            {
                NotificationCallback(e.Message);
            }
            
            // send a notify as feedback when done
            NotificationCallback("Sent");
        }
    }

    protected bool AllGood()
    {
        return ValidateModel(ModelPage);
    }

    private bool ValidateModel(PageModel modelPage)
    {
        return !(modelPage.Child != null &&
                 modelPage.Cost > 0 &&
                 modelPage.PayTransfer?.Description.Length > 2);
    }
}