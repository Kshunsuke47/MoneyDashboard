using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using MoneyDashboard.App.Services;
using MoneyDashboard.App.ViewModels.Items;
using MoneyDashboard.Core.Enums;
using MoneyDashboard.Core.Models;
using MoneyDashboard.Core.Services;
using System.Collections.ObjectModel;


namespace MoneyDashboard.App.ViewModels;

public partial class SubscriptionViewModel : ObservableObject
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<SubscriptionViewModel> _logger;


    public SubscriptionViewModel(
        ISubscriptionService subscriptionService,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<SubscriptionViewModel> logger)
    {
        _subscriptionService = subscriptionService;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _logger = logger;
    }

    [ObservableProperty]
    private string serviceName = string.Empty;

    [ObservableProperty]
    private long? selectedCategoryId;

    [ObservableProperty]
    private long? paymentAmount;

    [ObservableProperty]
    private PaymentCycle paymentCycle = PaymentCycle.Monthly;

    [ObservableProperty]
    private int? paymentMonth;

    [ObservableProperty]
    private int? paymentDay;

    [ObservableProperty]
    private SubscriptionStatus status = SubscriptionStatus.Active;

    [ObservableProperty]
    private string note = string.Empty;

    [ObservableProperty]
    private long activeMonthlyTotal;

    [ObservableProperty]
    private int activeSubscriptionCount;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isEditMode;

    [ObservableProperty]
    private long? editingSubscriptionId;

    public bool IsRegisterMode => !IsEditMode;

    public bool IsPaymentMonthEnabled =>
        PaymentCycle == PaymentCycle.Yearly;

    public string FormTitle =>
        IsEditMode
            ? "サブスクの編集"
            : "サブスクの登録";

    public IReadOnlyList<PaymentCycle> PaymentCycles { get; } =
        Enum.GetValues<PaymentCycle>();

    public IReadOnlyList<SubscriptionStatus> Statuses { get; } =
        Enum.GetValues<SubscriptionStatus>();

    public ObservableCollection<CategoryItem>
        Categories
    { get; } = new();

    public ObservableCollection<SubscriptionListItem>
        Subscriptions
    { get; } = new();

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(IsRegisterMode));
        OnPropertyChanged(nameof(FormTitle));

        _navigationService.IsNavigationLocked = value;
    }

    partial void OnPaymentCycleChanged(PaymentCycle value)
    {
        OnPropertyChanged(nameof(IsPaymentMonthEnabled));

        if (value == PaymentCycle.Monthly)
        {
            PaymentMonth = null;
        }
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
            await _subscriptionService.GetCategoriesAsync();

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

            var subscriptions =
                await _subscriptionService.GetAllAsync();

            ActiveMonthlyTotal =
                await _subscriptionService
                    .GetActiveMonthlyTotalAsync();

            ActiveSubscriptionCount =
                await _subscriptionService
                    .GetActiveCountAsync();

            Subscriptions.Clear();

            foreach (var subscription in subscriptions)
            {
                var categoryName =
                    Categories
                        .FirstOrDefault(
                            x => x.CategoryId ==
                                 subscription.CategoryId)
                        ?.CategoryName
                    ?? string.Empty;

                Subscriptions.Add(
                    new SubscriptionListItem
                    {
                        SubscriptionId =
                            subscription.SubscriptionId,

                        ServiceName =
                            subscription.ServiceName,

                        CategoryName =
                            categoryName,

                        PaymentAmount =
                            subscription.PaymentAmount,

                        PaymentCycle =
                            subscription.PaymentCycle,

                        PaymentMonth =
                            subscription.PaymentMonth,

                        PaymentDay =
                            subscription.PaymentDay,

                        MonthlyEquivalentAmount =
                            _subscriptionService
                                .CalculateMonthlyAmount(
                                    subscription),

                        Status =
                            subscription.Status,

                        Note =
                            subscription.Note
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load subscription data.");

            _dialogService.ShowError(
                "サブスクデータの取得に失敗しました。");
        }
        finally
        {
            IsLoading = false;
        }
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
        if (IsEditMode ||
            IsLoading ||
            !ValidateInput())
        {
            return;
        }

        var succeeded = false;

        try
        {
            IsLoading = true;

            var subscription =
                CreateSubscription();

            await _subscriptionService.RegisterAsync(
                subscription);

            ResetInput();

            succeeded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to register subscription.");

            _dialogService.ShowError(
                "サブスクの登録に失敗しました。");
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
    private async Task BeginEditAsync(
        long subscriptionId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;

            var subscription =
                await _subscriptionService.GetByIdAsync(
                    subscriptionId);

            if (subscription is null)
            {
                _dialogService.ShowError(
                    "対象のサブスクが見つかりませんでした。");

                return;
            }

            EditingSubscriptionId =
                subscription.SubscriptionId;

            ServiceName =
                subscription.ServiceName;

            SelectedCategoryId =
                subscription.CategoryId;

            PaymentAmount =
                subscription.PaymentAmount;

            PaymentCycle =
                subscription.PaymentCycle;

            PaymentMonth =
                subscription.PaymentMonth;

            PaymentDay =
                subscription.PaymentDay;

            Status =
                subscription.Status;

            Note =
                subscription.Note ?? string.Empty;

            IsEditMode = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load subscription for editing. SubscriptionId: {SubscriptionId}",
                subscriptionId);

            _dialogService.ShowError(
                "サブスクデータの取得に失敗しました。");
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
            EditingSubscriptionId is null ||
            IsLoading ||
            !ValidateInput())
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var subscription =
                CreateSubscription();

            subscription.SubscriptionId =
                EditingSubscriptionId.Value;

            var result =
                await _subscriptionService.UpdateAsync(
                    subscription);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象のサブスクが見つかりませんでした。");
            }

            IsEditMode = false;
            EditingSubscriptionId = null;

            ResetInput();

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update subscription. SubscriptionId: {SubscriptionId}",
                EditingSubscriptionId);

            _dialogService.ShowError(
                "サブスクの更新に失敗しました。");
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
        EditingSubscriptionId = null;

        ResetInput();
    }

    [RelayCommand]
    private async Task DeleteAsync(
        long subscriptionId)
    {
        if (IsEditMode || IsLoading)
        {
            return;
        }

        var confirmed =
            _dialogService.ShowConfirmation(
                "このサブスクを削除しますか？");

        if (!confirmed)
        {
            return;
        }

        var shouldRefresh = false;

        try
        {
            IsLoading = true;

            var result =
                await _subscriptionService.DeleteAsync(
                    subscriptionId);

            if (!result)
            {
                _dialogService.ShowError(
                    "対象のサブスクが見つかりませんでした。");
            }

            shouldRefresh = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete subscription. SubscriptionId: {SubscriptionId}",
                subscriptionId);

            _dialogService.ShowError(
                "サブスクの削除に失敗しました。");
        }
        {
            IsLoading = false;
        }

        if (shouldRefresh)
        {
            await RefreshAsync();
        }
    }

    private Subscription CreateSubscription()
    {
        return new Subscription
        {
            ServiceName = ServiceName.Trim(),

            CategoryId =
                SelectedCategoryId!.Value,

            PaymentAmount =
                PaymentAmount!.Value,

            PaymentCycle =
                PaymentCycle,

            PaymentMonth =
                PaymentCycle == PaymentCycle.Yearly
                    ? PaymentMonth
                    : null,

            PaymentDay =
                PaymentDay!.Value,

            Status =
                Status,

            Note =
                string.IsNullOrWhiteSpace(Note)
                    ? null
                    : Note.Trim()
        };
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            _dialogService.ShowError(
                "サービス名を入力してください。");

            return false;
        }

        if (ServiceName.Length > 50)
        {
            _dialogService.ShowError(
                "サービス名は50文字以内で入力してください。");

            return false;
        }

        if (SelectedCategoryId is null)
        {
            _dialogService.ShowError(
                "カテゴリを選択してください。");

            return false;
        }

        if (PaymentAmount is null ||
            PaymentAmount < 1 ||
            PaymentAmount > 999_999_999)
        {
            _dialogService.ShowError(
                "支払金額は1円以上999,999,999円以下で入力してください。");

            return false;
        }

        if (PaymentCycle == PaymentCycle.Yearly &&
            PaymentMonth is null)
        {
            _dialogService.ShowError(
                "年払いの場合は支払月を入力してください。");

            return false;
        }

        if (PaymentMonth is < 1 or > 12)
        {
            _dialogService.ShowError(
                "支払月は1月から12月で入力してください。");

            return false;
        }

        if (PaymentDay is null ||
            PaymentDay < 1 ||
            PaymentDay > 31)
        {
            _dialogService.ShowError(
                "支払日は1日から31日で入力してください。");

            return false;
        }

        if (PaymentCycle == PaymentCycle.Yearly &&
            !IsValidYearlyPaymentDate(
                PaymentMonth!.Value,
                PaymentDay.Value))
        {
            _dialogService.ShowError(
                "指定した支払月には、その支払日は存在しません。");

            return false;
        }

        if (Note.Length > 200)
        {
            _dialogService.ShowError(
                "備考は200文字以内で入力してください。");

            return false;
        }

        return true;
    }

    private static bool IsValidYearlyPaymentDate(
        int month,
        int day)
    {
        // 2月29日は有効とするため、うるう年を使用
        const int leapYear = 2024;

        return day <=
            DateTime.DaysInMonth(
                leapYear,
                month);
    }

    private void ResetInput()
    {
        ServiceName = string.Empty;
        SelectedCategoryId = null;
        PaymentAmount = null;

        PaymentCycle =
            PaymentCycle.Monthly;

        PaymentMonth = null;
        PaymentDay = null;

        Status =
            SubscriptionStatus.Active;

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