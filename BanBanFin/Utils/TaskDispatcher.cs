namespace BanBanFin.Utils;

public class TaskDispatcher
{
    public static void Run(Action action)
    {
        Task.Run(() =>
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }
}