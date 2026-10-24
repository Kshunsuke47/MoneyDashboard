using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels.Items;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Services;
using System.Collections.ObjectModel;

namespace MoneyDashboard.App.ViewModels;

public partial class ExpenseViewModel : ObservableObject
{
    private readonly IExpenseService _expenseService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ExpenseViewModel> _logger;

    public ExpenseViewModel(
        IExpenseService expenseService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<ExpenseViewModel> logger)
    {
        _expenseService = expenseService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;

        var today = DateOnly.FromDateTime(DateTime.Today);

        TargetMonth =
            new DateOnly(today.Year, today.Month, 1);

        ExpenseDate = today;
        _logger = logger;
    }

    [ObservableProperty]
    private DateOnly targetMonth;

    [ObservableProperty]
    private DateOnly? expenseDate;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private long? selectedCategoryId;

    [ObservableProperty]
    private long? amount;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private bool isUnexpected;

    [ObservableProperty]
    private long expenseTotal;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private long? editingExpenseId;

    public bool IsRegisterMode => !IsEditMode;

    public string FormTitle =>
        IsEditMode
            ? "支出の編集"
            : "支出の登録";

    public ObservableCollection<CategoryItem>
        Categories
    { get; } = new();

    public ObservableCollection<ExpenseListItem>
        Expenses
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
            await _expenseService.GetCategoriesAsync();

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

            var expenses =
                await _expenseService.GetByMonthAsync(
                    TargetMonth);

            ExpenseTotal =
                await _expenseService.GetTotalByMonthAsync(
                    TargetMonth);

            Expenses.Clear();

            foreach (var expense in expenses)
            {
                var categoryName =
                    Categories
                        .FirstOrDefault(
                            x => x.CategoryId ==
                                 expense.CategoryId)
                        ?.CategoryName
                    ?? string.Empty;

                Expenses.Add(
                    new ExpenseListItem
                    {
                        ExpenseId = expense.ExpenseId,
                        ExpenseDate = expense.ExpenseDate,
                        Description = expense.Description,
                        CategoryName = categoryName,
                        Amount = expense.Amount,
                        Note = expense.Note,
                        IsUnexpected = expense.IsUnexpected
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load expense data. TargetMonth: {TargetMonth}",
                TargetMonth.ToString("yyyy-MM"));

            _dialogService.ShowError(
                "支出データの取得に失敗しました。");
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

        TargetMonth = TargetMonth.AddMonths(-1);

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

        TargetMonth = TargetMonth.AddMonths(1);

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
        if (IsEditMode || IsLoading || !ValidateInput())
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            var expense = new Expense
            {
                ExpenseDate = ExpenseDate!.Value,
                Description = Description.Trim(),
                CategoryId = SelectedCategoryId!.Value,
                Amount = Amount!.Value,
                Note = string.IsNullOrWhiteSpace(Note)
                    ? null
                    : Note.Trim(),
                IsUnexpected = IsUnexpected
            };

            await _expenseService.RegisterAsync(expense);

            ResetInput();

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to register expense.");

            _dialogService.ShowError(
                "支出の登録に失敗しました。");
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
    private async Task BeginEditAsync(long expenseId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var expense =
                await _expenseService.GetByIdAsync(expenseId);

            if (expense is null)
            {
                _dialogService.ShowError(
                    "対象の支出データが見つかりませんでした。");

                return;
            }

            EditingExpenseId = expense.ExpenseId;
            ExpenseDate = expense.ExpenseDate;
            Description = expense.Description;
            SelectedCategoryId = expense.CategoryId;
            Amount = expense.Amount;
            Note = expense.Note ?? string.Empty;
            IsUnexpected = expense.IsUnexpected;

            IsEditMode = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load expense for editing. ExpenseId: {ExpenseId}",
                expenseId);

            _dialogService.ShowError(
                "支出データの取得に失敗しました。");
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
            EditingExpenseId is null ||
            IsLoading ||
            !ValidateInput())
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var expense = new Expense
            {
                ExpenseId = EditingExpenseId.Value,
                ExpenseDate = ExpenseDate!.Value,
                Description = Description.Trim(),
                CategoryId = SelectedCategoryId!.Value,
                Amount = Amount!.Value,
                Note = string.IsNullOrWhiteSpace(Note)
                    ? null
                    : Note.Trim(),
                IsUnexpected = IsUnexpected
            };

            var result =
                await _expenseService.UpdateAsync(expense);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象の支出データが見つかりませんでした。");
            }

            IsEditMode = false;
            EditingExpenseId = null;

            ResetInput();

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update expense. ExpenseId: {ExpenseId}",
                EditingExpenseId);

            _dialogService.ShowError(
                "支出の更新に失敗しました。");
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
        EditingExpenseId = null;

        ResetInput();
    }

    [RelayCommand]
    private async Task DeleteAsync(long expenseId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        var confirmed =
            _dialogService.ShowConfirmation(
                "この支出データを削除しますか？");

        if (!confirmed)
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var result =
                await _expenseService.DeleteAsync(expenseId);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象の支出データが見つかりませんでした。");
            }

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete expense. ExpenseId: {ExpenseId}",
                expenseId);

            _dialogService.ShowError(
                "支出の削除に失敗しました。");
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
        if (ExpenseDate is null)
        {
            _dialogService.ShowError(
                "支出日を入力してください。");

            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            _dialogService.ShowError(
                "支出内容を入力してください。");

            return false;
        }

        if (Description.Length > 50)
        {
            _dialogService.ShowError(
                "支出内容は50文字以内で入力してください。");

            return false;
        }

        if (SelectedCategoryId is null)
        {
            _dialogService.ShowError(
                "カテゴリを選択してください。");

            return false;
        }

        if (Amount is null ||
            Amount < 1 ||
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

        if (ExpenseDate.Value < startDate ||
            ExpenseDate.Value >= nextMonthStart)
        {
            _dialogService.ShowError(
                "支出日は対象年月内の日付を入力してください。");

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
            ExpenseDate = today;
        }
        else
        {
            ExpenseDate =
                new DateOnly(
                    TargetMonth.Year,
                    TargetMonth.Month,
                    1);
        }

        Description = string.Empty;
        SelectedCategoryId = null;
        Amount = null;
        Note = string.Empty;

        // 通常の支出へ戻す
        IsUnexpected = false;
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