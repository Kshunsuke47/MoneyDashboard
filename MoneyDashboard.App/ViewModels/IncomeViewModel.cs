using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels.Items;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Services;
using System.Collections.ObjectModel;

namespace MoneyDashboard.App.ViewModels;

public partial class IncomeViewModel : ObservableObject
{
    private readonly IIncomeService _incomeService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<IncomeViewModel> _logger;

    public IncomeViewModel(
        IIncomeService incomeService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<IncomeViewModel> logger)
    {
        _incomeService = incomeService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        var today = DateOnly.FromDateTime(DateTime.Today);

        TargetMonth =
            new DateOnly(today.Year, today.Month, 1);

        IncomeDate = today;
        _logger = logger;
    }

    [ObservableProperty]
    private DateOnly targetMonth;

    [ObservableProperty]
    private DateOnly? incomeDate;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private long? selectedCategoryId;

    [ObservableProperty]
    private long? amount;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private long incomeTotal;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private long? editingIncomeId;

    public bool IsRegisterMode => !IsEditMode;

    public string FormTitle =>
        IsEditMode
            ? "収入の編集"
            : "収入の登録";

    public ObservableCollection<CategoryItem>
        Categories
    { get; } = new();

    public ObservableCollection<IncomeListItem>
        Incomes
    { get; } = new();

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRegisterMode));
        OnPropertyChanged(nameof(FormTitle));

        _navigationService.IsNavigationLocked = value;
    }

    public async Task LoadAsync()
    {
        await LoadCategoriesAsync();
        await RefreshAsync();

        ResetInput();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories =
            await _incomeService.GetCategoriesAsync();

        Categories.Clear();

        foreach (var category in categories)
        {
            Categories.Add(
                new CategoryItem
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName
                });
        }
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

            var incomes =
                await _incomeService.GetByMonthAsync(
                    TargetMonth);

            IncomeTotal =
                await _incomeService.GetTotalByMonthAsync(
                    TargetMonth);

            Incomes.Clear();

            foreach (var income in incomes)
            {
                var categoryName =
                    Categories
                        .FirstOrDefault(
                            x => x.CategoryId ==
                                 income.CategoryId)
                        ?.CategoryName
                    ?? string.Empty;

                Incomes.Add(
                    new IncomeListItem
                    {
                        IncomeId = income.IncomeId,
                        IncomeDate = income.IncomeDate,
                        Description = income.Description,
                        CategoryName = categoryName,
                        Amount = income.Amount,
                        Note = income.Note
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load income data. TargetMonth: {TargetMonth}",
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "収入データの取得に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        TargetMonth =
            TargetMonth.AddMonths(-1);

        ResetInput();

        await RefreshAsync();
    }

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        TargetMonth =
            TargetMonth.AddMonths(1);

        ResetInput();

        await RefreshAsync();
    }

    [RelayCommand]
    private void Clear()
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        ResetInput();
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        if (!ValidateInput())
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            var income = new Income
            {
                IncomeDate = IncomeDate!.Value,
                Description = Description.Trim(),
                CategoryId = SelectedCategoryId!.Value,
                Amount = Amount!.Value,
                Note = string.IsNullOrWhiteSpace(Note)
                    ? null
                    : Note.Trim()
            };

            await _incomeService.RegisterAsync(income);

            ResetInput();
            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to register income.");

            _dialogService.ShowError(
                "収入の登録に失敗しました。");
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
    private async Task BeginEditAsync(long incomeId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var income =
                await _incomeService.GetByIdAsync(incomeId);

            if (income is null)
            {
                _dialogService.ShowError(
                    "対象の収入データが見つかりませんでした.");

                return;
            }

            EditingIncomeId = income.IncomeId;
            IncomeDate = income.IncomeDate;
            Description = income.Description;
            SelectedCategoryId = income.CategoryId;
            Amount = income.Amount;
            Note = income.Note ?? string.Empty;

            IsEditMode = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load income for editing. IncomeId: {IncomeId}",
                incomeId);

            _dialogService.ShowError(
                "収入データの取得に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (!IsEditMode ||
            EditingIncomeId is null ||
            IsLoading)
        {
            return;
        }

        if (!ValidateInput())
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var income = new Income
            {
                IncomeId = EditingIncomeId.Value,
                IncomeDate = IncomeDate!.Value,
                Description = Description.Trim(),
                CategoryId = SelectedCategoryId!.Value,
                Amount = Amount!.Value,
                Note = string.IsNullOrWhiteSpace(Note)
                    ? null
                    : Note.Trim()
            };

            var result =
                await _incomeService.UpdateAsync(income);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象の収入データが見つかりませんでした。");
            }

            IsEditMode = false;
            EditingIncomeId = null;

            ResetInput();

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update income. IncomeId: {IncomeId}",
                EditingIncomeId);

            _dialogService.ShowError(
                "収入の更新に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (shouldRefresh)
        {
            await RefreshAsync();
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (!IsEditMode || IsLoading)
        {
            return;
        }

        IsEditMode = false;
        EditingIncomeId = null;

        ResetInput();
    }

    [RelayCommand]
    private async Task DeleteAsync(long incomeId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        var confirmed =
            _dialogService.ShowConfirmation(
                "この収入データを削除しますか？");

        if (!confirmed)
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var result =
                await _incomeService.DeleteAsync(incomeId);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象の収入データが見つかりませんでした。");
            }

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete income. IncomeId: {IncomeId}",
                incomeId);

            _dialogService.ShowError(
                "収入の削除に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }

        if (shouldRefresh)
        {
            await RefreshAsync();
        }
    }

    private bool ValidateInput()
    {
        if (IncomeDate is null)
        {
            _dialogService.ShowError(
                "収入日を入力してください。");

            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            _dialogService.ShowError(
                "収入内容を入力してください。");

            return false;
        }

        if (Description.Length > 50)
        {
            _dialogService.ShowError(
                "収入内容は50文字以内で入力してください。");

            return false;
        }

        if (SelectedCategoryId is null)
        {
            _dialogService.ShowError(
                "カテゴリを選択してください。");

            return false;
        }

        if (Amount is null)
        {
            _dialogService.ShowError(
                "金額を入力してください。");

            return false;
        }

        if (Amount < 1 ||
            Amount > 999_999_999)
        {
            _dialogService.ShowError(
                "金額は1円以上999,999,999円以下で入力してください。");

            return false;
        }

        if (Note.Length > 200)
        {
            _dialogService.ShowError(
                "備考は200文字以内で入力してください。");

            return false;
        }

        var startDate =
            new DateOnly(
                TargetMonth.Year,
                TargetMonth.Month,
                1);

        var nextMonthStart =
            startDate.AddMonths(1);

        if (IncomeDate.Value < startDate ||
            IncomeDate.Value >= nextMonthStart)
        {
            _dialogService.ShowError(
                "収入日は対象年月内の日付を入力してください。");

            return false;
        }

        return true;
    }

    private void ResetInput()
    {
        var today =
            DateOnly.FromDateTime(DateTime.Today);

        if (today.Year == TargetMonth.Year &&
            today.Month == TargetMonth.Month)
        {
            IncomeDate = today;
        }
        else
        {
            IncomeDate =
                new DateOnly(
                    TargetMonth.Year,
                    TargetMonth.Month,
                    1);
        }

        Description = string.Empty;
        SelectedCategoryId = null;
        Amount = null;
        Note = string.Empty;
    }

    [RelayCommand]
    private void Navigate(string destination)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        _navigationService.Navigate(destination);
    }
}