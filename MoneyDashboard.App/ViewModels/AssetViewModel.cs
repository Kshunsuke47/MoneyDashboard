using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels.Items;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Services;
using System.Collections.ObjectModel;

namespace MoneyDashboard.App.ViewModels;

public partial class AssetViewModel : ObservableObject
{
    private readonly IAssetService _assetService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<AssetViewModel> _logger;

    public AssetViewModel(
        IAssetService assetService,
        IDialogService dialogService,
        INavigationService navigationService,
        ILogger<AssetViewModel> logger)
    {
        _assetService = assetService;
        _dialogService = dialogService;
        _navigationService = navigationService;

        var today =
            DateOnly.FromDateTime(DateTime.Today);

        TargetMonth =
            new DateOnly(today.Year, today.Month, 1);
        _navigationService = navigationService;
        _logger = logger;
    }

    [ObservableProperty]
    private DateOnly targetMonth;

    [ObservableProperty]
    private string accountName = string.Empty;

    [ObservableProperty]
    private bool isAccountEditMode;

    [ObservableProperty]
    private long? editingAccountId;

    [ObservableProperty]
    private long? openingBankBalance;

    [ObservableProperty]
    private long? nisaEvaluationAmount;

    [ObservableProperty]
    private long? totalAssets;

    [ObservableProperty]
    private bool isLoading;

    public bool IsAccountRegisterMode =>
        !IsAccountEditMode;

    public string AccountFormTitle =>
        IsAccountEditMode
            ? "口座名の編集"
            : "口座の登録";

    public ObservableCollection<MonthlyBankBalanceItem>
        BankBalances
    { get; } = new();

    partial void OnIsAccountEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAccountRegisterMode));
        OnPropertyChanged(nameof(AccountFormTitle));

        _navigationService.IsNavigationLocked = value;
    }

    public async Task LoadAsync()
    {
        await RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var data =
                await _assetService.GetAssetDataAsync(
                    TargetMonth);

            OpeningBankBalance =
                data.OpeningBankBalance;

            NisaEvaluationAmount =
                data.NisaEvaluationAmount;

            TotalAssets =
                data.TotalAssets;

            BankBalances.Clear();

            foreach (var account in data.BankAccounts)
            {
                var balance =
                    data.MonthlyBalances
                        .FirstOrDefault(
                            x => x.AccountId ==
                                 account.AccountId);

                // 利用中口座は表示
                // 利用終了口座は、その月の残高がある場合だけ表示
                if (!account.IsActive &&
                    balance is null)
                {
                    continue;
                }

                BankBalances.Add(
                    new MonthlyBankBalanceItem
                    {
                        AccountId =
                            account.AccountId,

                        AccountName =
                            account.AccountName,

                        IsActive =
                            account.IsActive,

                        OpeningBalance =
                            balance?.OpeningBalance
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load asset data. TargetMonth: {TargetMonth}",
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "資産データの取得に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        TargetMonth =
            TargetMonth.AddMonths(-1);

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        TargetMonth =
            TargetMonth.AddMonths(1);

        await RefreshAsync();
    }

    [RelayCommand]
    private void ClearAccount()
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        AccountName = string.Empty;
    }

    [RelayCommand]
    private async Task RegisterAccountAsync()
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        if (!ValidateAccountName())
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            var account = new BankAccount
            {
                AccountName =
                    AccountName.Trim(),

                IsActive = true
            };

            await _assetService
                .RegisterAccountAsync(account);

            AccountName = string.Empty;

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to register bank account.");

            _dialogService.ShowError(
                "口座の登録に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (succeeded)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private void BeginAccountEdit(long accountId)
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        var account =
            BankBalances.FirstOrDefault(
                x => x.AccountId == accountId);

        if (account is null)
        {
            _dialogService.ShowError(
                "対象の口座が見つかりませんでした。");

            return;
        }

        EditingAccountId = account.AccountId;
        AccountName = account.AccountName;

        IsAccountEditMode = true;
    }

    [RelayCommand]
    private async Task UpdateAccountAsync()
    {
        if (!IsAccountEditMode ||
            EditingAccountId is null ||
            IsLoading)
        {
            return;
        }

        if (!ValidateAccountName())
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            await _assetService.UpdateAccountAsync(
                EditingAccountId.Value,
                AccountName.Trim());

            IsAccountEditMode = false;
            EditingAccountId = null;

            AccountName = string.Empty;

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update bank account. AccountId: {AccountId}",
                EditingAccountId);

            _dialogService.ShowError(
                "口座名の更新に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (succeeded)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private void CancelAccountEdit()
    {
        if (!IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        IsAccountEditMode = false;
        EditingAccountId = null;

        AccountName = string.Empty;
    }

    [RelayCommand]
    private async Task DeactivateAccountAsync(
        long accountId)
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        var confirmed =
            _dialogService.ShowConfirmation(
                "この口座を利用終了にしますか？");

        if (!confirmed)
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            await _assetService
                .DeactivateAccountAsync(accountId);

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to deactivate bank account. AccountId: {AccountId}",
                accountId);

            _dialogService.ShowError(
                "口座の利用終了処理に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (succeeded)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task SaveBalanceAsync(
        MonthlyBankBalanceItem item)
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        if (item.OpeningBalance is null)
        {
            _dialogService.ShowError(
                "月初預金残高を入力してください。");

            return;
        }

        if (item.OpeningBalance < 0 ||
            item.OpeningBalance > 999_999_999)
        {
            _dialogService.ShowError(
                "月初預金残高は0円以上999,999,999円以下で入力してください。");

            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            await _assetService
                .SaveOpeningBalanceAsync(
                    item.AccountId,
                    TargetMonth,
                    item.OpeningBalance.Value);

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save opening balance. AccountId: {AccountId}, TargetMonth: {TargetMonth}",
                item.AccountId,
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "月初預金残高の保存に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (succeeded)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private async Task SaveNisaAsync()
    {
        if (IsAccountEditMode ||
            IsLoading)
        {
            return;
        }

        if (NisaEvaluationAmount is null)
        {
            _dialogService.ShowError(
                "NISA評価額を入力してください。");

            return;
        }

        if (NisaEvaluationAmount < 0 ||
            NisaEvaluationAmount > 999_999_999)
        {
            _dialogService.ShowError(
                "NISA評価額は0円以上999,999,999円以下で入力してください。");

            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            await _assetService.SaveNisaValueAsync(
                TargetMonth,
                NisaEvaluationAmount.Value);

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to save NISA value. TargetMonth: {TargetMonth}",
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "NISA評価額の保存に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (succeeded)
        {
            await RefreshAsync();
        }
    }

    private bool ValidateAccountName()
    {
        if (string.IsNullOrWhiteSpace(
            AccountName))
        {
            _dialogService.ShowError(
                "口座名を入力してください。");

            return false;
        }

        if (AccountName.Length > 50)
        {
            _dialogService.ShowError(
                "口座名は50文字以内で入力してください。");

            return false;
        }

        return true;
    }
}