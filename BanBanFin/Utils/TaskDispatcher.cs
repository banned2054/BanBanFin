namespace BanBanFin.Utils;

public class TaskHelp
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