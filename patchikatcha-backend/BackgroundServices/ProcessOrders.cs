namespace patchikatcha_backend.BackgroundServices
{
    public class ProcessOrders : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await Task.Yield();

            while (token.IsCancellationRequested == false)
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                Console.WriteLine("Teste");
            }
        }
    }
}
