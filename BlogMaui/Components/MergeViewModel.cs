using CommunityToolkit.Mvvm.ComponentModel;

namespace BlogMaui.Components;

public partial class MergeViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedCandidate = string.Empty;

    public List<string> Candidates { get; }

    private Action<string> setValue;

    public MergeViewModel(List<string> candidates, Action<string> setValue)
    {
        this.Candidates = candidates;
        this.setValue = setValue;
    }

    partial void OnSelectedCandidateChanged(string? oldValue, string newValue)
    {
        if (newValue != null)
        {
            setValue(newValue);
        }
    }
}