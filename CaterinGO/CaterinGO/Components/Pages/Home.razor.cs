using System.ComponentModel.DataAnnotations;
using CaterinGO.Configuration;
using CaterinGO.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace CaterinGO.Components.Pages;

public partial class Home : IDisposable
{
    [Inject]
    protected IWaitingListService WaitingListService { get; set; } = default!;

    [Inject]
    protected IOptions<CaterinGoOptions> CaterinGoOptions { get; set; } = default!;

    [Inject]
    protected LanguageState LanguageState { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected WaitListFormModel WaitListModel { get; } = new();

    protected bool IsSubmitting { get; private set; }

    protected bool SubmissionFailed { get; private set; }

    protected string? FeedbackMessage { get; private set; }

    protected bool IsEnglish => LanguageState.Current == "en";

    protected static string StoreButtonStyle(string imagePath) =>
        $"background-image: url('{imagePath}');";

    protected void NavigateToWaitList() =>
        Navigation.NavigateTo("#lista-oczekujacych");

    protected override void OnInitialized()
    {
        LanguageState.Changed += OnLanguageChanged;
    }

    protected async Task SubmitWaitListAsync()
    {
        if (IsSubmitting)
        {
            return;
        }

        IsSubmitting = true;
        SubmissionFailed = false;
        FeedbackMessage = null;

        try
        {
            await WaitingListService.AddOrRefreshAsync(WaitListModel.Email);
            FeedbackMessage = IsEnglish
                ? "Thank you! You are on our list."
                : "Dziękujemy! Jesteś na naszej liście.";
            WaitListModel.Email = string.Empty;
        }
        catch
        {
            SubmissionFailed = true;
            FeedbackMessage = IsEnglish
                ? "We could not save your address. Please try again shortly."
                : "Nie udało się zapisać adresu. Spróbuj ponownie za chwilę.";
        }
        finally
        {
            IsSubmitting = false;
        }
    }

    private void OnLanguageChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        LanguageState.Changed -= OnLanguageChanged;
    }

    protected sealed class WaitListFormModel
    {
        [Required(ErrorMessage = "Podaj adres e-mail.")]
        [EmailAddress(ErrorMessage = "Wpisz poprawny adres e-mail.")]
        [MaxLength(320)]
        public string Email { get; set; } = string.Empty;
    }
}
