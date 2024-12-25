using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia.Base;

namespace Henson.MessageBox;

public class CustomMsBox<V, VM, T> : IMsBox<T> where V : UserControl, IFullApi<T>, ISetCloseAction where VM : ISetFullApi<T>, IInput
{
    private readonly V _view;
    private readonly VM _viewModel;
    private readonly Styling _styling;

    public Task<T> ShowAsPopupAsync(Window owner)
    {
        throw new System.NotImplementedException();
    }

    public string InputValue => _viewModel.InputValue;

    public CustomMsBox(V view, VM viewModel, Styling styling)
    {
        _view = view;
        _viewModel = viewModel;
        _styling = styling;
    }

    public Task<T> ShowAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<T> ShowWindowAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<T> ShowWindowDialogAsync(Window owner)
    {
        _viewModel.SetFullApi(_view);
        DockPanel dockPanel = new()
        {
            Children = { _view }
        };
        var window = new CustomMessageBoxWindow(_styling)
        {
            Content = dockPanel,
            DataContext = _viewModel
        };
        window.Closed += _view.CloseWindow!;
        var tcs = new TaskCompletionSource<T>();

        _view.SetCloseAction(() =>
        {
            tcs.TrySetResult(_view.GetButtonResult());
            window.Close();
        });

        window.ShowDialog(owner);
        return tcs.Task;
    }

    public Task<T> ShowAsPopupAsync(ContentControl owner)
    {
        throw new System.NotImplementedException();
    }
}